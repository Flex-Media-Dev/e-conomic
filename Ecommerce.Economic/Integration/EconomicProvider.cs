using Dynamicweb.DataIntegration;
using Dynamicweb.DataIntegration.Integration;
using Dynamicweb.DataIntegration.Integration.Interfaces;
using Dynamicweb.DataIntegration.ProviderHelpers;
using Dynamicweb.Ecommerce.Common;
using Dynamicweb.Ecommerce.International;
using Dynamicweb.Ecommerce.Shops;
using Dynamicweb.Extensibility.AddIns;
using Dynamicweb.Extensibility.Editors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	[AddInDescription("Updates and creates eCommerce product(s) and productgroup(s) from e-conomic")]
	[AddInIgnore(false)]
	[AddInLabel("E-conomic provider")]
	[AddInName("Product importer")]
	public class EconomicProvider : BaseProvider, ISource, INotDestination, IDropDownOptions
	{
		private Schema _schema;

		public new string FilesFolderName
		{
			get;
			set;
		}

		/// <summary>
		/// LangID setting
		/// </summary>
		[AddInParameter("Language")]
		[AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;")]
		[AddInParameterGroup("Source")]
		public string LangId
		{
			get;
			set;
		}

		/// <summary>
		/// ShopID setting
		/// </summary>
		[AddInParameter("Shop")]
		[AddInParameterEditor(typeof(DropDownParameterEditor), "NewGUI=true;")]
		[AddInParameterGroup("Source")]
		public string ShopId
		{
			get;
			set;
		}

		public override string WorkingDirectory
		{
			get;
			set;
		}

		public EconomicProvider()
		{
		}

		public EconomicProvider(XmlNode xmlNode)
		{
			foreach (XmlNode childNode in xmlNode.ChildNodes)
			{
				string name = childNode.Name;
				if (name == "Schema")
				{
					this._schema = new Schema(childNode);
				}
				else if (name == "Shop")
				{
					if (!childNode.HasChildNodes)
					{
						continue;
					}
					this.ShopId = childNode.FirstChild.Value;
				}
				else
				{
					if (name != "Language")
					{
						throw new Exception(string.Concat("Unexpected node: ", childNode));
					}
					if (!childNode.HasChildNodes)
					{
						continue;
					}
					this.LangId = childNode.FirstChild.Value;
				}
			}
		}

		public new bool CheckCondition(MappingConditional mc, Dictionary<string, object> row)
		{
			return BaseProvider.CheckConditionStatic(mc, row);
		}

		public List<SchemaComparerResult> CheckMapping(Mapping map)
		{
			return new List<SchemaComparerResult>();
		}

		public override void Close()
		{
		}

		private new XElement CreateParameterNode(Type t, string name, string value)
		{
			XElement xElement = null;
			if (!(t != null) || string.IsNullOrEmpty(name))
			{
				return xElement;
			}
			if (!string.IsNullOrEmpty(value))
			{
				return new XElement("Parameter", new object[] { new XAttribute("addin", t.FullName), new XAttribute("name", name), new XAttribute("value", value) });
			}
			return new XElement("Parameter", new object[] { new XAttribute("addin", t.FullName), new XAttribute("name", name), new XAttribute("value", "") });
		}

		public Hashtable GetOptions(string dropdownName)
		{
			Hashtable hashtables = new Hashtable();
			if (dropdownName == "Language")
			{
				foreach (Language language in (new LanguageService()).GetLanguages())
				{
					hashtables.Add(language.LanguageId, language.Name);
				}
			}
			else if (dropdownName == "Shop")
			{
				foreach (Shop shop in (new ShopService()).GetShops())
				{
					hashtables.Add(shop.Id, shop.Name);
				}
			}
			return hashtables;
		}

		public override Schema GetOriginalSourceSchema()
		{
			Schema schema = new Schema();
			schema.AddTable("E-conomicGroups");
			schema.AddTable("E-conomicProducts");
			schema.AddTable("E-conomicGroupProductRelation");
			schema.AddTable("E-conomicShopGroupRelation");
			foreach (Table table in schema.GetTables())
			{
				string name = table.Name;
				if (name == "E-conomicGroups")
				{
					table.AddColumn(new SqlColumn("GroupID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("GroupLanguageID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("GroupNumber", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("GroupName", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
				}
				else if (name == "E-conomicProducts")
				{
					table.AddColumn(new SqlColumn("ProductID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductLanguageID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductVariantID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductDefaultShopID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductNumber", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductName", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductShortDescription", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductStock", typeof(double), SqlDbType.Float, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductCostPrice", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductActive", typeof(bool), SqlDbType.Bit, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductPrice", typeof(double), SqlDbType.Float, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductVolume", typeof(double), SqlDbType.Float, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductType", typeof(int), SqlDbType.Int, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductPriceType", typeof(int), SqlDbType.Int, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductCreated", typeof(DateTime), SqlDbType.DateTime, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductUpdated", typeof(DateTime), SqlDbType.DateTime, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductGrossWeight", typeof(double), SqlDbType.Float, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ProductNetWeight", typeof(double), SqlDbType.Float, table, -1, false, false, false));
				}
				else if (name == "E-conomicGroupProductRelation")
				{
					table.AddColumn(new SqlColumn("GroupProductRelationGroupID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("GroupProductRelationProductID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
				}
				else if (name == "E-conomicShopGroupRelation")
				{
					table.AddColumn(new SqlColumn("ShopGroupShopID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
					table.AddColumn(new SqlColumn("ShopGroupGroupID", typeof(string), SqlDbType.NVarChar, table, -1, false, false, false));
				}
			}
			return schema;
		}

		public override ISourceReader GetReader(Mapping mapping)
		{
			if (string.IsNullOrEmpty(this.LangId))
			{
				this.LangId = Dynamicweb.Ecommerce.Common.Context.LanguageID;
			}
			if (string.IsNullOrEmpty(this.ShopId) && Application.DefaultShop != null)
			{
				this.ShopId = Application.DefaultShop.Id;
			}
			return new EconomicSourceReader(mapping, this.ShopId, this.LangId, base.Logger);
		}

		public override Schema GetSchema()
		{
			if (this._schema == null)
			{
				this._schema = this.GetOriginalSourceSchema();
			}
			return this._schema;
		}

		public override void Initialize()
		{
		}

		public override void LoadSettings(Job job)
		{
		}

		public override void OverwriteSourceSchemaToOriginal()
		{
			this._schema = this.GetOriginalSourceSchema();
		}

		public override void SaveAsXml(XmlTextWriter textWriter)
		{
			textWriter.WriteElementString("Language", this.LangId);
			textWriter.WriteElementString("Shop", this.ShopId);
			this._schema.SaveAsXml(textWriter);
		}

		public override string Serialize()
		{
			XDocument xDocument = new XDocument(new XDeclaration("1.0", "utf-8", string.Empty), Array.Empty<object>());
			XElement xElement = new XElement("Parameters");
			xDocument.Add(xElement);
			xElement.Add(this.CreateParameterNode(base.GetType(), "Shop", this.ShopId));
			xElement.Add(this.CreateParameterNode(base.GetType(), "Language", this.LangId));
			return xDocument.ToString();
		}

		public override void UpdateSourceSettings(ISource source)
		{
			EconomicProvider economicProvider = (EconomicProvider)source;
			this.ShopId = economicProvider.ShopId;
			this.LangId = economicProvider.LangId;
		}

		public override string ValidateSourceSettings()
		{
			if (string.IsNullOrEmpty(this.LangId))
			{
				return "Language field can not be empty";
			}
			if (string.IsNullOrEmpty(this.ShopId))
			{
				return "Shop field can not be empty";
			}
			return "";
		}
	}
}