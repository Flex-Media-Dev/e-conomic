using System;

namespace Dynamicweb.Ecommerce.Economic
{
	public class Result
	{
		private string _text;

		private string _value;

		private bool _selected;

		/// <summary>
		/// Indicates which item is selected
		/// </summary>
		public bool Selected
		{
			get
			{
				return this._selected;
			}
			set
			{
				this._selected = value;
			}
		}

		/// <summary>
		/// Holds the result text/name
		/// </summary>
		public string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value;
			}
		}

		/// <summary>
		/// Holds the result value
		/// </summary>
		public string Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}

		/// <summary>
		/// Empty contructor
		/// </summary>
		public Result()
		{
		}

		/// <summary>
		/// Default contructor
		/// </summary>
		/// <param name="text">Result text</param>
		/// <param name="value">Result value</param>
		/// <param name="selected">Result selected</param>
		public Result(string text, string value, bool selected)
		{
			this.Text = text;
			this.Value = value;
			this.Selected = selected;
		}
	}
}