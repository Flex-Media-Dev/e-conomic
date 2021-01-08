using Economic.Api.Data;
using System;

namespace Dynamicweb.Ecommerce.Economic.Integration
{
	/// <summary>
	/// Extention class to intercept how E-conomic Products are stored in Dynamicweb eCommerce
	/// </summary>
	public class ProductImportActivityExtender
	{
		public ProductImportActivityExtender()
		{
		}

		/// <summary>
		/// Method to do manipulation with the E-conomic Product object before is is imported to Dynamicweb
		/// </summary>        
		/// <param name="economicProduct">E-conomic IProduct object that is currently being transfered</param>
		/// <returns>E-conomic IProduct object</returns>
		public virtual IProductData ProductConverter(IProductData economicProduct)
		{
			return economicProduct;
		}

		/// <summary>
		/// Method to do manipulation with the E-conomic Group object before it is imported to Dynamicweb
		/// </summary>        
		/// <param name="economicProductGroup">E-conomic IProductGroup object that is currently being transfered</param>
		/// <returns>E-conomic Group object</returns>
		public virtual IProductGroupData ProductGroupConverter(IProductGroupData economicProductGroup)
		{
			return economicProductGroup;
		}
	}
}