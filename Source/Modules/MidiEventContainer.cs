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

using modest100.Internals;

namespace modest100.Forms
{
	using Keys=System.Windows.Forms.Keys;
	
	public class MidiEventContainer : MasterViewContainer
	{
		public override string Title { get { return "Event View"; } }
		public override MidiControlBase GetView() { return new MidiEventControl(); }
		public MidiEventContainer()
		{
		}
		
		public override System.Windows.Forms.Keys? ShortcutKeys {
			get {
				return Keys.F9;
			}
		}
	}
}
