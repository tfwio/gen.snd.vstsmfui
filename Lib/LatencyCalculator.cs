#region User/License
// oio * 10/20/2012 * 8:50 AM

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
#endregion
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace modest100
{
	public class LatencyCalculator
	{
		public int SampleRate { get;set; }
		
		public double LatencyInMilliseconds { get { return GetIntMilliseconds(latencyInSamples); } }
		
		public double LatencyInSamples {
			get { return latencyInSamples; }
			set { latencyInSamples = value; }
		} double latencyInSamples = 1024;
		
		int GetIntMilliseconds(double value) { return Convert.ToInt32( GetMilliseconds(value) ); }
		double GetMilliseconds(double value)
		{
			return Convert.ToInt32(Math.Floor(value / SampleRate * 1000D));
		}
		
		public int ResetValue(int rate, int latencyInSamples)
		{
			this.SampleRate = rate;
			this.latencyInSamples = latencyInSamples;
			return GetIntMilliseconds(latencyInSamples);
		}
		
		public LatencyCalculator() : this(48000,1024) {}
		public LatencyCalculator(int rate, int latencySmps)
		{
			ResetValue(rate,latencySmps);
		}
	}
}
