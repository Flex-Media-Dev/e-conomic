using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using Dynamicweb.Extensibility;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Notifications;
using Economic.Api;
using Economic.Api.Data;
using Economic.Api.Exceptions;

namespace Dynamicweb.eCommerce.Economic.Integration
{

    [Subscribe(Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsComplete)]
    public class OrderStepsCompleted : NotificationSubscriber
    {
        public override void OnNotify(string notification, NotificationArgs args)
        {
            var orderArgs = (Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsCompleteArgs)args;
            var handler = new NotificationHandler();
            handler.HandleOrder(orderArgs.Order);
        }
    }

    [Subscribe(Notifications.eCommerce.Order.Steps.Completed)]
    public class OrderCompleteNotification : NotificationSubscriber
    {
        public override void OnNotify(string notification, object[] args)
        {
            var handler = new NotificationHandler();
            handler.HandleOrder((Orders.Order)args[0]);
        }
    }

    internal class NotificationHandler
    {
        private IProduct _feeProduct = null;
        private IProduct FeeProduct
        {
            get
            {
                if (_feeProduct == null)
                {
                    string productNumber = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/FeeProductNumber"));
                    if (!string.IsNullOrEmpty(productNumber))
                        _feeProduct = EconomicCommunicator.GetProductByNumber(productNumber);
                }
                return _feeProduct;
            }
        }

        private IProduct _discountProduct = null;
        private IProduct DiscountProduct
        {
            get
            {
                if (_discountProduct == null)
                {
                    string productNumber = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DiscountProductNumber"));
                    if (!string.IsNullOrEmpty(productNumber))
                        _discountProduct = EconomicCommunicator.GetProductByNumber(productNumber);
                }
                return _discountProduct;
            }
        }

        private EconomicCommunicator _economicCommunicator = null;
        private EconomicCommunicator EconomicCommunicator
        {
            get
            {
                if (_economicCommunicator == null)
                {
                    _economicCommunicator = new EconomicCommunicator();
                }
                return _economicCommunicator;
            }
        }

        private Ecommerce.Orders.Order _dynamicwebOrder = null;
        private Ecommerce.Orders.Order DynamicwebOrder
        {
            get { return _dynamicwebOrder; }
            set { _dynamicwebOrder = value; }
        }

        private List<OrderCompleteNotificationExtender> AddinInstances
        {
            get
            {
                ArrayList addins = AddInManager.GetAddInClasses(typeof(OrderCompleteNotificationExtender));
                List<OrderCompleteNotificationExtender> extenders = new List<OrderCompleteNotificationExtender>();
                foreach (object addin in addins)
                {
                    Type type = AddInManager.GetAddInTypeByName(addin.ToString());
                    if (type != null)
                    {
                        extenders.Add(Activator.CreateInstance(type) as OrderCompleteNotificationExtender);
                    }
                }
                return extenders;
            }
        }

        private string _orderNumberPrefix = null;
        private string OrderNumberPrefix
        {
            get
            {
                if (_orderNumberPrefix == null)
                {
                    _orderNumberPrefix = Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/OrderNumberPrefix");
                    if (!string.IsNullOrEmpty(_orderNumberPrefix) && !_orderNumberPrefix.Contains(":"))
                        _orderNumberPrefix = string.Format("{0}: ", _orderNumberPrefix);                    
                }
                return _orderNumberPrefix;
            }
        }

