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
using modest100.Internals;

namespace modest100.Views
{
using Keys=System.Windows.Forms.Keys;
	public class MidiVstPluginListContainer : MasterViewContainer
	{
		public override System.Windows.Forms.Keys? ShortcutKeys {
			get {
				return Keys.F12;
			}
		}
		public override string Title { get { return "VST/VSTi Plugins"; } }
		public override MidiControlBase GetView() {
			return new MidiVstPluginListView();
		}
		public MidiVstPluginListContainer() {}
		
	}
}
