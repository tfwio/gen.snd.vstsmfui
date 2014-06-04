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
using DspAudio.Midi;
//using modēst100.Forms;

namespace modest100.Forms
{
	public class GraphicsPens : IDisposable
	{
		void IDisposable.Dispose()
		{
			if (GridPen!=null) GridPen.Dispose();
			if (GridRowMid!=null) GridRowMid.Dispose();
			if (GridRowHeavy!=null) GridRowHeavy.Dispose();
			if (GridBar!=null) GridBar.Dispose();
			if (GridRowDiv!=null) GridRowDiv.Dispose();
			if (SemiBlack!=null) SemiBlack.Dispose();
			if (SemiBlackBrush!=null) SemiBlackBrush.Dispose();
			if (AnotherBrush!=null) AnotherBrush.Dispose();
		}
		public Pen GridPen = new Pen(Color.Silver, 1){ Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round };
		public Pen GridRowMid = new Pen(Color.FromArgb(127, Color.Red), 1) { Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round};
		public Pen GridRowHeavy=new Pen(Color.Gray, 1){Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round};
		public Pen GridBar=new Pen(Color.FromArgb(0,0,0), 1){Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round};
		public Pen GridRowDiv=new Pen(Color.FromArgb(127,0, 127, 255), 1){Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round};
		public Pen SemiBlack=new Pen(Color.FromArgb(200, Color.Black), 1){Alignment=PenAlignment.Left,StartCap=LineCap.Round,EndCap=LineCap.Round};
		public Brush SemiBlackBrush = new SolidBrush(Color.FromArgb(200, Color.Black));
		public Brush AnotherBrush = new SolidBrush(Color.FromArgb(48, Color.DodgerBlue));
		
	}
}
