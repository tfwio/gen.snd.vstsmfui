#region Using
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
using System.Internals;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DspAudio;
using DspAudio.Forms;
using DspAudio.Midi;
using DspAudio.Vst;
using DspAudio.Vst.Forms;
using DspAudio.Vst.Module;
using DspAudio.Vst.Xml;
using GenericWAV.Views;
using Jacobi.Vst.Core;
using modest100.Internals;
using NAudio.Wave;

// Gram Hancock.com: legend of spinx , arc of the covenent, quest for civilization, underworld?
// lost kingdoms of the ice age
#endregion
namespace modest100.Forms
{
	/// <summary>
	/// cater directly with interfaces as no control should generally cross interface boundary.
	/// </summary>
	public class ModestForm : Form, IMidiParserUI
	{
		
		#region ...
		ContextMenuStrip cms = new ContextMenuStrip();
		System.ComponentModel.ComponentCollection Children;
		IList<MidiControlBase> ChildrenControls = new List<MidiControlBase>();
		#endregion
		
		#region Timing, Transport
		SampleClock clock = new SampleClock();
		
		#region Action_PlayerUpDown2Ppq
		void Action_PlayerUpDown2Ppq() { VstContainer.VstPlayer.Settings.Division = Convert.ToInt32(numPpq.Value); }
		void Event_PlayerUpDown2Ppq(object sender, EventArgs e) { Action_PlayerUpDown2Ppq(); }
		#endregion
		#region Action_PlayerPpq2UpDown
		void Action_PlayerPpq2UpDown() { numPpq.Value = Convert.ToDecimal(VstContainer.VstPlayer.Settings.Division); }
		void Event_PlayerPpq2UpDown(object sender, EventArgs e) { Action_PlayerPpq2UpDown(); }
		#endregion
		#region Action_Player_Tempo2UpDown controls
		void Action_Player_Tempo2UpDown() { numTempo.Value = Convert.ToDecimal(VstContainer.VstPlayer.Settings.Tempo); }
		void Action_Player_UpDown2Tempo() { VstContainer.VstPlayer.Settings.Tempo = Convert.ToDouble(numTempo.Value); }
		void Event_PlayerUpDown2Tempo(object sender, EventArgs e) { Action_Player_UpDown2Tempo(); }
		#endregion
		
		void Event_BufferCycle(object sender, NAudioVSTCycleEventArgs e)
		{
			SampleClock s2 = vstContainer.VstPlayer.SampleTime.Clone<SampleClock>();
			label2.Text = string.Format(
				Strings.Format_LabelTimeInfo_Main,
				s2.Tempo,
				s2.MeasureString,
				vstContainer.VstPlayer.CurrentSampleLength,
				vstContainer.VstPlayer.SampleOffset,
				s2.TimeString,
				s2.Frame,
				insname,
				efxname,
				s2.Pulses
			);
		}
		
		void Event_PlayerSetLoopBegin(object sender, EventArgs e)
		{
			if (sender==numBarStart) vstContainer.VstPlayer.Settings.BarStart = (double) numBarStart.Value;
			if (sender==numBarLen) vstContainer.VstPlayer.Settings.BarLength = (double) numBarLen.Value;
			vstContainer.VstPlayer.SampleOffset = vstContainer.VstPlayer.SampleOffset;
		}

		#endregion
		#region MIDI FILE event
		
		public event EventHandler ClearMidiTrack;
		protected virtual void OnClearMidiTrack()
		{
			if (ClearMidiTrack != null) ClearMidiTrack(this, EventArgs.Empty);
		}
		
		public event EventHandler GotMidiFile;
		protected virtual void OnGotMidiFile()
		{
			this.numPpq.Value   = VstContainer.VstPlayer.Settings.Division;
			this.numTempo.Value = Convert.ToDecimal(VstContainer.VstPlayer.Settings.Tempo);
			if (GotMidiFile != null) GotMidiFile(this, EventArgs.Empty);
		}
		
		#endregion
		#region MIDI TRACK changed
		
		int trackLen { get { return this.midiFile.SmfFileHandle[midiFile.SelectedTrackNumber].track.Length; } }
		int cycles = 0, cycle = 12;
		void ShowProgress(MidiMsgType t, int track, int offset, int imsg, byte bmsg, ulong ppq, int rse, bool isrse)
		{
			try { this.toolStripProgressBar1.Value = (int)(((double)offset / trackLen) * 100f); cycles = (cycles++ % cycle); } catch {}
		}
		void Event_ListBoxContextMenuItem(object sender, EventArgs args)
		{
			cms.Items.Clear();
			ToolStripMenuItem g = new ToolStripMenuItem("Generators");
			ToolStripMenuItem e = new ToolStripMenuItem("Effects");
			cms.Items.Add(g);
			cms.Items.Add(e);
			foreach (VstPlugin plugin in VstMidiEnumerator.GetInstruments(this))
			{
				ToolStripMenuItem item = new ToolStripMenuItem(plugin.Title,null,Event_AddChannelMapping);
				if (vstContainer.PluginManager.MasterPluginInstrument==plugin) item.CheckState = CheckState.Indeterminate;
				item.ToolTipText = System.IO.Path.GetFileName(plugin.PluginPath);
//				for (int i=0; i<plugin.PluginCommandStub.GetNumberOfMidiInputChannels(); i++) item.DropDownItems.Add(i.ToString());
				g.DropDownItems.Add(item);
			}
			foreach (VstPlugin plugin in VstMidiEnumerator.GetEffects(this))
			{
				ToolStripMenuItem item = new ToolStripMenuItem(plugin.Title,null,Event_AddChannelMapping);
				if (vstContainer.PluginManager.MasterPluginInstrument==plugin) item.CheckState = CheckState.Indeterminate;
				item.ToolTipText = System.IO.Path.GetFileName(plugin.PluginPath);
//				for (int i=0; i<plugin.PluginCommandStub.GetNumberOfMidiInputChannels(); i++) item.DropDownItems.Add(i.ToString());
				e.DropDownItems.Add(item);
			}
//			cms.ShowImageMargin = false;
			cms.Show(MousePosition,ToolStripDropDownDirection.BelowRight);
		}
		void Event_AddChannelMapping(object sender, EventArgs e)
		{
			MessageBox.Show(string.Format(sender.ToString()));
		}
		void Event_MidiActiveTrackChanged_ListBoxItemSelected(object o, EventArgs e)
		{
			#if DEBUG
			StartLoad();
			#else
			KillThread();
			thread = new System.Threading.Thread(StartLoad);
			thread.Priority = System.Threading.ThreadPriority.Highest;
			thread.Start();
			#endif
			this.toolStripProgressBar1.Value = 0;
			this.toolStripProgressBar1.Maximum = 0;
			this.toolStripProgressBar1.Enabled = false;
		}
		void TracksToListBoxContext()
		{
			listBoxContextMenuStrip.Items.Clear();
			List<int> channels = new List<int>();
			foreach(KeyValuePair<int,string> track in VstMidiEnumerator.GetMidiTrackNamesByIndex(midiFile))
			{
				channels.Clear();
				ToolStripMenuItem tn = new ToolStripMenuItem(track.Value);
				tn.Tag = track.Key;
				listBoxContextMenuStrip.Items.Add( tn );
				
				
				foreach (MidiMessage i in VstMidiEnumerator.MidiTrackDistinctChannels(track.Key,midiFile))
					if (i is MidiChannelMessage) channels.Add(i.ChannelBit);
				foreach (int i in channels) {
					tn.DropDownItems.Add(i.ToString(),null,Event_ListBoxContextMenuItem)
						.Tag = new KeyValuePair<int,int>(track.Key,i);
				}
				if (channels.Count > 0) tn.DropDownItems.Insert(0,new ToolStripSeparator());
				if (channels.Count > 0)
					// all channels node
					// only added to tracks that have channels
				{
					ToolStripItem another = new ToolStripMenuItem("All Channels",null,Event_ListBoxContextMenuItem);
					another.Tag = new KeyValuePair<int,int>(track.Key,-1);
					tn.DropDownItems.Insert(0,another);
				}
			}
			channels.Clear();
			channels = null;
		}
		
		#endregion
		
		#region FILE MIDI
		
