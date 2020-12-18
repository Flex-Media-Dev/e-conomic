using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml.Linq;
using System.Xml;
using Dynamicweb.Ecommerce.Shops;
using Dynamicweb.Extensibility;
using Dynamicweb.Extensibility.Editors;
using Dynamicweb.Ecommerce.International;
using Dynamicweb.DataIntegration.Providers;
using Dynamicweb.DataIntegration.Integration;
using Dynamicweb.DataIntegration.Integration.Interfaces;
using System.Data;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.DataIntegration;
using Dynamicweb.DataIntegration.ProviderHelpers;

namespace Dynamicweb.eCommerce.Economic.Integration
{
    [
    AddInName("Product importer"),    
    AddInLabel("E-conomic provider"),
    AddInDescription("Updates and creates eCommerce product(s) and productgroup(s) from e-conomic"),
    AddInIgnore(false)
    ]
    public class EconomicProvider : BaseProvider, ISource, INotDestination, IDropDownOptions
    {        
        private Schema _schema;
        private string _logFile;

        public EconomicProvider()
        {
        }

        #region properties
        /// <summary>
        /// LangID setting
        /// </summary>
        [AddInParameter("Language"), AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;"), AddInParameterGroup("Source")]
        public string LangId { get; set; }

        /// <summary>
        /// ShopID setting
        /// </summary>
        [AddInParameter("Shop"), AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;"), AddInParameterGroup("Source")]
        public string ShopId { get; set; }
        #endregion

        Hashtable IDropDownOptions.GetOptions(string strName)
        {
            Hashtable ht = new Hashtable();
            switch (strName)
            {
                case "Language":
                    LanguageCollection languages = Language.GetLanguages();
                    foreach (Language language in languages)
                    {
                        ht.Add(language.LanguageId, language.Name);
                    }
                    break;
                case "Shop":
                    var shops = Shop.GetShops();
                    foreach (Shop shop in shops)
                    {
                        ht.Add(shop.Id, shop.Name);
                    }
                    break;
            }
            return ht;
        }

        public new string Serialize()
        {
            XDocument document = new XDocument(new XDeclaration("1.0", "utf-8", string.Empty));
            XElement root = new XElement("Parameters");
            document.Add(root);
            root.Add(CreateParameterNode(GetType(), "Shop", ShopId));
            root.Add(CreateParameterNode(GetType(), "Language", LangId));
            string ret = document.ToString();
            return ret;
        }

        public EconomicProvider(XmlNode xmlNode)
        {
            foreach (XmlNode node in xmlNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Schema":
                        _schema = new Schema(node);
                        break;
                    case "Shop":
                        if (node.HasChildNodes)
                            ShopId = node.FirstChild.Value;
                        break;
                    case "Language":
                        if (node.HasChildNodes)
                            LangId = node.FirstChild.Value;
                        break;
                    default:
                        throw new Exception("Unexpected node: " + node);
                }
            }
        }

        public new void SaveAsXml(XmlTextWriter xmlTextWriter)
        {
            xmlTextWriter.WriteElementString("Language", LangId);
            xmlTextWriter.WriteElementString("Shop", ShopId);
            _schema.SaveAsXml(xmlTextWriter);
        }        

        public new string ValidateSourceSettings()
        {
            if (string.IsNullOrEmpty(LangId))
                return "Language field can not be empty";
            if (string.IsNullOrEmpty(ShopId))
                return "Shop field can not be empty";
            return "";
        }

        public new void UpdateSourceSettings(ISource source)
        {
            EconomicProvider newProvider = (EconomicProvider)source;
            ShopId = newProvider.ShopId;
            LangId = newProvider.LangId;
        }

        public new Schema GetOriginalSourceSchema()
        {
            Schema result = new Schema();
            
            result.AddTable("E-conomicGroups");
            result.AddTable("E-conomicProducts");
            result.AddTable("E-conomicGroupProductRelation");
            result.AddTable("E-conomicShopGroupRelation");
            foreach (Table table in result.GetTables())
            {
                switch (table.Name)
                {
                    case "E-conomicGroups":
                        table.AddColumn(new SqlColumn(("GroupID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("GroupLanguageID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("GroupNumber"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("GroupName"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        break;
                    case "E-conomicProducts":
                        table.AddColumn(new SqlColumn(("ProductID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductLanguageID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductVariantID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductDefaultShopID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductNumber"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductName"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductShortDescription"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductStock"), typeof(double), SqlDbType.Float, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductCostPrice"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductActive"), typeof(bool), SqlDbType.Bit, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductPrice"), typeof(double), SqlDbType.Float, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductVolume"), typeof(double), SqlDbType.Float, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductType"), typeof(int), SqlDbType.Int, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductPriceType"), typeof(int), SqlDbType.Int, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductCreated"), typeof(DateTime), SqlDbType.DateTime, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ProductUpdated"), typeof(DateTime), SqlDbType.DateTime, table, -1, false, false, false));
                        break;
                    case "E-conomicGroupProductRelation":
                        table.AddColumn(new SqlColumn(("GroupProductRelationGroupID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("GroupProductRelationProductID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        break;
                    case "E-conomicShopGroupRelation":
                        table.AddColumn(new SqlColumn(("ShopGroupShopID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        table.AddColumn(new SqlColumn(("ShopGroupGroupID"), typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
                        break;
                }
            }            
            return result;
        }

        public new Schema GetSchema()
        {
            if (_schema == null)
            {
                _schema = GetOriginalSourceSchema();
            }
            return _schema;
        }

        public new void OverwriteSourceSchemaToOriginal()
        {
            _schema = GetOriginalSourceSchema();
        }

        public override ISourceReader GetReader(Mapping mapping)
        {
            if(string.IsNullOrEmpty(this.LangId))
                LangId = Ecommerce.Common.Context.LanguageID;
            if (string.IsNullOrEmpty(this.ShopId) && Ecommerce.Common.Application.DefaultShop != null)
                ShopId = Ecommerce.Common.Application.DefaultShop.Id;
            return new EconomicSourceReader(mapping, ShopId, LangId, _logFile);
        }

        private new XElement CreateParameterNode(Type t, string name, string value)
        {
            XElement ret = null;

            if ((t != null) && !string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(value))
                    return new XElement("Parameter", new XAttribute("addin", t.FullName), new XAttribute("name", name), new XAttribute("value", value));

                return new XElement("Parameter", new XAttribute("addin", t.FullName), new XAttribute("name", name), new XAttribute("value", ""));

            }

            return ret;
        }

        public new void Close()
        {            
        }

        public new void Initialize()
        {            
        }

        public new void LoadSettings(Job job, string logFile)
        {
            _logFile = logFile;
        }

        public new bool CheckCondition(MappingConditional mc, Dictionary<string, object> row)
        {
            return BaseProvider.CheckConditionStatic(mc, row);
        }
        
        public override string WorkingDirectory{ get; set; }
        public new string FilesFolderName { get; set; }

		#region ISource Members

		List<SchemaComparerResult> ISource.CheckMapping(Mapping map)
		{
			return new List<SchemaComparerResult>();
		}

		#endregion
	}
}
