/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using DspAudio;
using DspAudio.Vst;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;
using NAudio.Wave;

// Gram Hancock.com: legend of spinx , arc of the covenent, quest for civilization, underworld?
// lost kingdoms of the ice age
namespace modest100.Forms
{
	static class VstListView
	{
		static string group_effects = "Effects";
		static string group_instruments = "Instruments";
		
		/// <summary>This method is a response to VstPluginManager.PluginListRefreshed</summary>
		static public void ItemsRefresh(ListView list, VstPluginManager PluginManager)
		{
			list.Items.Clear();
			list.Groups.Clear();
			list.ShowGroups = true;
			
			list.Groups.Add(group_effects,group_effects);
			list.Groups.Add(group_instruments,group_instruments);
			
			foreach (VstPlugin ctx in PluginManager.VstEffects)
			{
				ListViewItem lvItem = new ListViewItem(ctx.PluginCommandStub.GetEffectName());
				lvItem.Group = list.Groups[group_effects];
				lvItem.SubItems.Add(ctx.PluginCommandStub.GetProductString());
				lvItem.SubItems.Add(ctx.PluginCommandStub.GetVendorString());
				lvItem.SubItems.Add(IntVersion.GetString(ctx.PluginCommandStub.GetVendorVersion()));
				lvItem.SubItems.Add(ctx.Find<string>("PluginPath"));
				lvItem.Tag = ctx;
				list.Items.Add(lvItem);
			}
			foreach (VstPlugin ctx in PluginManager.VstInstruments)
			{
				ListViewItem lvItem = new ListViewItem(ctx.PluginCommandStub.GetEffectName());
				lvItem.Group = list.Groups[group_instruments];
				lvItem.SubItems.Add(ctx.PluginCommandStub.GetProductString());
				lvItem.SubItems.Add(ctx.PluginCommandStub.GetVendorString());
				lvItem.SubItems.Add(IntVersion.GetString(ctx.PluginCommandStub.GetVendorVersion()));
				lvItem.SubItems.Add(ctx.Find<string>("PluginPath"));
				lvItem.Tag = ctx;
				list.Items.Add(lvItem);
			}
		}
		
		/// <summary>Action: ListView.ItemSelectionChangd (</summary>
		static public void HandlePluginSelected(ListView list, VstPluginManager PluginManager)
		{
			if (list.SelectedItems.Count > 0) PluginManager.ActivePlugin = list.SelectedItems[0].Tag as VstPlugin;
	//			else PluginManager.ActivePlugin = null;
		}
		
		/// <summary>Handles ActivePluginReset event</summary>
		static public void HandlePluginReload(ListView list, VstPluginManager PluginManager)
		{
			if ( list.SelectedItems.Count > 0 ) list.SelectedItems[0].Tag = PluginManager.ActivePlugin;
		}
	}
}
