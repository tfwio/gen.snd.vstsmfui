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
using System.Drawing.Drawing2D;

#endregion
namespace modest100.Rendering
{
	class PianoGridRendererSnapSettings
	{
		
		/// <param name="ppn">parts per note</param>
		/// <param name="tpq">ticks per quarter</param>
		public PianoGridRendererSnapSettings(int ppn, int tpq)
		{
			pxPpn = ppn;
			pxTpq = tpq;
			pxTpn = pxPpn * 4;
			pxTpb = pxTpn * 4;
		}
		
		#region Snapping
		
		/// <param name="ppt">parts per tick?</param>
		/// <param name="ppn">parts per note</param>
		/// <param name="ppb">parts per bar</param>
		public void InitializeSnappingBoundaries(int ppt, int ppn, int ppb) { pxTpb = (pxTpn = (pxPpn = ppt) * ppn) * ppb; }
		
		#endregion
		
		int pxTpq;
		int pxTpn;
		int pxTpb;
		int pxPpn;
		public int PxPpn { get { return pxPpn; } set { pxPpn = value; } }
		public int PxTpq { get { return pxTpq * pxPpn; } }
		public int PxTpn { get { return pxTpn; } }
		public int PxTpb { get { return pxTpb; } }
		
	}
	class PianoGridRendererResources
	{
		public Pen GridPen { get; set; }
		public Pen GridRowMid { get; set; }
		public Pen GridRowHeavy { get; set; }
		public Pen GridBar { get; set; }
		public Pen GridRowDiv { get; set; }
		
		static Pen DefaultPenStyle(Color clr, float width=1)
		{
			Pen p = new Pen(clr,width);
			p.Alignment = PenAlignment.Left;
			p.StartCap = LineCap.Round;
			p.EndCap = LineCap.Round;
			return p;
		}
		
		static public PianoGridRendererResources Create()
		{
			PianoGridRendererResources res = new PianoGridRendererResources();
			res.GridPen = DefaultPenStyle(Color.Silver, 1);
			res.GridRowMid = DefaultPenStyle(Color.FromArgb(127, Color.Red));
			res.GridRowHeavy = DefaultPenStyle(Color.Gray);
			res.GridBar = DefaultPenStyle(Color.FromArgb(0,0,0));
			res.GridRowDiv = DefaultPenStyle(Color.FromArgb(127,0, 127, 255));
			return res;
		}
	}
}
