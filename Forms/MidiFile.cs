/*
 * Date: 11/12/2005
 * Time: 4:19 PM
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using DspAudio.Midi;
using DspAudio.Midi.Common;

namespace MidiSmf.Format
{
	public class MidiFile : MidiReader
	{
		ListView lve;
		
		public MidiFile( string fileName, ListView lve )
		{
			this.MidiFileName = fileName;
			this.LoadTrack = this.GetTrackMessage;
			this.lve = lve;
			this.MessageHandler = this.GotMidiEventD;
		}
		
		public override void ResetTiming()
		{
			base.ResetTiming();
			lve.Items.Clear();
		}
		
		public void Read()
		{
			GetMemory();
			OnFileLoaded(EventArgs.Empty);
		}
		
		public void GetMemory()
		{
			this.MidiMessage -= GotMidiEventE;
			this.SmfFileHandle = MidiUtil.GetMthd(MidiFileName);
			this.MidiMessage += GotMidiEventE;
		}
		
		void GotMidiEventE(object sender, MidiMessageEvent e)
		{
			switch (e.MsgT)
			{
				case MsgType.MetaStr:
					lve.AddItem( c4, MeasureBarTick( e.Ppq ), e.Ppq.ToString(), ""/*(RunningStatus32 & 0x0F)+1*/, MetaHelpers.MetaNameFF( e.IntMsg ) , GetMetaString( e.Offset ) );
					break;
				case MsgType.MetaInf:
				case MsgType.System:
					lve.AddItem(GetEventColor(e.IntMsg,cR),MeasureBarTick(e.Ppq),e.Ppq.ToString(),""/*(RunningStatus32 & 0x0F)+1*/,MetaHelpers.MetaNameFF(e.IntMsg),GetMetaSTRValue(e.Offset));
					break;
				default:
					if (e.IsRse)
						lve.AddItem( GetRseEventColor( Colors["225"] ), MeasureBarTick( e.Ppq ), e.Ppq.ToString(), ch,GetRseEventString( e.Offset ), chRseV( e.Offset ) );
					else
						lve.AddItem( GetEventColor( Colors["225"] ), MeasureBarTick( e.Ppq ), e.Ppq.ToString(), ch,GetEventString( e.Offset ), chV( e.Offset ) );
					break;
			}
		}
		void GotMidiEventD(MsgType t, int track, int offset, int imsg, byte bmsg, ulong ppq, bool rse)
		{
			switch (t)
			{
				case MsgType.MetaStr:
					lve.AddItem( c4, MeasureBarTick( ppq ), ppq.ToString(), string.Empty, MetaHelpers.MetaNameFF( imsg ) , GetMetaString( offset ) );
					break;
				case MsgType.MetaInf:
				case MsgType.System:
					lve.AddItem( GetEventColor(imsg,cR), MeasureBarTick( ppq ), ppq.ToString(), string.Empty, MetaHelpers.MetaNameFF( imsg ), GetMetaSTRValue( offset ));
					break;
				default:
					if (rse) lve.AddItem( GetRseEventColor( Colors["225"] ), MeasureBarTick( ppq ), ppq.ToString(), ch,GetRseEventString( offset ), chRseV( offset ) );
					else     lve.AddItem( GetEventColor   ( Colors["225"] ), MeasureBarTick( ppq ), ppq.ToString(), ch,GetEventString   ( offset ), chV   ( offset ) );
					break;
			}
		}

		///<summary>a track is selected</summary>
		public string TrackSelect ( ListView lve )
		{
			this.lve.Items.Clear();
			return base.TrackSelectAction();
		}

	}
}
