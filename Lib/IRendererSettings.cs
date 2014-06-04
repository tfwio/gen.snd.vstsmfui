/*
 * Created by SharpDevelop.
 * User: tfooo
 * Date: 11/12/2005
 * Time: 4:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;

using DspAudio;
using DspAudio.Forms;

namespace modest100.Forms
{
	public interface IPianoView
	{
		int XQuarter { get; }
		int XPart  { get; }
		int XNote  { get; }
		int XBar  { get; }
		int XSnap { get;set; }
	}
	public interface INodeView
	{
		SizeF NodeSize { get; }
		/// <summary>
		/// Maximum count of nodes that will fit on the screen in X and Y space.
		/// Note that Math.Floor has been acted on your results.
		/// </summary>
		FloatPoint NodeMax { get; }
		/// Maximum count of nodes that will fit on the screen in X space.
		float NodeXMax  { get; }
		/// Maximum count of nodes that will fit on the screen in Y space.
		float NodeYMax  { get; }
	}
	public interface IRendererSettings : /*IPianoView,*/ INodeView
	{
//		int TextOffset { get; }
//		int TextOffsetE { get; }
//		FloatPoint GridSalad {   get;}
		IUi Ui { get; }
		
		int Width { get; }
		int Height { get; }
		
		Padding ClientPadding { get; set; }
		int CalculatedWidth { get; }
		int CalculatedHeight { get; }
		/// <summary>Client Padding (Converted to FloatRect)</summary>
		FloatRect C1 { get; }
		/// <summary>
		/// Target Client Area.
		/// </summary>
		FloatRect Rect { get; }
		
		
	}
}
