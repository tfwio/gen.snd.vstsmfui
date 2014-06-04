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
using System.Internals;
using System.Linq;

using DspAudio;

namespace modest100.Forms
{
	using Keys=System.Windows.Forms.Keys;
	
	
	public class MidiControlBase : UserView, IMidiView
	{
		public string Title { get { return this.Text; } }
		virtual public string Description { get { return string.Empty; } }
		public IMidiParserUI UserInterface { get; set; }
		
		virtual public void ApplyRegistrySettings() { }
		virtual public void SetUI(IMidiParserUI ui)
		{
			this.UserInterface = ui;
			this.UserInterface.ClearMidiTrack -= ClearTrack;
			this.UserInterface.ClearMidiTrack += ClearTrack;
			this.UserInterface.GotMidiFile -= FileLoaded;
			this.UserInterface.GotMidiFile += FileLoaded;
		}
		public MidiControlBase() : base() {}
		
		virtual public void ClearTrack(object sender, EventArgs e){ Debug.Print("ClearTrack:{0}",this.GetType().Name); }
		virtual public void TrackChanged(object sender, EventArgs e){ Debug.Print("TrackChanged:{0}",this.GetType().Name); }
		virtual public void AfterTrackLoaded(object sender, EventArgs e){ Debug.Print("AfterTrackLoaded:{0}",this.GetType().Name); }
		virtual public void BeforeTrackLoaded(object sender, EventArgs e){ Debug.Print("BeforeTrackLoaded:{0}",this.GetType().Name); }
		virtual public void FileLoaded(object sender, EventArgs e)
		{
			Debug.Print("FileLoaded:{0}",this.GetType().Name);
			
			ClearTrack(sender,e);
			
			this.UserInterface.MidiParser.TrackChanged -= TrackChanged;
			this.UserInterface.MidiParser.TrackChanged += TrackChanged;
			
			this.UserInterface.MidiParser.BeforeTrackLoaded -= BeforeTrackLoaded;
			this.UserInterface.MidiParser.BeforeTrackLoaded += BeforeTrackLoaded;
			
			this.UserInterface.MidiParser.AfterTrackLoaded -= AfterTrackLoaded;
			this.UserInterface.MidiParser.AfterTrackLoaded += AfterTrackLoaded;
		}
	}
	
}
