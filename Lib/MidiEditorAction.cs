/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Linq;
using DspAudio.Midi;

namespace modest100.Midi
{
	public class MidiEditorAction
	{
		public MidiEditType Flags {
			get {
				MidiEditType flags = 0;
				if (oldDelta != newDelta) flags |= MidiEditType.Delta;
				if (oldMessage != newMessage) flags |= MidiEditType.Message;
				if (oldData != newData) flags |= MidiEditType.Data;
				return flags;
			}
		}
		
		public int Track {
			get { return track; }
		} internal int track;
		
		internal ulong oldDelta,newDelta;
		internal int oldMessage,newMessage;
		internal byte[] oldData, newData;
		
	}
}
