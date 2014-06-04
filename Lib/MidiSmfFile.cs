/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using DspAudio;
using DspAudio.Vst;
using DspAudio.Vst.Module;

namespace modest100.Xml
{
	public class MidiSmfFile : SerializableClass<MidiSmfFile>
	{
		const string DefaultFileFilter = "modest100 Runtime|*.vstmid";
		protected override string FileFilter {
			get { return DefaultFileFilter; }
		}
		[XmlElement("settings")]
		public MidiSmfFileSettings Settings { get;set; }
		
		public MidiSmfFile()
		{
			base.fileFilter = DefaultFileFilter;
		}
	}
}
