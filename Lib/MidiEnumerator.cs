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
using System.Linq;

using DspAudio.Vst;
using Jacobi.Vst.Core;

namespace DspAudio.Midi
{
	class MidiEnumerator
	{
		#region Midi Enumerations
		
		static public VstEvent[] EnumerateMidiData(IMidiParserUI ui, Loop loop)
		{
			return EnumerateMidiData(ui,loop.Begin,loop.Length.FloorMinimum(0).ToInt32());
		}
		static public VstEvent[] EnumerateMidiData(IMidiParserUI ui, int blockSize)
		{
			return EnumerateMidiData(ui,ui.VstContainer.VstPlayer.SampleOffset,blockSize);
		}
		static public VstEvent[] EnumerateMidiData(IMidiParserUI ui, double start, int len)
		{
			if (ui==null) return null;
			if (ui.MidiParser.MidiDataList.Count==0) return null;
			List<VstEvent> list = new List<VstEvent>();
	//			lock (locker)
			{
				SampleClock c = new SampleClock(ui.VstContainer.VstPlayer.Settings);
				foreach (MidiMessage item in EnumerateMidiMessages( ui, new Loop(){Begin=start,Length=len}))
				{
					if (item is MidiChannelMessage) list.Add(item.ToVstMidiEvent(Convert.ToInt32(ui.VstContainer.VstPlayer.SampleOffset),ui.VstContainer.VstPlayer.Settings,c));
					else if (item is MidiSysexMessage)
						list.Add(
							item.ToVstMidiSysex(
								ui.VstContainer.VstPlayer.SampleOffset.ToInt32(),
								ui.VstContainer.VstPlayer.Settings,
								c
							)
						);
				}
				c = null;
			}
			list.Sort(SortAlgo);
			return list.ToArray();
		}
		
		static public int SortAlgo( VstEvent a, VstEvent b )
		{
			return a.DeltaFrames.CompareTo(b.DeltaFrames);
		}
		
		static public IEnumerable<MidiMessage> EnumerateMidiMessages(IMidiParserUI Parser, Loop loop)
		{
			SampleClock c = new SampleClock(Parser.VstContainer.VstPlayer.Settings);
			for (int trackId = 0; trackId < Parser.MidiParser.SmfFileHandle.NumberOfTracks; trackId++)
			{
				var elements = Parser.MidiParser.MidiDataList[trackId]
					.Where( msg0 => msg0.IsContained(  c, loop ) );
				foreach ( MidiMessage item in elements )
				{
					yield return item;
				}
			}
			c = null;
		}
	
		#endregion
	}
}
