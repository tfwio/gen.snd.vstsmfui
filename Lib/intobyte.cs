using System;
using System.Collections.Generic;
using DspAudio;
using DspAudio.Midi;
using DspAudio.Vst;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;

namespace modest100.Forms
{
	static class intobyte
	{
		static public int ToInt32(this decimal toInt){ return Convert.ToInt32(toInt); }
		static public byte ToByte(this int i) { return Convert.ToByte(i & 0x000000FF); }
		static public byte Mod(this int a, int m) { return (a % m).ToByte(); }
		static public byte Div(this int a, int d) { return (a / d).ToByte(); }
		static public int GetSignificand(this int a, int m, int l) { return ((a * m)+l).ToByte(); }
	}
}
