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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

using DspAudio;
using DspAudio.Vst;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;

namespace DspAudio.Vst.Xml
{
	public class MidiSmfFileSettings
	{
		private float mindb = -48;
		[XmlAttribute("smf")] public string MidiFileName { get;set; }
		[XmlAttribute("cfg")] public string ConfigurationFile { get;set; }
		[XmlAttribute("tk")] public int SelectedMidiTrack { get;set; }
		
		[XmlElement] public Loop Bar { get;set; }
		
		[XmlArrayItem("file",typeof(MidiFile))]
		[XmlArray("midi")]
		public List<MidiFile> Midi {
			get { return midi; }
			set { midi = value; }
		} List<MidiFile> midi = new List<MidiFile>();
		
//		[XmlAttribute]
		public string ActiveInstrument { get;set; }
//		[XmlAttribute]
		public string ActiveEffect { get;set; }
		
		[XmlArrayItem("module",typeof(VstModule))]
		[XmlArray("generators")]
		public List<VstModule> Generators {
			get { return generators; }
			set { generators = value; }
		} List<VstModule> generators;
		
		[XmlArrayItem("module",typeof(VstModule))]
		[XmlArray("modules")]
		public List<VstModule> Modules {
			get { return modules; }
			set { modules = value; }
		} List<VstModule> modules;
		
		/// <summary>
		/// float number from 0 to 1.
		/// </summary>
		[XmlAttribute("db"),DefaultValue(1.0)] public float MasterVolume { get;set; }
		[XmlIgnore] public float db { get { return (float)Math.Log10(MasterVolume); } }
		[XmlIgnore] public float percent { get { return (float) 1-(db/mindb); } }
	//			float db = 20 * (float)Math.Log10(Volume);
	//			float percent = 1 - (db / MinDb);
		[XmlElement("plugin")]
		public Plugin Plugin { get;set; }
	
		public MidiSmfFileSettings()
		{
		}
	}
	public class MidiFile
	{
		[XmlAttribute] public string Title { get;set; }
		[XmlAttribute] public string Path { get;set; }
		
		[XmlElement("gen")]
		public List<MidiToGen> Gen {
			get { return gen; }
			set { gen = value; }
		} List<MidiToGen> gen;
		
		public MidiFile()
		{
		}
	}
	public class MidiToGen
	{
		[XmlAttribute("id")] public int Index { get;set; }
		/// <summary>blank for no midifilters</summary>
		[XmlAttribute("from")] public string From { get;set; }
		/// <summary>blank for no midifilters</summary>
		[XmlAttribute("to")] public string To { get;set; }
		/// <summary>blank for cc re-mapping</summary>
		[XmlAttribute("cc")] public string CC { get;set; }
		/// <summary>Default = 0</summary>
		[DefaultValue(0),XmlAttribute("pat")] public int Pat { get;set; }
//		[XmlAttribute("pgm")] public int Pgm { get;set; }
		public MidiToGen()
		{
		}
		public MidiToGen Create(
			VstPluginManager manager,
			int index)
		{
			return Create(manager,index,null,null,null);
		}
		public MidiToGen Create(
			VstPluginManager manager,
			int index,
			int[] c_from,
			int[] c_to,
			string[] cc)
		{
			MidiToGen module = new MidiToGen();
			module.Index = index;
			module.From = c_from.ToCdfString();
			module.To = c_to.ToCdfString();
			module.Pat = manager.GeneratorModules[index].PluginCommandStub.GetProgram();
			if (cc!=null) module.CC = string.Join(",",cc);
			return module;
		}
	}
	public class VstModule : PluginBase
	{
		[XmlAttribute,DefaultValue(1.0f)] public float Amplitude {
			get { return amplitude; }
			set { amplitude = value; }
		} float amplitude;
		[XmlAttribute] public string Title {
			get { return title; }
			set { title = value; }
		} string title;
		[XmlAttribute,DefaultValue(0)] public int ProgramId {
			get { return programId; }
			set { programId = value; }
		} int programId;
		[XmlAttribute,DefaultValue(0)] public int PatchId {
			get { return patchId; }
			set { patchId = value; }
		} int patchId;
		[XmlAttribute] public string Category {
			get { return category; }
			set { category = value; }
		} string category;
		
		public VstModule()
		{
		}
		public VstModule(VstPlugin plugin)
		{
			title = plugin.Title;
			patchId = plugin.PluginCommandStub.GetProgram();
			category = plugin.PluginCommandStub.GetCategory().ToString();
//			var result = plugin.PluginCommandStub.BeginLoadBank(
//				new VstPatchChunkInfo(
//					1,
//					plugin.PluginInfo.PluginID,
//					plugin.PluginInfo.PluginVersion,
//					plugin.PluginInfo.ProgramCount
//				)
//			);
//			if (result== VstCanDoResult.Yes) { }
			try { this.ProgramDump = plugin.PluginCommandStub.GetChunk(false); } catch { }
			try { this.PatchDump   = plugin.PluginCommandStub.GetChunk(true); } catch { }
		}
	}
	static class ConvertHelper
	{
		static public int[] ToInt32Array(this string data)
		{
			List<int> list = new List<int>();
			foreach (string item in data.Split(','))
			{
				list.Add(int.Parse(item.Trim()));
			}
			return list.ToArray();
		}
		static public string ConvertIntData(int[] data)
		{
			if (data==null) return null;
			string[] strings = new string[data.Length];
			for (int d =0; d < data.Length; d++) strings[d] = data.ToString();
			string returned = string.Join(", ",strings);
			Array.Clear(strings,0,strings.Length);
			strings = null;
			return returned;
		}
		static public string ToCdfString(this int[] data)
		{
			return ConvertIntData(data);
		}
	}
}
