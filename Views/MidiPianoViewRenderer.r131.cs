/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

using DspAudio.Forms;

namespace MidiSmf.Forms
{
	class MidiPianoViewRenderer_r131
	{
		static internal MidiPianoViewSettings ui;
		static int TextOffset = 32;
		static int TextOffsetE = 6;
		static int XPart = 4, XNote = XPart * 4, XBar = XNote * 4;
		//
		#region Pens
		public static Pen GridPen {
			get {
				Pen p = new Pen(Color.Silver, 1);
				p.Alignment = PenAlignment.Left;
				p.StartCap = LineCap.Round;
				p.EndCap = LineCap.Round;
				return p;
			}
		}
		public static Pen GridRowMid {
			get {
				Pen p = new Pen(Color.FromArgb(127, Color.Red), 1);
				p.Alignment = PenAlignment.Left;
				p.StartCap = LineCap.Round;
				p.EndCap = LineCap.Round;
				return p;
			}
		}
		public static Pen GridRowHeavy {
			get {
				Pen p = new Pen(Color.Gray, 1);
				p.Alignment = PenAlignment.Left;
				p.StartCap = LineCap.Round;
				p.EndCap = LineCap.Round;
				return p;
			}
		}
		public static Pen GridBar {
			get {
				Pen p = new Pen(Color.FromArgb(0,0,0), 1);
				p.Alignment = PenAlignment.Center;
				p.StartCap = LineCap.Round;
				p.EndCap = LineCap.Round;
				return p;
			}
		}
		public static Pen GridRowDiv {
			get {
				Pen p = new Pen(Color.FromArgb(127,0, 127, 255), 1);
				p.Alignment = PenAlignment.Left;
				p.StartCap = LineCap.Round;
				p.EndCap = LineCap.Round;
				return p;
			}
		}
		#endregion
		
		static MidiPianoViewRenderer_r131()
		{
			Refit(4, 4, 4);
		}
		
		public static Image GetBackground(int off)
		{
			Image img = new Bitmap(ui.Width, ui.Height, PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(img)) {
				g.Clear(ui.GridBackgroundColor);
				RenderGrid(g, off);
			}
			return img;
		}
		public static void RenderBackground(Graphics g, Image background)
		{
			g.DrawImage(background, 0, 0);
		}
		
		public static void RenderGrid(Graphics g, int off)
		{
			RenderGridE(g, off);
			RenderGridX(g, 0);
			RenderGridY(g, off);
		}
		/// <summary>
		/// </summary>
		/// <param name="nNote">Number of quarters</param>
		/// <param name="nNotes"></param>
		/// <param name="nBars"></param>
		public static void Refit(int nPart, int nNotes, int nBars)
		{
			XPart = nPart;
			XNote = XPart * nNotes;
			XBar = XNote * nBars;
			Debug.Print("{0},{1},{2}", XPart, XNote, XBar);
		}
		
		static bool CheckXMod(float value, int offset, int against) { return (GetX(value, offset)) % against == 0; }
		static bool CheckYMod(float value, int offset, int against) { return (GetY(value, offset)) % against == 0; }
		static bool CheckYMod2(float value, int offset, int against) { int a = GetY(value, offset) + 127; a = 128 - a; return a % against == 0; }
		
		static int GetX(float value, int off) { return (int)((value - ui.Gutter.Left) / ui.NodeSize.Width) + off; }
		static int GetY(float value, int off) { return (int)((value - ui.Gutter.Top) / ui.NodeSize.Height) - off; }
		
		static bool CheckYEbony(float value, int off, int against)
		{
			int a = GetY(value, off) + 127;
			a = 127 - a;
			int ck = (int)(a) % against;
			switch (ck) { case 1: case 3: case 6: case 8: case 10: return true; default: return false; }
		}
		
