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
using System.IO;
using System.Xml;

namespace MidiSmf.Vst
{
	static class PluginCollector
	{
		static public void Write(string filename, params string[] plugins)
		{
			XmlWriterSettings ws = new XmlWriterSettings();
			ws.Indent=true;
			ws.ConformanceLevel = ConformanceLevel.Document;
			ws.NewLineChars = "\r\n";
			ws.IndentChars = "\t";
	//			ws.NewLineHandling = NewLineHandling.Entitize;
			File.Delete(filename);
			using  (System.IO.FileStream fs = new System.IO.FileStream(filename,FileMode.Create))
				using (XmlWriter writer = XmlTextWriter.Create(fs,ws))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("Plugins");
				foreach (string c in plugins)
					if (!string.IsNullOrEmpty(c))
						writer.WriteElementString("plugin",c);
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}
		static public List<string> Read(string filename)
		{
			List<string> list = new List<string>();
			if (!File.Exists(filename)) return null;
			using (System.IO.FileStream fs = new System.IO.FileStream(filename,FileMode.Open))
				using (XmlReader reader = new XmlTextReader(fs))
			{
				while (reader.Read())
				{
					switch (reader.NodeType)
					{
						case XmlNodeType.Text:
							list.Add(reader.Value);
							break;
					}
				}
			}
			return list;
		}
	}
}
