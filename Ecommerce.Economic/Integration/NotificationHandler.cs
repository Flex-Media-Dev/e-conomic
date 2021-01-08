using Dynamicweb;
using Dynamicweb.Configuration;
using Dynamicweb.Content.Files;
using Dynamicweb.Core;
using Dynamicweb.Core.Helpers;
using Dynamicweb.Ecommerce;
using Dynamicweb.Ecommerce.Economic;
using Dynamicweb.Ecommerce.Frontend;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Ecommerce.Orders.Gateways;
using Dynamicweb.Ecommerce.Prices;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Ecommerce.Products.Taxes;
using Dynamicweb.Environment;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Mailing;
using Dynamicweb.Rendering;
using Dynamicweb.Security.Licensing;
using Economic.Api;
using Economic.Api.Data;
using Economic.Api.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	internal class NotificationHandler : IDisposable
	{
		private IProduct _feeProduct;

		private IProduct _discountProduct;

		private Dynamicweb.Ecommerce.Economic.EconomicCommunicator _economicCommunicator;

		private Dynamicweb.Ecommerce.Orders.Order _dynamicwebOrder;

		private string _orderNumberPrefix;

		private List<OrderCompleteNotificationExtender> AddinInstances
		{
			get
			{
				ArrayList addInClasses = AddInManager.GetAddInClasses(typeof(OrderCompleteNotificationExtender));
				List<OrderCompleteNotificationExtender> orderCompleteNotificationExtenders = new List<OrderCompleteNotificationExtender>();
				foreach (object addInClass in addInClasses)
				{
					Type addInTypeByName = AddInManager.GetAddInTypeByName(addInClass.ToString());
					if (addInTypeByName == null)
					{
						continue;
					}
					orderCompleteNotificationExtenders.Add(Activator.CreateInstance(addInTypeByName) as OrderCompleteNotificationExtender);
				}
				return orderCompleteNotificationExtenders;
			}
		}

		private IProduct DiscountProduct
		{
			get
			{
				if (this._discountProduct == null)
				{
					string str = Converter.ToString(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DiscountProductNumber"));
					if (!string.IsNullOrEmpty(str))
					{
						this._discountProduct = this.EconomicCommunicator.GetProductByNumber(str);
					}
				}
				return this._discountProduct;
			}
		}

		private Dynamicweb.Ecommerce.Orders.Order DynamicwebOrder
		{
			get
			{
				return this._dynamicwebOrder;
			}
			set
			{
				this._dynamicwebOrder = value;
			}
		}

		private Dynamicweb.Ecommerce.Economic.EconomicCommunicator EconomicCommunicator
		{
			get
			{
				if (this._economicCommunicator == null)
				{
					this._economicCommunicator = new Dynamicweb.Ecommerce.Economic.EconomicCommunicator();
				}
				return this._economicCommunicator;
			}
		}

		private IProduct FeeProduct
		{
			get
			{
				if (this._feeProduct == null)
				{
					string str = Converter.ToString(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/FeeProductNumber"));
					if (!string.IsNullOrEmpty(str))
					{
						this._feeProduct = this.EconomicCommunicator.GetProductByNumber(str);
					}
				}
				return this._feeProduct;
			}
		}

		private string OrderNumberPrefix
		{
			get
			{
				if (this._orderNumberPrefix == null)
				{
					this._orderNumberPrefix = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/OrderNumberPrefix");
					if (!string.IsNullOrEmpty(this._orderNumberPrefix) && !this._orderNumberPrefix.Contains(":"))
					{
						this._orderNumberPrefix = string.Format("{0}: ", this._orderNumberPrefix);
					}
				}
				return this._orderNumberPrefix;
			}
		}

		public NotificationHandler()
		{
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this._economicCommunicator != null)
			{
				this._economicCommunicator.Dispose();
			}
		}

		private string GetEconomicOrderDeliveryAddress(Dynamicweb.Ecommerce.Orders.Order dwOrder)
		{
			string empty = string.Empty;
			if (!string.IsNullOrEmpty(dwOrder.DeliveryName))
			{
				empty = string.Concat(dwOrder.DeliveryName, "\n");
			}
			if (!string.IsNullOrEmpty(dwOrder.DeliveryCompany))
			{
				empty = string.Concat(empty, dwOrder.DeliveryCompany, "\n");
			}
			if (!string.IsNullOrEmpty(dwOrder.DeliveryAddress))
			{
				empty = string.Concat(empty, dwOrder.DeliveryAddress, "\n");
			}
			if (!string.IsNullOrEmpty(dwOrder.DeliveryAddress2))
			{
				empty = string.Concat(empty, dwOrder.DeliveryAddress2);
			}
			return empty;
		}

		private void HandleEanException(ValidationException ex, IDebtorData debitorData)
		{
			string ean;
			if (string.Equals(ex.Code, "E04000", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(ex.Message) && (ex.Message.ToLower().Contains("european article number") || ex.Message.ToLower().Contains("ean")))
			{
				if (debitorData == null)
				{
					OrderDebuggingInfo.Save(this.DynamicwebOrder, "Economic EAN validation error", "Economic integration");
				}
				else
				{
					OrderDebuggingInfo.Save(this.DynamicwebOrder, string.Format("Economic EAN validation error. Debitor EAN: {0}.", debitorData.Ean), "Economic integration");
				}
				if (SystemConfiguration.get_Instance().GetBoolean("/Globalsettings/Ecom/EconomicIntegration/Ordering/CancelOrdersWithEanValidationError") && this.DynamicwebOrder.Complete)
				{
					OrderDebuggingInfo.Save(this.DynamicwebOrder, "Try to cancel Order due to Economic EAN validation error", "Economic integration");
					if ((new OrderManager(this.DynamicwebOrder)).CancelOrder())
					{
						Dynamicweb.Ecommerce.Services.Products.UpdateStock(this.DynamicwebOrder, ProductOrderStockOperator.Positive);
						TaxSetting.GetActiveTaxSettings(this.DynamicwebOrder).CancelTaxes(this.DynamicwebOrder);
					}
				}
				if (SystemConfiguration.get_Instance().GetBoolean("/Globalsettings/Ecom/EconomicIntegration/Ordering/NotifyOnEanValidationError"))
				{
					string value = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Notification/Recipients");
					if (!string.IsNullOrEmpty(value))
					{
						string str = string.Concat(ex.Code, " - ", ex.Message);
						if (debitorData != null)
						{
							ean = debitorData.Ean;
						}
						else
						{
							ean = null;
						}
						this.SendMail(str, ean, value.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries));
					}
				}
			}
		}

		/// <summary>
		/// OrderCompleteNotification : Exports the order to economic when the order is complete.
		/// </summary>
		/// <param name="dwOrder">The dynamicweb order.</param>
		public void HandleOrder(Dynamicweb.Ecommerce.Orders.Order dwOrder)
		{
			double quantity;
			decimal? unitNetPrice;
			decimal num;
			if (!LicenseManager.LicenseHasFeature("eCom_economic"))
			{
				return;
			}
			IDebtorData debtorDatum = null;
			try
			{
				Dynamicweb.Ecommerce.Economic.EconomicCommunicator economicCommunicator = null;
				try
				{
					economicCommunicator = this.EconomicCommunicator;
				}
				catch (Exception exception)
				{
				}
				if (economicCommunicator != null)
				{
					this.DynamicwebOrder = dwOrder;
					NotificationHandler.OrderBillingHandler(dwOrder);
					foreach (OrderCompleteNotificationExtender addinInstance in this.AddinInstances)
					{
						try
						{
							dwOrder = addinInstance.DynamicwebOrder(dwOrder);
						}
						catch (Exception exception1)
						{
						}
					}
					debtorDatum = economicCommunicator.MakeDebitor(dwOrder.CustomerNumber, dwOrder.CustomerCompany, string.Concat(dwOrder.CustomerAddress, "\n", dwOrder.CustomerAddress2), dwOrder.CustomerCity, dwOrder.CustomerCountry, dwOrder.CustomerEan, dwOrder.CustomerEmail, dwOrder.CustomerZip, dwOrder.CustomerPhone);
					foreach (OrderCompleteNotificationExtender orderCompleteNotificationExtender in this.AddinInstances)
					{
						debtorDatum = orderCompleteNotificationExtender.EconomicDebitor(dwOrder, debtorDatum);
					}
					IDebtor debtor = null;
					try
					{
						debtor = economicCommunicator.SaveDebitor(debtorDatum);
					}
					catch (IntegrityException integrityException)
					{
					}
					catch (AuthorizationException authorizationException)
					{
					}
					if (debtor != null)
					{
						economicCommunicator.SaveDebitorContact(debtor, dwOrder.CustomerName, dwOrder.CustomerPhone, dwOrder.CustomerEmail);
						dwOrder.CustomerNumber = debtor.Number;
						string economicOrderDeliveryAddress = this.GetEconomicOrderDeliveryAddress(dwOrder);
						IOrderData orderDatum = economicCommunicator.MakeOrder(dwOrder.CustomerNumber, string.Concat(this.OrderNumberPrefix, dwOrder.Id), economicOrderDeliveryAddress, dwOrder.DeliveryCity, dwOrder.DeliveryCountry, dwOrder.DeliveryZip, "", dwOrder.Comment, DateTime.Now, dwOrder.Date, DateTime.Now, "", dwOrder.CurrencyCode);
						foreach (OrderCompleteNotificationExtender addinInstance1 in this.AddinInstances)
						{
							orderDatum = addinInstance1.EconomicOrder(dwOrder, orderDatum);
						}
						Economic.Api.IOrder order = null;
						try
						{
							order = economicCommunicator.SaveOrder(orderDatum);
						}
						catch (IntegrityException integrityException1)
						{
						}
						if (order != null)
						{
							int number = order.Number;
							foreach (Dynamicweb.Ecommerce.Orders.OrderLine orderLine in dwOrder.OrderLines)
							{
								if (orderLine.HasType(OrderLineType.Discount) || orderLine.HasType(OrderLineType.ProductDiscount))
								{
									continue;
								}
								string productNumber = orderLine.ProductNumber;
								quantity = orderLine.Quantity;
								decimal num1 = decimal.Parse(quantity.ToString());
								quantity = orderLine.UnitPrice.PriceWithoutVAT;
								IOrderLineData orderLineDatum = economicCommunicator.MakeOrderLine(number, productNumber, num1, decimal.Parse(quantity.ToString()));
								if (orderLineDatum.Product == null)
								{
									continue;
								}
								foreach (OrderCompleteNotificationExtender orderCompleteNotificationExtender1 in this.AddinInstances)
								{
									orderLineDatum = orderCompleteNotificationExtender1.EconomicOrderline(orderLine, orderLineDatum);
								}
								try
								{
									economicCommunicator.SaveOrderLine(orderLineDatum);
								}
								catch (IntegrityException integrityException3)
								{
									IntegrityException integrityException2 = integrityException3;
									this.OrderErrorHandler(string.Concat("E-conomic IntegrityException - SaveOrderLine failed : ", integrityException2.Code, " - ", integrityException2.Message));
									economicCommunicator.DeleteOrderFromEconomic(number);
									order = null;
									break;
								}
								catch (AuthorizationException authorizationException2)
								{
									AuthorizationException authorizationException1 = authorizationException2;
									this.OrderErrorHandler(string.Concat("E-conomic AuthorizationException - SaveOrderLine failed : ", authorizationException1.Code, " - ", authorizationException1.Message));
									economicCommunicator.DeleteOrderFromEconomic(number);
									order = null;
									break;
								}
							}
							if (order != null)
							{
								dwOrder.IntegrationOrderId = order.Number.ToString();
								(new OrderService()).Save(dwOrder);
								if (this.DiscountProduct != null)
								{
									foreach (Dynamicweb.Ecommerce.Orders.OrderLine orderLine1 in dwOrder.OrderLines)
									{
										if (!orderLine1.HasType(OrderLineType.Discount) && !orderLine1.HasType(OrderLineType.ProductDiscount))
										{
											continue;
										}
										string str = this.DiscountProduct.Number;
										quantity = orderLine1.Quantity;
										decimal num2 = decimal.Parse(quantity.ToString());
										quantity = orderLine1.UnitPrice.PriceWithoutVAT;
										IOrderLineData orderLineDatum1 = economicCommunicator.MakeOrderLine(number, str, num2, decimal.Parse(quantity.ToString()), orderLine1.ProductName);
										foreach (OrderCompleteNotificationExtender addinInstance2 in this.AddinInstances)
										{
											orderLineDatum1 = addinInstance2.EconomicDiscountOrderline(orderLine1, orderLineDatum1);
										}
										if (orderLineDatum1 == null)
										{
											continue;
										}
										unitNetPrice = orderLineDatum1.UnitNetPrice;
										num = new decimal();
										if ((unitNetPrice.GetValueOrDefault() == num ? unitNetPrice.HasValue : false))
										{
											continue;
										}
										this.SaveOrderLine(economicCommunicator, orderLineDatum1);
									}
								}
								if (this.FeeProduct != null)
								{
									if (!string.IsNullOrEmpty(dwOrder.PaymentMethodId))
									{
										string number1 = this.FeeProduct.Number;
										decimal one = decimal.One;
										quantity = dwOrder.PaymentFee.PriceWithoutVAT;
										IOrderLineData orderLineDatum2 = economicCommunicator.MakeOrderLine(number, number1, one, decimal.Parse(quantity.ToString()), dwOrder.PaymentMethodDescription);
										foreach (OrderCompleteNotificationExtender orderCompleteNotificationExtender2 in this.AddinInstances)
										{
											orderLineDatum2 = orderCompleteNotificationExtender2.EconomicPaymentFeeOrderline(orderLineDatum2);
										}
										if (orderLineDatum2 != null)
										{
											unitNetPrice = orderLineDatum2.UnitNetPrice;
											num = new decimal();
											if ((unitNetPrice.GetValueOrDefault() == num ? !unitNetPrice.HasValue : true))
											{
												this.SaveOrderLine(economicCommunicator, orderLineDatum2);
											}
										}
									}
									if (!string.IsNullOrEmpty(dwOrder.ShippingMethodId))
									{
										string str1 = this.FeeProduct.Number;
										decimal one1 = decimal.One;
										quantity = dwOrder.ShippingFee.PriceWithoutVAT;
										IOrderLineData orderLineDatum3 = economicCommunicator.MakeOrderLine(number, str1, one1, decimal.Parse(quantity.ToString()), dwOrder.ShippingMethodDescription);
										foreach (OrderCompleteNotificationExtender addinInstance3 in this.AddinInstances)
										{
											orderLineDatum3 = addinInstance3.EconomicShippingFeeOrderline(orderLineDatum3);
										}
										if (orderLineDatum3 != null)
										{
											unitNetPrice = orderLineDatum3.UnitNetPrice;
											num = new decimal();
											if ((unitNetPrice.GetValueOrDefault() == num ? !unitNetPrice.HasValue : true))
											{
												this.SaveOrderLine(economicCommunicator, orderLineDatum3);
											}
										}
									}
								}
							}
						}
					}
				}
			}
			catch (IntegrityException integrityException5)
			{
				IntegrityException integrityException4 = integrityException5;
				this.OrderErrorHandler(string.Concat("E-conomic IntegrityException - Integrity action failed : ", integrityException4.Code, " - ", integrityException4.Message));
			}
			catch (SchemaException schemaException1)
			{
				SchemaException schemaException = schemaException1;
				this.OrderErrorHandler(string.Concat("E-conomic SchemaException - Schema action failed : ", schemaException.Code, " - ", schemaException.Message));
			}
			catch (ValidationException validationException1)
			{
				ValidationException validationException = validationException1;
				this.HandleEanException(validationException, debtorDatum);
				this.OrderErrorHandler(string.Concat("E-conomic ValidationException - Validation action failed : ", validationException.Code, " - ", validationException.Message));
			}
			catch (ServerException serverException1)
			{
				ServerException serverException = serverException1;
				this.OrderErrorHandler(string.Concat("E-conomic ServerException - Server action failed : ", serverException.Code, " - ", serverException.Message));
			}
			catch (AuthorizationException authorizationException4)
			{
				AuthorizationException authorizationException3 = authorizationException4;
				this.OrderErrorHandler(string.Concat("E-conomic AuthorizationException - Authorization action failed : ", authorizationException3.Code, " - ", authorizationException3.Message));
			}
			catch (UserException userException1)
			{
				UserException userException = userException1;
				this.OrderErrorHandler(string.Concat("E-conomic UserException - User action failed : ", userException.Code, " - ", userException.Message));
			}
			catch (Exception exception2)
			{
				this.OrderErrorHandler(string.Concat("General Exception", exception2.ToString()));
			}
		}

		private static void OrderBillingHandler(Dynamicweb.Ecommerce.Orders.Order order)
		{
			if (string.IsNullOrEmpty(order.CustomerNumber))
			{
				order.CustomerNumber = order.CustomerPhone;
			}
			if (string.IsNullOrEmpty(order.DeliveryAddress))
			{
				order.DeliveryAddress = order.CustomerAddress;
			}
			if (string.IsNullOrEmpty(order.DeliveryAddress2))
			{
				order.DeliveryAddress2 = order.CustomerAddress2;
			}
			if (string.IsNullOrEmpty(order.DeliveryCity))
			{
				order.DeliveryCity = order.CustomerCity;
			}
			if (string.IsNullOrEmpty(order.DeliveryCountry))
			{
				order.DeliveryCountry = order.CustomerCountry;
			}
			if (string.IsNullOrEmpty(order.DeliveryEmail))
			{
				order.DeliveryEmail = order.CustomerEmail;
			}
			if (string.IsNullOrEmpty(order.DeliveryFax))
			{
				order.DeliveryFax = order.CustomerFax;
			}
			if (string.IsNullOrEmpty(order.DeliveryName))
			{
				order.DeliveryName = order.CustomerName;
			}
			if (string.IsNullOrEmpty(order.DeliveryPhone))
			{
				order.DeliveryPhone = order.CustomerPhone;
			}
			if (string.IsNullOrEmpty(order.DeliveryZip))
			{
				order.DeliveryZip = order.CustomerZip;
			}
			if (string.IsNullOrEmpty(order.DeliveryTitle))
			{
				order.DeliveryTitle = order.CustomerTitle;
			}
			if (string.IsNullOrEmpty(order.DeliveryFirstName))
			{
				order.DeliveryFirstName = order.CustomerFirstName;
			}
			if (string.IsNullOrEmpty(order.DeliveryMiddleName))
			{
				order.DeliveryMiddleName = order.CustomerMiddleName;
			}
			if (string.IsNullOrEmpty(order.DeliverySurname))
			{
				order.DeliverySurname = order.CustomerSurname;
			}
			if (string.IsNullOrEmpty(order.DeliveryHouseNumber))
			{
				order.DeliveryHouseNumber = order.CustomerHouseNumber;
			}
		}

		private void OrderErrorHandler(string exception)
		{
			Dynamicweb.Ecommerce.Orders.Order dynamicwebOrder = this.DynamicwebOrder;
			if (dynamicwebOrder != null)
			{
				string str = Context.get_Current().get_Server().MapPath(string.Concat("/Files/", FilesAndFolders.GetFilesFolderName(), "/Integration/eCommerce/economic/OrderErrors"));
				try
				{
					if (!System.IO.Directory.Exists(str))
					{
						System.IO.Directory.CreateDirectory(str);
					}
					XmlWriterSettings xmlWriterSetting = new XmlWriterSettings()
					{
						Indent = true,
						NewLineHandling = NewLineHandling.Entitize,
						NewLineOnAttributes = true,
						Encoding = Encoding.UTF8
					};
					using (FileStream fileStream = null)
					{
						fileStream = new FileStream(string.Concat(str, "/", dynamicwebOrder.Id, ".xml"), FileMode.Create);
						using (XmlWriter xmlWriter = XmlWriter.Create(fileStream, xmlWriterSetting))
						{
							xmlWriter.WriteStartDocument();
							xmlWriter.WriteStartElement("Order");
							xmlWriter.WriteAttributeString("ID", dynamicwebOrder.Id);
							xmlWriter.WriteAttributeString("Date", dynamicwebOrder.Date.ToString());
							xmlWriter.WriteAttributeString("CurrencyCode", dynamicwebOrder.CurrencyCode);
							xmlWriter.WriteAttributeString("CustomerNumber", dynamicwebOrder.CustomerNumber);
							xmlWriter.WriteAttributeString("DeliveryAddress", dynamicwebOrder.DeliveryAddress);
							xmlWriter.WriteAttributeString("DeliveryAddress2", dynamicwebOrder.DeliveryAddress2);
							xmlWriter.WriteAttributeString("DeliveryCity", dynamicwebOrder.DeliveryCity);
							xmlWriter.WriteAttributeString("DeliveryCountry", dynamicwebOrder.DeliveryCountry);
							xmlWriter.WriteAttributeString("DeliveryZip", dynamicwebOrder.DeliveryZip);
							foreach (Dynamicweb.Ecommerce.Orders.OrderLine orderLine in dynamicwebOrder.OrderLines)
							{
								xmlWriter.WriteStartElement("Orderline");
								xmlWriter.WriteAttributeString("ProductNumber", orderLine.ProductNumber);
								xmlWriter.WriteAttributeString("VariantText", orderLine.ProductVariantText);
								xmlWriter.WriteAttributeString("Quantity", orderLine.Quantity.ToString());
								xmlWriter.WriteAttributeString("UnitPriceWithVAT", orderLine.UnitPrice.PriceWithVATFormattedNoSymbol);
								xmlWriter.WriteAttributeString("UnitPriceWithoutVAT", orderLine.UnitPrice.PriceWithoutVATFormattedNoSymbol);
								xmlWriter.WriteEndElement();
							}
							if (!string.IsNullOrEmpty(exception))
							{
								xmlWriter.WriteStartElement("Exception");
								xmlWriter.WriteAttributeString("msg", exception);
								xmlWriter.WriteEndElement();
							}
							xmlWriter.WriteEndElement();
						}
					}
				}
				catch (Exception exception1)
				{
				}
			}
		}

		private void SaveOrderLine(Dynamicweb.Ecommerce.Economic.EconomicCommunicator ec, IOrderLineData orderlineData)
		{
			try
			{
				ec.SaveOrderLine(orderlineData);
			}
			catch (IntegrityException integrityException1)
			{
				IntegrityException integrityException = integrityException1;
				this.OrderErrorHandler(string.Concat("E-conomic IntegrityException - SaveOrderLine failed : ", integrityException.Code, " - ", integrityException.Message));
			}
			catch (AuthorizationException authorizationException1)
			{
				AuthorizationException authorizationException = authorizationException1;
				this.OrderErrorHandler(string.Concat("E-conomic AuthorizationException - SaveOrderLine failed : ", authorizationException.Code, " - ", authorizationException.Message));
			}
		}

		private void SendMail(string message, string ean, string[] recipients)
		{
			if (recipients.Length != 0)
			{
				using (MailMessage mailMessage = new MailMessage())
				{
					string value = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Notification/SenderEmail");
					if (!StringHelper.IsValidEmailAddress(value))
					{
						value = "noreply@dynamicweb-cms.com";
					}
					mailMessage.From = new MailAddress(value, SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Notification/SenderName"));
					mailMessage.Subject = string.Format("E-conomic EAN error on send order {0}", this.DynamicwebOrder.Id);
					string[] strArrays = recipients;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str = strArrays[i];
						mailMessage.To.Add(str.Trim());
					}
					message = string.Format("E-conomic ValidationException: {0}.", message);
					string value1 = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Notification/Template");
					if (string.IsNullOrEmpty(value1) || !File.Exists(Context.get_Current().get_Server().MapPath(string.Concat("/Files/Templates/Economic/Notifications/", value1))))
					{
						mailMessage.Body = string.Format("E-conomic ValidationException: {0}.", message);
					}
					else
					{
						Template template = new Template(string.Concat("/Economic/Notifications/", value1));
						template.SetTag("EconomicIntegration.Order.Error", message);
						if (!string.IsNullOrEmpty(ean))
						{
							template.SetTag("EconomicIntegration.Order.Debitor.EAN", ean);
						}
						(new Renderer()).RenderOrderDetails(template, this.DynamicwebOrder, true);
						mailMessage.IsBodyHtml = true;
						mailMessage.Body = template.Output();
					}
					mailMessage.BodyEncoding = Encoding.UTF8;
					mailMessage.SubjectEncoding = Encoding.UTF8;
					EmailHandler.Send(mailMessage, true);
				}
			}
		}
	}
}