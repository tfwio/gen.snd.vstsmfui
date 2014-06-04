#region Using
/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Internals;
using System.Linq;

using modest100.Forms;
//using modēst100.Forms;
using modest100.Internals;

#endregion
namespace modest100.Views
{
	using Keys=System.Windows.Forms.Keys;
	public class MidiPianoContainer : MasterViewContainer
	{
		public override Keys? ShortcutKeys { get { return Keys.F11; } }
		public override string Title { get { return "Piano Layout"; } }
		public override MidiControlBase GetView() { return new MidiPianoView(); }
		public MidiPianoContainer() {}
	}
}