		public void Action_MidiFileOpen()
		{
			if (midiFile!=null) {
				midiFile.Dispose();
				midiFile = null;
			}
			MidiFileDialog.Filter = Strings.FileFilter_MidiFile;
			Text = Strings.Dialog_Title_0;
			
			if (MidiFileDialog.ShowDialog() == DialogResult.OK)
				if (File.Exists(MidiFileDialog.FileName))
					Action_MidiFileOpen(MidiFileDialog.FileName,0);
		}
		
		void Action_MidiFileOpen_QuietAppend(string filename, int trackNo)
		{
			Event_MidiClearMemory(null,EventArgs.Empty);
			
			if (string.IsNullOrEmpty(filename)) return;
			if (!System.IO.File.Exists(filename)) {
				MessageBox.Show(filename,"Error loading file.");
				return;
			}
			Text=string.Format(Strings.Dialog_Title_1,Path.GetFileNameWithoutExtension(filename));
			
			midiFile = new MidiReader( filename );
			midiFile.SelectedTrackNumber = trackNo;
			
			midiFile.ClearView -= Event_MidiClearMemory;
			midiFile.FileLoaded -= Event_MidiFileLoaded;
			midiFile.TrackChanged -= Event_MidiActiveTrackChanged_ListBoxItemSelected;
			
			midiFile.ClearView += Event_MidiClearMemory;
			midiFile.FileLoaded += Event_MidiFileLoaded;
			midiFile.TrackChanged += Event_MidiActiveTrackChanged_ListBoxItemSelected;
			#if DEBUG
			midiFile.MessageHandlers.Add(ShowProgress);
			#endif
			midiFile.Read();
			VstContainer.VstPlayer.Settings.FromMidi(midiFile);
			//foreach (Action a in afteropen) a();
			
			OnGotMidiFile();
		}
		
		public void Action_MidiFileOpen(string filename, int trackNo)
		{
			Event_MidiClearMemory(null,EventArgs.Empty);
			
			if (string.IsNullOrEmpty(filename)) return;
			if (!System.IO.File.Exists(filename)) {
				MessageBox.Show(filename,"Error loading file.");
				return;
			}
			
			Text=string.Format(Strings.Dialog_Title_1,Path.GetFileNameWithoutExtension(filename));
			
			midiFile = new MidiReader( filename );
			midiFile.SelectedTrackNumber = trackNo;
			
			midiFile.ClearView -= Event_MidiClearMemory;
			midiFile.FileLoaded -= Event_MidiFileLoaded;
			midiFile.TrackChanged -= Event_MidiActiveTrackChanged_ListBoxItemSelected;
			
			midiFile.ClearView += Event_MidiClearMemory;
			midiFile.FileLoaded += Event_MidiFileLoaded;
			midiFile.TrackChanged += Event_MidiActiveTrackChanged_ListBoxItemSelected;
			#if DEBUG
			midiFile.MessageHandlers.Add(ShowProgress);
			#endif
			midiFile.Read();
			VstContainer.VstPlayer.Settings.FromMidi(midiFile);
			//foreach (Action a in afteropen) a();
			
			OnGotMidiFile();
			
		}
		
		void Event_MidiFileOpen(object sender, EventArgs e)
		{
			Action_MidiFileOpen();
//			this.midiPianoView1.ParserUI = this;
		}
		void Event_MidiFileLoaded(object sender, EventArgs e)
		{
			TracksToToolStripMenu();
			TracksToListBox();
			TracksToListBoxContext();
			MidiTree.TracksToTreeView(this);
			LoadTracks = midiFile.TrackSelectAction;
		}
		
		#region MIDI ListBox (Event_MidiChangeTrack_MenuItemSelected,Event_FormToggleMidiListBox)
		void Event_MidiChangeTrack_MenuItemSelected(object sender, EventArgs e)
		{
			cycles = 0;
			cycle = trackLen > 230 ? 1 : (int)(trackLen * 0.07f);
			this.toolStripProgressBar1.Maximum = 100;
			this.toolStripProgressBar1.Enabled = true;
			
			OnClearMidiTrack();
			if (midiFile == null) return;
			if (sender is ToolStripMenuItem) midiFile.SelectedTrackNumber = (int)(sender as ToolStripMenuItem).Tag;
			else if (sender is ListBox && listBox1.SelectedIndex > -1) midiFile.SelectedTrackNumber = listBox1.SelectedIndex;
		}
		void Event_FormToggleMidiListBox(object sender, EventArgs e)
		{
			this.splitContainer1.Panel1Collapsed =! this.splitContainer1.Panel1Collapsed;
		}
		#endregion
		#region MIDI ListBox (TracksToListBox,TracksToToolStripMenu)
		void TracksToToolStripMenu()
		{
			foreach(KeyValuePair<int,string> track in VstMidiEnumerator.GetMidiTrackNamesByIndex(midiFile))
			{
				ToolStripMenuItem tn = new ToolStripMenuItem(track.Value,null,Event_MidiChangeTrack_MenuItemSelected);
				tn.Tag = track.Key;
				btn_pick_track.DropDownItems.Add( tn );
			}
		}
		void TracksToListBox()
		{
			listBox1.DataSource = btn_pick_track.DropDownItems;
			listBox1.DisplayMember = "Text";
		}
		#endregion
		#region MIDI (Clear) Memory
		/// <remarks>Action_ClearMemory: Reset (ListBox) listBox1 and (ToolStripMenuItem) btn_pick_track bound to Midi</remarks>
		void Event_MidiClearMemory(object sender, EventArgs e) { Action_ClearMemory(); }
		
		/// <summary>Clear Midi (Parsed) Memory, Stop Playback, Unload Midi Track Lists, Etc,,</summary>
		/// <remarks>Reset (ListBox) listBox1 and (ToolStripMenuItem) btn_pick_track bound to Midi</remarks>
		void Action_ClearMemory()
		{
			btn_pick_track.DropDownItems.Clear();
			listBox1.DataSource = null;
		}
		
		#endregion
		#endregion
		#region Thread Helpers for MIDI Track Loader
		
		Func<string> LoadTracks = null;
		System.Threading.Thread thread;
		
		void StartLoad()
		{
//			LoaderWorker = new BackgroundWorker();
			label1.Text = LoadTracks(  );
		}
		void KillThread()
		{
			if (thread!=null)
			{
				if (thread.IsAlive) try { thread.Abort(); } catch {  }
				thread = null;
			}
		}
		
		#endregion
		
		#region VST
		
		#region VST HasInstrument, HasEffect

		bool hasins { get { return vstContainer.PluginManager.MasterPluginInstrument!=null; } }
		VstPlugin ins { get { return vstContainer.PluginManager.MasterPluginInstrument; } }
		string insname { get { return hasins ? ins.Title : "?"; } }
		
		bool hasefx { get { return vstContainer.PluginManager.MasterPluginEffect!=null; } }
		VstPlugin efx { get { return vstContainer.PluginManager.MasterPluginEffect; } }
		string efxname { get { return hasefx ? efx.Title : "?"; } }

		#endregion
		#region VST IContainer
		
		public INaudioVstContainer VstContainer {
			get { return vstContainer; }
		} NAudioVstContainer vstContainer;
		OpenFileDialog MidiFileDialog = new OpenFileDialog();
		public IMidiParser MidiParser {
			get { return midiFile; }
		} MidiReader midiFile;

		#endregion
		
		#region VST Plugin
		
		public event EventHandler ActivePluginReset;
		
		void Action_PluginAdd(string[] path) { PluginManager.AddPlugin(path); }
		
		void Event_PluginBrowse(object sender, EventArgs e)
		{
			if (PluginManager.ActivePlugin!=null) OpenFileDlg.FileName = PluginManager.ActivePlugin.PluginPath;
			OpenFileDlg.Multiselect = true;
			if (OpenFileDlg.ShowDialog(this) == DialogResult.OK) PluginManager.AddPlugin(OpenFileDlg.FileNames);
		}
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
		
		#endregion
		
		#region VST PluginManager Loaders/Unloaders
		VstPluginManager PluginManager { get { return VstContainer.PluginManager; } }
		
