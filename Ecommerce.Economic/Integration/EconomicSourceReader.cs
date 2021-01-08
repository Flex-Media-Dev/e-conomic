using Dynamicweb.Configuration;
using Dynamicweb.DataIntegration.Integration;
using Dynamicweb.DataIntegration.Integration.Interfaces;
using Dynamicweb.Ecommerce.Economic;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Logging;
using Economic.Api;
using Economic.Api.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	public class EconomicSourceReader : ISourceReader, IDisposable
	{
		private Mapping _mapping;

		private string _shopId;

		private string _langId;

		private bool _done;

		private EconomicCommunicator _ec;

		private List<IProductData> _productsList;

		private List<IProductGroupData> _groupsList;

		private int _groupsCounter;

		private int _productsCounter;

		private int _groupProductRelationCounter;

		private int _shopGroupRelationCounter;

		private ILogger _logger;

		private ILogger _systemLogger;

		private List<ProductImportActivityExtender> _addinInstances;

		private Dictionary<string, Dictionary<string, double>> _productsWeightData;

		private List<ProductImportActivityExtender> AddinInstances
		{
			get
			{
				if (this._addinInstances == null)
				{
					this._addinInstances = new List<ProductImportActivityExtender>();
					try
					{
						foreach (object addInClass in AddInManager.GetAddInClasses(typeof(ProductImportActivityExtender)))
						{
							Type addInTypeByName = AddInManager.GetAddInTypeByName(addInClass.ToString());
							if (addInTypeByName == null)
							{
								continue;
							}
							this._addinInstances.Add(Activator.CreateInstance(addInTypeByName) as ProductImportActivityExtender);
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("STD EPI: Get add-in classes, Standard method to get add-in classes failed : ", exception.Message));
					}
				}
				return this._addinInstances;
			}
		}

		private List<IProductGroupData> GroupsList
		{
			get
			{
				if (this._groupsList == null)
				{
					this._groupsList = new List<IProductGroupData>();
					try
					{
						if (SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Products/Groups").Length > 0)
						{
							int[] numArray = Array.ConvertAll<string, int>(SystemConfiguration.get_Instance().GetValue("/Globalsettings/Ecom/EconomicIntegration/Products/Groups").Split(new char[] { ',' }), new Converter<string, int>(Convert.ToInt32));
							this._groupsList.AddRange(this._ec.GetProductGroupsDataByIDs(numArray));
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("STD EPI: Get Economic Product Groups, Standard method to get economic product groups failed : ", exception.Message));
					}
				}
				return this._groupsList;
			}
		}

		private List<IProductData> ProductsList
		{
			get
			{
				if (this._productsList == null)
				{
					int[] array = (
						from g in this.GroupsList
						select g.Number).ToArray<int>();
					try
					{
						IProductGroup[] productGroupById = this._ec.GetProductGroupById(array);
						List<IProduct> products = new List<IProduct>();
						IProductGroup[] productGroupArray = productGroupById;
						for (int i = 0; i < (int)productGroupArray.Length; i++)
						{
							products.AddRange(productGroupArray[i].GetProducts());
						}
						this._productsList = this._ec.GetProductsData(products.ToArray()).ToList<IProductData>();
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("STD EPI: Get Economic Products, Method to get Economic products failed : ", exception.Message));
						this._productsList = new List<IProductData>();
					}
				}
				return this._productsList;
			}
		}

		private Dictionary<string, Dictionary<string, double>> ProductsWeightData
		{
			get
			{
				if (this._productsWeightData == null)
				{
					try
					{
						this._productsWeightData = this._ec.GetProductsWeightData();
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("Method get products weight failed: ", exception.Message));
						this._productsWeightData = new Dictionary<string, Dictionary<string, double>>();
					}
				}
				return this._productsWeightData;
			}
		}

		public EconomicSourceReader(Mapping mapping, string shopId, string langId, ILogger logger)
		{
			this._mapping = mapping;
			this._shopId = shopId;
			this._langId = langId;
			this._logger = logger;
			this._systemLogger = LogManager.System.GetLogger("Provider", this.GetType().FullName);
			this._ec = new EconomicCommunicator();
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (this._ec != null)
			{
				this._ec.Dispose();
			}
		}

		public Dictionary<string, object> GetNext()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			if (this._mapping.SourceTable.Name == "E-conomicGroups")
			{
				strs = this.GetNextGroup();
			}
			if (this._mapping.SourceTable.Name == "E-conomicProducts")
			{
				strs = this.GetNextProduct();
			}
			if (this._mapping.SourceTable.Name == "E-conomicGroupProductRelation")
			{
				strs = this.GetNextGroupProductRelation();
			}
			if (this._mapping.SourceTable.Name == "E-conomicShopGroupRelation")
			{
				strs = this.GetNextShopGroupRelation();
			}
			return strs;
		}

		private Dictionary<string, object> GetNextGroup()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			if (this.GroupsList.Count > this._groupsCounter)
			{
				IProductGroupData item = this.GroupsList[this._groupsCounter];
				foreach (ProductImportActivityExtender addinInstance in this.AddinInstances)
				{
					try
					{
						item = addinInstance.ProductGroupConverter(item);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("Add-ins EPI: Product group converter, add-in ", addinInstance.ToString(), " function Product Group Converter failed : ", exception.Message));
						this._systemLogger.Error(string.Concat("Add-ins EPI: Product Group Converter, add-in ", addinInstance.ToString()), exception);
					}
				}
				int number = item.Number;
				strs.Add("GroupID", number.ToString());
				strs.Add("GroupLanguageID", this._langId);
				number = item.Number;
				strs.Add("GroupNumber", number.ToString());
				strs.Add("GroupName", item.Name);
				this._groupsCounter++;
			}
			this._done = (this._groupsCounter == this.GroupsList.Count ? true : false);
			return strs;
		}

		private Dictionary<string, object> GetNextGroupProductRelation()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			if (this.ProductsList.Count > this._groupProductRelationCounter)
			{
				int number = this.ProductsList[this._groupProductRelationCounter].ProductGroup.Number;
				strs.Add("GroupProductRelationGroupID", number.ToString());
				strs.Add("GroupProductRelationProductID", this.ProductsList[this._groupProductRelationCounter].Number.ToString());
				this._groupProductRelationCounter++;
				this._done = false;
			}
			this._done = (this._groupProductRelationCounter == this.ProductsList.Count ? true : false);
			return strs;
		}

		private Dictionary<string, object> GetNextProduct()
		{
			decimal value;
			string str;
			Dictionary<string, object> strs = new Dictionary<string, object>();
			if (this.ProductsList.Count > this._productsCounter)
			{
				IProductData item = this.ProductsList[this._productsCounter];
				foreach (ProductImportActivityExtender addinInstance in this.AddinInstances)
				{
					try
					{
						item = addinInstance.ProductConverter(item);
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this._logger.Log(string.Concat("Add-ins EPI: Product Converter, Add-in ", addinInstance.ToString(), " function Product Converter failed : ", exception.Message));
						this._systemLogger.Error(string.Concat("Add-ins EPI: Product Converter, Add-in ", addinInstance.ToString()), exception);
					}
				}
				strs.Add("ProductID", item.Number.ToString());
				strs.Add("ProductLanguageID", this._langId);
				strs.Add("ProductVariantID", string.Empty);
				strs.Add("ProductDefaultShopID", this._shopId);
				strs.Add("ProductNumber", item.Number.ToString());
				strs.Add("ProductName", item.Name);
				strs.Add("ProductShortDescription", (item.Description == null ? string.Empty : item.Description));
				Dictionary<string, object> strs1 = strs;
				if (!item.Available.HasValue || !item.Available.HasValue)
				{
					str = "0";
				}
				else
				{
					value = item.Available.Value;
					str = value.ToString();
				}
				strs1.Add("ProductStock", double.Parse(str));
				value = item.CostPrice;
				strs.Add("ProductCostPrice", value.ToString());
				strs.Add("ProductActive", item.IsAccessible);
				value = item.SalesPrice;
				strs.Add("ProductPrice", double.Parse(value.ToString()));
				value = item.Volume;
				strs.Add("ProductVolume", double.Parse(value.ToString()));
				strs.Add("ProductType", ProductType.Stock);
				strs.Add("ProductPriceType", ProductPriceType.FixedPrice);
				strs.Add("ProductCreated", DateTime.Now);
				strs.Add("ProductUpdated", DateTime.Now);
				Dictionary<string, double> strs2 = null;
				if (!this.ProductsWeightData.TryGetValue(item.Number, out strs2) || strs2 == null)
				{
					strs.Add("ProductGrossWeight", 0);
					strs.Add("ProductNetWeight", 0);
				}
				else
				{
					strs.Add("ProductGrossWeight", (strs2.ContainsKey("ProductGrossWeight") ? strs2["ProductGrossWeight"] : 0));
					strs.Add("ProductNetWeight", (strs2.ContainsKey("ProductNetWeight") ? strs2["ProductNetWeight"] : 0));
				}
				this._productsCounter++;
				this._done = false;
			}
			this._done = (this._productsCounter == this.ProductsList.Count ? true : false);
			return strs;
		}

		private Dictionary<string, object> GetNextShopGroupRelation()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			if (this.GroupsList.Count > this._shopGroupRelationCounter)
			{
				strs.Add("ShopGroupShopID", this._shopId);
				int number = this.GroupsList[this._shopGroupRelationCounter].Number;
				strs.Add("ShopGroupGroupID", number.ToString());
				this._shopGroupRelationCounter++;
				this._done = false;
			}
			this._done = (this._shopGroupRelationCounter == this.GroupsList.Count ? true : false);
			return strs;
		}

		public bool IsDone()
		{
			return this._done;
		}
	}
}