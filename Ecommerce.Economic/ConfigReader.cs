using Dynamicweb.Configuration;
using Dynamicweb.Core;
using System;
using System.Runtime.CompilerServices;

namespace Dynamicweb.Ecommerce.Economic
{
	/// <summary>
	/// Summary description for ConfigReader - economic configuration settings
	/// </summary>
	public class ConfigReader
	{
		/// <summary>
		/// App secret token from the E-conomic developer app that connects to E-conomic account
		/// </summary>
		public string AppSecretToken;

		/// <summary>
		/// Grnat token from the E-conomic account that accepts the app
		/// </summary>
		public string AgreementGrantToken;

		/// <summary>
		/// Debitor currency
		/// </summary>
		public string DebitorCurrency
		{
			get;
			set;
		}

		/// <summary>
		/// Debitor group to store new debitor in
		/// </summary>
		public string DebitorGroup
		{
			get;
			set;
		}

		/// <summary>
		/// Debitor layout
		/// </summary>
		public string DebitorLayout
		{
			get;
			set;
		}

		/// <summary>
		/// Debitor terms of payment
		/// </summary>
		public string DebitorTermsOfPayment
		{
			get;
			set;
		}

		/// <summary>
		/// Debitor vat group
		/// </summary>
		public string DebitorVatGroup
		{
			get;
			set;
		}

		/// <summary>
		/// Error email
		/// </summary>
		public string ErrorEmail
		{
			get;
			set;
		}

		/// <summary>
		/// Order layout
		/// </summary>
		public string Orderlayout
		{
			get;
			set;
		}

		/// <summary>
		/// Order terms of payment
		/// </summary>
		public string OrderTermsOfPayment
		{
			get;
			set;
		}

		/// <summary>
		/// Product unit
		/// </summary>
		public string ProductUnit
		{
			get;
			set;
		}

		/// <summary>
		/// Constructor that loads config automatic. It makes a call to LoadConfig() after initialization
		/// </summary>
		public ConfigReader()
		{
			this.DebitorTermsOfPayment = "Netto 8 Dage";
			this.DebitorGroup = "1";
			this.DebitorLayout = "Norm. m. girokort";
			this.DebitorVatGroup = "1";
			this.DebitorCurrency = "DKK";
			this.ProductUnit = "-1";
			this.ErrorEmail = "email@example.com";
			this.OrderTermsOfPayment = "-1";
			this.Orderlayout = "-1";
			this.LoadConfig();
		}

		/// <summary>
		/// Loads configuration from various sources.
		/// </summary>
		public void LoadConfig()
		{
			this.AppSecretToken = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/AppSecretToken");
			this.AgreementGrantToken = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Credentials/AgreementGrantToken");
			this.DebitorTermsOfPayment = Converter.ToString(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/Paymentcondition"));
			this.DebitorLayout = Converter.ToString(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DebitorOrderLayout"));
			this.DebitorGroup = SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/DebitorGroup");
			this.OrderTermsOfPayment = this.DebitorTermsOfPayment;
			this.Orderlayout = this.DebitorLayout;
		}
	}
}