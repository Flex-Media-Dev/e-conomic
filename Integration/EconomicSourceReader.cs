using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Economic.Api;
using Economic.Api.Data;
using Dynamicweb.Extensibility;
using Dynamicweb.DataIntegration.Integration;
using Dynamicweb.DataIntegration.Integration.Interfaces;
using Dynamicweb.DataIntegration.Providers;
using Dynamicweb.Ecommerce.Products;
using Dynamicweb.Extensibility.AddIns;
using NLog;

namespace Dynamicweb.eCommerce.Economic.Integration
{    
    public class EconomicSourceReader : ISourceReader
    {
        private Mapping _mapping;
        private string _shopId;
        private string _langId;
        private bool _done;
        private EconomicCommunicator _ec;                
        private List<IProductData> _productsList;
        private List<IProductGroupData> _groupsList;
        private int _groupsCounter = 0;
        private int _productsCounter = 0;
        private int _groupProductRelationCounter = 0;
        private int _shopGroupRelationCounter = 0;
        private Logger _logger;
        private List<ProductImportActivityExtender> _addinInstances;

        public EconomicSourceReader(Mapping mapping, string shopId, string langId, string logFile)
        {            
            _mapping = mapping;
            _shopId = shopId;
            _langId = langId;
            _logger = LogManager.GetCurrentClassLogger();            
            _ec = new EconomicCommunicator();
        }

        #region Properties

        private List<IProductGroupData> GroupsList
        {
            get
            {
                if (_groupsList == null)
                {
                    _groupsList = new List<IProductGroupData>();                    
                    try
                    {
                        if (Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Products/Groups").Length > 0)
                        {
                            string[] groups = Configuration.SystemConfiguration.Instance.GetValue("/Globalsettings/Ecom/EconomicIntegration/Products/Groups").Split(',');
                            int[] ids = Array.ConvertAll<string, int>(groups, new Converter<string, int>(Convert.ToInt32));                            
                            _groupsList.AddRange(_ec.GetProductGroupsDataByIDs(ids));
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error("STD EPI: Get E-conomic Product Groups, Standard method to get e-conomic product groups failed : " + e.Message);
                    }
                }
                return _groupsList;
            }
        }

        private List<IProductData> ProductsList
        {
            get
            {
                if (_productsList == null)
                {                    
                    int[] ids = GroupsList.Select(g => g.Number).ToArray<int>();
                    try
                    {
                        IProductGroup[] groups = _ec.GetProductGroupById(ids);//need to get ProductGroups for Products Loading
                        List<IProduct> products = new List<IProduct>();
                        foreach (IProductGroup g in groups)
                            products.AddRange(g.GetProducts());
                        _productsList = _ec.GetProductsData(products.ToArray()).ToList<IProductData>();
                    }
                    catch (Exception e)
                    {
                        _logger.Error("STD EPI: Get E-conomic Products, Method to get e-conomic products failed : " + e.Message);
                        _productsList = new List<IProductData>();
                    }
                }
                return _productsList;
            }
        }

        private List<ProductImportActivityExtender> AddinInstances
        {
            get
            {
                if (_addinInstances == null)
                {
                    _addinInstances = new List<ProductImportActivityExtender>();
                    try
                    {
                        ArrayList addInExtenders = AddInManager.GetAddInClasses(typeof(ProductImportActivityExtender));
                        foreach (object addin in addInExtenders)
                        {
                            Type type = AddInManager.GetAddInTypeByName(addin.ToString());
                            if (type != null)                            
                                _addinInstances.Add(Activator.CreateInstance(type) as ProductImportActivityExtender);                            
                        }                        
                    }
                    catch (Exception e)
                    {
                        _logger.Error("STD EPI: GetAddInClasses, Standard method to get addin classes failed : " + e.Message);
                    }
                }
                return _addinInstances;
            }
        }

        #endregion

        public bool IsDone()
        {
            return _done;
        }

        // Gets data from Webservice, save locally, return next product/group as dictionary, containing only the columns found in  _mapping.GetColumnMappings()
        public Dictionary<string, object> GetNext()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();            

            if (_mapping.SourceTable.Name == "E-conomicGroups")
            {
                result = GetNextGroup();
            }

            if (_mapping.SourceTable.Name == "E-conomicProducts")
            {
                result = GetNextProduct();                
            }

            if (_mapping.SourceTable.Name == "E-conomicGroupProductRelation")
            {
                result = GetNextGroupProductRelation();                
            }

            if (_mapping.SourceTable.Name == "E-conomicShopGroupRelation")
            {
                result = GetNextShopGroupRelation();                
            }

            return result;
        }

        private Dictionary<string, object> GetNextGroup()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (GroupsList.Count > _groupsCounter)
            {
                IProductGroupData group = GroupsList[_groupsCounter];                

                foreach (ProductImportActivityExtender extender in AddinInstances)
                {                    
                    try
                    {
                        group = extender.ProductGroupConverter(group);// Run the add-in
                    }
                    catch (Exception e)
                    {
                        _logger.Error("AddIns EPI: ProductGroupConverter, Addin " + extender.ToString() + " function ProductGroupConverter failed : " + e.Message);
                    }
                }

                result.Add("GroupID", group.Number.ToString());
                result.Add("GroupLanguageID", _langId);
                result.Add("GroupNumber", group.Number.ToString());
                result.Add("GroupName", group.Name);
                _groupsCounter++;
            }
            _done = _groupsCounter == GroupsList.Count ? true : false;
            return result;
        }

