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

using Jacobi.Vst.Core;

namespace DspAudio.Midi
{
	class MidiMessager
	{
		
		#region MIDI VstEvent, MidiMessage
		
		readonly object locker = new object();
		
		static bool IsContained(MidiMessage message, Loop constraint, SampleClock clockRef, double min, double max)
		{
			double samplePos = clockRef.SolveSamples(message.DeltaTime).Samples32;
			return samplePos >= min && samplePos < max && samplePos < constraint.End;
		}
		
		static public void GetMidiData(IMidiParserUI ui, Loop b, IList<VstEvent> midiBuffer, int blockSize)
		{
			if (ui.VstContainer.PluginManager.MasterPluginInstrument==null)
			{
				midiBuffer = new List<VstEvent>();
				return;
			}
			midiBuffer.Clear();
			{
				midiBuffer = null;
				midiBuffer = new List<VstEvent>(MidiEnumerator.EnumerateMidiData( ui, blockSize));
				//ui.VstContainer.VstPlayer.SampleOffset, blockSize
				
				if (midiBuffer!=null)
					if ( midiBuffer.Count > 0 )
						ui.VstContainer.PluginManager.MasterPluginInstrument
							.PluginCommandStub.ProcessEvents(
								midiBuffer.ToArray()
							);
			}
		}
		
		#endregion
	
	}
}
