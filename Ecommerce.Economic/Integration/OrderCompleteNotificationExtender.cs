using Dynamicweb.Ecommerce.Orders;
using Economic.Api.Data;
using System;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	/// <summary>
	/// Extention class to intercept how Dynamicweb eCommerce orders are stored in E-conomic
	/// </summary>
	public class OrderCompleteNotificationExtender
	{
		public OrderCompleteNotificationExtender()
		{
		}

		/// <summary>
		/// Method to modify Dynamicweb order information. Order to be stored in e-conomic
		/// </summary>
		/// <param name="order">Dynamicweb eCommerce order</param>
		/// <returns>Dynamicweb eCommerce order</returns>
		public virtual Order DynamicwebOrder(Order order)
		{
			return order;
		}

		/// <summary>
		/// Method to do manipulation with the objEconomicDebitor object
		/// </summary>
		/// <param name="order">Dynamicweb eCommerce Order object that is currently under processing</param>
		/// <param name="debitor">E-conomic IDebtorData object that is being created</param>
		/// <returns>IDebtorData object</returns>
		public virtual IDebtorData EconomicDebitor(Order order, IDebtorData debitor)
		{
			return debitor;
		}

		/// <summary>
		/// Method to do manipulation with the Discount EconomicOrderline object before it is sent to E-conomic
		/// </summary>
		/// <param name="orderLine">Dynamiceb eCommerce Discount OrderLine object that is currently under processing</param>
		/// <param name="economicsDiscountOrderLine">E-conomic IOrderLineData object that is currently being created</param>        
		/// <returns>IOrderLineData object</returns>
		public virtual IOrderLineData EconomicDiscountOrderline(OrderLine orderLine, IOrderLineData economicsDiscountOrderLine)
		{
			return economicsDiscountOrderLine;
		}

		/// <summary>
		/// Method to do manipulation with the objEconomicOrder object
		/// </summary>
		/// <param name="order">Dynamicweb eCommerce Order object that is currently under processing</param>
		/// <param name="economicsOrder">E-conomic IOrderData object that is currently being created</param>
		/// <returns>IOrderData object</returns>
		public virtual IOrderData EconomicOrder(Order order, IOrderData economicsOrder)
		{
			return economicsOrder;
		}

		/// <summary>
		/// Method to do manipulation with the ordinary objEconomicOrderline object before it is sent to E-conomic
		/// </summary>
		/// <param name="orderLine">Dynamiceb eCommerce OrderLine object that is currently under processing</param>
		/// <param name="economicsOrderLine">E-conomic IOrderLineData object that is currently being created</param>
		/// <returns>IOrderLineData object</returns>
		public virtual IOrderLineData EconomicOrderline(OrderLine orderLine, IOrderLineData economicsOrderLine)
		{
			return economicsOrderLine;
		}

		/// <summary>
		/// Method to do manipulation with the Payment Fee EconomicOrderline object before it is sent to E-conomic
		/// </summary>        
		/// <param name="paymentFeeOrderline">E-conomic IOrderLineData object that is currently being created</param>
		/// <returns>IOrderLineData object</returns>
		public virtual IOrderLineData EconomicPaymentFeeOrderline(IOrderLineData paymentFeeOrderline)
		{
			return paymentFeeOrderline;
		}

		/// <summary>
		/// Method to do manipulation with the Shipping Fee EconomicOrderline object before it is sent to E-conomic
		/// </summary>        
		/// <param name="shippingFeeOrderLine">E-conomic IOrderLineData object that is currently being created</param>
		/// <returns>IOrderLineData object</returns>
		public virtual IOrderLineData EconomicShippingFeeOrderline(IOrderLineData shippingFeeOrderLine)
		{
			return shippingFeeOrderLine;
		}
	}
}