using System;
using System.Collections.Generic;
using System.ComponentModel;

using Economic.Api;
using Economic.Api.Data;

namespace Dynamicweb.eCommerce.Economic
{
    /// <summary>
    /// Wrapper class for Economic API
    /// </summary>
    [DataObject(true)]
    public class EconomicCommunicator : IDisposable
    {

        private EconomicSession _economicSession;
        private ConfigReader _configReader;

        /// <summary>
        /// object: ConfigReader where all settings are loaded and stored
        /// </summary>
        public ConfigReader ConfigReader { get { return _configReader; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public EconomicCommunicator() : this(true) {} 

        public EconomicCommunicator(bool connect)
        {
            _configReader = new ConfigReader();
            _economicSession = new EconomicSession();
            
            if (connect)
            {
                this.Connect();
            }
        }

        /// <summary>
        /// Disposes and closes the connection of the E-conomic session
        /// </summary>
        public void Dispose()
        {
            this.DisConnect();
        }

        public string Connect()
        {
            return _economicSession.Connect(_configReader.AgreementNumber, _configReader.Username, _configReader.Password);
        }
        public void DisConnect()
        {
            _economicSession.Disconnect();
        }

        /// <summary>
        /// E-conomic API Information
        /// </summary>
        /// <returns>string: E-conomic API info</returns>
        public string GetApiInfo()
        {
            return _economicSession.GetApiInformation();
        }

        /// <summary>
        /// Gets a debitor by number
        /// </summary>
        /// <param name="debtorNumber">string: number of the debitor in your e-conomic account</param>
        /// <returns>IDeptor object: a special type from E-conomic API</returns>
        public IDebtor GetDebitor(string debtorNumber)
        {
            int testDebtorNumber = 0;
            if (int.TryParse(debtorNumber, out testDebtorNumber))
            {
                return _economicSession.Debtor.FindByNumber(debtorNumber);
            }
            return (null);
        }

        /// <summary>
        /// Checks wether a debitor exists in your E-conomic account
        /// </summary>
        /// <param name="debtorNumber">string: number of the debitor in your e-conomic account</param>
        /// <returns>bool: true of the debitor exists else false</returns>
        public bool DebitorExists(string debtorNumber)
        {
            try
            {
                bool rtnValue = false;
                IDebtor checkDebtor = _economicSession.Debtor.FindByNumber(debtorNumber);
                if ((checkDebtor != null))
                {
                    rtnValue = true;
                }
                return (rtnValue);
            }
            catch (Exception)
            {
                return false;
            }
                    
        }

        /// <summary>
        /// Checks wether the Productgroup exists
        /// </summary>
        /// <param name="productGroupName">string: name of the productgroup to search for</param>
        /// <returns>int: E-conomics productgroup number of it is found else 0 (zero)</returns>
        public int ProductgroupExistsByName(string productGroupName)
        {
            int rtnValue = 0;
            IProductGroup[] checkProductGroups = _economicSession.ProductGroup.FindByName(productGroupName);
            if ((checkProductGroups.Length > 0))
            {
                rtnValue = checkProductGroups[0].Number;
            }
            return (rtnValue);
        }

        /// <summary>
        /// Check wehter the productgroup exists
        /// </summary>
        /// <param name="productNumber">string: product number</param>
        /// <returns>string: returns the product number if found else empty string</returns>
        public string ProductExistsByNumber(string productNumber)
        {
            string rtnValue = "";
            IProduct checkProduct = _economicSession.Product.FindByNumber(productNumber);
            if ((checkProduct != null))
            {
                rtnValue = checkProduct.Number;
            }
            return (rtnValue);
        }

        /// <summary>
        /// Gets a product group
        /// </summary>
        /// <param name="groupId">int: Group ID to search for</param>
        /// <returns>IProductGroup object</returns>
        public IProductGroup GetProductGroupById(int groupId)
        {
            IProductGroup group = _economicSession.ProductGroup.FindByNumber(groupId);
            return (group);
        }

        /// <summary>
        /// Gets a product groups
        /// </summary>
        /// <param name="groupIds">int: Groups IDs to search for</param>
        /// <returns>IProductGroup array</returns>
        public IProductGroup[] GetProductGroupById(int[] groupIds)
        {
            return _economicSession.ProductGroup.FindByNumber(groupIds);            
        }

        public IEnumerable<Result> GetProducts(string groupName)
        {
            List<Result> result = new List<Result>();

            int groupId = GetProductGroupIdByName(groupName);
            IProductGroup group = GetProductGroupById(groupId);

            IProduct[] products = group.GetProducts();

            foreach (var item in products)
            {
                result.Add(new Result() { Text = item.Name, Selected = false, Value = item.Number });
            }

            return result;
        }

        /// <summary>
        /// Gets all debitor groups
        /// </summary>
        /// <returns>IDeptorGroup array: a special type from the E-conomic API</returns>
        public IDebtorGroup[] GetAllDebitorGroups()
        {
            IDebtorGroup[] groups = _economicSession.DebtorGroup.GetAll();
            return (groups);
        }

        public IEnumerable<Result> GetDebitorGroups()
        {
            List<Result> result = new List<Result>();

            IDebtorGroup[] collection = _economicSession.DebtorGroup.GetAll();
            foreach (var item in collection)
            {
                result.Add(new Result() {Text = item.Name, Selected = false, Value = item.Number.ToString()});
            }

            return result;
        }

        /// <summary>
        /// Gets all VAT groups
        /// </summary>
        /// <returns>IVapAccount array: a special type from the E-conomic API</returns>
        public IVatAccount[] GetAllVatGroups()
        {
            IVatAccount[] groups = _economicSession.VatAccount.GetAll();
            return (groups);
        }

        /// <summary>
        /// Gets all Currencies
        /// </summary>
        /// <returns>ICurrency array: a special type from the E-conomic API</returns>
        public ICurrency[] GetAllCurrencies()
        {
            ICurrency[] currencies = _economicSession.Currency.GetAll();
            return (currencies);
        }

        /// <summary>
        /// Gets all Terms of payment see also <seealso cref="GetAllTermsOfPayments"/>
        /// </summary>
        /// <returns><see cref="ITermOfPayment"/> array: a special type from the E-conomic API</returns>
        public ITermOfPayment[] GetAllPaymenttypes()
        {
            ITermOfPayment[] paymenttypes = _economicSession.TermOfPayment.GetAll();
            return (paymenttypes);
        }

        public IEnumerable<Result> GetPaymenttypes()
        {
            List<Result> result = new List<Result>();

            ITermOfPayment[] collection = _economicSession.TermOfPayment.GetAll();
            foreach (var item in collection)
            {
               result.Add(new Result() {Text = item.Name, Selected = false, Value = item.Name});
            }

            return result;
        }

        /// <summary>
        /// Gets all debitor layouts
        /// </summary>
        /// <returns><see cref="ITemplateCollection"/> array: a special type from the E-conomic API</returns>
        public ITemplateCollection[] GetAllDebitorLayouts()
        {
            ITemplateCollection[] layouts = _economicSession.TemplateCollection.GetAll();
            return (layouts);
        }

        public IEnumerable<Result> GetDebitorLayouts()
        {
            List<Result> result = new List<Result>();

            ITemplateCollection[] collection = _economicSession.TemplateCollection.GetAll();
            foreach (var item in collection)
            {
                result.Add(new Result() { Text = item.Name, Selected = false, Value = item.Name });
            }

            return result;
        }

        /// <summary>
        /// Gets all Terms of payment see also <seealso cref="GetAllPaymenttypes"/>
        /// </summary>
        /// <returns><see cref="ITermOfPayment"/> array: a special type from the E-conomic API</returns>
        public ITermOfPayment[] GetAllTermsOfPayments()
        {
            ITermOfPayment[] terms = _economicSession.TermOfPayment.GetAll();
            return (terms);
        }

        /// <summary>
        /// Gets alle productgroups
        /// </summary>
        /// <returns><see cref="IProductGroup"/> array: a special type from the E-conomic API</returns>
        public IProductGroup[] GetAllProductGroups()
        {
            IProductGroup[] groups = _economicSession.ProductGroup.GetAll();
            return (groups);
        }

        public IEnumerable<Result> GetProductGroups()
        {
            List<Result> result = new List<Result>();

            IProductGroup[] collection = _economicSession.ProductGroup.GetAll();
            foreach (var item in collection)
            {
                result.Add(new Result() { Text = item.Name, Selected = false, Value = item.Number.ToString() });
            }

            return result;
        }        

        /// <summary>
        /// Get all Units
        /// </summary>
        /// <returns><see cref="IUnit"/> array: a special type from the E-conomic API</returns>
        public IUnit[] GetAllUnits()
        {
            IUnit[] units = _economicSession.Unit.GetAll();
            return (units);
        }

        /// <summary>
        /// Gets all accounts
        /// </summary>
        /// <returns><see cref="IAccount"/> array: a special type from the E-conomic API</returns>
        public IAccount[] GetAllAccounts()
        {
            IAccount[] accounts = _economicSession.Account.GetAll();
            return (accounts);
        }

        /// <summary>
        /// Gets all invoices
        /// </summary>
        /// <returns><see cref="IInvoice"/> array: a special type from the E-conomic API</returns>
        public IInvoice[] GetAllInvoices()
        {
            IInvoice[] invoices = _economicSession.Invoice.GetAll();
            return (invoices);
        }

        /// <summary>
        /// Gets all invoices made in the provided date range
        /// </summary>
        /// <param name="from">DateTime: from date</param>
        /// <param name="to">DateTime: to date</param>
        /// <returns><see cref="IInvoice"/> array of all invoices within the provided date range</returns>
        public IInvoice[] GetInvoicesByDate(DateTime from, DateTime to)
        {
            IInvoice[] invoices = _economicSession.Invoice.FindByDate(from, to);
            return (invoices);
        }

        /// <summary>
        /// Gets all invoices made on the specified date
        /// </summary>
        /// <param name="date">DateTime: the date</param>
        /// <returns><see cref="IInvoice"/> array of all invoices made on the provided date</returns>
        public IInvoice[] GetInvoicesByDate(DateTime date)
        {
            IInvoice[] invoices = _economicSession.Invoice.FindByDate(date, date.AddDays(1));
            return (invoices);
        }

        /// <summary>
        /// Gets the PDF document of a specific invoice
        /// </summary>
        /// <param name="invoiceNumber">int: InvoiceNumber</param>
        /// <returns>byte[] array of the PDF file</returns>
        public byte[] GetInvoicePdf(int invoiceNumber)
        {
            IInvoice invoice = _economicSession.Invoice.FindByNumber(invoiceNumber);
            return (invoice.GetPdf());
        }

        /// <summary>
        /// Gets the Invoice object 
        /// </summary>
        /// <param name="invoiceNumber">int: InvoiceNumber</param>
        /// <returns><see cref="IInvoice"/> object of the Invoice</returns>
        public IInvoice GetInvoice(int invoiceNumber)
        {
            return _economicSession.Invoice.FindByNumber(invoiceNumber);
        }

        /// <summary>
        /// Gets a productgroup id by providing the group name
        /// </summary>
        /// <param name="groupName">string: name of the group</param>
        /// <returns>int: productgroup id</returns>
        public int GetProductGroupIdByName(string groupName)
        {
            int rtnValue = 0;
            IProductGroup[] groups = _economicSession.ProductGroup.FindByName(groupName);
            if ((groups.Length > 0))
            {
                rtnValue = groups[0].Number;
            }
            return (rtnValue);
        }

        /// <summary>
        /// Gets all products from e-conomic <remarks>Watch out for performance issue, there could be many products in E-conomic to fetch</remarks>
        /// </summary>
        /// <returns>object: array of IProduct</returns>
        public IProduct[] GetAllProducts()
        {
            IProduct[] products = _economicSession.Product.GetAll();
            return (products);
        }

        /// <summary>
        /// Gets a single product by simple search by number
        /// </summary>
        /// <param name="productNumber">string: the product number to search for</param>
        /// <returns>object: IProduct if found else null</returns>
        public IProduct GetProductByNumber(string productNumber)
        {
            IProduct product = _economicSession.Product.FindByNumber(productNumber);
            return (product);
        }                

        /// <summary>
        /// Gets all products found
        /// </summary>
        /// <param name="productNumbers">array of strings as productnumbers</param>
        /// <returns>IProduct array</returns>
        /// <remarks>Be careful about matching the length of input array with output array because you can retrieve null objects</remarks>
        public IProduct[] GetProductsByNumber(string[] productNumbers)
        {
            IProduct[] products = _economicSession.Product.FindByNumber(productNumbers);
            return (products);
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
            IOrder order = _economicSession.Order.FindByNumber(orderNumber);
            if ((order != null))
            {
                order.Delete();
            }
        }

        /// <summary>
        /// Saves a debitor in e-conomic
        /// </summary>
        /// <param name="debitorData"><paramref name="debitorData"/> object <see cref="debitorData"/></param>
        /// <returns>IDebtor object</returns>
        public IDebtor SaveDebitor(IDebtorData debitorData)
        {
            IDebtor debitor = null;
            if (!DebitorExists(debitorData.Number))
            {
                debitor = _economicSession.Debtor.CreateFromData(debitorData);
            }
            else
            {
                debitor = _economicSession.Debtor.UpdateFromData(debitorData);
            }
            return (debitor);
        }

        /// <summary>
        /// Makes a IDebtorData object with most default properties set
        /// </summary>
        /// <param name="debitorNummer">string: debitor number</param>
        /// <param name="debitorName">string: debitor name</param>
        /// <param name="debitorAddress">string: debitor address</param>
        /// <param name="debitorCity">string: debitor city</param>
        /// <param name="debitorCountry">string debitor country</param>
        /// <param name="debitorEan">string: EAN number</param>
        /// <param name="debitorEmail">string: debitor e-mail</param>
        /// <param name="debitorZipcode">string: debitor zipcode</param>
        /// <param name="debitorTelefonFax">string: debitor phone and fax number</param>
        /// <returns>IDebtorData object that can be used with <paramref name="SaveDebitor"/></returns>
        public IDebtorData MakeDebitor(string debitorNummer, string debitorName, string debitorAddress, string debitorCity, string debitorCountry, string debitorEan, string debitorEmail, string debitorZipcode, string debitorTelefonFax)
        {
            ConfigReader config = new ConfigReader();

            IDebtorGroup debitorGroup = _economicSession.DebtorGroup.FindByNumber(int.Parse(config.DebitorGroup));
            VatZone vatZone = VatZone.HomeCountry;

            switch (config.DebitorVatGroup.Trim())
            {
                case "I25":
                    vatZone = VatZone.HomeCountry;
                    break;
                case "REP":
                    vatZone = VatZone.EU;
                    break;
                case "U25":
                    vatZone = VatZone.Abroad;
                    break;
            }

            IDebtorData debitor = null;

            if (!DebitorExists(debitorNummer))
            {
                debitor = _economicSession.DebtorData.Create(debitorNummer, debitorGroup, debitorName, vatZone);
                debitor.IsAccessible = true;
            }
            else
            {
                debitor = _economicSession.DebtorData.GetData(_economicSession.Debtor.FindByNumber(debitorNummer));
            }

            debitor.DebtorGroup = debitorGroup;
            //if (!string.IsNullOrEmpty(debitorName))
            //{
            //    debitor.Name = debitorName;
            //}
            //else
            //{
            //    debitor.Name = "N/A";    //  N/A
            //}
            debitor.Name = debitorName;

            debitor.Address = debitorAddress;
            debitor.City = debitorCity;
            debitor.Country = debitorCountry;
            debitor.Currency = _economicSession.Currency.FindByCode(config.DebitorCurrency);
            debitor.DebtorGroup = debitorGroup;
            if (!string.IsNullOrEmpty(debitorEan))
            {
                debitor.Ean = debitorEan;
            }

            debitor.Email = debitorEmail;
            debitor.Layout = _economicSession.TemplateCollection.FindByName(config.DebitorLayout)[0];
            debitor.PostalCode = debitorZipcode;
            debitor.TelephoneAndFaxNumber = debitorTelefonFax;
            debitor.TermOfPayment = _economicSession.TermOfPayment.FindByName(config.DebitorTermsOfPayment)[0];
            debitor.VatZone = vatZone;

            return (debitor);
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
            IDebtorContact[] attPersons = _economicSession.DebtorContact.FindByName(contactName);
            IDebtorContact att = null;
            foreach (IDebtorContact attPerson in attPersons)
            {
                if ((attPerson.Debtor.Number == debitor.Number))
                {
                    att = attPerson;
                    break;
                }
            }

            if ((att == null))
            {
                if (!string.IsNullOrEmpty(contactName))
                {
                    IDebtorContactData contactData = _economicSession.DebtorContactData.Create(debitor, contactName);
                    contactData.Email = email;
                    contactData.TelephoneNumber = phoneNumber;
                    contactData.IsToReceiveEmailCopyOfInvoice = true;
                    contactData.IsToReceiveEmailCopyOfOrder = true;

                    att = _economicSession.DebtorContact.CreateFromData(contactData);
                    IDebtorData debitorAtt = _economicSession.DebtorData.GetData(debitor);
                    debitorAtt.Attention = att;
                    _economicSession.Debtor.UpdateFromData(debitorAtt);
                }
            }
            else
            {
                IDebtorData debitorAtt = _economicSession.DebtorData.GetData(debitor);
                debitorAtt.Attention = att;
                _economicSession.Debtor.UpdateFromData(debitorAtt);
            }
            return (att);
        }

        /// <summary>
        /// Saves the order in E-conomic
        /// </summary>
        /// <param name="orderData">IOrderData object <see cref="MakeOrder"/></param>
        /// <returns>IOrder object</returns>
        public IOrder SaveOrder(IOrderData orderData)
        {
            IOrder order = _economicSession.Order.CreateFromData(orderData);
            return (order);
        }

        /// <summary>
        /// Makes an order for <see cref="SaveOrder"/>
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
            ConfigReader config = new ConfigReader();

            // Debitor settings 
            IDebtorGroup debitorGroup = _economicSession.DebtorGroup.FindByNumber(int.Parse(config.DebitorGroup));
            ICurrency debitorCurrency = _economicSession.Currency.FindByCode(debitorCurrencyCode);
            ITemplateCollection debitorLayout = _economicSession.TemplateCollection.FindByName(config.DebitorLayout)[0];

            ITermOfPayment debitorTermOfPayment = _economicSession.TermOfPayment.FindByName(config.DebitorTermsOfPayment)[0];

            // Order settings 
            ICurrency orderCurrency = debitorCurrency;
            ITemplateCollection orderLayout = debitorLayout;

            ITermOfPayment orderPaymenttype = debitorTermOfPayment;

            switch (termsOfDelivery)
            {
                case "9":
                    orderPaymenttype = _economicSession.TermOfPayment.FindByName("Dankort")[0];
                    break;
                case "4":
                    orderPaymenttype = _economicSession.TermOfPayment.FindByName("Bank overførsel")[0];
                    break;
                case "10":
                    orderPaymenttype = _economicSession.TermOfPayment.FindByName("Netto 14 Dage")[0];
                    break;
            }

            // Order Settings 
            IDebtor debitor = _economicSession.Debtor.FindByNumber(debitorNumber);
            IOrderData orderData = _economicSession.OrderData.Create(debitor);

            orderData.Heading = orderName;
            orderData.Currency = orderCurrency;
            orderData.ExchangeRate = 100;
            orderData.Date = orderDate;
            orderData.Attention = debitor.Attention;
            orderData.DebtorAddress = debitor.Address;
            orderData.DebtorCity = debitor.City;
            orderData.DebtorCountry = debitor.Country;
            orderData.DebtorEan = debitor.Ean;
            orderData.DebtorName = debitor.Name;
            orderData.DebtorPostalCode = debitor.PostalCode;            
            orderData.DeliveryDate = dateDeliveryDate;
            orderData.Date = orderDate;
            orderData.DueDate = dueDate;            
            orderData.DeliveryAddress = deliveryAddress;
            orderData.DeliveryCity = deliveryCity;
            orderData.DeliveryCountry = deliveryCountry;
            orderData.DeliveryPostalCode = deliveryPostalCode;
            orderData.YourReference = debitor.Attention;
            orderData.TermOfPayment = orderPaymenttype;
            orderData.TermsOfDelivery = termsOfDelivery;
            orderData.TextLine1 = comments;
            orderData.Layout = orderLayout;
            orderData.IsVatIncluded = true;

            return (orderData);
        }

        /// <summary>
        /// Saves a Orderline
        /// </summary>
        /// <param name="orderlineData">IOrderLineData object</param>
        /// <returns>IOrderLine object</returns>
        public IOrderLine SaveOrderLine(IOrderLineData orderlineData)
        {
            IOrderLine orderline = _economicSession.OrderLine.CreateFromData(orderlineData);
            return (orderline);
        }        

        /// <summary>
        /// Saves an array of Orderlines
        /// </summary>
        /// <param name="orderlineData">IOrderLineData object array</param>
        /// <returns>IOrderLine object array</returns>
        public IOrderLine[] SaveOrderLines(IOrderLineData[] orderlineData)
        {
            IOrderLine[] orderlines = _economicSession.OrderLine.CreateFromDataArray(orderlineData);
            return (orderlines);
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
            return MakeOrderLine(orderNumber, productNumber, quantity, price, string.Empty);
        }
        
        public IOrderLineData MakeOrderLine(int orderNumber, string productNumber, decimal quantity, decimal price, string productName)
        {
            IOrder order = _economicSession.Order.FindByNumber(orderNumber);
            IOrderLineData orderLineData = _economicSession.OrderLineData.Create(order);
            IProduct product = _economicSession.Product.FindByNumber(productNumber);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(productName))
                {
                    product.Name = productName;
                }

                orderLineData.Product = product;
                orderLineData.Description = product.Name;
                orderLineData.Quantity = quantity;
                orderLineData.Unit = product.Unit;
                orderLineData.UnitNetPrice = price;
            } // TODO Create product in E-conomic if not found
            return orderLineData;
        }

