using Dynamicweb;
using Dynamicweb.Controls;
using Dynamicweb.Controls.Extensibility;
using Dynamicweb.Controls.Icons;
using Dynamicweb.Ecommerce.Orders;
using Dynamicweb.Environment;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Security.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	[AddInDescription("Send order to Economic")]
	[AddInName("Send order")]
	[AddInTarget("/Admin/Module/eCom_Catalog/dw7/edit/EcomOrder_Edit.aspx")]
	[AddInTarget("/Admin/Module/eCom_Catalog/dw7/OrderList.aspx")]
	public class SendOrderAddIn : RibbonBarAddIn
	{
		private readonly string TransferOrdersButtonClickedSessionKey = "EconomicTransferOrdersButton_Click";

		private string SessionKey
		{
			get
			{
				string str = "Economic.TransferredOrder";
				Order dataSource = base.Ribbon.DataContext.DataSource as Order;
				if (dataSource != null)
				{
					str = string.Concat(str, dataSource.Id);
				}
				return str;
			}
		}

		public SendOrderAddIn(RibbonBar ribbon) : base(ribbon)
		{
		}

		private RibbonBarButton CreateTransferOrderButton()
		{
			RibbonBarButton ribbonBarButton;
			RibbonBarGroup ribbonBarGroup = base.CreateDefaultContainer();
			using (RibbonBarButton ribbonBarButton1 = new RibbonBarButton())
			{
				ribbonBarGroup.AddItem(ribbonBarButton1);
				ribbonBarButton1.Text = "Transfer to Economic";
				ribbonBarButton1.Image = Icon.Type.Export;
				ribbonBarButton1.Size = Icon.Size.Large;
				ribbonBarButton = ribbonBarButton1;
			}
			return ribbonBarButton;
		}

		private List<Order> GetSelectedOrders()
		{
			List<Order> orders = new List<Order>();
			if (Context.get_Current().get_Session().get_Item("DW.Controls.Tree.List") == null || !(Context.get_Current().get_Session().get_Item("DW.Controls.Tree.List") is ListRowCollection))
			{
				orders = null;
			}
			else
			{
				ListRowCollection item = Context.get_Current().get_Session().get_Item("DW.Controls.Tree.List") as ListRowCollection;
				if (item != null && item.Count > 0)
				{
					if (item.Any<ListRow>((ListRow r) => r.Selected))
					{
						OrderService orderService = new OrderService();
						foreach (string str in (
							from r in item
							where r.Selected
							select r.ItemID).Distinct<string>())
						{
							Order order = orderService.GetOrder(str);
							if (order == null)
							{
								continue;
							}
							orders.Add(order);
						}
					}
				}
			}
			return orders;
		}

		public override void Load()
		{
			if (LicenseManager.LicenseHasFeature("eCom_economic"))
			{
				if (base.Ribbon.DataContext.DataSource is Order[])
				{
					RibbonBarButton ribbonBarButton = this.CreateTransferOrderButton();
					ribbonBarButton.OnClientClick = "var selectedRows = List.getSelectedRows('List'); if (selectedRows.length > 0) {    if (!confirm('Transfer selected orders to Economic?')) {         return false;     } } else {     alert('No orders to transfer'); return false; };";
					ribbonBarButton.EnableServerClick = true;
					ribbonBarButton.Click += new EventHandler<EventArgs>(this.TransferOrdersButton_Click);
				}
				else if (base.Ribbon.DataContext.DataSource is Order)
				{
					Order dataSource = base.Ribbon.DataContext.DataSource as Order;
					if (dataSource != null)
					{
						RibbonBarButton ribbonBarButton1 = this.CreateTransferOrderButton();
						if (!string.IsNullOrEmpty(dataSource.IntegrationOrderId))
						{
							ribbonBarButton1.OnClientClick = "alert('Order has been transferred');";
						}
						else
						{
							ribbonBarButton1.OnClientClick = string.Concat("if(!confirm('Transfer ", dataSource.Id, " to Economic?')) { return false; };");
							ribbonBarButton1.EnableServerClick = true;
							ribbonBarButton1.Click += new EventHandler<EventArgs>(this.TransferOrderButton_Click);
						}
					}
				}
				Context.get_Current().get_Session().set_Item(this.TransferOrdersButtonClickedSessionKey, null);
			}
		}

		public override void Render(HtmlTextWriter writer)
		{
			if (!Context.get_Current().get_Items().Contains("EconomicSendOrderAddInRender"))
			{
				if (Context.get_Current().get_Session().get_Item(this.TransferOrdersButtonClickedSessionKey) != null)
				{
					List<Order> selectedOrders = this.GetSelectedOrders();
					if (selectedOrders != null)
					{
						Context.get_Current().get_Session().set_Item(this.SessionKey, this.TransferOrders(selectedOrders));
					}
				}
				if (Context.get_Current().get_Session().get_Item(this.SessionKey) != null && !string.IsNullOrEmpty(HttpContext.Current.Session[this.SessionKey].ToString()))
				{
					writer.Write(string.Concat("<script type='text/javascript'>alert('", HttpUtility.HtmlEncode(HttpContext.Current.Session[this.SessionKey]), "')</script>"));
					Context.get_Current().get_Session().Remove(this.SessionKey);
				}
				base.Render(writer);
				Context.get_Current().get_Items().Add("EconomicSendOrderAddInRender", null);
			}
		}

		private bool TransferOrder(Order order)
		{
			using (NotificationHandler notificationHandler = new NotificationHandler())
			{
				notificationHandler.HandleOrder(order);
			}
			return !string.IsNullOrEmpty(order.IntegrationOrderId);
		}

		/// <summary>
		/// Occures when the button was clicked from edit order page
		/// </summary>        
		private void TransferOrderButton_Click(object sender, EventArgs e)
		{
			string empty = string.Empty;
			Order dataSource = base.Ribbon.DataContext.DataSource as Order;
			if (dataSource != null)
			{
				if (!string.IsNullOrEmpty(dataSource.IntegrationOrderId))
				{
					empty = "Order already transferred";
				}
				else
				{
					empty = (!this.TransferOrder(dataSource) ? "Error creating order in Economic. See Economic log for details" : "Order successfully transferred to Economic");
				}
			}
			Context.get_Current().get_Session().set_Item(this.SessionKey, empty);
		}

		private string TransferOrders(List<Order> orders)
		{
			string empty = string.Empty;
			if (orders == null)
			{
				empty = "Can not load selected orders from the order list";
			}
			else if (orders.Count <= 0)
			{
				empty = "No orders selected for transfer";
			}
			else
			{
				List<string> strs = new List<string>();
				List<string> strs1 = new List<string>();
				using (NotificationHandler notificationHandler = new NotificationHandler())
				{
					foreach (Order order in orders)
					{
						if (!string.IsNullOrEmpty(order.IntegrationOrderId))
						{
							strs1.Add(order.Id);
						}
						else
						{
							notificationHandler.HandleOrder(order);
							if (string.IsNullOrEmpty(order.IntegrationOrderId))
							{
								continue;
							}
							strs.Add(order.Id);
						}
					}
				}
				if (strs1.Count > 0 && strs1.Count == orders.Count<Order>())
				{
					empty = "All selected orders are already transferred to Economic";
				}
				else if (strs.Count <= 0 && strs1.Count <= 0)
				{
					empty = "All selected orders were not transferred to Economic. See Economic log for details";
				}
				else if (strs.Count + strs1.Count != orders.Count<Order>())
				{
					if (strs1.Count > 0)
					{
						empty = string.Concat(empty, string.Format("Orders with IDs [{0}] are already transferred to Economic. ", string.Join(",", strs1)));
					}
					if (strs.Count > 0)
					{
						empty = string.Concat(empty, string.Format("Orders with IDs [{0}] were successfully transferred to Economic. ", string.Join(",", strs)));
					}
					empty = string.Concat(empty, string.Format("Orders with IDs [{0}] were not transferred to Economic. See Economic log for details", string.Join(",", orders.Where<Order>((Order o) => {
						if (strs.Contains(o.Id))
						{
							return false;
						}
						return !strs1.Contains(o.Id);
					}).Select<Order, string>((Order o) => o.Id).Distinct<string>().ToArray<string>())));
				}
				else
				{
					empty = "All selected orders were successfully transferred to Economic";
				}
			}
			return empty;
		}

		/// <summary>
		/// Occures when the button was clicked from orders list page
		/// </summary>        
		private void TransferOrdersButton_Click(object sender, EventArgs e)
		{
			Context.get_Current().get_Session().set_Item(this.TransferOrdersButtonClickedSessionKey, this.TransferOrdersButtonClickedSessionKey);
		}
	}
}