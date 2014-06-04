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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using DspAudio;
using DspAudio.Forms;
using DspAudio.Midi;
using DspAudio.Vst;
using DspAudio.Vst.Forms;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;
using NAudio.Wave;

// Gram Hancock.com: legend of spinx , arc of the covenent, quest for civilization, underworld?
// lost kingdoms of the ice age
namespace modest100.Forms
{
	public class PluginManagerComponent : Component
	{
		
		public event EventHandler ActivePluginReset;
		
		public PluginManagerComponent()
		{
			InitializeComponent();
		}

		void InitializeComponent()
		{
			
		}

		#region Plugin
		
		#if no
		
		void Event_PluginBrowse(object sender, EventArgs e)
		{
			if (PluginManager.ActivePlugin!=null) OpenFileDlg.FileName = PluginManager.ActivePlugin.PluginPath;
			if (OpenFileDlg.ShowDialog(this) == DialogResult.OK) Action_PluginAdd(OpenFileDlg.FileName);;
		}
		void Action_PluginAdd(string path) { PluginManager.AddPlugin(path); }
		
		void Event_PluginViewInfo(object sender, EventArgs e)
		{
			PluginForm dlg = new PluginForm();
			dlg.PluginContext = PluginManager.ActivePlugin;
			dlg.ShowDialog(this);
		}
		
		void Event_PluginRemoveSelected(object sender, EventArgs e) { PluginManager.RemovePlugin(PluginManager.ActivePlugin); }
		
		void Event_PluginReload(object sender, EventArgs e)
		{
			PluginManager.ReloadActivePlugin();
		}
		
		#endif
		
		#endregion

	}
}
