#region User/License
// oio * 10/31/2012 * 12:27 PM

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
using System;
using System.Drawing;
#endregion
namespace modest100.Rendering
{
	public struct Decible
	{
		static public string fmt_db = "{0:F2} dB";
		static public string fmt_percent = "{0:F2} %";
		static public readonly float MinDb = -48;
		
		public static object NaN { get { return naN; }
		} static readonly public object naN = new Object();
		
		public double Input { get; set; }
		
		public double Db { get { return 20 * Math.Log( Input ); } }
		public double Percent { get { return 1 - ( Db / MinDb ); } }
		
		static public implicit operator String(Decible value) { return value.ToString(); }
		static public implicit operator Decible(double value) { return Decible.Create(value); }
		static public implicit operator Decible(string value) { return Decible.Create(value); }
		
		public string DecibleString { get { return string.Format(fmt_db,Db); } }
		public string PercentString { get { return string.Format(fmt_percent,Percent); } }
		
		public override string ToString() { return DecibleString; }
		
		public bool SetText(string value)
		{
			string clone = value.ToLower();
			if (clone.Contains("db"))
			{
				Input = Convert.ToDouble(clone.Replace("db",""));
				return true;
			}
			return false;
		}
		
		static public Decible Create(string value)
		{
			Decible db = new Decible();
			db.SetText( value );
			return db;
		}
		
		static public Decible Create(double value)
		{
			Decible db = new Decible();
			db.Input = value;
			return db;
		}
		
	}
}