        /// <summary>
        /// OrderCompleteNotification : Exports the order to economic when the order is complete.
        /// </summary>
        /// <param name="dwOrder">The dynamicweb order.</param>
        public void HandleOrder(Ecommerce.Orders.Order dwOrder)
        {
            // Module check
            if (!Security.UserManagement.License.IsModuleInstalled("eCom_economic", true)) { return; }

            // Setting check
            if (Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/ExportOrderOnComplete") != "True") { return; }            

            try
            {

                EconomicCommunicator ec = null;
                try
                {
                    ec = EconomicCommunicator;
                }
                catch (Exception)
                {
                    //Log( "EOI: Can't connect to E-conomic API", "Connection to E-conomic API failed" );
                }


                if (ec != null)
                {
                    //DW ORDER
                    //Orders.Order dwOrder = (Orders.Order)args[0];
                    DynamicwebOrder = dwOrder;
                    OrderBillingHandler(dwOrder);

                    //Modify Dynamicweb order information
                    foreach (OrderCompleteNotificationExtender addin in AddinInstances)
                    {
                        try
                        {
                            dwOrder = addin.DynamicwebOrder(dwOrder);
                        }
                        catch (Exception)
                        {
                            //Log("EOI Addin: Get orders", "Economic Order Integration Addin method to get orders failed");
                        }
                    }


                    //Creating debitor
                    IDebtorData debitorData = ec.MakeDebitor(dwOrder.CustomerNumber, dwOrder.CustomerCompany, dwOrder.CustomerAddress + "\n" + dwOrder.CustomerAddress2, dwOrder.CustomerCity, dwOrder.CustomerCountry, dwOrder.CustomerEan, dwOrder.CustomerEmail, dwOrder.CustomerZip, dwOrder.CustomerPhone);

                    //Modify Debitor information
                    foreach (OrderCompleteNotificationExtender addin in AddinInstances)
                    {
                        debitorData = addin.EconomicDebitor(dwOrder, debitorData);
                    }

                    //Getting the debitor
                    IDebtor debitor = null;
                    try
                    {
                        debitor = ec.SaveDebitor(debitorData);
                    }
                    catch (IntegrityException)
                    {
                        //Log("EOI " + e.Code + ": " + e.Diagnostic, "SaveDebitor failed");
                    }
                    catch (AuthorizationException)
                    {
                        //Log("EOI " + e.Code + ": " + e.Diagnostic, "SaveDebitor failed");
                    }

                    if (debitor != null)
                    {
                        ec.SaveDebitorContact(debitor, dwOrder.CustomerName, dwOrder.CustomerPhone, dwOrder.CustomerEmail);
                        dwOrder.CustomerNumber = debitor.Number;

                        string deliveryAddress = GetEconomicOrderDeliveryAddress(dwOrder);
                        IOrderData orderData = ec.MakeOrder(dwOrder.CustomerNumber, OrderNumberPrefix + dwOrder.Id, deliveryAddress, dwOrder.DeliveryCity, dwOrder.DeliveryCountry, dwOrder.DeliveryZip, "", dwOrder.Comment, DateTime.Now, dwOrder.Date, DateTime.Now, "", dwOrder.CurrencyCode);

                        //Modify economic order information
                        foreach (OrderCompleteNotificationExtender addin in AddinInstances)
                        {
                            orderData = addin.EconomicOrder(dwOrder, orderData);
                        }

                        // init and save the order
                        IOrder order = null;
                        try
                        {
                            order = ec.SaveOrder(orderData);
                        }
                        catch (IntegrityException)
                        {
                            //eLog.Log("EOI " + e.Code + ": " + e.Diagnostic, "SaveOrder failed");
                        }

                        if (order != null)
                        {

                            int orderId = order.Number;

                            // Adding ordinary orderline
                            foreach (Ecommerce.Orders.OrderLine ol in dwOrder.OrderLines)
                            {
                                if (!ol.HasType(Ecommerce.Orders.OrderLineType.Discount) && !ol.HasType(Ecommerce.Orders.OrderLineType.ProductDiscount))
                                {
                                    IOrderLineData orderlineData = ec.MakeOrderLine(orderId, ol.ProductNumber, decimal.Parse(ol.Quantity.ToString()), decimal.Parse(ol.UnitPrice.PriceWithoutVAT.ToString()));

                                    // don't add product, if not found in economic
                                    if (orderlineData.Product != null)
                                    {
                                        //Modify economic orderline information
                                        foreach (OrderCompleteNotificationExtender addin in AddinInstances)
                                        {
                                            orderlineData = addin.EconomicOrderline(ol, orderlineData);
                                        }

                                        try
                                        {
                                            ec.SaveOrderLine(orderlineData);
                                        }
                                        catch (IntegrityException e)
                                        {
                                            OrderErrorHandler("E-conomic IntegrityException - SaveOrderLine failed : " + e.Code + " - " + e.Message);
                                            ec.DeleteOrderFromEconomic(orderId);
                                            order = null;
                                            break;
                                        }
                                        catch (AuthorizationException e)
                                        {
                                            OrderErrorHandler("E-conomic AuthorizationException - SaveOrderLine failed : " + e.Code + " - " + e.Message);
                                            ec.DeleteOrderFromEconomic(orderId);
                                            order = null;
                                            break;
                                        }
                                    }

                                }

                            }

                            if (order != null)
                            {
                                // Adding discount orderline
                                if (DiscountProduct != null)
                                {
                                    foreach (Ecommerce.Orders.OrderLine ol in dwOrder.OrderLines)
                                    {
                                        if (ol.HasType(Ecommerce.Orders.OrderLineType.Discount) || ol.HasType(Ecommerce.Orders.OrderLineType.ProductDiscount))
                                        {
                                            IOrderLineData discountOrderLine = ec.MakeOrderLine(orderId, DiscountProduct.Number, decimal.Parse(ol.Quantity.ToString()), decimal.Parse(ol.Price.PriceWithoutVAT.ToString()), ol.ProductName);
                                            //Modify economic discount orderline information                                            
                                            foreach (OrderCompleteNotificationExtender addin in AddinInstances)                                            
                                                discountOrderLine = addin.EconomicDiscountOrderline(ol, discountOrderLine);                                            
											if (discountOrderLine != null && discountOrderLine.UnitNetPrice != 0)//If Discount is 0 then don't send it to E-conomic
                                                SaveOrderLine(ec, discountOrderLine);
                                        }
                                    }
                                }

                                if (FeeProduct != null)
                                {
                                    // Add Payment Fee Orderline
                                    if (!string.IsNullOrEmpty(dwOrder.PaymentMethodId))
                                    {
                                        IOrderLineData paymentFeeOrderline = ec.MakeOrderLine(orderId, FeeProduct.Number, 1, decimal.Parse(dwOrder.PaymentFee.PriceWithoutVAT.ToString()), dwOrder.PaymentMethodDescription);
                                        //Modify economic payment fee orderline information                                        
                                        foreach (OrderCompleteNotificationExtender addin in AddinInstances)                                        
                                            paymentFeeOrderline = addin.EconomicPaymentFeeOrderline(paymentFeeOrderline);                                        
                                        if (paymentFeeOrderline != null && paymentFeeOrderline.UnitNetPrice != 0)//If Payment fee is 0 then don't send it to E-conomic
                                        	SaveOrderLine(ec, paymentFeeOrderline);
                                    }

                                    // Add Shipping Fee Orderline
                                    if (!string.IsNullOrEmpty(dwOrder.ShippingMethodId))
                                    {                                                                                
                                        IOrderLineData shippingFeeOrderline = ec.MakeOrderLine(orderId, FeeProduct.Number, 1, decimal.Parse(dwOrder.ShippingFee.PriceWithoutVAT.ToString()), dwOrder.ShippingMethodDescription);
                                        //Modify economic shipping fee orderline information                                        
                                        foreach (OrderCompleteNotificationExtender addin in AddinInstances)                                        
                                            shippingFeeOrderline = addin.EconomicShippingFeeOrderline(shippingFeeOrderline);                                        
                                        if (shippingFeeOrderline != null && shippingFeeOrderline.UnitNetPrice != 0)//If Shipping Fee is 0 then don't send it to E-conomic
                                        	SaveOrderLine(ec, shippingFeeOrderline);
                                    }
                                }
                            }
                            else
                            {

                            }

                        }
                    }

                }
            }
            catch (IntegrityException e)
            {
                OrderErrorHandler("E-conomic IntegrityException - Integrity action failed : " + e.Code + " - " + e.Message);
            }
            catch (SchemaException e)
            {
                OrderErrorHandler("E-conomic SchemaException - Schema action failed : " + e.Code + " - " + e.Message);
            }
            catch (ValidationException e)
            {
                OrderErrorHandler("E-conomic ValidationException - Validation action failed : " + e.Code + " - " + e.Message);
            }
            catch (ServerException e)
            {
                OrderErrorHandler("E-conomic ServerException - Server action failed : " + e.Code + " - " + e.Message);
            }
            catch (AuthorizationException e)
            {
                OrderErrorHandler("E-conomic AuthorizationException - Authorization action failed : " + e.Code + " - " + e.Message);
            }
            catch (UserException e)
            {
                OrderErrorHandler("E-conomic UserException - User action failed : " + e.Code + " - " + e.Message);
            }
            catch (Exception e)
            {
                OrderErrorHandler("General Exception" + e.ToString());
            }
        }
        
        private void SaveOrderLine(EconomicCommunicator ec, IOrderLineData orderlineData)
        {     
            try
            {
                ec.SaveOrderLine(orderlineData);
            }
            catch (IntegrityException e)
            {
                OrderErrorHandler("E-conomic IntegrityException - SaveOrderLine failed : " + e.Code + " - " + e.Message);                
            }
            catch (AuthorizationException e)
            {
                OrderErrorHandler("E-conomic AuthorizationException - SaveOrderLine failed : " + e.Code + " - " + e.Message);                
            }         
        }

        private string GetEconomicOrderDeliveryAddress(Ecommerce.Orders.Order dwOrder)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(dwOrder.DeliveryName))
                result = dwOrder.DeliveryName + "\n";
            if (!string.IsNullOrEmpty(dwOrder.DeliveryCompany))
                result += dwOrder.DeliveryCompany + "\n";
            if (!string.IsNullOrEmpty(dwOrder.DeliveryAddress))
                result += dwOrder.DeliveryAddress + "\n";
            if (!string.IsNullOrEmpty(dwOrder.DeliveryAddress2))
                result += dwOrder.DeliveryAddress2;
            return result;
        }

