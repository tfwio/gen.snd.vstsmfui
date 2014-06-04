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

namespace DspAudio.Midi
{
	[Flags]
	public enum MidiEditType
	{
		None = 0,
		Delta,
		Message,
		Data,
	}
}
