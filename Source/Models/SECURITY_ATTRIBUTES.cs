/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace modest100.Win32
{
	[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Ansi )]
	public struct SECURITY_ATTRIBUTES {
		public short nLength;
		public short lpSecurityDescriptor;
		public short bInheritHandle;
	}
}
