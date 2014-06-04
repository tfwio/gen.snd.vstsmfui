/*
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 */
 #define MODE1
using System;
using System.Collections.Generic;
using System.Internals;
using System.Windows.Forms;

using modest100.Forms;
using modest100.Internals;

namespace modest100
{

	class app
	{
		static List<MasterViewContainer> ViewCollection;
		/// 1. How are we to handle argument usage if we have no arguments?
		/// 2. We could very well provide changes to the timing configuration here.
		/// 3. Provide external database controls possably in an external thread.
		///    a. Initialize SQLite database if none exists.  For this we will
		///       need a initial table script (sql)
		///    b. 
		[STAThread]
		public static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			
			// Customize Time Configuration
			DspAudio.TimeConfiguration.Instance.Latency = 100;
			DspAudio.TimeConfiguration.Instance.Rate = 48000;
			ViewCollection = ViewPoint.EnumerateViewTypes<MasterViewContainer>(System.Reflection.Assembly.GetExecutingAssembly());
			
			List<string> views = new List<string>();
			foreach (IViewPoint view in ViewCollection)
				views.Add(view.Title);
			
			MessageBox.Show(string.Join("\n",views.ToArray()));
			Application.Run( new ModestForm(ViewCollection) );
//			try { Application.Run( container.GetExportedValue<ModestForm>() ); }
//			catch (Exception ex) {
//				if ( MessageBox.Show(string.Format("{0}\n---------\n{1}",ex.Message,ex.StackTrace), "OK to Exit, Cancel to throw Exception.", MessageBoxButtons.OKCancel,MessageBoxIcon.Error) == DialogResult.OK ) throw ex;
//				else return;
//			}
		}
	}
}
