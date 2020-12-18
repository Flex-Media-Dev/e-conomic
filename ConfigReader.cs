using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamicweb.eCommerce.Economic
{
    /// <summary>
    /// Summary description for ConfigReader - economic configuration settings
    /// </summary>
    public class ConfigReader
    {
        private int _agreementNumber;
        private string _username;
        private string _password;
        private string _debitorGroup;
        private string _debitorVatGroup;
        private string _debitorCurrency;
        private string _debitorPaymentCondisions;
        private string _debitorLayout;
        private string _productUnit;
        private string _errorEmail;
        private string _orderPaymentCondisions;
        private string _orderLayout;


        /// <summary>
        /// Constructor that loads config automatic. It makes a call to LoadConfig() after initialization
        /// </summary>
        public ConfigReader()
        {
            this._agreementNumber = 0;
            this._username = "";
            this._password = "";
            this._debitorPaymentCondisions = "Netto 8 Dage"; //Dankort
            this._debitorGroup = "1";
            this._debitorLayout = "Norm. m. girokort";
            this._debitorVatGroup = "1";
            this._debitorCurrency = "DKK";
            this._productUnit = "-1";
            this._errorEmail = "email@example.com";
            this._orderPaymentCondisions = "-1";
            this._orderLayout = "-1";

            this.LoadConfig();
        }

        /// <summary>
        /// Loads configuration from various sources.
        /// </summary>
        public void LoadConfig()
        {
            this.AgreementNumber = Core.Converter.ToInt32(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/SolutionNumber"));
            this.Username = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/Username"));
            if (Core.Converter.ToBoolean(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/PasswordEncrypted")))
                this.Password = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/Password")); 
            else
                this.Password = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/Password"));

            this.DebitorTermsOfPayment = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/Paymentcondition"));
            this.DebitorLayout = Core.Converter.ToString(Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DebitorOrderLayout"));
            this.DebitorGroup = Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DebitorGroup");
            this.OrderTermsOfPayment = this.DebitorTermsOfPayment;
            this.Orderlayout = this.DebitorLayout;
        }
        
        /// <summary>
        /// Agreementnumber for your E-conomic account
        /// </summary>
        public int AgreementNumber
        {
            get { return this._agreementNumber; }
            set { this._agreementNumber = value; }
        }

        /// <summary>
        /// E-conomic username to get access to your E-conomic solution via API
        /// </summary>
        public string Username
        {
            get { return this._username; }
            set { this._username = value; }
        }

        /// <summary>
        /// E-conomic password to get access to your E-conomic solution via API
        /// </summary>
        public string Password
        {
            get { return this._password; }
            set { this._password = value; }
        }

        /// <summary>
        /// Debitor group to store new debitor in
        /// </summary>
        public string DebitorGroup
        {
            get { return this._debitorGroup; }
            set { this._debitorGroup = value; }
        }

        /// <summary>
        /// Debitor vat group
        /// </summary>
        public string DebitorVatGroup
        {
            get { return this._debitorVatGroup; }
            set { this._debitorVatGroup = value; }
        }

        /// <summary>
        /// Debitor currency
        /// </summary>
        public string DebitorCurrency
        {
            get { return this._debitorCurrency; }
            set { this._debitorCurrency = value; }
        }

        /// <summary>
        /// Debitor terms of payment
        /// </summary>
        public string DebitorTermsOfPayment
        {
            get { return this._debitorPaymentCondisions; }
            set { this._debitorPaymentCondisions = value; }
        }

        /// <summary>
        /// Debitor layout
        /// </summary>
        public string DebitorLayout
        {
            get { return this._debitorLayout; }
            set { this._debitorLayout = value; }
        }

        /// <summary>
        /// Product unit
        /// </summary>
        public string ProductUnit
        {
            get { return this._productUnit; }
            set { this._productUnit = value; }
        }

        /// <summary>
        /// Error email
        /// </summary>
        public string ErrorEmail
        {
            get { return this._errorEmail; }
            set { this._errorEmail = value; }
        }

        /// <summary>
        /// Order terms of payment
        /// </summary>
        public string OrderTermsOfPayment
        {
            get { return this._orderPaymentCondisions; }
            set { this._orderPaymentCondisions = value; }
        }

        /// <summary>
        /// Order layout
        /// </summary>
        public string Orderlayout
        {
            get { return this._orderLayout; }
            set { this._orderLayout = value; }
        }

       

    }
}
