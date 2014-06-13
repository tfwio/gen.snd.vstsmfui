using System;
using System.Internals;
using System.Linq;

using modest100.Forms;
using modest100.Internals;

namespace modest100.Views
{
	using Keys=System.Windows.Forms.Keys;
	public class MidiNoteContainer : MasterViewContainer
	{
		public override System.Windows.Forms.Keys? ShortcutKeys {
			get {
				return Keys.F10;
			}
		}
		public override string Title { get { return "Notes"; } }
		public override MidiControlBase GetView() { return new MidiNoteView(); }
		
		public MidiNoteContainer() {}
	}
}
