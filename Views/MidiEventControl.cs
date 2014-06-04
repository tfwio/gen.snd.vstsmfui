/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;
using System.Windows.Forms;

using DspAudio;
using DspAudio.Forms;
using DspAudio.Midi;
using DspAudio.Midi.Common;

// 7285Hz
namespace modest100.Forms
{
	public class MidiEventControl : MidiControlBase
	{
		SampleClock timing = new SampleClock();
		
		#region Registry
		const string regpath = @"Software\tfoxo\midi";
		const string reg_list_columns 	= "EventList2.Columns[]";
		const string reg_list_fontsize 	= "EventList2.FontSize";
		
		[System.ComponentModel.BrowsableAttribute(false),System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public string ListColumns {
			get { return Reg.GetKeyValueString(regpath,reg_list_columns); }
			set { Reg.SetKeyValueString(regpath,reg_list_columns,value); Debug.Print("Saving Col Sizes: {0}",value); }
		}
		[System.ComponentModel.BrowsableAttribute(false),System.ComponentModel.DesignerSerializationVisibilityAttribute(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
		public string ListFontSize {
			get { return Reg.GetKeyValueString(regpath,reg_list_fontsize); }
			set { Reg.SetKeyValueString(regpath,reg_list_fontsize,value); }
		}
		
		/// <summary>
		/// Get: Returns integer values from ListView.
		/// <para>set: Sets values to the list view.</para>
		/// </summary>
		int[] ListColSize
		{
			get
			{
				Debug.Print("Loading Column Sizes");
				var cols = new int[lve.Columns.Count];
				for (int i =0;i < lve.Columns.Count; i++)
					cols[i] = lve.Columns[i].Width;
				return cols;
			}
			set
			{
				Debug.Print("Setting Col Size");
				for (int i =0;i < lve.Columns.Count; i++)
					lve.Columns[i].Width = value[i];
			}
		}
		
		internal void FontLarger(object sender, EventArgs e)
		{
			Reg.SetControlFontSize(lve,12);
			ListFontSize = "12";
		}
		internal void FontSmaller(object sender, EventArgs e)
		{
			Reg.SetControlFontSize(lve,9);
			ListFontSize = "9";
		}

		#endregion
		
		public MidiEventControl() : base()
		{
			this.InitializeComponent();
			this.Text = "Event View";
			WindowsInterop.WindowsTheme.HandleTheme(this.lve);
		}
		public MidiEventControl(IMidiParserUI parserUI) : this()
		{
			SetUI(parserUI);
		}
		
		#region Overrides
		
		public override void SetUI(IMidiParserUI ui)
		{
			base.SetUI(ui);
			ApplyRegistrySettings();
		}
		public override void ClearTrack(object sender, EventArgs e)
		{
			base.ClearTrack(sender, e);
			lve.Items.Clear();
		}
		public override void FileLoaded(object sender, EventArgs e)
		{
			base.FileLoaded(sender, e);
			if (!UserInterface.MidiParser.MessageHandlers.Contains(GotMidiEventD))
				UserInterface.MidiParser.MessageHandlers.Add(GotMidiEventD);
		}
		public override void BeforeTrackLoaded(object sender, EventArgs e)
		{
			Debug.Print("Midi Event List Hidden");
			this.lve.Visible = false;
			base.BeforeTrackLoaded(sender, e);
		}
		public override void AfterTrackLoaded(object sender, EventArgs e)
		{
			Debug.Print("Midi Event List Shown");
			base.AfterTrackLoaded(sender, e);
			this.lve.Visible = true;
		}
		public override void ApplyRegistrySettings()
		{
			Debug.Print("Loading Settings for MidiListView");
			try
			{
				if (!string.IsNullOrEmpty(ListFontSize)) {
					Reg.SetControlFontSize(this.lve,float.Parse(ListFontSize));
				}
			}
			catch {
				Debug.Print("Error updating font size");
			}
			try
			{
				Debug.Print("got value: {0}",ListColumns);
				if (string.IsNullOrEmpty(ListColumns))
				{
					Debug.Print("got error!: {0}",ListColumns);
				}
				var vals = Reg.TranslateString(ListColumns);
				if (vals!=null) this.ListColSize = vals;
			}
			catch {
				Debug.Print("Error updating list-col size");
			}
			Debug.Print("Adding Col-Resized Event");
			
			this.lve.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.LveColumnWidthChanged);
		}
		
		#endregion
		
		void GotMidiEventD(MidiMsgType t, int track, int offset, int imsg, byte bmsg, ulong ppq, int rse, bool isrse)
		{
			timing.SolveSamples(
				ppq,
				UserInterface.VstContainer.VstPlayer.Settings.Rate,
				UserInterface.MidiParser.MidiTimeInfo.Tempo,
				UserInterface.MidiParser.SmfFileHandle.Division,
				true
			);
			switch (t)
			{
				case MidiMsgType.MetaStr:
					lve.AddItem( MidiReader.c4, UserInterface.MidiParser.GetMbtString( ppq ), string.Empty, string.Empty, MetaHelpers.MetaNameFF( imsg ), UserInterface.MidiParser.GetMetaString( offset ) );
					break;
				case MidiMsgType.MetaInf:
					lve.AddItem( UserInterface.MidiParser.GetEventColor(imsg,MidiReader.cR), UserInterface.MidiParser.GetMbtString( ppq ), string.Empty, string.Empty, MetaHelpers.MetaNameFF( imsg ), UserInterface.MidiParser.GetMetaSTR( offset ) );
					break;
				case MidiMsgType.SysCommon:
				case MidiMsgType.System:
					lve.AddItem( UserInterface.MidiParser.GetEventColor(imsg,MidiReader.cR), UserInterface.MidiParser.GetMbtString( ppq ), string.Empty, string.Empty, MetaHelpers.MetaNameFF( imsg ), UserInterface.MidiParser.GetMetaSTR( offset ) );
					break;
				default:
//				case MsgType.Channel:
					if (isrse) lve.AddItem( UserInterface.MidiParser.GetRseEventColor( UserInterface.MidiParser.Colors["225"] ), UserInterface.MidiParser.GetMbtString( ppq ), timing.TimeString, bmsg==0xF0?"":(rse & 0x0F).ToString(),UserInterface.MidiParser.GetRseEventString( offset ), UserInterface.MidiParser.chRseV( offset ) );
					else       lve.AddItem( UserInterface.MidiParser.GetEventColor   ( UserInterface.MidiParser.Colors["225"] ), UserInterface.MidiParser.GetMbtString( ppq ), timing.TimeString, bmsg==0xF0?"":(rse & 0x0F).ToString(),UserInterface.MidiParser.GetEventString   ( offset ), UserInterface.MidiParser.chV   ( offset ) );
//				if (t== MsgType.NoteOn||t== MsgType.NoteOff) UserInterface.MidiParser.CheckNote(t,ppq,Convert.ToByte((rse) & 0x0F),offset,bmsg,isrse);
					break;
			}
		}
		
		void SaveColumnSetting()
		{
			string v = Reg.TranslateString(ListColSize);
			Debug.Print("Saving Col Sizes");
			Debug.Print(v);
			if (!string.IsNullOrEmpty(v)) ListColumns = v;
			ListFontSize = lve.Font.SizeInPoints.ToString();
		}
		
		void LveColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			SaveColumnSetting();
		}
		
		#region Design
		private System.ComponentModel.IContainer components;
		
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			this.lve = new System.Windows.Forms.ListView();
			this.cctime = new System.Windows.Forms.ColumnHeader();
			this.ccchanel = new System.Windows.Forms.ColumnHeader();
			this.ccevent = new System.Windows.Forms.ColumnHeader();
			this.ccmsg = new System.Windows.Forms.ColumnHeader();
			this.cchhmmss = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// lve
			// 
			this.lve.BackColor = System.Drawing.SystemColors.Window;
			this.lve.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lve.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			                          	this.cctime,
			                          	this.cchhmmss,
			                          	this.ccchanel,
			                          	this.ccevent,
			                          	this.ccmsg});
			this.lve.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lve.Font = new System.Drawing.Font("Ubuntu Mono", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
			this.lve.FullRowSelect = true;
			this.lve.LabelWrap = false;
			this.lve.Location = new System.Drawing.Point(0, 0);
			this.lve.MultiSelect = false;
			this.lve.Name = "lve";
			this.lve.Size = new System.Drawing.Size(575, 285);
			this.lve.TabIndex = 17;
			this.lve.UseCompatibleStateImageBehavior = false;
			this.lve.View = System.Windows.Forms.View.Details;
			// 
			// cctime
			// 
			this.cctime.Text = "MBT";
			this.cctime.Width = 130;
			// 
			// ccchanel
			// 
			this.ccchanel.Text = "@";
			// 
			// ccevent
			// 
			this.ccevent.Text = "Info";
			this.ccevent.Width = 181;
			// 
			// ccmsg
			// 
			this.ccmsg.Text = "Byte Data";
			this.ccmsg.Width = 304;
			// 
			// cchhmmss
			// 
			this.cchhmmss.Text = "hh:mm:ss";
			// 
			// MidiListView
			// 
			this.Controls.Add(this.lve);
			this.Name = "MidiListView";
			this.Size = new System.Drawing.Size(575, 285);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ColumnHeader cchhmmss;
		private System.Windows.Forms.ColumnHeader ccmsg;
		private System.Windows.Forms.ColumnHeader ccevent;
		private System.Windows.Forms.ColumnHeader ccchanel;
		private System.Windows.Forms.ColumnHeader cctime;
		internal System.Windows.Forms.ListView lve;
		#endregion
		
	}
}
