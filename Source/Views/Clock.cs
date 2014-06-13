/*
 * Created by SharpDevelop.
 * User: oIo
 * Date: 2/25/2011
 * Time: 2:09 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace modest100.Forms
{
	/// Description of Clock.
	public partial class Clock : Form
	{
		DoublePoint AutoRadius
		{
			get {
				return new DoublePoint(ClientSize.Width > ClientSize.Height ? ClientSize.Width : ClientSize.Height);
			}
		}

		public Clock()
		{
			InitializeComponent();
			Timer tim = new Timer();
			tim.Tick += new EventHandler(FormTickHandler);
		}
		void FormTickHandler(object o, EventArgs e)
		{
			
		}
	}
}
