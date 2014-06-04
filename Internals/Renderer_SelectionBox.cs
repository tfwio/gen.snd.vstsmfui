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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace modest100.Internals
{
	static class Renderer_SelectionBox
	{
		static SmoothingMode Smooth { get { return SmoothingMode.HighQuality; } }
		static InterpolationMode Bicubic { get { return InterpolationMode.HighQualityBicubic; } }
		
		static void SetQuality(this Graphics g, SmoothingMode smooth, InterpolationMode ipol)
		{
			g.SmoothingMode = smooth;
			g.InterpolationMode = ipol;
		}
	//		
	//		static void DrawSelectionBox(
	//			this Graphics g,
	//			int fgAlpha, Color fgColor, int fgSize,
	//			int bgAlpha, Color bgColor,
	//			RenderStateType n
	//		)
	//		{
	//			if (n.HasFlag(RenderStateType.Select)||n.HasFlag(RenderStateType.Deselect))
	//			{
	//				using (Pen r = new Pen(Color.FromArgb(fgAlpha,fgColor),fgSize))
	//					using (SolidBrush b = new SolidBrush(Color.FromArgb(bgAlpha,bgColor)))
	//				{
	//					MouseState state = MouseState.Create(this);
	//					g.SetQuality(Smooth,Bicubic);
	//					g.DrawRectangle(r,new FloatRect(state.InvMousePoint,state.InvMouseTrail));
	//					g.FillRectangle(b,new FloatRect(state.InvMousePoint,state.InvMouseTrail));
	//					state = null;
	//				}
	//			}
	//		}
	}
}
