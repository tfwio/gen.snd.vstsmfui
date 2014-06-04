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
using System.Drawing;
using System.Windows.Forms;

using DspAudio;
using DspAudio.Forms;
// using modest100.Forms;
// modēst100
#endregion
namespace modest100.Forms
{
	class MidiPianoViewSettings : IRendererSettings
	{
		#region Node Min/Max
		
		/// <inheritdoc/>
		public FloatPoint NodeMax { get { return new FloatPoint(NodeXMax,NodeYMax); } }
		/// <inheritdoc/>
		public float NodeXMax  { get { return (float)Math.Floor(/*axmax*/Rect.Width / NodeSize.Width); } }
		/// <inheritdoc/>
		public float NodeYMax  { get { return (float)Math.Floor(/*aymax*/Rect.Height / NodeSize.Height); } }
		#endregion
		
		public FloatPoint GridNodeMax {
			get {
				return new FloatPoint(
					ClientRect.Width/this.NodeSize.Width,
					ClientRect.Height/this.NodeSize.Height
				).Floored;
			}
		}
		
		#region Colors
		
		public Color GridForegroundTextColor {
			get { return gridForegroundTextColor; }
			set { gridForegroundTextColor = value; }
		} Color gridForegroundTextColor = Color.White;
		
		public Color GridBackgroundColor {
			get { return gridBackgroundColor; }
			set { gridBackgroundColor = value; }
		} Color gridBackgroundColor = Color.DarkGray;
		
		#endregion
		
		public IUi Ui { get { return uiComponent; } } readonly IUi uiComponent;
		
		#region Client
		
		public int Width { get { return Convert.ToInt32(ClientSize.X); } }
		public int Height { get { return Convert.ToInt32(ClientSize.Y); } }
		
		public FloatRect ClientRect { get { return FloatRect.FromClientInfo(ClientSize,ClientPadding); } }
		public FloatPoint ClientSize { get { return Ui.ClientRectangle.Size; } }
		public Padding ClientPadding { get { return gutter; } set { gutter = value; } } Padding gutter;
		public Rectangle ClipBackground { get { return new Rectangle(0, gutter.Top, Width - gutter.Right, CalculatedHeight); } }
		
		#endregion
		
		#region Node
		
		public float NodeWidth {
			get { return rowWidth; }
		} float rowWidth = 4;
		public float NodeHeight {
			get { return rowHeight; }
		} float rowHeight = 13;
		public SizeF NodeSize { get { return new SizeF(rowWidth, rowHeight); } set { rowWidth = value.Width; rowHeight=value.Height; } }

		#endregion
		
		public FloatPoint GridSalad {
			get {
				return new FloatPoint(
					Math.Floor((double)Width / rowWidth),
					Math.Floor((double)Height / rowHeight)
				);
			}
		}
		
		public int VisibleWidth { get { return (int)Math.Floor((double)Width / rowWidth); } }
		public int VisibleHeight { get { return (int)Math.Floor((double)Height / rowHeight); } }
		
		/// <summary>
		/// Per Quarter Note
		/// </summary>
		public int XSnap {
			get { return xSnap; } set { xSnap = value; }
		} int xSnap = 4;
		
		FloatPoint Calculated { get { return new FloatPoint(Ui.ClientRectangle.Width - ClientPadding.Horizontal,Ui.ClientRectangle.Height - ClientPadding.Vertical); } }
		public int CalculatedWidth  { get { return Ui.ClientRectangle.Width - ClientPadding.Horizontal; } }
		public int CalculatedHeight { get { return Ui.ClientRectangle.Height - ClientPadding.Vertical; } }
		
		/// <inherit/>
		public FloatRect C1 { get { return FloatRect.FromPadding(ClientPadding); } }
		/// <inherit/>
		public FloatRect Rect { get { return new FloatRect(C1.Location, C1.Size+Calculated); } }
		// C1.Location, C1.Size
		public MidiPianoViewSettings(IUi iui)
		{
			this.uiComponent = iui;
			ClientPadding = new Padding(64, 60, 18, 0);
			
		}
		
	}
}
