/*
 * Created by SharpDevelop.
 * User: oio
 * Date: 2/2/2013
 * Time: 7:20 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace modest100.Forms
{
	/// <summary>
	/// Description of VolumeSlider.
	/// </summary>
	public partial class VolumeSlider : NAudio_VolumeSlider
	{
		public VolumeSlider()
		{
			InitializeComponent();
			DoubleBuffered = true;
		}
		protected override void OnPaint(PaintEventArgs pe)
		{
//			base.OnPaint(pe);
            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
//            if (this.BackgroundImage!=null) 
//            {
            	pe.Graphics.Clear(this.BackColor);
//            } else {
//            }
            pe.Graphics.DrawRectangle(Pens.Black, 0, 0, this.Width - 1, this.Height - 1);
            float db = 20 * (float)Math.Log10(Volume);
            float percent = 1 - (db / MinDb);

            pe.Graphics.FillRectangle(Brushes.LightGreen, 1, 1, (int)((this.Width - 2) * percent), this.Height - 2);
            string dbValue = String.Format("{0:F2} dB", db);
            /*if(Double.IsNegativeInfinity(db))
            {
                dbValue = "-\x221e db"; // -8 dB
            }*/

            pe.Graphics.DrawString(dbValue, this.Font,
                Brushes.Black, this.ClientRectangle, format);
            // Calling the base class OnPaint
            //base.OnPaint(pe);
		}
	}
}
