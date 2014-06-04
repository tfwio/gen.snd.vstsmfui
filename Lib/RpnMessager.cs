using System;
using System.Collections.Generic;
using DspAudio;
using DspAudio.Midi;
using DspAudio.Vst;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;

namespace modest100.Forms
{
	/// <remarks>
	/// Cater NRPN and RPN messages.  Specifically though Control messages.
	/// </remarks>
	/// <summary>
	/// I pulled this guy together to construct and send midi data
	/// to the hosted process such as NRPN and RPN messages.
	/// Generally, these messages might follow instrument patch setup.
	/// </summary>
	class RpnNrpn
	{
		public byte lsb,msb,lsbd,msbd;

		#region Const
		
		// -------------------------
		// message format other one
		// -------------------------
		/// 0x63
		const byte nrpm_lsb = 0x63;  // 99
		/// 0x62
		const byte nrpm_msb = 0x62;  // 98
		/// 0x08
		const byte nrpn_lsb_d = 0x08; // 
		/// 0x24
		const byte nrpn_msb_d = 0x24;  // 36
		// -------------------------
		// note off message?
		// -------------------------
		/// 0xB0
		const byte midi_msg = 0xB0;  // 
		// -------------------------
		// message format one
		// -------------------------
		/// 0x65
		const byte rpn_lsb = 0x65;
		/// 0x64
		const byte rpn_msb = 0x64;
		/// 0x06
		const byte lsb_data = 0x06;  //
		/// 0x26
		const byte msb_data = 0x26;  //
		#endregion
		
		VstEvent q(byte b1, byte b2) { return new VstMidiEvent(0,0,0,new byte[]{ midi_msg, b1, b2, 0 },0,0); }
		VstEvent q(byte[] b) { return new VstMidiEvent(0,0,0,b,0,0); }
		
		#region NRPN
		/// 0xB0 0x63 [nn] 0x00
		byte[] NrpnLsb { get { return new byte[]{ midi_msg, nrpm_lsb, lsb, 0 }; } }
		/// 0xB0 0x62 [nn] 0x00
		byte[] NrpnMsb { get { return new byte[]{ midi_msg, nrpm_msb, msb, 0 }; } }
		/// 0xB0 0x08 [nn] 0x00
		byte[] NrpnLsbData { get { return new byte[]{ midi_msg, nrpn_lsb_d, lsbd, 0 }; } }
		/// 0xB0 0x08 [nn] 0x00
		byte[] NrpnMsbData { get { return new byte[]{ midi_msg, nrpn_msb_d, msbd, 0 }; } }
		#endregion
		#region NRPN STRING
		/// 0xB0 0x63 [nn] 0x00
		public string StrNrpnLsb { get { return MidiReader.SmfStringFormatter.byteToString(NrpnLsb); } }
		/// 0xB0 0x62 [nn] 0x00
		public string StrNrpmMsb { get { return MidiReader.SmfStringFormatter.byteToString(NrpnMsb); } }
		/// 0xB0 0x08 [nn] 0x00
		public string StrNrpnLsbData { get { return MidiReader.SmfStringFormatter.byteToString(NrpnLsbData); } }
		/// 0xB0 0x24 [nn] 0x00
		public string StrNrpnMsbData { get { return MidiReader.SmfStringFormatter.byteToString(NrpnMsbData); } }
		#endregion
		
		#region RPN
		/// 0xB0 0x65 [nn] 0x00
		byte[] RpnLsb { get { return new byte[]{ midi_msg, rpn_lsb, lsb, 0 }; } }
		/// 0xB0 0x64 [nn] 0x00
		byte[] RpnMsb { get { return new byte[]{ midi_msg, rpn_msb, msb, 0 }; } }
		/// 0xB0 0x06 [nn] 0x00
		byte[] RpnLsbData { get { return new byte[]{ midi_msg, lsb_data, lsbd, 0 }; } }
		/// 0xB0 0x26 [nn] 0x00
		byte[] RpnMsbData { get { return new byte[]{ midi_msg, msb_data, msbd, 0 }; } }
		#endregion
		#region RPN STRING
		/// 0xB0 0x65 [nn] 0x00
		public string StrRpnLsb { get { return MidiReader.SmfStringFormatter.byteToString(RpnLsb); } }
		/// 0xB0 0x64 [nn] 0x00
		public string StrRpnMsb { get { return MidiReader.SmfStringFormatter.byteToString(RpnMsb); } }
		/// 0xB0 0x06 [nn] 0x00
		public string StrRpnLsbData { get { return MidiReader.SmfStringFormatter.byteToString(RpnLsbData); } }
		/// 0xB0 0x26 [nn] 0x00
		public string StrRpnMsbData { get { return MidiReader.SmfStringFormatter.byteToString(RpnMsbData); } }
		#endregion
		
		#region EVENTS

		public VstEvent[] GetRpnEvents()
		{
			//			msg.str_rpn_lsb,msg.str_rpn_msb,msg.LsbData,msg.MsbData
			List<VstEvent> list = new List<VstEvent>();
			//			if (nm!=0)
			list.Add(q(NrpnLsb));// 0x65
			list.Add(q(NrpnMsb));// 0x64
			//if (dm!=0)
			list.Add(q(NrpnLsbData)); //x26
			list.Add(q(NrpnMsbData)); //x06
			return list.ToArray();
		}
		public VstEvent[] GetNrpnEvents()
		{
			List<VstEvent> list = new List<VstEvent>();
			//			if (nm!=0)
			list.Add(q(NrpnLsb)); // 0x63
			list.Add(q(NrpnMsb)); // 0x62
			//			if (dm!=0)
			list.Add(q(NrpnLsbData)); // 0x24
			list.Add(q(NrpnMsbData)); // 0x06
			return list.ToArray();
		}

		#endregion
		
		static public RpnNrpn FromInt(int a, int b)
		{
			RpnNrpn msg = new RpnNrpn();
			msg.lsb = a.Div(128);
			msg.msb = a.Mod(128);
			msg.lsbd = b.Div(128);
			msg.msbd = b.Mod(128);
			return msg;
		}
//		static public RpnNrpn FromByte(params byte[] d)
//		{
//			RpnNrpn msg = new RpnNrpn();
//			msg.lsb = d[1];
//			msg.msb = d[0];
//			msg.lsbd = d[3];
//			msg.msbd = d[2];
//			return msg;
//		}
	}
}
