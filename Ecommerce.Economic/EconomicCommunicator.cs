using Dynamicweb.Core;
using Dynamicweb.Core.Helpers;
using Economic.Api;
using Economic.Api.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Dynamicweb.Ecommerce.Economic
{
	/// <summary>
	/// Wrapper class for Economic API
	/// </summary>
	[DataObject(true)]
	public class EconomicCommunicator : IDisposable
	{
		private EconomicSession _economicSession;

		private Dynamicweb.Ecommerce.Economic.ConfigReader _configReader;

		private HttpClient client;

		/// <summary>
		/// object: ConfigReader where all settings are loaded and stored
		/// </summary>
		public Dynamicweb.Ecommerce.Economic.ConfigReader ConfigReader
		{
			get
			{
				return this._configReader;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public EconomicCommunicator() : this(true)
		{
		}

		public EconomicCommunicator(bool connect)
		{
			string str = "DynamicwebIntegration/1.1 (http://dynamicweb.com; noreply@dynamicweb.com) BasedOnSuperLib/1.4";
			this._configReader = new Dynamicweb.Ecommerce.Economic.ConfigReader();
			this._economicSession = new EconomicSession(str);
			this.client = new HttpClient()
			{
				BaseAddress = new Uri("https://restapi.e-conomic.com")
			};
			this.client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			this.client.DefaultRequestHeaders.Add("X-AppSecretToken", this._configReader.AppSecretToken);
			this.client.DefaultRequestHeaders.Add("X-AgreementGrantToken", this._configReader.AgreementGrantToken);
			if (connect)
			{
				this.Connect();
			}
		}

		public string Connect()
		{
			return this._economicSession.ConnectWithToken(this._configReader.AgreementGrantToken, this._configReader.AppSecretToken);
		}

		/// <summary>
		/// Checks wether a debitor exists in your E-conomic account
		/// </summary>
		/// <param name="debtorNumber">string: number of the debitor in your e-conomic account</param>
		/// <returns>bool: true of the debitor exists else false</returns>
		public bool DebitorExists(string debtorNumber)
		{
			bool flag;
			try
			{
				bool flag1 = false;
				if (this._economicSession.Debtor.FindByNumber(debtorNumber) != null)
				{
					flag1 = true;
				}
				flag = flag1;
			}
			catch (Exception exception)
			{
				flag = false;
			}
			return flag;
		}

		/// <summary>
		/// Deletes an order in e-conomic
		/// </summary>
		/// <param name="orderNumber">int: Ordernumber</param>
		/// <remarks>
		/// 	Use with caution it delete an entire order in e-conomic
		/// 	Should typically only be used when an exception is thrown by creating orderlines, then you can delete the order so that nothing half-finished is left for administrating
		/// </remarks>
		public void DeleteOrderFromEconomic(int orderNumber)
		{
			IOrder order = this._economicSession.Order.FindByNumber(orderNumber);
			if (order != null)
			{
				order.Delete();
			}
		}

		public void DisConnect()
		{
			this._economicSession.Disconnect();
		}

		/// <summary>
		/// Disposes and closes the connection of the E-conomic session
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			this.DisConnect();
			this.client.Dispose();
		}

		/// <summary>
		/// Gets all accounts
		/// </summary>
		/// <returns><see cref="T:Economic.Api.IAccount" /> array: a special type from the E-conomic API</returns>
		public IAccount[] GetAllAccounts()
		{
			return this._economicSession.Account.GetAll();
		}

		/// <summary>
		/// Gets all Currencies
		/// </summary>
		/// <returns>ICurrency array: a special type from the E-conomic API</returns>
		public ICurrency[] GetAllCurrencies()
		{
			return this._economicSession.Currency.GetAll();
		}

		/// <summary>
		/// Gets all debitor groups
		/// </summary>
		/// <returns>IDeptorGroup array: a special type from the E-conomic API</returns>
		public IDebtorGroup[] GetAllDebitorGroups()
		{
			return this._economicSession.DebtorGroup.GetAll();
		}

		/// <summary>
		/// Gets all debitor layouts
		/// </summary>
		/// <returns><see cref="T:Economic.Api.ITemplateCollection" /> array: a special type from the E-conomic API</returns>
		public ITemplateCollection[] GetAllDebitorLayouts()
		{
			return this._economicSession.TemplateCollection.GetAll();
		}

		/// <summary>
		/// Gets all invoices
		/// </summary>
		/// <returns><see cref="T:Economic.Api.IInvoice" /> array: a special type from the E-conomic API</returns>
		public IInvoice[] GetAllInvoices()
		{
			return this._economicSession.Invoice.GetAll();
		}

		/// <summary>
		/// Gets all Terms of payment see also <seealso cref="M:Dynamicweb.Ecommerce.Economic.EconomicCommunicator.GetAllTermsOfPayments" />
		/// </summary>
		/// <returns><see cref="T:Economic.Api.ITermOfPayment" /> array: a special type from the E-conomic API</returns>
		public ITermOfPayment[] GetAllPaymenttypes()
		{
			return this._economicSession.TermOfPayment.GetAll();
		}

		/// <summary>
		/// Gets alle productgroups
		/// </summary>
		/// <returns><see cref="T:Economic.Api.IProductGroup" /> array: a special type from the E-conomic API</returns>
		public IProductGroup[] GetAllProductGroups()
		{
			return this._economicSession.ProductGroup.GetAll();
		}

		/// <summary>
		/// Gets all products from e-conomic <remarks>Watch out for performance issue, there could be many products in E-conomic to fetch</remarks>
		/// </summary>
		/// <returns>object: array of IProduct</returns>
		public IProduct[] GetAllProducts()
		{
			return this._economicSession.Product.GetAll();
		}

		/// <summary>
		/// Gets all Terms of payment see also <seealso cref="M:Dynamicweb.Ecommerce.Economic.EconomicCommunicator.GetAllPaymenttypes" />
		/// </summary>
		/// <returns><see cref="T:Economic.Api.ITermOfPayment" /> array: a special type from the E-conomic API</returns>
		public ITermOfPayment[] GetAllTermsOfPayments()
		{
			return this._economicSession.TermOfPayment.GetAll();
		}

		/// <summary>
		/// Get all Units
		/// </summary>
		/// <returns><see cref="T:Economic.Api.IUnit" /> array: a special type from the E-conomic API</returns>
		public IUnit[] GetAllUnits()
		{
			return this._economicSession.Unit.GetAll();
		}

		/// <summary>
		/// Gets all VAT groups
		/// </summary>
		/// <returns>IVapAccount array: a special type from the E-conomic API</returns>
		public IVatAccount[] GetAllVatGroups()
		{
			return this._economicSession.VatAccount.GetAll();
		}

		/// <summary>
		/// E-conomic API Information
		/// </summary>
		/// <returns>string: E-conomic API info</returns>
		public string GetApiInfo()
		{
			return this._economicSession.GetApiInformation();
		}

		/// <summary>
		/// Gets a debitor by number
		/// </summary>
		/// <param name="debtorNumber">string: number of the debitor in your e-conomic account</param>
		/// <returns>IDeptor object: a special type from E-conomic API</returns>
		public IDebtor GetDebitor(string debtorNumber)
		{
			int num = 0;
			if (!int.TryParse(debtorNumber, out num))
			{
				return null;
			}
			return this._economicSession.Debtor.FindByNumber(debtorNumber);
		}

		public IEnumerable<Result> GetDebitorGroups()
		{
			List<Result> results = new List<Result>();
			IDebtorGroup[] all = this._economicSession.DebtorGroup.GetAll();
			for (int i = 0; i < (int)all.Length; i++)
			{
				IDebtorGroup debtorGroup = all[i];
				results.Add(new Result()
				{
					Text = debtorGroup.Name,
					Selected = false,
					Value = debtorGroup.Number.ToString()
				});
			}
			return results;
		}

		public IEnumerable<Result> GetDebitorLayouts()
		{
			List<Result> results = new List<Result>();
			ITemplateCollection[] all = this._economicSession.TemplateCollection.GetAll();
			for (int i = 0; i < (int)all.Length; i++)
			{
				ITemplateCollection templateCollection = all[i];
				results.Add(new Result()
				{
					Text = templateCollection.Name,
					Selected = false,
					Value = templateCollection.Name
				});
			}
			return results;
		}

		/// <summary>
		/// Gets the Invoice object 
		/// </summary>
		/// <param name="invoiceNumber">int: InvoiceNumber</param>
		/// <returns><see cref="T:Economic.Api.IInvoice" /> object of the Invoice</returns>
		public IInvoice GetInvoice(int invoiceNumber)
		{
			return this._economicSession.Invoice.FindByNumber(invoiceNumber);
		}

		/// <summary>
		/// Gets the PDF document of a specific invoice
		/// </summary>
		/// <param name="invoiceNumber">int: InvoiceNumber</param>
		/// <returns>byte[] array of the PDF file</returns>
		public byte[] GetInvoicePdf(int invoiceNumber)
		{
			return this._economicSession.Invoice.FindByNumber(invoiceNumber).GetPdf();
		}

		/// <summary>
		/// Gets all invoices made in the provided date range
		/// </summary>
		/// <param name="from">DateTime: from date</param>
		/// <param name="to">DateTime: to date</param>
		/// <returns><see cref="T:Economic.Api.IInvoice" /> array of all invoices within the provided date range</returns>
		public IInvoice[] GetInvoicesByDate(DateTime from, DateTime to)
		{
			return this._economicSession.Invoice.FindByDate(from, to);
		}

		/// <summary>
		/// Gets all invoices made on the specified date
		/// </summary>
		/// <param name="date">DateTime: the date</param>
		/// <returns><see cref="T:Economic.Api.IInvoice" /> array of all invoices made on the provided date</returns>
		public IInvoice[] GetInvoicesByDate(DateTime date)
		{
			return this._economicSession.Invoice.FindByDate(date, date.AddDays(1));
		}

		public IEnumerable<Result> GetPaymenttypes()
		{
			List<Result> results = new List<Result>();
			ITermOfPayment[] all = this._economicSession.TermOfPayment.GetAll();
			for (int i = 0; i < (int)all.Length; i++)
			{
				ITermOfPayment termOfPayment = all[i];
				results.Add(new Result()
				{
					Text = termOfPayment.Name,
					Selected = false,
					Value = termOfPayment.Name
				});
			}
			return results;
		}

		/// <summary>
		/// Gets a single product by simple search by number
		/// </summary>
		/// <param name="productNumber">string: the product number to search for</param>
		/// <returns>object: IProduct if found else null</returns>
		public IProduct GetProductByNumber(string productNumber)
		{
			return this._economicSession.Product.FindByNumber(productNumber);
		}

		/// <summary>
		/// Gets a product group
		/// </summary>
		/// <param name="groupId">int: Group ID to search for</param>
		/// <returns>IProductGroup object</returns>
		public IProductGroup GetProductGroupById(int groupId)
		{
			return this._economicSession.ProductGroup.FindByNumber(groupId);
		}

		/// <summary>
		/// Gets a product groups
		/// </summary>
		/// <param name="groupIds">int: Groups IDs to search for</param>
		/// <returns>IProductGroup array</returns>
		public IProductGroup[] GetProductGroupById(int[] groupIds)
		{
			return this._economicSession.ProductGroup.FindByNumber(groupIds);
		}

		/// <summary>
		/// Gets a productgroup id by providing the group name
		/// </summary>
		/// <param name="groupName">string: name of the group</param>
		/// <returns>int: productgroup id</returns>
		public int GetProductGroupIdByName(string groupName)
		{
			int number = 0;
			IProductGroup[] productGroupArray = this._economicSession.ProductGroup.FindByName(groupName);
			if (productGroupArray.Length != 0)
			{
				number = productGroupArray[0].Number;
			}
			return number;
		}

		public IEnumerable<Result> GetProductGroups()
		{
			List<Result> results = new List<Result>();
			IProductGroup[] all = this._economicSession.ProductGroup.GetAll();
			for (int i = 0; i < (int)all.Length; i++)
			{
				IProductGroup productGroup = all[i];
				results.Add(new Result()
				{
					Text = productGroup.Name,
					Selected = false,
					Value = productGroup.Number.ToString()
				});
			}
			return results;
		}

		/// <summary>
		/// Gets Product Groups Data by Group's ID's
		/// </summary>                        
		/// <param name="groupsIds">int: Groups ID's to search for</param>
		/// <returns>IProductGroupData array</returns>
		public IProductGroupData[] GetProductGroupsDataByIDs(int[] groupsIds)
		{
			IProductGroup[] productGroupArray = this._economicSession.ProductGroup.FindByNumber(groupsIds);
			return this._economicSession.ProductGroupData.GetDataArray(productGroupArray);
		}

		public IEnumerable<Result> GetProducts(string groupName)
		{
			List<Result> results = new List<Result>();
			int productGroupIdByName = this.GetProductGroupIdByName(groupName);
			IProduct[] products = this.GetProductGroupById(productGroupIdByName).GetProducts();
			for (int i = 0; i < (int)products.Length; i++)
			{
				IProduct product = products[i];
				results.Add(new Result()
				{
					Text = product.Name,
					Selected = false,
					Value = product.Number
				});
			}
			return results;
		}

		/// <summary>
		/// Gets all products found
		/// </summary>
		/// <param name="productNumbers">array of strings as productnumbers</param>
		/// <returns>IProduct array</returns>
		/// <remarks>Be careful about matching the length of input array with output array because you can retrieve null objects</remarks>
		public IProduct[] GetProductsByNumber(string[] productNumbers)
		{
			return this._economicSession.Product.FindByNumber(productNumbers);
		}

		/// <summary>
		/// Gets a ProdcutData
		/// </summary>
		/// <param name="products">Array of IProduct</param>
		/// <returns>Array of IProductData</returns>
		public IProductData[] GetProductsData(IProduct[] products)
		{
			return this._economicSession.ProductData.GetDataArray(products);
		}

		public Dictionary<string, Dictionary<string, double>> GetProductsWeightData()
		{
			dynamic obj;
			object str;
			Dictionary<string, Dictionary<string, double>> strs = new Dictionary<string, Dictionary<string, double>>();
			string str1 = "/products?pagesize=1000&filter=(inventory.grossWeight$ne:0$or:inventory.netWeight$ne:0)";
			int num = 0;
			dynamic obj1 = null;
			do
			{
				string str2 = string.Concat(str1, string.Format("&skippages={0}", num));
				HttpResponseMessage result = this.client.GetAsync(str2).Result;
				string result1 = null;
				try
				{
					result.EnsureSuccessStatusCode();
					result1 = result.Content.ReadAsStringAsync().Result;
				}
				catch (HttpRequestException httpRequestException)
				{
					string message = httpRequestException.Message;
					string reasonPhrase = result.ReasonPhrase;
					HttpRequestMessage requestMessage = result.RequestMessage;
					if (requestMessage != null)
					{
						Uri requestUri = requestMessage.RequestUri;
						if (requestUri != null)
						{
							str = requestUri.ToString();
						}
						else
						{
							str = null;
						}
					}
					else
					{
						str = null;
					}
					throw new Exception(string.Format("REST products returned: {0} ({1}) {2}.", message, reasonPhrase, str));
				}
				obj1 = Converter.Deserialize<object>(result1);
				dynamic obj2 = obj1 != (dynamic)null;
				dynamic obj3 = (!obj2 ? obj2 : obj2 & obj1.collection != (dynamic)null);
				if ((!obj3 ? obj3 : obj3 & obj1.collection.HasValues))
				{
					foreach (dynamic obj4 in (IEnumerable)obj1.collection)
					{
						if (obj4.productNumber == (dynamic)null)
						{
							continue;
						}
						string str3 = (string)obj4.productNumber;
						if (strs.ContainsKey(str3))
						{
							continue;
						}
						if (obj4.inventory == (dynamic)null)
						{
							continue;
						}
						Dictionary<string, double> strs1 = new Dictionary<string, double>();
						if (obj4.inventory.grossWeight != (dynamic)null)
						{
							strs1.Add("ProductGrossWeight", typeof(Converter).ToDouble(obj4.inventory.grossWeight));
						}
						if (obj4.inventory.netWeight != (dynamic)null)
						{
							strs1.Add("ProductNetWeight", typeof(Converter).ToDouble(obj4.inventory.netWeight));
						}
						strs.Add(str3, strs1);
					}
				}
				num++;
				obj2 = obj1 != (dynamic)null;
				obj3 = (!obj2 ? obj2 : obj2 & obj1.collection != (dynamic)null);
				obj = (!obj3 ? obj3 : obj3 & obj1.collection.HasValues);
			}
			while (obj);
			return strs;
		}

		/// <summary>
		/// Makes a IDebtorData object with most default properties set
		/// </summary>
		/// <param name="debitorNumber">string: debitor number</param>
		/// <param name="debitorName">string: debitor name</param>
		/// <param name="debitorAddress">string: debitor address</param>
		/// <param name="debitorCity">string: debitor city</param>
		/// <param name="debitorCountry">string debitor country</param>
		/// <param name="debitorEan">string: EAN number</param>
		/// <param name="debitorEmail">string: debitor e-mail</param>
		/// <param name="debitorZipCode">string: debitor zipcode</param>
		/// <param name="debitorTelephoneAndFaxNumber">string: debitor phone and fax number</param>
		/// <returns>IDebtorData object that can be used with <paramref name="SaveDebitor" /></returns>
		public IDebtorData MakeDebitor(string debitorNumber, string debitorName, string debitorAddress, string debitorCity, string debitorCountry, string debitorEan, string debitorEmail, string debitorZipCode, string debitorTelephoneAndFaxNumber)
		{
			Dynamicweb.Ecommerce.Economic.ConfigReader configReader = new Dynamicweb.Ecommerce.Economic.ConfigReader();
			IDebtorGroup debtorGroup = this._economicSession.DebtorGroup.FindByNumber(int.Parse(configReader.DebitorGroup));
			VatZone vatZone = VatZone.HomeCountry;
			string str = configReader.DebitorVatGroup.Trim();
			if (str == "I25")
			{
				vatZone = VatZone.HomeCountry;
			}
			else if (str == "REP")
			{
				vatZone = VatZone.EU;
			}
			else if (str == "U25")
			{
				vatZone = VatZone.Abroad;
			}
			IDebtorData data = null;
			if (this.DebitorExists(debitorNumber))
			{
				data = this._economicSession.DebtorData.GetData(this._economicSession.Debtor.FindByNumber(debitorNumber));
			}
			else
			{
				data = this._economicSession.DebtorData.Create(debitorNumber, debtorGroup, debitorName, vatZone);
				data.IsAccessible = true;
			}
			data.DebtorGroup = debtorGroup;
			if (string.IsNullOrEmpty(debitorName))
			{
				data.Name = "N/A";
			}
			else
			{
				data.Name = debitorName;
			}
			data.Address = debitorAddress;
			data.City = debitorCity;
			data.Country = debitorCountry;
			data.Currency = this._economicSession.Currency.FindByCode(configReader.DebitorCurrency);
			data.DebtorGroup = debtorGroup;
			if (!string.IsNullOrEmpty(debitorEan))
			{
				data.Ean = debitorEan;
			}
			data.Email = debitorEmail;
			data.Layout = this._economicSession.TemplateCollection.FindByName(configReader.DebitorLayout)[0];
			data.PostalCode = debitorZipCode;
			data.TelephoneAndFaxNumber = debitorTelephoneAndFaxNumber;
			data.TermOfPayment = this._economicSession.TermOfPayment.FindByName(configReader.DebitorTermsOfPayment)[0];
			data.VatZone = vatZone;
			return data;
		}

		/// <summary>
		/// Makes an order for <see cref="M:Dynamicweb.Ecommerce.Economic.EconomicCommunicator.SaveOrder(Economic.Api.Data.IOrderData)" />
		/// </summary>
		/// <param name="debitorNumber">string: debitor number</param>
		/// <param name="orderName">string: order name</param>
		/// <param name="deliveryAddress">string: delivery address</param>
		/// <param name="deliveryCity">string: delivery city</param>
		/// <param name="deliveryCountry">string: delivery country</param>
		/// <param name="deliveryPostalCode">string: delivery zipcode</param>
		/// <param name="termsOfDelivery">string: terms of delivery</param>
		/// <param name="comments">string: comments</param>
		/// <param name="dateDeliveryDate">DateTime: delivery date</param>
		/// <param name="orderDate">DateTime: order date</param>
		/// <param name="dueDate">DateTime: due date</param>
		/// <param name="orderLayoutName">The name of the layout</param>
		/// <param name="debitorCurrencyCode">string: currency code for the debitor</param>
		/// <returns>IOrderData object</returns>
		public IOrderData MakeOrder(string debitorNumber, string orderName, string deliveryAddress, string deliveryCity, string deliveryCountry, string deliveryPostalCode, string termsOfDelivery, string comments, DateTime dateDeliveryDate, DateTime orderDate, DateTime dueDate, string orderLayoutName, string debitorCurrencyCode)
		{
			Dynamicweb.Ecommerce.Economic.ConfigReader configReader = new Dynamicweb.Ecommerce.Economic.ConfigReader();
			this._economicSession.DebtorGroup.FindByNumber(int.Parse(configReader.DebitorGroup));
			ICurrency currency = this._economicSession.Currency.FindByCode(debitorCurrencyCode);
			ITemplateCollection templateCollection = this._economicSession.TemplateCollection.FindByName(configReader.DebitorLayout)[0];
			ICurrency currency1 = currency;
			ITemplateCollection templateCollection1 = templateCollection;
			ITermOfPayment termOfPayment = this._economicSession.TermOfPayment.FindByName(configReader.DebitorTermsOfPayment)[0];
			if (termsOfDelivery == "9")
			{
				termOfPayment = this._economicSession.TermOfPayment.FindByName("Dankort")[0];
			}
			else if (termsOfDelivery == "4")
			{
				termOfPayment = this._economicSession.TermOfPayment.FindByName("Bank overførsel")[0];
			}
			else if (termsOfDelivery == "10")
			{
				termOfPayment = this._economicSession.TermOfPayment.FindByName("Netto 14 Dage")[0];
			}
			IDebtor debtor = this._economicSession.Debtor.FindByNumber(debitorNumber);
			IOrderData num = this._economicSession.OrderData.Create(debtor);
			num.Heading = orderName;
			num.Currency = currency1;
			num.ExchangeRate = new decimal(100);
			num.Date = orderDate;
			num.Attention = debtor.Attention;
			num.DebtorAddress = debtor.Address;
			num.DebtorCity = debtor.City;
			num.DebtorCountry = debtor.Country;
			num.DebtorEan = debtor.Ean;
			num.DebtorName = debtor.Name;
			num.DebtorPostalCode = debtor.PostalCode;
			num.DeliveryDate = new DateTime?(dateDeliveryDate);
			num.Date = orderDate;
			num.DueDate = new DateTime?(dueDate);
			num.DeliveryAddress = deliveryAddress;
			num.DeliveryCity = deliveryCity;
			num.DeliveryCountry = deliveryCountry;
			num.DeliveryPostalCode = deliveryPostalCode;
			num.YourReference = debtor.Attention;
			num.TermOfPayment = termOfPayment;
			num.TermsOfDelivery = termsOfDelivery;
			num.TextLine1 = comments;
			num.Layout = templateCollection1;
			num.IsVatIncluded = true;
			return num;
		}

		/// <summary>
		/// Makes an IOrderLineData object
		/// </summary>
		/// <param name="orderNumber">int: Order number</param>
		/// <param name="productNumber">string: product number</param>
		/// <param name="quantity">decimal: Quantity</param>
		/// <param name="price">decimal: Price</param>
		/// <returns>IOrderLineData object</returns>
		public IOrderLineData MakeOrderLine(int orderNumber, string productNumber, decimal quantity, decimal price)
		{
			return this.MakeOrderLine(orderNumber, productNumber, quantity, price, string.Empty);
		}

		public IOrderLineData MakeOrderLine(int orderNumber, string productNumber, decimal quantity, decimal price, string productName)
		{
			IOrder order = this._economicSession.Order.FindByNumber(orderNumber);
			IOrderLineData name = this._economicSession.OrderLineData.Create(order);
			IProduct product = this._economicSession.Product.FindByNumber(productNumber);
			if (product != null)
			{
				if (!string.IsNullOrEmpty(productName))
				{
					product.Name = productName;
				}
				name.Product = product;
				name.Description = product.Name;
				name.Quantity = new decimal?(quantity);
				name.Unit = product.Unit;
				name.UnitNetPrice = new decimal?(price);
			}
			return name;
		}

		/// <summary>
		/// Makes a IProductData object
		/// </summary>
		/// <param name="productNumber">string: Product number</param>
		/// <param name="productGroupNumber">int: Product group number</param>
		/// <param name="productName">string: Product name</param>
		/// <param name="productDescription">string: product description</param>
		/// <param name="productSalesPrice">decimal: Sales price</param>
		/// <param name="productVolume">decimal: Product volume</param>
		/// <returns>IProductData object</returns>
		public IProductData MakeProduct(string productNumber, int productGroupNumber, string productName, string productDescription, decimal productSalesPrice, decimal productVolume)
		{
			IProductData productDatum = this._economicSession.ProductData.Create(productNumber, this._economicSession.ProductGroup.FindByNumber(productGroupNumber), productName);
			if (productDescription.IndexOf("<") >= 0)
			{
				productDescription = StringHelper.StripHtmlAlternative(productDescription);
			}
			if (productDescription.Length > 255)
			{
				productDescription = string.Concat(productDescription.Substring(0, 250), "...");
			}
			productDatum.Description = productDescription;
			productDatum.Name = productName;
			productDatum.Unit = this._economicSession.Unit.FindByNumber(int.Parse(this._configReader.ProductUnit));
			productDatum.SalesPrice = productSalesPrice;
			productDatum.IsAccessible = true;
			productDatum.Volume = productVolume;
			return productDatum;
		}

		/// <summary>
		/// Check wehter the productgroup exists
		/// </summary>
		/// <param name="productNumber">string: product number</param>
		/// <returns>string: returns the product number if found else empty string</returns>
		public string ProductExistsByNumber(string productNumber)
		{
			string number = "";
			IProduct product = this._economicSession.Product.FindByNumber(productNumber);
			if (product != null)
			{
				number = product.Number;
			}
			return number;
		}

		/// <summary>
		/// Checks wether the Productgroup exists
		/// </summary>
		/// <param name="productGroupName">string: name of the productgroup to search for</param>
		/// <returns>int: E-conomics productgroup number of it is found else 0 (zero)</returns>
		public int ProductgroupExistsByName(string productGroupName)
		{
			int number = 0;
			IProductGroup[] productGroupArray = this._economicSession.ProductGroup.FindByName(productGroupName);
			if (productGroupArray.Length != 0)
			{
				number = productGroupArray[0].Number;
			}
			return number;
		}

		/// <summary>
		/// Saves a debitor in e-conomic
		/// </summary>
		/// <param name="debitorData"><paramref name="debitorData" /> object <see cref="!:debitorData" /></param>
		/// <returns>IDebtor object</returns>
		public IDebtor SaveDebitor(IDebtorData debitorData)
		{
			IDebtor debtor = null;
			debtor = (this.DebitorExists(debitorData.Number) ? this._economicSession.Debtor.UpdateFromData(debitorData) : this._economicSession.Debtor.CreateFromData(debitorData));
			return debtor;
		}

		/// <summary>
		/// Saves a contact person on a debitor and creates the contact person if not found by strContactName.
		/// </summary>
		/// <param name="debitor">IDebtor object to assign the contact person to</param>
		/// <param name="contactName">string: name of the contact person</param>
		/// <param name="phoneNumber">string phone number</param>
		/// <param name="email">string: e-mail that is used to receive order confirmations and invoices.</param>
		/// <returns>IDebtorContact object</returns>
		public IDebtorContact SaveDebitorContact(IDebtor debitor, string contactName, string phoneNumber, string email)
		{
			IDebtorContact debtorContact = null;
			IDebtorContact[] debtorContactArray = this._economicSession.DebtorContact.FindByName(contactName);
			int num = 0;
			while (num < (int)debtorContactArray.Length)
			{
				IDebtorContact debtorContact1 = debtorContactArray[num];
				if (debtorContact1.Debtor.Number != debitor.Number)
				{
					num++;
				}
				else
				{
					debtorContact = debtorContact1;
					break;
				}
			}
			if (debtorContact != null)
			{
				IDebtorData data = this._economicSession.DebtorData.GetData(debitor);
				data.Attention = debtorContact;
				this._economicSession.Debtor.UpdateFromData(data);
			}
			else if (!string.IsNullOrEmpty(contactName))
			{
				IDebtorContactData debtorContactDatum = this._economicSession.DebtorContactData.Create(debitor, contactName);
				debtorContactDatum.Email = email;
				debtorContactDatum.TelephoneNumber = phoneNumber;
				debtorContactDatum.IsToReceiveEmailCopyOfInvoice = true;
				debtorContactDatum.IsToReceiveEmailCopyOfOrder = true;
				debtorContact = this._economicSession.DebtorContact.CreateFromData(debtorContactDatum);
				IDebtorData debtorDatum = this._economicSession.DebtorData.GetData(debitor);
				debtorDatum.Attention = debtorContact;
				this._economicSession.Debtor.UpdateFromData(debtorDatum);
			}
			return debtorContact;
		}

		/// <summary>
		/// Saves the order in E-conomic
		/// </summary>
		/// <param name="orderData">IOrderData object <see cref="M:Dynamicweb.Ecommerce.Economic.EconomicCommunicator.MakeOrder(System.String,System.String,System.String,System.String,System.String,System.String,System.String,System.String,System.DateTime,System.DateTime,System.DateTime,System.String,System.String)" /></param>
		/// <returns>IOrder object</returns>
		public IOrder SaveOrder(IOrderData orderData)
		{
			return this._economicSession.Order.CreateFromData(orderData);
		}

		/// <summary>
		/// Saves a Orderline
		/// </summary>
		/// <param name="orderLineData">IOrderLineData object</param>
		/// <returns>IOrderLine object</returns>
		public IOrderLine SaveOrderLine(IOrderLineData orderLineData)
		{
			return this._economicSession.OrderLine.CreateFromData(orderLineData);
		}

		/// <summary>
		/// Saves an array of Orderlines
		/// </summary>
		/// <param name="orderLineData">IOrderLineData object array</param>
		/// <returns>IOrderLine object array</returns>
		public IOrderLine[] SaveOrderLines(IOrderLineData[] orderLineData)
		{
			return this._economicSession.OrderLine.CreateFromDataArray(orderLineData);
		}

		/// <summary>
		/// Saves a product in e-conomic
		/// </summary>
		/// <param name="productData">IProductData object</param>
		/// <returns>IProduct object</returns>
		public IProduct SaveProduct(IProductData productData)
		{
			return this._economicSession.Product.CreateFromData(productData);
		}

		/// <summary>
		/// Saves a product group in e-conomic
		/// </summary>
		/// <param name="groupNumber">int: product group number</param>
		/// <param name="groupName">string: product group name</param>
		/// <param name="accountNumber">int: account number</param>
		/// <returns>IProductGroup object</returns>
		public IProductGroup SaveProductGroup(int groupNumber, string groupName, int accountNumber)
		{
			IAccount account = this._economicSession.Account.FindByNumber(accountNumber);
			IProductGroupData productGroupDatum = this._economicSession.ProductGroupData.Create(groupNumber, groupName, account);
			return this._economicSession.ProductGroup.CreateFromData(productGroupDatum);
		}
	}
}