		/// Dont bother with this guy.
		void Append(object sender, EventArgs e) { PluginManager.Append(); }
		void Event_ConfigUnload(object sender, EventArgs e) { PluginManager.ReleaseAllPlugins(true); }
		void Event_ConfigOpenDefault(object sender, EventArgs e) { PluginManager.Read(); }
		void Event_ConfigOpen(object sender, EventArgs e) { if (ofd_config.ShowDialog() == DialogResult.OK) PluginManager.Read(ofd_config.FileName); }
		void Event_ConfigSaveDefault(object sender, EventArgs e) { PluginManager.Write(); }
		void Event_ConfigSaveAs(object sender, EventArgs e) { if (sfd_config.ShowDialog() == DialogResult.OK) PluginManager.Write(sfd_config.FileName); }

		#endregion
		#region VST TREE Plugin Selection (Tree DoubleClick)
		
		void Event__PluginSelected(object sender, TreeNodeMouseClickEventArgs e) {
			if (!(e.Node.Tag is VstPlugin)) return;
			if (e.Node.Tag == null) return;
			PluginManager.ActivePlugin = tree.SelectedNode.Tag as VstPlugin;
//			VstListView.HandlePluginSelected(PluginListVw,PluginManager);
		}
		void Event__PluginReload(object sender, EventArgs e) { MidiTree.Reload(this.tree,PluginManager); }
		void Event__PluginListReset(object sender, EventArgs e) { MidiTree.ItemsRefresh(this.tree, PluginManager); }
		
		#endregion

		#endregion
		#region AUDIO
		#region AUDIO NAudio
		private void InitialiseDirectSoundControls()
		{
			comboBoxDirectSound.DataSource = NAudio.Wave.DirectSoundOut.Devices;
			comboBoxDirectSound.DisplayMember = "Description";
			comboBoxDirectSound.ValueMember = "Guid";
			this.comboBoxDirectSound.SelectedIndex = 0;
		}
		void ComboBoxDirectSoundSelectedIndexChanged(object sender, EventArgs e)
		{
			VstContainer.VstPlayer.DriverId = (comboBoxDirectSound.SelectedItem as DirectSoundDeviceInfo).Guid;
		}
		#endregion
		#region AUDIO Playback
		
		void HandlePlayerStart(object sender, EventArgs e)
		{
			this.volumeSlider1.VolumeChanged -= Event_VolumeSliderValueChange;
			this.volumeSlider1.VolumeChanged += Event_VolumeSliderValueChange;
		}
		void HandlePlayerStop(object sender, EventArgs e)
		{
			this.volumeSlider1.VolumeChanged -= Event_VolumeSliderValueChange;
		}
		
		void Event_VolumeSliderValueChange(object sender, EventArgs e)
		{
			this.VstContainer.VstPlayer.Volume = volumeSlider1.Volume;
		}
		
		#endregion
		#region AUDIO Playback (NAudio Player and Editor)
		
		void Action_EditorShow() { if (VstContainer.PluginManager.ActivePlugin!=null) vstContainer.PluginManager.ActivePlugin.EditorCreate(); }
		void Event_EditorShow(object sender, EventArgs e) { Action_EditorShow(); }
		void Event_EditorShowOnPlay(object sender, EventArgs e) { btnShowEditor.Checked = !btnShowEditor.Checked; }
		
		void Action_PlayerDestroy()
		{
//			lock (threadlock)
			{
				if (VstContainer.IsPlayerStopped)
				{
					VstContainer.VstPlayer.ResetBufferToZero();
//					HandleProcessed(null,null);
					return;
				}
				VstContainer.PlayerDestroy();
			}
			btnPlay.Checked = true;
//			HandleProcessed(null,null);
		}
		void Action_PlayerPlay()
		{
			if (PluginManager.ActivePlugin==null) return;
			VstContainer.PlayerPlay(/*PluginManager.ActivePlugin*/);
			btnPlay.Checked = true;
			if (btnShowEditor.Checked)
				Action_EditorShow();
		}
		void Event_PlayerPlay(object sender, EventArgs e)
		{
			if (btnPlay.Checked) {
				Action_PlayerDestroy();
				btnPlay.Checked = false;
				toolStripDropDownButton1.Image = Icons.control;
			} else {
				Action_PlayerPlay();
				Event_VolumeSliderValueChange(this,EventArgs.Empty);
				btnPlay.Checked = true;
				toolStripDropDownButton1.Image = Icons.control_stop_square;
			}
		}
		void Action_PlayerPause()
		{
			if (PluginManager.ActivePlugin==null) return;
			
			vstContainer.VstPlayer.Pause();
			btnPause.Checked = vstContainer.VstPlayer.IsPaused;
		}
		
		void Event_PlayerPause(object sender, EventArgs e) { Action_PlayerPause(); }
		void Event_PlayerStop(object sender, EventArgs e) { Action_PlayerDestroy(); }
		
		static readonly byte[] Bmpty = new byte[0];
		
		#endregion
		#endregion
		#region Configuration VST/MIDI/UI
		#region ConfigurationFile Runtime
		
		public MidiSmfFile localfile = new MidiSmfFile();
		
		void Event_LoadRuntime(object sender, EventArgs e)
		{
			Action_RuntimeLoad();
		}
		void Event_SaveRuntime(object sender, EventArgs e)
		{
			Action_RuntimeSave();
		}
		
		void Action_RuntimeSave()
		{
			SaveRuntime(
				this,
				new Loop(){
					Begin=Convert.ToDouble(numBarStart.Value).FloorMinimum(0),
					Length = Convert.ToDouble(numBarLen.Value).FloorMinimum(0)
				},
				volumeSlider1.Volume
			);
		}
		
		static void SaveRuntime(IMidiParserUI modest, Loop loop, float volume)
		{
			
			MidiSmfFile newfile = new MidiSmfFile();
			newfile.Settings = new MidiSmfFileSettings();
			newfile.Settings.ConfigurationFile = modest.VstContainer.PluginManager.CurrentConfigurationFile;
			
			if (modest.MidiParser != null) {
				newfile.Settings.SelectedMidiTrack = modest.MidiParser.SelectedTrackNumber;
				newfile.Settings.MidiFileName = modest.MidiParser.MidiFileName;
			}
			
			newfile.Settings.MasterVolume = volume;
			
			// Modules
			// ---------------------------------------------------
			newfile.Settings.Generators = new List<VstModule>();
			
			if (modest.VstContainer.PluginManager.MasterPluginInstrument != null)
			{
				newfile.Settings.Generators.Add(
					new VstModule(modest.VstContainer.PluginManager.MasterPluginInstrument)
				);
			}
			// Generators
			// ---------------------------------------------------
			newfile.Settings.Modules = new List<VstModule>();
			
			if (modest.VstContainer.PluginManager.MasterPluginEffect != null)
			{
				newfile.Settings.Modules.Add(
					new VstModule(modest.VstContainer.PluginManager.MasterPluginEffect)
				);
			}
			
			// OLD MasterPluginInstrument
			// ---------------------------------------------------
			if (modest.VstContainer.PluginManager.MasterPluginInstrument != null)
			{
				newfile.Settings.Plugin = new Plugin(
					modest.VstContainer
					.PluginManager.MasterPluginInstrument
				);
				newfile.Settings.Plugin.Path =
					modest.VstContainer
					.PluginManager.MasterPluginInstrument.PluginPath;
			}
			
			// 
			// Bar
			// ---------------------------------------------------
			newfile.Settings.Bar = loop;
			// Automation
			// ---------------------------------------------------
			PulseValue bar = new PulseValue(modest.MidiParser.SmfFileHandle.Division*4*4,DeltaType.Ticks);
			PulseValue measure = new PulseValue(bar.Value * 4,DeltaType.Ticks);
			
			newfile.Settings.AutoParams = new List<AutomationParam>();
			// one measure = 2:1:0
			// = 4bars = 4quarters*4 = (4*div)*4
			// incrementing per quarter note
			//    = 4 * 4 * 4 * value
			// or = value / 4 / 4 / 4
			// value is 0 - 1 (zero inclusive 64
			double m = 0.015625;
			int counter = 0;
			for (
				double i = 0;
				i < measure.Value;
				i+=modest.MidiParser.SmfFileHandle.Division
			)
			{
				double d = i / modest.MidiParser.SmfFileHandle.Division;
				double v = counter * m;
				newfile.Settings.AutoParams.Add(AutomationParam.Create(i,DeltaType.Ticks,0,v.ToSingle()));
				counter++;
			}
			
			// (string) ActiveInstrument
			// ---------------------------------------------------
			if (modest.VstContainer.PluginManager.MasterPluginInstrument!=null)
				newfile.Settings.ActiveInstrument = modest
					.VstContainer
					.PluginManager.MasterPluginInstrument.Title;
			// (string) ActiveEffect
			// ---------------------------------------------------
			if (modest.VstContainer.PluginManager.MasterPluginEffect!=null)
				newfile.Settings.ActiveEffect = modest.VstContainer
					.PluginManager.MasterPluginEffect.Title;
			newfile.Save(newfile);
			// maybe we should have checked for errors.
			modest.VstContainer.RuntimeSettings = newfile;
		}
		static public MidiSmfFile LoadRuntime(IMidiParserUI modest)
		{
			
			MidiSmfFile newfile = MidiSmfFile.Load();
			
			if (newfile==null)
			{
				MessageBox.Show("Couldn't create config.");
				return null;
			}
			
			if (modest.VstContainer.PluginManager.CurrentConfigurationFile==null||modest.VstContainer.PluginManager.CurrentConfigurationFile!=newfile.Settings.ConfigurationFile)
				modest.VstContainer.PluginManager.CurrentConfigurationFile = newfile.Settings.ConfigurationFile;
			
			modest.Action_MidiFileOpen(newfile.Settings.MidiFileName, newfile.Settings.SelectedMidiTrack);
			
			foreach (VstPlugin p in modest.VstContainer.PluginManager.Plugins)
			{
				{
					// active effect
					if (!string.IsNullOrEmpty(newfile.Settings.ActiveEffect))
						if (p.Title==newfile.Settings.ActiveEffect)
							modest.VstContainer.PluginManager.ActivePlugin = modest.VstContainer.PluginManager.MasterPluginEffect = p;
					// active instrument
					if (!string.IsNullOrEmpty(newfile.Settings.ActiveInstrument))
						if (p.Title==newfile.Settings.ActiveInstrument)
							modest.VstContainer.PluginManager.ActivePlugin = modest.VstContainer.PluginManager.MasterPluginInstrument = p;
				}
			}
			modest.TrySetProgram(newfile,modest.VstContainer.PluginManager.MasterPluginInstrument);
			return newfile;
		}
		class ChannelAssigner
		{
			MidiChannelSet Inputs, Outputs;
			static ChannelAssigner Assigner(int midifileindex, VstPlugin module, MidiChannelSet inputs, MidiChannelSet outputs)
			{
				ChannelAssigner assigner = new ModestForm.ChannelAssigner();
				assigner.Inputs = inputs;
				assigner.Outputs = outputs;
				return assigner;
			}
		}
		
