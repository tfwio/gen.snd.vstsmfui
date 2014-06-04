/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MidiSmf.Forms
{
//	class NoteTextRenderer
//	{
//		Rectangle ClipRegion(Graphics g, Rectangle client, Padding padd)
//		{
//			return new Rectangle(0, padd.Top, MidiPianoViewRenderer.ui.Width - padd.Right, MidiPianoViewRenderer.ui.CalculatedHeight);
//		}
//		void Render(Graphics g, int offset, Rectangle client, Padding padd)
//		{
//			try {
//				if (n.HasFlag(Needs.Text))
//				{
//					g.SetClip(ClipRegion(g,client,padd));
//					MidiPianoViewRenderer.NoteText(g, offsetY);
//					g.ResetClip();
//				}
//			} catch {}
//		}
//		void NoteText(Graphics g, int off, Rectangle client, Padding padd)
//		{
//			FloatRect r = ui.Rect;
//			float xposition = MidiPianoViewRenderer.ui.Rect.Left - MidiPianoViewRenderer.TextXOffset;
//			using (SolidBrush b = new SolidBrush(Color.Black))
//				foreach (int i in MidiPianoViewRenderer.GetInt32YEnumeration())
//			{
//				int value = GetY(i, off) + 127;
//				value = 127 - value;
//				if (value < 0 || value > 127) continue;
//				
//				FloatPoint position = new FloatPoint(xposition, i);
//				
//				// Shift ebony positions over
//				if (CheckYEbony(i, off, 12)) position.X -= MidiPianoViewRenderer.TextXOffsetE.X;
//				
//				g.DrawString(value.ToString().PadLeft(3, '0'), NoteFont, b, position);
//				
//			}
//		}
//	}
}