        /// <summary>
        /// Saves a product in e-conomic
        /// </summary>
        /// <param name="productData">IProductData object</param>
        /// <returns>IProduct object</returns>
        public IProduct SaveProduct(IProductData productData)
        {
            IProduct product = _economicSession.Product.CreateFromData(productData);
            return (product);
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
            IProductData productData = _economicSession.ProductData.Create(productNumber, _economicSession.ProductGroup.FindByNumber(productGroupNumber), productName);            
            
            if ((productDescription.IndexOf("<") >= 0))
            {
                productDescription = Base.StripHTML_ALT(productDescription); // Core.Converter.ToString
            }
            if ((productDescription.Length > 255))
            {
                productDescription = productDescription.Substring(0, 250) + "...";
            }
            productData.Description = productDescription;
            productData.Name = productName;
            productData.Unit = _economicSession.Unit.FindByNumber(int.Parse(_configReader.ProductUnit));
            productData.SalesPrice = productSalesPrice;
            productData.IsAccessible = true;
            productData.Volume = productVolume;
            return (productData);
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
            IAccount account = _economicSession.Account.FindByNumber(accountNumber);
            IProductGroupData groupData = _economicSession.ProductGroupData.Create(groupNumber, groupName, account);
            IProductGroup group = _economicSession.ProductGroup.CreateFromData(groupData);
            return (group);
        }

        #region Economic.Api.Data
        
        /// <summary>
        /// Gets Product Groups Data by Group's ID's
        /// </summary>                        
        /// <param name="groupsIDs">int: Groups ID's to search for</param>
        /// <returns>IProductGroupData array</returns>
        public IProductGroupData[] GetProductGroupsDataByIDs(int[] groupsIDs)
        {
            IProductGroup[] groups = _economicSession.ProductGroup.FindByNumber(groupsIDs);
            return _economicSession.ProductGroupData.GetDataArray(groups);
        }

        /// <summary>
        /// Gets a ProdcutData
        /// </summary>
        /// <param name="products">Array of IProduct</param>
        /// <returns>Array of IProductData</returns>
        public IProductData[] GetProductsData(IProduct[] products)
        {
            return _economicSession.ProductData.GetDataArray(products);
        }

        #endregion        
    }
}
