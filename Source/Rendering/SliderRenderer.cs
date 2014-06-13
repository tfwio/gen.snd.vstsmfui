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
	/// <summary>
	/// meme
	/// </summary>
	static class SliderRenderer
	{
		static public void RenderSlider(Color back, Graphics g, Decible decible, System.Windows.Forms.Control control)
		{
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Alignment = StringAlignment.Center;
			
			using (Pen p = new Pen(control.ForeColor,1)) g.DrawRectangle(p, 0, 0, control.Width - 1, control.Height - 1);
			using (Brush b = new SolidBrush(back)) g.FillRectangle(b, 1, 1, (int)((control.Width - 2) * decible.Percent), control.Height - 2);
			using (Brush b = new SolidBrush(control.ForeColor))
				g.DrawString(
					decible.ToString(),
					control.Font,
					b,
					control.ClientRectangle,
					format
				);
			
		}
	}
}