        private void OrderErrorHandler(string exception)
        {
            Ecommerce.Orders.Order order = DynamicwebOrder;
            if (order != null)
            {
                string path = HttpContext.Current.Server.MapPath("/Files/" + "/Integration/eCommerce/economic/OrderErrors"); // + Dynamicweb.Content.Management.Installation.FilesFolderName +

                try
                {

                    //CREATE FOLDER
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    //XML Settings
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.NewLineHandling = NewLineHandling.Entitize;
                    settings.NewLineOnAttributes = true;
                    settings.Encoding = Encoding.UTF8;

                    var fs = new System.IO.FileStream(path + "/" + order.Id + ".xml", System.IO.FileMode.Create);

                    //WRITE ORDER XML
                    XmlWriter w = XmlWriter.Create(fs, settings);
                    w.WriteStartDocument();
                    w.WriteStartElement("Order");
                    w.WriteAttributeString("ID", order.Id);
                    w.WriteAttributeString("Date", order.Date.ToString());
                    w.WriteAttributeString("CurrencyCode", order.CurrencyCode);
                    w.WriteAttributeString("CustomerNumber", order.CustomerNumber);
                    w.WriteAttributeString("DeliveryAddress", order.DeliveryAddress);
                    w.WriteAttributeString("DeliveryAddress2", order.DeliveryAddress2);
                    w.WriteAttributeString("DeliveryCity", order.DeliveryCity);
                    w.WriteAttributeString("DeliveryCountry", order.DeliveryCountry);
                    w.WriteAttributeString("DeliveryZip", order.DeliveryZip);

                    //ORDER LINES
                    foreach (Ecommerce.Orders.OrderLine ol in order.OrderLines)
                    {
                        w.WriteStartElement("Orderline");
                        w.WriteAttributeString("ProductNumber", ol.ProductNumber);
                        w.WriteAttributeString("VariantText", ol.ProductVariantText);
                        w.WriteAttributeString("Quantity", ol.Quantity.ToString());
                        w.WriteAttributeString("UnitPriceWithVAT", ol.UnitPrice.PriceWithVATFormattedNoSymbol);
                        w.WriteAttributeString("UnitPriceWithoutVAT", ol.UnitPrice.PriceWithoutVATFormattedNoSymbol);
                        w.WriteEndElement(); //Orderline
                    }

                    if (!string.IsNullOrEmpty(exception))
                    {
                        //EXCEPTION
                        w.WriteStartElement("Exception");
                        w.WriteAttributeString("msg", exception);
                        w.WriteEndElement(); //Exception
                    }

                    w.WriteEndElement();//Order
                    w.Close();

                    fs.Dispose();
                    fs.Close();
                }
                catch (Exception)
                {
                    throw new Exception("Nothing ?");
                    //Nothing ?
                }
            }
        }

