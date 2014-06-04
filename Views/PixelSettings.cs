using System;

namespace MidiSmf.Forms
{
	public class PixelSettings
	{
		int pixelsPerTick, pixelsPerQuarter, pixelsPerNote, pixelsPerBar;
		/// Usually set to 1;
		/// This should be our number of ticks per pixel, but it is reserved and set to 1.
		public int PixelsPerTick { get { return pixelsPerTick; } set { pixelsPerTick = value; } }
		public int PixelsPerQuarter { get { return pixelsPerQuarter * PixelsPerTick; } }
		public int PixelsPerNote { get { return pixelsPerNote * PixelsPerQuarter; } }
		public int PixelsPerBar { get { return pixelsPerBar * PixelsPerNote; } }
		
		public PixelSettings(int nTick, int nQuarter, int nNotes, int nBars)
		{
			Refit (nTick,nQuarter,nNotes,nBars);
		}
		
		void Refit(int nTick, int nQuarter, int nNotes, int nBars)
		{
			PixelsPerTick = nTick;
			pixelsPerQuarter = nQuarter;
			pixelsPerNote = nNotes;
			pixelsPerBar = nBars * pixelsPerNote;
		}
	}
}