		public static Font NoteFont = new Font("Ubuntu Mono", 10, GraphicsUnit.Pixel);
//		static public List<int> Coalculate()
//		{
//			List<int> list = new List<int>();
//			FloatRect r = ui.Rect;
//			using (SolidBrush b = new SolidBrush(Color.Black))
//				for (float i = r.Top; i <= (r.Bottom - ui.Gutter.Bottom); i += ui.RowHeight)
//			{
//				
//				int value = GetY(i, off) + 127;
//				value = 127 - value;
//				if (value < 0 || value > 127) continue;
//				
//				if (CheckYEbony(i, off, 12)) g.DrawString((value).ToString().PadLeft(3, '0'), NoteFont, b, ui.Rect.Left - TextOffset - TextOffsetE, i);
//				else g.DrawString(value.ToString().PadLeft(3, '0'), NoteFont, b, ui.Rect.Left - TextOffset, i);
//			}
//		}
		public static void NoteText(Graphics g, int off)
		{
			FloatRect r = ui.Rect;
			using (SolidBrush b = new SolidBrush(ui.GridForegroundTextColor))
				for (float i = r.Top; i <= (r.Bottom - ui.Gutter.Bottom); i += ui.RowHeight)
			{
				int value = GetY(i, off) + 127;
				value = 127 - value;
				if (value < 0 || value > 127) continue;
				if (CheckYEbony(i, off, 12)) g.DrawString((value).ToString().PadLeft(3, '0'), NoteFont, b, ui.Rect.Left - TextOffset - TextOffsetE, i);
				else g.DrawString(value.ToString().PadLeft(3, '0'), NoteFont, b, ui.Rect.Left - TextOffset, i);
			}
		}
		/// <summary>
		/// X[n, n+QuadrantWidth]
		/// </summary>
		/// <param name="g"></param>
		/// <param name="off"></param>
		public static void RenderGridX(Graphics g, int off)
		{
			using (Pen b = GridBar) using (Pen m = GridRowMid) using (Pen d = GridRowDiv) using (Pen pen = GridPen)
				for (float i = ui.Rect.Left; i <= ui.Rect.Right; i += ui.RowWidth)
			{
				if (CheckXMod(i, off, XBar))
					g.DrawLine(b, i, ui.Rect.Top, i, ui.Rect.Bottom); else if (CheckXMod(i, off, XNote))
					g.DrawLine(m, i, ui.Rect.Top, i, ui.Rect.Bottom); else if (CheckXMod(i, off, XPart))
					g.DrawLine(d, i, ui.Rect.Top, i, ui.Rect.Bottom);
				else
					g.DrawLine(pen, i, ui.Rect.Top, i, ui.Rect.Bottom);
			}
		}
		/// Y[n, n+RowHeight] (n = ui.Rect.Top(
		public static void RenderGridY(Graphics g, int off)
		{
			using (SolidBrush b = new SolidBrush(Color.FromArgb(24, Color.DodgerBlue)))
				using (Pen d = GridRowHeavy) using (Pen m = GridRowMid) using (Pen p = GridPen)
					for (float i = ui.Rect.Top; i <= ui.Rect.Bottom; i += ui.RowHeight)
			{
				int value = GetY(i, off) + 127;
				value = 127 - value;
				if (value < -1 || value > 127) continue;
				if (CheckYMod2(i, off, 12)) {
					g.DrawLine(p, 0, i, ui.Rect.Left, i);
					g.DrawLine(d, ui.Rect.Left, i, ui.Rect.Right, i);
				} else {
					g.DrawLine(p, 0, i, ui.Rect.Right, i);
				}
			}
		}
		/// <summary>
		/// Draw Ebony Lines
		/// </summary>
		/// <param name="g"></param>
		/// <param name="off"></param>
		public static void RenderGridE(Graphics g, int off)
		{
			using (SolidBrush b = new SolidBrush(Color.FromArgb(48, Color.DodgerBlue)))
				for (float i = ui.Rect.Top; i <= ui.Rect.Bottom; i += ui.RowHeight) {
				if (CheckYEbony(i, off, 12)) g.FillRectangle(b, 0, i, ui.Rect.Width, ui.NodeSize.Height);
			}
		}
	}
}