        private static void OrderBillingHandler(Ecommerce.Orders.Order order)
        {
            if (string.IsNullOrEmpty(order.CustomerNumber))
                order.CustomerNumber = order.CustomerPhone;

            // If delivery info is empty it's stated to be the same as the billing info
            if (string.IsNullOrEmpty(order.DeliveryAddress))
                order.DeliveryAddress = order.CustomerAddress;
            if (string.IsNullOrEmpty(order.DeliveryAddress2))
                order.DeliveryAddress2 = order.CustomerAddress2;
            if (string.IsNullOrEmpty(order.DeliveryCity))
                order.DeliveryCity = order.CustomerCity;
            if (string.IsNullOrEmpty(order.DeliveryCountry))
                order.DeliveryCountry = order.CustomerCountry;
            if (string.IsNullOrEmpty(order.DeliveryEmail))
                order.DeliveryEmail = order.CustomerEmail;
            if (string.IsNullOrEmpty(order.DeliveryFax))
                order.DeliveryFax = order.CustomerFax;
            if (string.IsNullOrEmpty(order.DeliveryName))
                order.DeliveryName = order.CustomerName;
            if (string.IsNullOrEmpty(order.DeliveryPhone))
                order.DeliveryPhone = order.CustomerPhone;
            if (string.IsNullOrEmpty(order.DeliveryZip))
                order.DeliveryZip = order.CustomerZip;
            if (string.IsNullOrEmpty(order.DeliveryTitle))
                order.DeliveryTitle = order.CustomerTitle;
            if (string.IsNullOrEmpty(order.DeliveryFirstName))
                order.DeliveryFirstName = order.CustomerFirstName;
            if (string.IsNullOrEmpty(order.DeliveryMiddleName))
                order.DeliveryMiddleName = order.CustomerMiddleName;
            if (string.IsNullOrEmpty(order.DeliverySurname ))
                order.DeliverySurname = order.CustomerSurname;
            if (string.IsNullOrEmpty(order.DeliveryHouseNumber ))
                order.DeliveryHouseNumber = order.CustomerHouseNumber;
            //-- If delivery info is empty it's stated to be the same as the billing info

        }

    }
}
