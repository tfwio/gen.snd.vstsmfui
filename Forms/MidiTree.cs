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
	static class MidiTree
	{
		static TreeNode NodeMidi,NodeVst,NodeVstI,NodeVstE;
		static public void InitializeTreeNodes(TreeView tree, IMidiParserUI ui)
		{
			WindowsInterop.WindowsTheme.HandleTheme(tree,true);
			tree.Nodes.Clear();
			NodeMidi=new TreeNode("MIDI");
			NodeVst=new TreeNode("VST");
			NodeVstI=new TreeNode("Instrument");
			NodeVstE=new TreeNode("Effect");
			
			tree.Nodes.Add(NodeMidi);
			tree.Nodes.Add(NodeVst);
			NodeVst.Nodes.Add(NodeVstE);
			NodeVst.Nodes.Add(NodeVstI);
		}
		static public void TracksToTreeView(IMidiParserUI ui)
		{
			NodeMidi.Nodes.Clear();
			if (ui.MidiParser==null) return;
			else if (ui.MidiParser.SmfFileHandle==null) return;
			else if (ui.MidiParser.SmfFileHandle.NumberOfTracks==0) return;
			for (int i = 0; i < ui.MidiParser.SmfFileHandle.NumberOfTracks; i++)
			{
				TreeNode tn = new TreeNode(string.Format("{0}",/*Strings.Filter_MidiTrack*/ i )); //Event_MidiChangeTrack_MenuItemSelected
				tn.Tag = i;
				NodeMidi.Nodes.Add( tn );
			}
		}
		
		/// <summary>Action: ListView.ItemSelectionChangd (</summary>
		static public void Reload(TreeView tree, VstPluginManager PluginManager)
		{
//			PluginManager.ReloadActivePlugin();
		}
		static void ActivePluginChanged(TreeView tree, VstPluginManager PluginManager)
		{
			
		}
		/// <summary>This method is a response to VstPluginManager.PluginListRefreshed</summary>
		static public void ItemsRefresh(TreeView tree, VstPluginManager PluginManager)
		{
			NodeVstI.Nodes.Clear();
			foreach (VstPlugin ctx in PluginManager.VstInstruments)
			{
				TreeNode node = NodeVstI.Nodes.Add(ctx.Title);
				node.Tag = ctx;
				node.ToolTipText = string.Format("{0}\nby {1}",ctx.Title,ctx.Vendor);
			}
			
			NodeVstE.Nodes.Clear();
			foreach (VstPlugin ctx in PluginManager.VstEffects)
			{
				TreeNode node = NodeVstE.Nodes.Add(ctx.Title);
				node.Tag = ctx;
				node.ToolTipText = string.Format("{0}\nby {1}",ctx.Title,ctx.Vendor);
			}
		}
		
	}

}