		void Action_RuntimeLoad()
		{
			vstContainer.RuntimeSettings = LoadRuntime(this);
			TrySetProgram(vstContainer.RuntimeSettings,PluginManager.MasterPluginInstrument);
			if (vstContainer.RuntimeSettings.Settings.Bar!=null)
			{
				numBarStart.Value = (decimal)vstContainer.RuntimeSettings.Settings.Bar.Begin;
				numBarLen.Value = (decimal)vstContainer.RuntimeSettings.Settings.Bar.Length;
			}
			volumeSlider1.Volume = vstContainer.RuntimeSettings.Settings.MasterVolume;
		}
		public void TrySetProgram(MidiSmfFile newfile, VstPlugin plugin)
		{
			try {
				plugin.PluginCommandStub.BeginSetProgram();
				if (plugin.ActiveProgram==null)
				{
					plugin.ActiveProgram = new VstCCPgm(PluginManager.MasterPluginInstrument,0);
				}
				plugin.PgmData = newfile.Settings.Plugin.ProgramDump;
				plugin.PluginCommandStub.EndSetProgram();
				
			} catch { MessageBox.Show("Error setting program",null,MessageBoxButtons.OK); }
		}
		#endregion
		#endregion
		
		#region Form Events
		void Event_FormQuit(object sender, EventArgs e)
		{
			Application.Exit();
		}
		#region File Dialogs
		OpenFileDialog ofd_config = new OpenFileDialog();
		SaveFileDialog sfd_config = new SaveFileDialog();
		#endregion
		#region Miscelaneous Forms/Dialogs/Tools
		// Tool Dialogs
		// ------------------
		#region BPM Calculator Dialog

		BpmCalculatorForm bcf = new BpmCalculatorForm();
		void CalculatorToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (bcf==null) bcf = new BpmCalculatorForm();
			bcf.ShowDialog(this);
		}
		
		#endregion
		#region MIDI Device Selection Dialog

		void Event_MidiShowDeviceSelectorDialog(object sender, EventArgs e)
		{
			DeviceSelector dc = new DeviceSelector();
			dc.ShowDialog(this);
		}

		#endregion
		#region Knob Image Tool
		
		KnobImageTool kit = new KnobImageTool();
		void ImageToolToolStripMenuItemClick(object sender, EventArgs e)
		{
			kit.ShowDialog(this);
		}
		
		#endregion

