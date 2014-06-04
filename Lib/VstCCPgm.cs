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

using Jacobi.Vst.Core.Host;
using Jacobi.Vst.Core.Plugin;
using IPluginCommander = Jacobi.Vst.Core.Host.IVstPluginCommandStub;
using Bitset = DspAudio.Midi.MidiReader.SmfStringFormatter;
namespace MidiSmf.Vst
{
	public class VstCCPgm
	{
		IVstPluginContext plugin;
		
		public VstCCParam this[int i] { get { return VstCCParam.Create(this,i); } }
		
		#region Internals
		VstPluginInfo info { get { return plugin.PluginInfo; } }
		int CountPrograms { get { return info.ProgramCount; } }
		public IPluginCommander Stub { get { return plugin.PluginCommandStub; } }
		#endregion
		
		#region Props
		public int ID  { get { return id; } set { id = value; Stub.SetProgram(id); } } int id;
		public string Name { get { return name; } set { name = value; Stub.SetProgramName(name); } } string name;
		#endregion
		
		public byte[] GetChunk(bool onlyCurrent) {
			return Stub.GetChunk(onlyCurrent);
		}
		public int SetChunk(IVstPluginContext context, byte[] data, bool isPresent) {
			return context.PluginCommandStub.SetChunk(data,isPresent);
		}
		
		public void Apply(IVstPluginContext pluginContext)
		{
			plugin.PluginCommandStub.SetProgram(this.id);
			byte[] ck = GetChunk(true);
			if (ck!=null) SetChunk(pluginContext,ck,true);
			bool isset = pluginContext.PluginCommandStub.EndSetProgram();
			Debug.Print("Program Dump Set: {0}, Bytes: {1}",isset, Bitset.byteToString(ck));
			pluginContext.PluginCommandStub.EditorIdle();
			ck = null;
		}
		public void Apply() { Apply(plugin); }
		
		public VstCCPgm(IVstPluginContext plugin, int id)
		{
			this.plugin = plugin;
			ID = id;
			Name = Stub.GetProgramName();
		}
		
		static public IEnumerable<VstCCPgm> EnumPrograms(IVstPluginContext ctx)
		{
			for (int i = 0; i < ctx.PluginInfo.ProgramCount; i++)
				yield return new VstCCPgm( ctx, i );
		}
		
	}
}
