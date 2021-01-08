using Dynamicweb.Configuration;
using Dynamicweb.Core;
using Dynamicweb.Ecommerce.Notifications;
using Dynamicweb.Extensibility.Notifications;
using System;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	[Subscribe("Ecom7CartCheckoutDoneOrderIsComplete")]
	public class OrderStepsCompleted : NotificationSubscriber
	{
		public OrderStepsCompleted()
		{
		}

		public override void OnNotify(string notification, NotificationArgs args)
		{
			if (Converter.ToBoolean(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Ordering/ExportOrderOnComplete")))
			{
				Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsCompleteArgs checkoutDoneOrderIsCompleteArg = (Dynamicweb.Ecommerce.Notifications.Ecommerce.Cart.CheckoutDoneOrderIsCompleteArgs)args;
				using (NotificationHandler notificationHandler = new NotificationHandler())
				{
					notificationHandler.HandleOrder(checkoutDoneOrderIsCompleteArg.Order);
				}
			}
		}
	}
}