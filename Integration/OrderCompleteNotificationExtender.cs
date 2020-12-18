using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Economic.Api.Data;
using Dynamicweb.Ecommerce.Orders;

namespace Dynamicweb.eCommerce.Economic.Integration
{
    /// <summary>
    /// Extention class to intercept how Dynamicweb eCommerce orders are stored in E-conomic
    /// </summary>
    public class OrderCompleteNotificationExtender
    {

        /// <summary>
        /// Method to modify Dynamicweb order information. Order to be stored in e-conomic
        /// </summary>
        /// <param name="order">Dynamicweb eCommerce order</param>
        /// <returns>Dynamicweb eCommerce order</returns>
        public virtual Order DynamicwebOrder(Dynamicweb.Ecommerce.Orders.Order order)
        {
            return order;
        }

        /// <summary>
        /// Method to do manipulation with the objEconomicDebitor object
        /// </summary>
        /// <param name="order">Dynamicweb eCommerce Order object that is currently under processing</param>
        /// <param name="economicDebitor">E-conomic IDebtorData object that is being created</param>
        /// <returns>IDebtorData object</returns>
        public virtual IDebtorData EconomicDebitor(Dynamicweb.Ecommerce.Orders.Order order, IDebtorData economicDebitor)
        {
            return economicDebitor;
        }

        /// <summary>
        /// Method to do manipulation with the objEconomicOrder object
        /// </summary>
        /// <param name="order">Dynamicweb eCommerce Order object that is currently under processing</param>
        /// <param name="economicOrder">E-conomic IOrderData object that is currently being created</param>
        /// <returns>IOrderData object</returns>
        public virtual IOrderData EconomicOrder(Dynamicweb.Ecommerce.Orders.Order order, IOrderData economicOrder)
        {
            return economicOrder;
        }
       
        /// <summary>
        /// Method to do manipulation with the ordinary objEconomicOrderline object before it is sent to E-conomic
        /// </summary>
        /// <param name="orderLine">Dynamiceb eCommerce OrderLine object that is currently under processing</param>
        /// <param name="economicOrderLine">E-conomic IOrderLineData object that is currently being created</param>
        /// <returns>IOrderLineData object</returns>
        public virtual IOrderLineData EconomicOrderline(Dynamicweb.Ecommerce.Orders.OrderLine orderLine, IOrderLineData economicOrderLine)
        {
            return economicOrderLine;
        }

        /// <summary>
        /// Method to do manipulation with the Discount EconomicOrderline object before it is sent to E-conomic
        /// </summary>
        /// <param name="orderLine">Dynamiceb eCommerce Discount OrderLine object that is currently under processing</param>
        /// <param name="economicDiscountOrderLine">E-conomic IOrderLineData object that is currently being created</param>        
        /// <returns>IOrderLineData object</returns>
        public virtual IOrderLineData EconomicDiscountOrderline(Dynamicweb.Ecommerce.Orders.OrderLine orderLine, IOrderLineData economicDiscountOrderLine)
        {            
            return economicDiscountOrderLine;
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
        /// <param name="paymentFeeOrderline">E-conomic IOrderLineData object that is currently being created</param>
        /// <returns>IOrderLineData object</returns>
        public virtual IOrderLineData EconomicShippingFeeOrderline(IOrderLineData shippingFeeOrderline)
        {
            return shippingFeeOrderline;
        }        
    }
}