		#endregion
		#endregion
		#region Overrides
		
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (sc.IsVisible) sc.ActionDestroy();
		}
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);
		}

		#endregion
		#region .ctor (and friends)
		SplashFormController sc;
		public ModestForm(IList<MasterViewContainer> tasks)
		{
			sc = new SplashFormController(this,Icons._2012_09_04_csmidi,true);
			this.InitializeComponent();
			this.InitializeModestForm(tasks);
		}
		void InitializeModestForm(IList<MasterViewContainer> tasks)
		{
			ofd_config.Filter = sfd_config.Filter = Strings.FileFilter_VstConfig;
			this.InitialiseDirectSoundControls();
			// construct the master container
			this.vstContainer = new NAudioVstContainer(this);
			this.vstContainer.VstPlayer.BufferCycle += Event_BufferCycle;
			this.vstContainer.VstPlayer.DriverId  = (this.comboBoxDirectSound.SelectedItem as DirectSoundDeviceInfo).Guid;
			// initialize the views 
			this.InitializeEnumerable(tasks);
			MidiTree.InitializeTreeNodes(this.tree,this);
			
			this.numTempo.ValueChanged += Event_PlayerUpDown2Tempo;
			this.numPpq.ValueChanged += this.Event_PlayerUpDown2Ppq;
			this.vstContainer.PluginManager.PluginListRefreshed += Event__PluginListReset;
			// the following would be ListView.ItemSelectionChangedEventHandler
			// this.vstContainer.PluginManager.PluginListRefreshed += Event__Plugin;
			this.Event_ConfigOpenDefault(null,null);
			
			this.tree.NodeMouseDoubleClick += Event__PluginSelected;
			this.vstContainer.PluginManager.ActivePluginReset += Event__PluginReload;
			this.listBoxContextMenuStrip.Font = listBox1.Font;
		}
		void InitializeEnumerable(IEnumerable<MasterViewContainer> tasks)
		{
			foreach (MasterViewContainer view in tasks)
			{
				MidiControlBase control = view.GetView();
				control.SuspendLayout();
				this.ChildrenControls.Add(control);
				this.splitContainer1.Panel2.Controls.Add(control);
				control.Show();
				control.Dock = DockStyle.Fill;
				control.SetUI(this);
				control.BringToFront();
				control.ResumeLayout(true);
				ToolStripItem item = new ToolStripMenuItem( view.Title , null , ShowElement );
				item.Tag = control;
				viewToolStripMenuItem.DropDownItems.Add(item);
			}
			btn_pick_track.Image = Icons.midi_in;
//			btn_pick_track.Image = fam3.famfam_silky.cursor;
			#if !DEBUG
			toolStripProgressBar1.Visible = false;
			#endif
		}
		
		public void ShowElement(object sender, EventArgs e)
		{
			foreach (Control ctl in ChildrenControls)
			{
				ctl.Hide();
			}
			((sender as ToolStripMenuItem).Tag as Control).BringToFront();
			((sender as ToolStripMenuItem).Tag as Control).Show();
			((sender as ToolStripMenuItem).Tag as Control).Invalidate();
		}
		
		#endregion
		
		#region Design
		
		System.ComponentModel.IContainer components;
		
		protected override void Dispose(bool disposing)
		{
			if (sc!=null)
			{
				(sc as IDisposable).Dispose();
				sc = null;
			}
			if (!VstContainer.IsPlayerStopped) Action_PlayerDestroy();
			
			KillThread();
			if (PluginManager.ActivePlugin != null)
				if (PluginManager.ActivePlugin.HasEditorDialog)
					PluginManager.ActivePlugin.EditorDestroy();
			PluginManager.ReleaseAllPlugins();
			
			foreach (Control c in ChildrenControls) c.Dispose();
			ChildrenControls.Clear();
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModestForm));
			this.ill = new System.Windows.Forms.ImageList(this.components);
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
			this.label2 = new System.Windows.Forms.ToolStripStatusLabel();
			this.volumeSlider1 = new modest100.Forms.VolumeSlider();
			this.label3 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.numBarLen = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numBarStart = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.numPpq = new System.Windows.Forms.NumericUpDown();
			this.numTempo = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this.tree = new System.Windows.Forms.TreeView();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.listBoxContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadRuntimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveRuntimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
			this.loadMIDIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveMIDIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.recentMIDIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripSeparator();
			this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mIDIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.mIDIPendingConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripSeparator();
			this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.loadDefaultConfigurationFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.closeConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripSeparator();
			this.saveConfigurationAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveConfigurationAsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.recentConfigurationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.configToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.midiToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripSplitButton();
			this.btnPlay = new System.Windows.Forms.ToolStripMenuItem();
			this.btnPause = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
			this.showHideMidiTrackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.btnShowEditor = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.showEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showExtendedInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.viewPluginInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.manageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.browseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btn_pick_track = new System.Windows.Forms.ToolStripMenuItem();
			this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
			this.OpenFileDlg = new System.Windows.Forms.OpenFileDialog();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.comboBoxDirectSound = new System.Windows.Forms.ComboBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.menuStrip2 = new System.Windows.Forms.MenuStrip();
			this.aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.calculatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.deviceSelectorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.largerFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.smallerFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripSeparator();
			this.imageToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openRuntimeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openConfigurationToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.openVstToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.statusStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBarLen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numBarStart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPpq)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numTempo)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.menuStrip2.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// ill
			// 
			this.ill.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.ill.ImageSize = new System.Drawing.Size(16, 16);
			this.ill.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// statusStrip1
			// 
			this.statusStrip1.BackColor = System.Drawing.Color.Transparent;
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.toolStripProgressBar1,
									this.label2});
			this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.statusStrip1.Location = new System.Drawing.Point(0, 358);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(803, 22);
			this.statusStrip1.TabIndex = 17;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBar1
			// 
			this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.toolStripProgressBar1.Name = "toolStripProgressBar1";
			this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 16);
			this.toolStripProgressBar1.Step = 1;
			// 
			// label2
			// 
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(0, 17);
			// 
			// volumeSlider1
			// 
			this.volumeSlider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.volumeSlider1.ForeColor = System.Drawing.Color.Black;
			this.volumeSlider1.Location = new System.Drawing.Point(193, 3);
			this.volumeSlider1.Name = "volumeSlider1";
			this.volumeSlider1.Size = new System.Drawing.Size(111, 14);
			this.volumeSlider1.TabIndex = 20;
			this.volumeSlider1.VolumeChanged += new System.EventHandler(this.Event_VolumeSliderValueChange);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label3.ForeColor = System.Drawing.Color.Gray;
			this.label3.Location = new System.Drawing.Point(3, 3);
			this.label3.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(17, 9);
			this.label3.TabIndex = 18;
			this.label3.Text = "BPM";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label6.ForeColor = System.Drawing.Color.Gray;
			this.label6.Location = new System.Drawing.Point(405, 3);
			this.label6.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(17, 9);
			this.label6.TabIndex = 19;
			this.label6.Text = "LEN";
			// 
			// numBarLen
			// 
			this.numBarLen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numBarLen.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numBarLen.ForeColor = System.Drawing.Color.Silver;
			this.numBarLen.Location = new System.Drawing.Point(428, 3);
			this.numBarLen.Maximum = new decimal(new int[] {
									1000000000,
									0,
									0,
									0});
			this.numBarLen.Minimum = new decimal(new int[] {
									1,
									0,
									0,
									0});
			this.numBarLen.Name = "numBarLen";
			this.numBarLen.Size = new System.Drawing.Size(66, 16);
			this.numBarLen.TabIndex = 16;
			this.numBarLen.Value = new decimal(new int[] {
									32,
									0,
									0,
									0});
			this.numBarLen.ValueChanged += new System.EventHandler(this.Event_PlayerSetLoopBegin);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label5.ForeColor = System.Drawing.Color.Gray;
			this.label5.Location = new System.Drawing.Point(310, 3);
			this.label5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 9);
			this.label5.TabIndex = 19;
			this.label5.Text = "BAR";
			// 
			// numBarStart
			// 
			this.numBarStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numBarStart.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numBarStart.ForeColor = System.Drawing.Color.Silver;
			this.numBarStart.Location = new System.Drawing.Point(333, 3);
			this.numBarStart.Maximum = new decimal(new int[] {
									1000000000,
									0,
									0,
									0});
			this.numBarStart.Name = "numBarStart";
			this.numBarStart.Size = new System.Drawing.Size(66, 16);
			this.numBarStart.TabIndex = 16;
			this.numBarStart.ValueChanged += new System.EventHandler(this.Event_PlayerSetLoopBegin);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label4.ForeColor = System.Drawing.Color.Gray;
			this.label4.Location = new System.Drawing.Point(98, 3);
			this.label4.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(17, 9);
			this.label4.TabIndex = 19;
			this.label4.Text = "PPQ";
			// 
			// numPpq
			// 
			this.numPpq.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numPpq.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numPpq.ForeColor = System.Drawing.Color.Silver;
			this.numPpq.Increment = new decimal(new int[] {
									24,
									0,
									0,
									0});
			this.numPpq.Location = new System.Drawing.Point(121, 3);
			this.numPpq.Maximum = new decimal(new int[] {
									99999,
									0,
									0,
									0});
			this.numPpq.Minimum = new decimal(new int[] {
									8,
									0,
									0,
									0});
			this.numPpq.Name = "numPpq";
			this.numPpq.Size = new System.Drawing.Size(66, 16);
			this.numPpq.TabIndex = 16;
			this.numPpq.Value = new decimal(new int[] {
									480,
									0,
									0,
									0});
			this.numPpq.ValueChanged += new System.EventHandler(this.Event_PlayerUpDown2Ppq);
			// 
			// numTempo
			// 
			this.numTempo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numTempo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numTempo.ForeColor = System.Drawing.Color.Silver;
			this.numTempo.Location = new System.Drawing.Point(26, 3);
			this.numTempo.Maximum = new decimal(new int[] {
									800,
									0,
									0,
									0});
			this.numTempo.Minimum = new decimal(new int[] {
									10,
									0,
									0,
									0});
			this.numTempo.Name = "numTempo";
			this.numTempo.Size = new System.Drawing.Size(66, 16);
			this.numTempo.TabIndex = 17;
			this.numTempo.Value = new decimal(new int[] {
									120,
									0,
									0,
									0});
			this.numTempo.ValueChanged += new System.EventHandler(this.Event_PlayerUpDown2Tempo);
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.label1.ForeColor = System.Drawing.Color.Silver;
			this.label1.Location = new System.Drawing.Point(0, 302);
			this.label1.Margin = new System.Windows.Forms.Padding(0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(4);
			this.label1.Size = new System.Drawing.Size(615, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			this.label1.UseCompatibleTextRendering = true;
			// 
			// splitContainer1
			// 
			this.splitContainer1.BackColor = System.Drawing.Color.Transparent;
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 24);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.label1);
			this.splitContainer1.Size = new System.Drawing.Size(803, 334);
			this.splitContainer1.SplitterDistance = 184;
			this.splitContainer1.TabIndex = 18;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.tree);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.listBox1);
			this.splitContainer2.Size = new System.Drawing.Size(184, 334);
			this.splitContainer2.SplitterDistance = 166;
			this.splitContainer2.TabIndex = 0;
			// 
			// tree
			// 
			this.tree.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tree.FullRowSelect = true;
			this.tree.HideSelection = false;
			this.tree.HotTracking = true;
			this.tree.LineColor = System.Drawing.Color.Silver;
			this.tree.Location = new System.Drawing.Point(0, 0);
			this.tree.Name = "tree";
			this.tree.ShowNodeToolTips = true;
			this.tree.ShowRootLines = false;
			this.tree.Size = new System.Drawing.Size(184, 166);
			this.tree.TabIndex = 0;
			// 
			// listBox1
			// 
			this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listBox1.ContextMenuStrip = this.listBoxContextMenuStrip;
			this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBox1.FormattingEnabled = true;
			this.listBox1.IntegralHeight = false;
			this.listBox1.Location = new System.Drawing.Point(0, 0);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(184, 164);
			this.listBox1.TabIndex = 17;
			this.listBox1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Event_MidiChangeTrack_MenuItemSelected);
			// 
			// listBoxContextMenuStrip
			// 
			this.listBoxContextMenuStrip.Font = new System.Drawing.Font("Consolas", 7F);
			this.listBoxContextMenuStrip.Name = "listBoxContextMenuStrip";
			this.listBoxContextMenuStrip.ShowImageMargin = false;
			this.listBoxContextMenuStrip.Size = new System.Drawing.Size(36, 4);
			// 
			// menuStrip1
			// 
			this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.Font = new System.Drawing.Font("Consolas", 7F);
			this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2);
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.fileToolStripMenuItem,
									this.toolStripDropDownButton1,
									this.btn_pick_track,
									this.viewToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(0);
			this.menuStrip1.Size = new System.Drawing.Size(186, 24);
			this.menuStrip1.TabIndex = 21;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.loadRuntimeToolStripMenuItem,
									this.saveRuntimeToolStripMenuItem,
									this.toolStripMenuItem9,
									this.loadMIDIToolStripMenuItem,
									this.saveMIDIToolStripMenuItem,
									this.recentMIDIToolStripMenuItem,
									this.toolStripMenuItem7,
									this.exportToolStripMenuItem,
									this.toolStripMenuItem6,
									this.configurationToolStripMenuItem,
									this.recentConfigurationsToolStripMenuItem});
			this.fileToolStripMenuItem.Image = global::modest100.Icons.cassette;
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(28, 24);
			this.fileToolStripMenuItem.Text = "&File";
			// 
			// loadRuntimeToolStripMenuItem
			// 
			this.loadRuntimeToolStripMenuItem.Name = "loadRuntimeToolStripMenuItem";
			this.loadRuntimeToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.loadRuntimeToolStripMenuItem.Text = "Load Runtime";
			this.loadRuntimeToolStripMenuItem.Click += new System.EventHandler(this.Event_LoadRuntime);
			// 
			// saveRuntimeToolStripMenuItem
			// 
			this.saveRuntimeToolStripMenuItem.Name = "saveRuntimeToolStripMenuItem";
			this.saveRuntimeToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.saveRuntimeToolStripMenuItem.Text = "Save Runtime";
			this.saveRuntimeToolStripMenuItem.Click += new System.EventHandler(this.Event_SaveRuntime);
			// 
			// toolStripMenuItem9
			// 
			this.toolStripMenuItem9.Name = "toolStripMenuItem9";
			this.toolStripMenuItem9.Size = new System.Drawing.Size(132, 6);
			// 
			// loadMIDIToolStripMenuItem
			// 
			this.loadMIDIToolStripMenuItem.Image = global::modest100.Icons.music_beam_16;
			this.loadMIDIToolStripMenuItem.Name = "loadMIDIToolStripMenuItem";
			this.loadMIDIToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.loadMIDIToolStripMenuItem.Text = "Load MIDI";
			this.loadMIDIToolStripMenuItem.Click += new System.EventHandler(this.Event_MidiFileOpen);
			// 
			// saveMIDIToolStripMenuItem
			// 
			this.saveMIDIToolStripMenuItem.Enabled = false;
			this.saveMIDIToolStripMenuItem.Name = "saveMIDIToolStripMenuItem";
			this.saveMIDIToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.saveMIDIToolStripMenuItem.Text = "Save MIDI";
			// 
			// recentMIDIToolStripMenuItem
			// 
			this.recentMIDIToolStripMenuItem.Enabled = false;
			this.recentMIDIToolStripMenuItem.Name = "recentMIDIToolStripMenuItem";
			this.recentMIDIToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.recentMIDIToolStripMenuItem.Text = "Recent";
			// 
			// toolStripMenuItem7
			// 
			this.toolStripMenuItem7.Name = "toolStripMenuItem7";
			this.toolStripMenuItem7.Size = new System.Drawing.Size(132, 6);
			// 
			// exportToolStripMenuItem
			// 
			this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.mIDIToolStripMenuItem,
									this.mIDIPendingConfigurationToolStripMenuItem});
			this.exportToolStripMenuItem.Enabled = false;
			this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
			this.exportToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.exportToolStripMenuItem.Text = "Export";
			// 
			// mIDIToolStripMenuItem
			// 
			this.mIDIToolStripMenuItem.Name = "mIDIToolStripMenuItem";
			this.mIDIToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.mIDIToolStripMenuItem.Text = "MIDI";
			// 
			// mIDIPendingConfigurationToolStripMenuItem
			// 
			this.mIDIPendingConfigurationToolStripMenuItem.Name = "mIDIPendingConfigurationToolStripMenuItem";
			this.mIDIPendingConfigurationToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.mIDIPendingConfigurationToolStripMenuItem.Text = "MIDI (Pending Configuration)";
			// 
			// toolStripMenuItem6
			// 
			this.toolStripMenuItem6.Name = "toolStripMenuItem6";
			this.toolStripMenuItem6.Size = new System.Drawing.Size(132, 6);
			// 
			// configurationToolStripMenuItem
			// 
			this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.openConfigurationToolStripMenuItem,
									this.loadDefaultConfigurationFileToolStripMenuItem,
									this.closeConfigurationToolStripMenuItem,
									this.toolStripMenuItem8,
									this.saveConfigurationAsToolStripMenuItem,
									this.saveConfigurationAsDefaultToolStripMenuItem});
			this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
			this.configurationToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.configurationToolStripMenuItem.Text = "Configuration";
			// 
			// openConfigurationToolStripMenuItem
			// 
			this.openConfigurationToolStripMenuItem.Name = "openConfigurationToolStripMenuItem";
			this.openConfigurationToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
			this.openConfigurationToolStripMenuItem.Text = "Load Configuration File";
			this.openConfigurationToolStripMenuItem.Click += new System.EventHandler(this.Event_ConfigOpen);
			// 
			// loadDefaultConfigurationFileToolStripMenuItem
			// 
			this.loadDefaultConfigurationFileToolStripMenuItem.Name = "loadDefaultConfigurationFileToolStripMenuItem";
			this.loadDefaultConfigurationFileToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
			this.loadDefaultConfigurationFileToolStripMenuItem.Text = "Load Default Configuration File";
			this.loadDefaultConfigurationFileToolStripMenuItem.Click += new System.EventHandler(this.Event_ConfigOpenDefault);
			// 
			// closeConfigurationToolStripMenuItem
			// 
			this.closeConfigurationToolStripMenuItem.Name = "closeConfigurationToolStripMenuItem";
			this.closeConfigurationToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
			this.closeConfigurationToolStripMenuItem.Text = "Unload Configuration";
			this.closeConfigurationToolStripMenuItem.Click += new System.EventHandler(this.Event_ConfigUnload);
			// 
			// toolStripMenuItem8
			// 
			this.toolStripMenuItem8.Name = "toolStripMenuItem8";
			this.toolStripMenuItem8.Size = new System.Drawing.Size(222, 6);
			// 
			// saveConfigurationAsToolStripMenuItem
			// 
			this.saveConfigurationAsToolStripMenuItem.Name = "saveConfigurationAsToolStripMenuItem";
			this.saveConfigurationAsToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
			this.saveConfigurationAsToolStripMenuItem.Text = "Save Configuration As";
			this.saveConfigurationAsToolStripMenuItem.Click += new System.EventHandler(this.Event_ConfigSaveAs);
			// 
			// saveConfigurationAsDefaultToolStripMenuItem
			// 
			this.saveConfigurationAsDefaultToolStripMenuItem.Name = "saveConfigurationAsDefaultToolStripMenuItem";
			this.saveConfigurationAsDefaultToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
			this.saveConfigurationAsDefaultToolStripMenuItem.Text = "Save Configuration As Default";
			this.saveConfigurationAsDefaultToolStripMenuItem.Click += new System.EventHandler(this.Event_ConfigSaveDefault);
			// 
			// recentConfigurationsToolStripMenuItem
			// 
			this.recentConfigurationsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.configToolStripMenuItem,
									this.midiToolStripMenuItem1});
			this.recentConfigurationsToolStripMenuItem.Enabled = false;
			this.recentConfigurationsToolStripMenuItem.Name = "recentConfigurationsToolStripMenuItem";
			this.recentConfigurationsToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
			this.recentConfigurationsToolStripMenuItem.Text = "Recent";
			// 
			// configToolStripMenuItem
			// 
			this.configToolStripMenuItem.Name = "configToolStripMenuItem";
			this.configToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.configToolStripMenuItem.Text = "Config";
			// 
			// midiToolStripMenuItem1
			// 
			this.midiToolStripMenuItem1.Name = "midiToolStripMenuItem1";
			this.midiToolStripMenuItem1.Size = new System.Drawing.Size(100, 22);
			this.midiToolStripMenuItem1.Text = "Midi";
			// 
			// toolStripDropDownButton1
			// 
			this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.btnPlay,
									this.btnPause,
									this.toolStripMenuItem10,
									this.showHideMidiTrackToolStripMenuItem,
									this.toolStripSeparator3,
									this.btnShowEditor,
									this.toolStripSeparator4,
									this.showEditorToolStripMenuItem,
									this.showExtendedInfoToolStripMenuItem,
									this.toolStripSeparator5,
									this.viewPluginInfoToolStripMenuItem,
									this.manageToolStripMenuItem});
			this.toolStripDropDownButton1.Image = global::modest100.Icons.control;
			this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
			this.toolStripDropDownButton1.Size = new System.Drawing.Size(32, 21);
			this.toolStripDropDownButton1.Text = "Generator";
			this.toolStripDropDownButton1.ButtonClick += new System.EventHandler(this.Event_DropDownButtonClick);
			// 
			// btnPlay
			// 
			this.btnPlay.Image = global::modest100.Icons.control;
			this.btnPlay.Name = "btnPlay";
			this.btnPlay.ShortcutKeys = System.Windows.Forms.Keys.F5;
			this.btnPlay.Size = new System.Drawing.Size(215, 22);
			this.btnPlay.Text = "Play/Stop";
			this.btnPlay.Click += new System.EventHandler(this.Event_PlayerPlay);
			// 
			// btnPause
			// 
			this.btnPause.Image = global::modest100.Icons.control_pause;
			this.btnPause.Name = "btnPause";
			this.btnPause.ShortcutKeys = System.Windows.Forms.Keys.F6;
			this.btnPause.Size = new System.Drawing.Size(215, 22);
			this.btnPause.Text = "Pause (Unpause)";
			this.btnPause.Click += new System.EventHandler(this.Event_PlayerPause);
			// 
			// toolStripMenuItem10
			// 
			this.toolStripMenuItem10.Name = "toolStripMenuItem10";
			this.toolStripMenuItem10.Size = new System.Drawing.Size(212, 6);
			// 
			// showHideMidiTrackToolStripMenuItem
			// 
			this.showHideMidiTrackToolStripMenuItem.Name = "showHideMidiTrackToolStripMenuItem";
			this.showHideMidiTrackToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.showHideMidiTrackToolStripMenuItem.Text = "Show/Hide Midi Track List";
			this.showHideMidiTrackToolStripMenuItem.Click += new System.EventHandler(this.Event_FormToggleMidiListBox);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(212, 6);
			// 
			// btnShowEditor
			// 
			this.btnShowEditor.Name = "btnShowEditor";
			this.btnShowEditor.Size = new System.Drawing.Size(215, 22);
			this.btnShowEditor.Text = "Show Editor On Play";
			this.btnShowEditor.Click += new System.EventHandler(this.Event_EditorShowOnPlay);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(212, 6);
			// 
			// showEditorToolStripMenuItem
			// 
			this.showEditorToolStripMenuItem.Image = global::modest100.Icons.equalizer__plus;
			this.showEditorToolStripMenuItem.Name = "showEditorToolStripMenuItem";
			this.showEditorToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
			this.showEditorToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.showEditorToolStripMenuItem.Text = "Show Editor";
			this.showEditorToolStripMenuItem.Click += new System.EventHandler(this.Event_EditorShow);
			// 
			// showExtendedInfoToolStripMenuItem
			// 
			this.showExtendedInfoToolStripMenuItem.Name = "showExtendedInfoToolStripMenuItem";
			this.showExtendedInfoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
									| System.Windows.Forms.Keys.R)));
			this.showExtendedInfoToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.showExtendedInfoToolStripMenuItem.Text = "Reset!";
			this.showExtendedInfoToolStripMenuItem.Click += new System.EventHandler(this.Event_PluginReload);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(212, 6);
			// 
			// viewPluginInfoToolStripMenuItem
			// 
			this.viewPluginInfoToolStripMenuItem.Image = global::modest100.Icons.equalizer__exclamation;
			this.viewPluginInfoToolStripMenuItem.Name = "viewPluginInfoToolStripMenuItem";
			this.viewPluginInfoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
									| System.Windows.Forms.Keys.I)));
			this.viewPluginInfoToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.viewPluginInfoToolStripMenuItem.Text = "View Plugin Info";
			this.viewPluginInfoToolStripMenuItem.Click += new System.EventHandler(this.Event_PluginViewInfo);
			// 
			// manageToolStripMenuItem
			// 
			this.manageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.browseToolStripMenuItem,
									this.toolStripSeparator2,
									this.removeToolStripMenuItem});
			this.manageToolStripMenuItem.Name = "manageToolStripMenuItem";
			this.manageToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.manageToolStripMenuItem.Text = "Manage";
			// 
			// browseToolStripMenuItem
			// 
			this.browseToolStripMenuItem.Name = "browseToolStripMenuItem";
			this.browseToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.browseToolStripMenuItem.Text = "Browse";
			this.browseToolStripMenuItem.Click += new System.EventHandler(this.Event_PluginBrowse);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(97, 6);
			// 
			// removeToolStripMenuItem
			// 
			this.removeToolStripMenuItem.Enabled = false;
			this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
			this.removeToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
			this.removeToolStripMenuItem.Text = "Remove";
			// 
			// btn_pick_track
			// 
			this.btn_pick_track.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.btn_pick_track.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.btn_pick_track.Name = "btn_pick_track";
			this.btn_pick_track.Size = new System.Drawing.Size(12, 24);
			this.btn_pick_track.Text = "Track";
			// 
			// viewToolStripMenuItem
			// 
			this.viewToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.viewToolStripMenuItem.Image = global::modest100.Icons.balloon__pencil;
			this.viewToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
			this.viewToolStripMenuItem.ShowDropDownArrow = false;
			this.viewToolStripMenuItem.Size = new System.Drawing.Size(20, 21);
			// 
			// OpenFileDlg
			// 
			this.OpenFileDlg.Filter = "Plugins (*.dll)|*.dll|All Files (*.*)|*.*";
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.flowLayoutPanel1.Controls.Add(this.label3);
			this.flowLayoutPanel1.Controls.Add(this.numTempo);
			this.flowLayoutPanel1.Controls.Add(this.label4);
			this.flowLayoutPanel1.Controls.Add(this.numPpq);
			this.flowLayoutPanel1.Controls.Add(this.volumeSlider1);
			this.flowLayoutPanel1.Controls.Add(this.label5);
			this.flowLayoutPanel1.Controls.Add(this.numBarStart);
			this.flowLayoutPanel1.Controls.Add(this.label6);
			this.flowLayoutPanel1.Controls.Add(this.numBarLen);
			this.flowLayoutPanel1.ForeColor = System.Drawing.Color.Yellow;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(186, 0);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(497, 22);
			this.flowLayoutPanel1.TabIndex = 23;
			this.flowLayoutPanel1.WrapContents = false;
			// 
			// comboBoxDirectSound
			// 
			this.comboBoxDirectSound.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxDirectSound.Font = new System.Drawing.Font("Consolas", 7.25F);
			this.comboBoxDirectSound.FormattingEnabled = true;
			this.comboBoxDirectSound.Location = new System.Drawing.Point(683, 1);
			this.comboBoxDirectSound.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
			this.comboBoxDirectSound.Name = "comboBoxDirectSound";
			this.comboBoxDirectSound.Size = new System.Drawing.Size(81, 20);
			this.comboBoxDirectSound.TabIndex = 21;
			this.comboBoxDirectSound.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDirectSoundSelectedIndexChanged);
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.menuStrip2, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.comboBoxDirectSound, 2, 0);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(803, 24);
			this.tableLayoutPanel1.TabIndex = 24;
			// 
			// menuStrip2
			// 
			this.menuStrip2.BackColor = System.Drawing.Color.Transparent;
			this.menuStrip2.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip2.Font = new System.Drawing.Font("Consolas", 7F);
			this.menuStrip2.GripMargin = new System.Windows.Forms.Padding(2);
			this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aToolStripMenuItem});
			this.menuStrip2.Location = new System.Drawing.Point(764, 0);
			this.menuStrip2.Name = "menuStrip2";
			this.menuStrip2.Padding = new System.Windows.Forms.Padding(0);
			this.menuStrip2.Size = new System.Drawing.Size(39, 24);
			this.menuStrip2.TabIndex = 24;
			this.menuStrip2.Text = "menuStrip2";
			// 
			// aToolStripMenuItem
			// 
			this.aToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.aToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.aToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.aboutToolStripMenuItem});
			this.aToolStripMenuItem.Name = "aToolStripMenuItem";
			this.aToolStripMenuItem.ShortcutKeyDisplayString = "";
			this.aToolStripMenuItem.Size = new System.Drawing.Size(37, 24);
			this.aToolStripMenuItem.Text = "Help";
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
			this.aboutToolStripMenuItem.Text = "About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItemClick);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.openToolStripMenuItem,
									this.clearToolStripMenuItem,
									this.toolStripMenuItem4,
									this.calculatorToolStripMenuItem,
									this.toolStripMenuItem2,
									this.optionsToolStripMenuItem,
									this.toolStripMenuItem1,
									this.exitToolStripMenuItem,
									this.openRuntimeToolStripMenuItem,
									this.openConfigurationToolStripMenuItem1,
									this.openVstToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(183, 198);
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.openToolStripMenuItem.Text = "Open";
			// 
			// clearToolStripMenuItem
			// 
			this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
			this.clearToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.clearToolStripMenuItem.Text = "Clear";
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(179, 6);
			// 
			// calculatorToolStripMenuItem
			// 
			this.calculatorToolStripMenuItem.Name = "calculatorToolStripMenuItem";
			this.calculatorToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.calculatorToolStripMenuItem.Text = "Calculator";
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(179, 6);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
									this.deviceSelectorToolStripMenuItem,
									this.toolStripMenuItem3,
									this.largerFontToolStripMenuItem,
									this.smallerFontToolStripMenuItem,
									this.toolStripMenuItem5,
									this.imageToolToolStripMenuItem});
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.optionsToolStripMenuItem.Text = "Options";
			// 
			// deviceSelectorToolStripMenuItem
			// 
			this.deviceSelectorToolStripMenuItem.Name = "deviceSelectorToolStripMenuItem";
			this.deviceSelectorToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.deviceSelectorToolStripMenuItem.Text = "Device Selector";
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(228, 6);
			// 
			// largerFontToolStripMenuItem
			// 
			this.largerFontToolStripMenuItem.Name = "largerFontToolStripMenuItem";
			this.largerFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Oemplus)));
			this.largerFontToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.largerFontToolStripMenuItem.Text = "Larger Font";
			// 
			// smallerFontToolStripMenuItem
			// 
			this.smallerFontToolStripMenuItem.Name = "smallerFontToolStripMenuItem";
			this.smallerFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.OemMinus)));
			this.smallerFontToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.smallerFontToolStripMenuItem.Text = "Smaller Font";
			// 
			// toolStripMenuItem5
			// 
			this.toolStripMenuItem5.Name = "toolStripMenuItem5";
			this.toolStripMenuItem5.Size = new System.Drawing.Size(228, 6);
			// 
			// imageToolToolStripMenuItem
			// 
			this.imageToolToolStripMenuItem.Name = "imageToolToolStripMenuItem";
			this.imageToolToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
			this.imageToolToolStripMenuItem.Text = "Image Tool";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(179, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			// 
			// openRuntimeToolStripMenuItem
			// 
			this.openRuntimeToolStripMenuItem.Name = "openRuntimeToolStripMenuItem";
			this.openRuntimeToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.openRuntimeToolStripMenuItem.Text = "Open Runtime";
			// 
			// openConfigurationToolStripMenuItem1
			// 
			this.openConfigurationToolStripMenuItem1.Name = "openConfigurationToolStripMenuItem1";
			this.openConfigurationToolStripMenuItem1.Size = new System.Drawing.Size(182, 22);
			this.openConfigurationToolStripMenuItem1.Text = "Open Configuration";
			// 
			// openVstToolStripMenuItem
			// 
			this.openVstToolStripMenuItem.Name = "openVstToolStripMenuItem";
			this.openVstToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
			this.openVstToolStripMenuItem.Text = "Open Vst";
			// 
			// ModestForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 13);
			this.ClientSize = new System.Drawing.Size(803, 380);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Controls.Add(this.statusStrip1);
			this.DoubleBuffered = true;
			this.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = System.Windows.Forms.ImeMode.On;
			this.KeyPreview = true;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "ModestForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "csmid";
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBarLen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numBarStart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPpq)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numTempo)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.menuStrip2.ResumeLayout(false);
			this.menuStrip2.PerformLayout();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip2;
		private System.Windows.Forms.ToolStripMenuItem openVstToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openConfigurationToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem openRuntimeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem imageToolToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem5;
		private System.Windows.Forms.ToolStripMenuItem smallerFontToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem largerFontToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem deviceSelectorToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem calculatorToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton viewToolStripMenuItem;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ContextMenuStrip listBoxContextMenuStrip;
		private System.Windows.Forms.ToolStripSplitButton toolStripDropDownButton1;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ToolStripMenuItem saveRuntimeToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
		private System.Windows.Forms.ToolStripMenuItem loadRuntimeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem btnPause;
		private System.Windows.Forms.TreeView tree;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem manageToolStripMenuItem;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.ComboBox comboBoxDirectSound;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.NumericUpDown numBarStart;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numBarLen;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem8;
		private System.Windows.Forms.ToolStripMenuItem showHideMidiTrackToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog OpenFileDlg;
		private System.Windows.Forms.NumericUpDown numTempo;
		private System.Windows.Forms.NumericUpDown numPpq;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private modest100.Forms.VolumeSlider volumeSlider1;
		private System.Windows.Forms.ToolStripMenuItem midiToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem configToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem closeConfigurationToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem7;
		private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mIDIPendingConfigurationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem mIDIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveMIDIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem viewPluginInfoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem showExtendedInfoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showEditorToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem btnShowEditor;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem btnPlay;
		private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem browseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem recentConfigurationsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveConfigurationAsDefaultToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveConfigurationAsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadDefaultConfigurationFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openConfigurationToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem6;
		private System.Windows.Forms.ToolStripMenuItem recentMIDIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem loadMIDIToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripStatusLabel label2;
		private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripMenuItem btn_pick_track;
		private System.Windows.Forms.ImageList ill;
		#endregion
		#region Junk
		#if FALSE
		class TrackLoader : BackgroundWorker
		{
			IMidiParser parser;
			string trackResult;
			
			public TrackLoader(IMidiParser parser) : base()
			{
				this.parser = parser;
				this.parser.TrackLoadProgressChanged += delegate (object o,ProgressChangedEventArgs e) {
					// we should use a cancelation pending type of thing somewhere?
					OnProgressChanged(e);
				};
			}
			
			protected override void OnDoWork(DoWorkEventArgs e)
			{
				base.OnDoWork(e);
				trackResult = parser.TrackSelectAction();
				OnProgressChanged(new ProgressChangedEventArgs(100));
				this.OnRunWorkerCompleted(new RunWorkerCompletedEventArgs(null,null,false));
			}
		}
		#endif
		#endregion
		
		void Event_DropDownButtonClick(object sender, EventArgs e)
		{
			Event_PlayerPlay(sender,e);
//			Event_EditorShow(sender,e);
		}
		void AboutToolStripMenuItemClick(object sender, EventArgs e)
		{
			if (sc!=null)
				sc.ShowCb(true);
		}
		
	}
}