        private Dictionary<string, object> GetNextProduct()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (ProductsList.Count > _productsCounter)
            {
                IProductData product = ProductsList[_productsCounter];

                foreach (ProductImportActivityExtender extender in AddinInstances)
                {
                    try
                    {
                        product = extender.ProductConverter(product);// Run the add-in
                    }
                    catch (Exception e)
                    {
                        _logger.Error("AddIns EPI: ProductGroupConverter, Addin " + extender.ToString() + " function ProductGroupConverter failed : " + e.Message);
                    }                    
                }

                result.Add("ProductID", product.Number.ToString());
                result.Add("ProductLanguageID", _langId);
                result.Add("ProductVariantID", string.Empty);
                result.Add("ProductDefaultShopID", _shopId);
                result.Add("ProductNumber", product.Number.ToString());
                result.Add("ProductName", product.Name);
                result.Add("ProductShortDescription", (product.Description == null) ? string.Empty : product.Description);
                result.Add("ProductStock", double.Parse((product.Available != null && product.Available.HasValue) ? product.Available.Value.ToString() : "0"));
                result.Add("ProductCostPrice", product.CostPrice.ToString());
                result.Add("ProductActive", product.IsAccessible);
                result.Add("ProductPrice", double.Parse(product.SalesPrice.ToString()));
                result.Add("ProductVolume", double.Parse(product.Volume.ToString()));
                // Comment: Waiting for E-conomic to give access to StockType on the ProductGroup to detected wether it is a service product or stock product.
                result.Add("ProductType", Dynamicweb.Ecommerce.Products.ProductType.Stock);
                result.Add("ProductPriceType", ProductPriceType.FixedPrice);
                result.Add("ProductCreated", DateTime.Now);
                result.Add("ProductUpdated", DateTime.Now);
                _productsCounter++;
                _done = false;
            }
            _done = _productsCounter == ProductsList.Count ? true : false;
            return result;
        }

        private Dictionary<string, object> GetNextGroupProductRelation()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (ProductsList.Count > _groupProductRelationCounter)
            {
                result.Add("GroupProductRelationGroupID", ProductsList[_groupProductRelationCounter].ProductGroup.Number.ToString());
                result.Add("GroupProductRelationProductID", ProductsList[_groupProductRelationCounter].Number.ToString());
                _groupProductRelationCounter++;
                _done = false;
            }
            _done = _groupProductRelationCounter == ProductsList.Count ? true : false;
            return result;
        }

        private Dictionary<string, object> GetNextShopGroupRelation()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            if (GroupsList.Count > _shopGroupRelationCounter)
            {
                result.Add("ShopGroupShopID", _shopId);
                result.Add("ShopGroupGroupID", GroupsList[_shopGroupRelationCounter].Number.ToString());
                _shopGroupRelationCounter++;
                _done = false;
            }
            _done = _shopGroupRelationCounter == GroupsList.Count ? true : false;
            return result;
        }

        public void Dispose()
        {            
        }                       
    }
}
