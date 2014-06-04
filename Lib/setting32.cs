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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DspAudio;
using DspAudio.Midi;
using DspAudio.Vst;

namespace modest100.Forms
{

	
	public struct setting32
	{
		/// (required for bar calculation)
		public double StartBar { get;set; }
		/// (required for bar calculation)
		public double Start { get;set; }
		/// (required for bar calculation)
		public double Length { get;set; }
		/// usually 0-127 for Note Value
		public double TickY { get;set; }
		
		/// add 1 for non-inclusive zero
		public double Measure { get { return ( tick / 4 / 4 / 4 ).FloorMinimum(0); } }
		/// Modulus % limited to 4; add 1 for non-inclusive zero
		public double Bar     { get { return ( tick / 4 / 4 ).FloorMinimum(0) % 4; } }
		/// Modulus % limited to 4; add 1 for non-inclusive zero
		public double Note    { get { return ( tick / 4 ).FloorMinimum(0) % 4; } }
		/// Modulus % limited to 4; add 1 for non-inclusive zero
		/// (this is the same as notes; alias)
		public double Quarter { get { return ( tick / 4 ).FloorMinimum(0) % 4; } }
		/// (required) the tick or x-pixel position.
		/// add 1 for non-inclusive zero
		public double Tick    { get { return   tick; } set { tick = value.FloorMinimum( 0 ); } } double tick;
		/// (required)
		public double MidiTick { get;set; }
		/// 
		public FloatPoint Mouse { get;set; }
		/// 
		public FloatRect Selection { get;set; }
		/// 
		public int Channel { get;set; }
		/// 
		static public setting32 FromTicks(double ticks, double ticky, int channel, double offset, double bar, double len){
			
			return new setting32(){
				TickY    = ticky,
				Channel  = channel,
				Tick     = ticks,
				StartBar = offset,
				Start    = bar,
				Length   = len,
			};
		}
	}
}
