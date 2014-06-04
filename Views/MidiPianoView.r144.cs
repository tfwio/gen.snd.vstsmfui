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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DspAudio;
using DspAudio.Midi;
using DspAudio.Vst;

namespace MidiSmf.Forms
{
	public class MidiPianoView : MidiControlBase, IUi
	{
		List<MidiNote> LocalNotes = new List<MidiNote>();
		
		#region Notes Part
		
		SampleClock timing = new SampleClock();
		List<int> ChanelNumbers = new List<int>();
		bool SameChanelComparer(MidiNote a, MidiNote b) { return a.Ch == b.Ch; }
		public List<MidiData> SetOfNotes {
			get { return setOfNotes; }
			set { setOfNotes = value; }
		} List<MidiData> setOfNotes = new List<MidiData>();
		
		bool HasEventsToRender {
			get { return !(this.UserInterface.MidiParser==null && this.UserInterface.MidiParser.Notes.Count > 0); }
		}
		/// <summary>
		/// Populates SetOfNotes and our list of Channels in the currently selected track.
		/// ChannelNumbers list contains a set of distinct channel ids.
		/// </summary>
		void GetOurNotesAgain()
		{
			GetOurNotesAgain(Ren.ui.ClientRect.Location);
		}
		void GetOurNotesAgain(FloatPoint mousePoint)
		{
			// see ClientMouse, HoverPoint
			SetOfNotes = new List<MidiData>( Parser.Notes.Where( xnote => Convert.ToDouble( xnote.Start ) < StopPointTicks ) );
			
			ChanelNumbers.Clear();
			
			foreach (MidiData n in SetOfNotes.Where(nn=> nn is MidiNote))
			{
				if (n.Ch.HasValue)
					if (!ChanelNumbers.Contains(n.Ch.Value)) ChanelNumbers.Add(n.Ch.Value);
			}
			
			tsChannelsMenuItem.DropDownItems.Clear();
			
			foreach (int i in ChanelNumbers) tsChannelsMenuItem
				.DropDownItems
				.Add( string.Format( "{0:00}" , i ) , null , channel_click )
				.Tag = i;
			
		}
		
		#endregion
		
		void channel_click(object s, EventArgs e)
		{
			int ch = (int)(s as ToolStripMenuItem).Tag;
		}
		
		public setting Transport { get;set; }
		
		#region Fields: Private
		
		const int shadowx = 2, shadowy = 1;
		SampleClock clock = new SampleClock();
		bool isReady = false;
		bool HasControlKey = false;
		/// <summary> Positioning for mouse information. </summary>
		Point MouseInfo = new Point( 30,12 );
		Image BackgroundImg = null;
		Binding OffsetBinding;
		
		DictionaryList<int,MidiMessage> notes = new DictionaryList<int,MidiMessage>();
		
		NAudioVST Player { get { return UserInterface.VstContainer.VstPlayer; } }
		ITimeConfiguration PlayerSettings { get { return Player.Settings; } }
		IMidiParser Parser { get { return Player.Parent.Parent.MidiParser; } }
		
		#endregion
		
		#region strings
		
		string clockstr { get { return clock.SolvePPQ(Player.SampleOffset,PlayerSettings).MeasureString; } }
		
		string MeasureString
		{
			get
			{
				clock.SolvePPQ( Player.SampleOffset, PlayerSettings );
				return string.Format(
					"MBQT: {0}, Pulses: {1:N0}, C: {2}, mouse: {3}, loc: {4}x{5}, siz: {6}x{7}",
					clock.MeasureString,
					Math.Floor(clock.Pulses),
					Parser.SelectedTrackNumber,IsMouseDown ? "•":" ",
					MousePoint.X, MousePoint.Y,
					MouseTrail.X, MouseTrail.Y
				);
			}
		}
		
		#endregion
		
		#region Setting struct
		
		public struct setting
		{
			/// (required for bar calculation)
			public double StartBar { get;set; }
			/// (required for bar calculation)
			public double Start { get;set; }
			/// (required for bar calculation)
			public double Length { get;set; }
			/// usually 0-127 for Note Value
			public double TickY { get;set; }
			
			/// add 1 for non-inclusive zero
			public double Measure { get { return ( tick / 4 / 4 / 4 ).FloorMinimum(0); } }
			/// Modulus % limited to 4; add 1 for non-inclusive zero
			public double Bar     { get { return ( tick / 4 / 4 ).FloorMinimum(0) % 4; } }
			/// Modulus % limited to 4; add 1 for non-inclusive zero
			public double Note    { get { return ( tick / 4 ).FloorMinimum(0) % 4; } }
			/// Modulus % limited to 4; add 1 for non-inclusive zero
			/// (this is the same as notes; alias)
			public double Quarter { get { return ( tick / 4 ).FloorMinimum(0) % 4; } }
			/// (required) the tick or x-pixel position.
			/// add 1 for non-inclusive zero
			public double Tick    { get { return   tick; } set { tick = value.FloorMinimum( 0 ); } } double tick;
			/// (required)
			public double MidiTick { get;set; }
			/// 
			public FloatPoint Mouse { get;set; }
			/// 
			public FloatRect Selection { get;set; }
			/// 
			public int Channel { get;set; }
			/// 
			static public setting FromTicks(double ticks, double ticky, int channel, double offset, double bar, double len){
				
				return new setting(){
					TickY    = ticky,
					Channel  = channel,
					Tick     = ticks,
					StartBar = offset,
					Start    = bar,
					Length   = len,
				};
			}
		}
		
		#endregion

		#region Focus Redirection

		public bool CanFocus {
			get { return canFocus; }
			set { canFocus = value; }
		} bool canFocus = true;
		
		public new bool Focused {
			get { return vScrollBar1.Focused; }
			set { if (value) vScrollBar1.Focus(); }
		}

		#endregion

		void GotMidiTrack(object sender, EventArgs e)
		{
			Invalidate(Needs.Notes);
		}

		#region Events XOffset, YOffset
		
		public event EventHandler XOffsetChanged;
		protected virtual void OnXOffsetChanged(EventArgs e)
		{
			if (XOffsetChanged != null) {
				XOffsetChanged(this, e);
				OnSizeChanged(null);
			}
		}
		
		public event EventHandler YOffsetChanged;
		protected virtual void OnYOffsetChanged(EventArgs e)
		{
			if (YOffsetChanged != null) {
				YOffsetChanged(this, e);
				OnSizeChanged(null);
			}
		}

		#endregion
		
		#region Property: Gutter
		
		public Padding Gutter {
			get { return new Padding(Ren.ui.ClientPadding.Left, Ren.ui.ClientPadding.Top, Ren.ui.ClientPadding.Right,Ren.ui.ClientPadding.Bottom); /**/ }
			set { Ren.ui.ClientPadding = value; Invalidate(); }
		}
		
		FloatPoint GutterYOffset { get { return new FloatPoint(0,Gutter.Top); } }
		
		#endregion
		
		#region Property: Renderer (Needs)
		
		[FlagsAttribute]
		public enum Needs { None, Select, Deselect, Background, Mouse, MouseWheel, Text, Notes, XScroll, YScroll }
		public Needs RenderRequest { get; set; }
		
		#endregion
		
		#region Properties: Offset
		
		/// <summary>
		/// Mouse Tracking to Scroll offset
		/// </summary>
		public int OffsetY {
			get { return offsetY; }
			set { this.vScrollBar1.Value = offsetY = value; Invalidate(Needs.Background); }
		} int offsetY = 64;
		
		/// <summary>
		/// Mouse Tracking to Note offset.
		/// </summary>
		public MBT OffsetX {
			get { return offsetX; } set { offsetX = value; Invalidate(Needs.Background); }
		} MBT offsetX = 0;

		#endregion
		
		#region Event Handlers
		void Event_ScrollV(object sender, EventArgs e)
		{
			this.OffsetY = this.vScrollBar1.Value;
		}
		#endregion
		
		#region Mouse To Client
		
		FloatPoint HoverPoint { get { return new FloatPoint( Math.Floor((double)ClientMouse.X / Ren.ui.NodeWidth), OffsetY-Math.Floor((double)ClientMouse.Y / Ren.ui.NodeHeight) ); } }
		FloatPoint ClientToPoint(double x, double y) { return new FloatPoint( x , y ).Divide( Ren.ui.NodeSize ).Floored; }
		FloatPoint ClientToPoint(FloatPoint client) { return ClientToPoint( client.X , client.Y ); }
		
		/// <summary>
		/// Simply put: d / division.
		/// This is because each pixel is 1 quarter or
		/// </summary>
		double GetTickX(double x, double division) { return ( ( x * division).FloorMinimum(0) ); }
		
		/// Simply put: d / division.  This is because each pixel is 1 quarter or </summary>
		double GetMidiTickFromX(double x, double divisor) { return GetTickX( x / divisor , Parser.SmfFileHandle.Division); }
		
		double GetTickXMod(double x) { return (x / 4 * Parser.SmfFileHandle.Division) % Parser.SmfFileHandle.Division; }
		
		/// This method simply passes tick input thru, where the number of ticks is returned.
		double UpdateTransport() { return UpdateTransport(Convert.ToDouble(HoverPoint.X+Transport.StartBar)); }
		/// This method simply passes tick input thru, where the number of ticks is returned.
		double UpdateTransport(double ticks)
		{
			try
			{
				Transport = setting.FromTicks(
					ticks,
					(int)Convert.ToDouble(HoverPoint.Y).FloorMinMax(0,127),
					Convert.ToInt32(numCH.Value),
					(double)numOff.Value,
					(double)numBar.Value,
					(double)numLen.Value);
			} catch {
				throw new ArgumentException();
			}
			return ticks;
		}
		string MouseString {
			get
			{
				float x = HoverPoint.X + Convert.ToSingle(Transport.StartBar);
				UpdateTransport(x);
				setting inf = Transport;
				try {
				return string.Format(
					/*         [     bar / 4 / 4 / 4    ] [ m%4] [ n%4 ] [      ]  [     ] [ ] [    ][ ]        [ ]       [ ]  */
					/*  - */	"{0:###,###,###,#00}:{1:00}:{9:000} ({10:N0}) {2:000} {3} {4,-5}{6} / Top: {7}; Btm: {8}",
					/*  0 */	inf.Measure,
					/*  1 */	inf.Bar,
					/*  2 */	inf.Note,
					/*  3 */	inf.Tick,
					/*  4 */	(HoverPoint / new FloatPoint(Ren.ui.NodeSize)).Floored,
					/*  5 */	MidiReader.SmfStringFormatter.GetKeySharp((int)Convert.ToDouble(HoverPoint.Y).FloorMinMax(0,127)),
					/*  6 */	MidiReader.SmfStringFormatter.GetOctave((int)Convert.ToDouble(HoverPoint.Y).FloorMinMax(0,127)),
					/*  7 */	offsetY, Convert.ToDouble(offsetY-Ren.ui.NodeMax.Y+1).FloorMinMax(0,127),
					/*  9 */	GetMidiTickFromX(x,Ren.ui.NodeWidth), GetMidiTickFromX(x,Ren.ui.NodeWidth)
				);
				}
				catch (Exception e)
				{
					return e.ToString();
				}
			}
		}
		
		Point ClientMouse {
			get {
				return new FloatPoint(PointToClient(MousePosition)) - new FloatPoint(Gutter.Left,Gutter.Top);
			}
		}
		
		#endregion
		
		public MidiPianoView() : base()
		{
			this.DoubleBuffered = true;
			this.InitializeComponent();
			try {
				MidiPianoViewRenderer.ui = new MidiPianoViewSettings(this);
			} catch {
			}
			
			int gt = MidiPianoViewRenderer.ui.ClientPadding.Top;
			
			this.Text = "Piano Layout";
			
			try {
				this.OffsetBinding = new Binding("Value", this, "OffsetY");
				this.OffsetBinding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
				this.vScrollBar1.DataBindings.Add(this.OffsetBinding);
			} catch {
				
			}
		}
		
		#region Mouse Overrides
		
		private bool IsMouseDown {
			get { return isMouseDown; }
		} bool isMouseDown;
		
		FloatPoint MousePoint = FloatPoint.Empty;
		FloatPoint MouseTrail = FloatPoint.Empty;
		
		protected override void OnMouseDown(MouseEventArgs e)
		{
			MouseTrail = new FloatPoint(0);
			isMouseDown = true;
			Focused = true;
			MousePoint = PointToClient(MousePosition);
			GetOurNotesAgain(MousePoint);
			base.OnMouseDown(e);
			Invalidate(Needs.Select|Needs.Mouse);
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!IsMouseDown) {
				base.OnMouseMove(e);
				Invalidate(Needs.Mouse);
				return;
			}
			MouseTrail = (new FloatPoint(PointToClient(MousePosition))-MousePoint);
			base.OnMouseMove(e);
			Invalidate(Needs.Select|Needs.Mouse);
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
//			MousePoint = MousePosition;
			MouseTrail = (new FloatPoint(PointToClient(MousePosition))-MousePoint);
			isMouseDown = false;
			base.OnMouseUp(e);
			Invalidate(Needs.Deselect|Needs.Mouse);
		}
		#endregion
		
		#region Pixel Per Pulse
		/// <summary>
		/// Pixels per quarter
		/// </summary>
		int PixelsPerQuarter = 4;
		/// <summary>total number of quarters visible (4quarters*4notes)</summary>
		int NodeCount = 4 /* quarter */ * 4 /* note */ * 4 /* bar*/;
		/// <summary>Node count in pulses.</summary>
		int StopPointPulses { get { return PixelsPerQuarter * NodeCount; } }
		int StopPointTicks { get { return StopPointPulses * Player.Settings.Division; } }
		#endregion
		
		/// we should have prepared a list of notes and each channel contained in
		/// our view.
		void UpdateView()
		{
			foreach (MidiNote n in SetOfNotes)
			{
				timing.SolveSamples(Convert.ToDouble(n.Start),Player.Settings);
				if (!(n is MidiNote)) continue;
				// s, n, o, l, off
//				listNotes.AddItem(
//					ListView.DefaultBackColor,
//					n.GetMBT(MidiReader.FileDivision),
//					timing.TimeString,
//					n.Ch.HasValue ? n.Ch.ToString() : "?",
//					n.KeyStr,
//					n.V1.ToString(),
//					n.GetMbtLen(MidiReader.FileDivision),
//					n.V2.ToString()
//				);
			}
		}
		
		#region trail
		bool IsTrailNeg { get { return IsTrailNegX || IsTrailNegY; } }
		bool IsTrailNegX { get { return MouseTrail.X < 0; } }
		bool IsTrailNegY { get { return MouseTrail.Y < 0; } }
		#endregion
		
		float GetLineHeight(Graphics g) { return Ren.NoteFont.GetHeight(g); }
		
		FloatPoint InvMouseTrail { get { return new FloatPoint( IsTrailNegX ? -1*MouseTrail.X : MouseTrail.X, IsTrailNegY ? -1*MouseTrail.Y : MouseTrail.Y ); } }
		FloatPoint InvMousePoint { get { return new FloatPoint( IsTrailNegX ? MousePoint.X+MouseTrail.X : MousePoint.X, IsTrailNegY ? MousePoint.Y+MouseTrail.Y : MousePoint.Y ); } }
		
		void Render(Graphics g, Needs n)
		{
			if (n.HasFlag(Needs.Background) | n.HasFlag(Needs.YScroll)) {
				this.BackgroundImg = MidiPianoViewRenderer.GetBackgroundGrid_Image(OffsetY);
			}
			{
				n |= Needs.Mouse|Needs.Text;
				g.SetClip(Ren.ui.ClipBackground);
				MidiPianoViewRenderer.GetBackgroundGrid_Image(g, BackgroundImg);
				g.ResetClip();
			}
			try { GetForeground_Caption(g, n); } catch {}
			try { GetForeground_Selection(g,n); } catch {}
			try { GetForeground_NoteText(g,n); } catch {}
			try { GetForeground_NewBars(g,n); } catch {}
			this.RenderRequest = Needs.None;
		}
		double Calc(double from)
		{
			return from / Parser.SmfFileHandle.Division;
		}
		#region Drawing Layers (not in background)
		void GetForeground_NewBars(Graphics g, MidiPianoView.Needs n)
		{
//			if (!n.HasFlag(Needs.Notes)) return;
			FloatRect grid = Ren.ui.ClientRect;
			FloatPoint max = Ren.ui.GridNodeMax;
			double multi	    = 4 * Parser.SmfFileHandle.Division;
			double m	    = Parser.SmfFileHandle.Division;
			
			double startpoint = multi * Transport.Start;
			double endpoint   = startpoint + (multi * Transport.Length) ;
			
			List<MidiData> midinotes = new List<MidiData>(
				this.SetOfNotes.Where(
					note=>
					note is MidiNote &&
					note.Start >= startpoint &&
					note.Start <  endpoint
				));
			Debug.Print("{{Search Start:{0,11:N0}, End:{1,11:N0} }}", startpoint, endpoint);
			foreach (MidiData midinote in midinotes)
			{
				MidiNote noteon = midinote as MidiNote;
				Debug.Print(
					"  {{Note[{0:000}:{1}], Calc: {2,11}, Start:{3,11}, End:{4,11} }}",
					noteon.K,
					MidiReader.SmfStringFormatter.GetKeySharp(noteon.K),
					Calc(noteon.Start),
					noteon.Start,
					noteon.Len,
					(noteon.Start + Convert.ToDouble(noteon.Len))
				);
			}
			using (Brush b = new SolidBrush(Color.FromArgb(196,Color.Purple)))
				for (int x = 0 ; x < max.X; x++)
					for (int y = 0 ; y < max.Y; y++)
			{
				if (x ==0 && y == 0)
				{
					FloatPoint pos = new FloatPoint( x * Ren.ui.NodeWidth , Ren.ui.NodeHeight * y );
					
					pos.X += Ren.ui.ClientPadding.Left;
					pos.Y += Ren.ui.ClientPadding.Top;
					
					if (x % 64 == 0) g.FillRectangle(Brushes.CadetBlue,(Rectangle)new FloatRect(pos,new FloatPoint(Ren.ui.NodeWidth*64/4,Ren.ui.NodeHeight)).Floored);
					if (x % 32 == 0) g.FillRectangle(b,(Rectangle)new FloatRect(pos,new FloatPoint(Ren.ui.NodeWidth*32/4,Ren.ui.NodeHeight)).Floored);
					if (x % 16 == 0) g.FillRectangle(Brushes.Red,(Rectangle)new FloatRect(pos,new FloatPoint(Ren.ui.NodeWidth*16/4,Ren.ui.NodeHeight)).Floored);
					if (x % 4  == 0) g.FillRectangle(Brushes.Black,(Rectangle)new FloatRect(pos,new FloatPoint(Ren.ui.NodeSize)).Floored);
					
				}
				x = (int)max.X;
				y = (int)max.Y;
			}
//			Ren.ui.NodeMax;
			grid = null;
			max  = null;
		}
		
		void GetForeground_Caption(Graphics g, Needs n)
		{
			if (n.HasFlag(Needs.Mouse)) {
				float line = GetLineHeight(g);
				using (SolidBrush b0 = new SolidBrush(Color.FromArgb(80,Color.Black)))
					using (SolidBrush b1 = new SolidBrush(Color.FromArgb(127, Color.DodgerBlue)))
				{
					g.DrawString(MeasureString, Ren.NoteFont, b0, MouseInfo.X+shadowx, GutterYOffset.Y-line-line+shadowy);
					g.DrawString(MouseString,   Ren.NoteFont, b0, MouseInfo.X+shadowx, GutterYOffset.Y-line+shadowy);
					
					g.DrawString(MeasureString, Ren.NoteFont, b1, MouseInfo.X, GutterYOffset.Y-line-line);
					g.DrawString(MouseString,   Ren.NoteFont, b1, MouseInfo.X, GutterYOffset.Y-line);
				}
			}
		}
		void GetForeground_NoteText(Graphics g, Needs n)
		{
			if (n.HasFlag(Needs.Text))
			{
//				g.SetClip(Ren.ui.ClientRect);
				MidiPianoViewRenderer.NoteText(g, offsetY);
//				g.ResetClip();
			}
		}
		void GetForeground_Selection(Graphics g, Needs n)
		{
			if (n.HasFlag(Needs.Select)||n.HasFlag(Needs.Deselect))
			{
				using (Pen r = new Pen(Color.FromArgb(64,Color.Black),1))
					using (SolidBrush b = new SolidBrush(Color.FromArgb(64,Color.Red)))
				{
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawRectangle(r,new FloatRect(InvMousePoint,InvMouseTrail));
					g.FillRectangle(b,new FloatRect(InvMousePoint,InvMouseTrail));
				}
			}
		}
		#endregion
		
		void Invalidate(Needs need) { RenderRequest = need; this.Invalidate(); }

		#region override parent
		
		public override void AfterTrackLoaded(object sender, EventArgs e)
		{
			base.AfterTrackLoaded(sender, e);
//			this.listNotes.Items.Clear();
			GetOurNotesAgain();
//			this.listNotes.Visible = true;
		}
		
		#endregion
		
		#region Overrides

		protected override void OnLoad(EventArgs e)
		{
			try {
				base.OnLoad(e);
				this.isReady = true;
				this.Invalidate();
			} catch {
			}
		}
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			Invalidate(Needs.Background);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			Debug.Print("Key Pressed");
		}
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			Debug.Print("Key Released");
		}
		
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (HasControlKey) {
				this.OffsetX = (e.Delta > 0) ? this.offsetX + 1 : this.offsetX - 1;
				if (this.offsetX <= 0) this.offsetX = 0;
			} else {
				
				this.OffsetY = (e.Delta > 0) ? this.offsetY + 1 : this.offsetY - 1;
				if (this.offsetY > 127) this.OffsetY = 127;
				else if (this.offsetY <= 0) this.OffsetY = 0;
			}
			
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			try {
				if (!this.isReady) return;
				this.Render(e.Graphics,RenderRequest);
			} catch { }
		}

		protected override void OnGotFocus(EventArgs e)
		{
			Debug.Print("GOTFOCUS: {0}",this.GetType().Name);
			Focused = true;
			this.Capture = true;
			base.OnGotFocus(e);
			this.Cursor = Cursors.Hand;
			this.FindForm().KeyDown += kd;
			this.FindForm().KeyUp += ku;
		}
		protected override void OnLostFocus(EventArgs e)
		{
			Debug.Print("LOSTFOCUS: {0}",this.GetType().Name);
			this.Capture = false;
			this.Cursor = Cursors.Default;
			base.OnLostFocus(e);
			this.FindForm().KeyDown -= kd;
			this.FindForm().KeyUp -= ku;
		}
		void kd(object o, KeyEventArgs a) { this.OnKeyDown(a); }
		void ku(object o, KeyEventArgs a) { this.OnKeyUp(a); }
		#endregion
		
		void Event_BarLocation(object sender, EventArgs e)
		{
			UpdateTransport();
			Invalidate(Needs.Notes);
		}
		
		class Ren : MidiPianoViewRenderer
		{
		}
		
		#region Design

		private System.ComponentModel.IContainer components;

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}


		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MidiPianoView));
			this.vScrollBar1 = new System.Windows.Forms.TrackBar();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.numBar = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.numLen = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.numOff = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.numCH = new System.Windows.Forms.NumericUpDown();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tsChannelsMenuItem = new System.Windows.Forms.ToolStripDropDownButton();
			((System.ComponentModel.ISupportInitialize)(this.vScrollBar1)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numLen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numOff)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCH)).BeginInit();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			                                                                | System.Windows.Forms.AnchorStyles.Right)));
			this.vScrollBar1.AutoSize = false;
			this.vScrollBar1.LargeChange = 12;
			this.vScrollBar1.Location = new System.Drawing.Point(787, 55);
			this.vScrollBar1.Maximum = 127;
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.vScrollBar1.Size = new System.Drawing.Size(14, 259);
			this.vScrollBar1.TabIndex = 1;
			this.vScrollBar1.TickStyle = System.Windows.Forms.TickStyle.None;
			this.vScrollBar1.Scroll += new System.EventHandler(this.Event_ScrollV);
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoSize = true;
			this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
			this.flowLayoutPanel1.Controls.Add(this.label5);
			this.flowLayoutPanel1.Controls.Add(this.numBar);
			this.flowLayoutPanel1.Controls.Add(this.label6);
			this.flowLayoutPanel1.Controls.Add(this.numLen);
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.numOff);
			this.flowLayoutPanel1.Controls.Add(this.label2);
			this.flowLayoutPanel1.Controls.Add(this.numCH);
			this.flowLayoutPanel1.Controls.Add(this.toolStrip1);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel1.ForeColor = System.Drawing.Color.Yellow;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(803, 25);
			this.flowLayoutPanel1.TabIndex = 24;
			this.flowLayoutPanel1.WrapContents = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label5.ForeColor = System.Drawing.Color.Gray;
			this.label5.Location = new System.Drawing.Point(3, 3);
			this.label5.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 9);
			this.label5.TabIndex = 19;
			this.label5.Text = "BAR";
			// 
			// numBar
			// 
			this.numBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numBar.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numBar.ForeColor = System.Drawing.Color.Silver;
			this.numBar.Location = new System.Drawing.Point(26, 3);
			this.numBar.Maximum = new decimal(new int[] {
			                                  	1000000000,
			                                  	0,
			                                  	0,
			                                  	0});
			this.numBar.Name = "numBar";
			this.numBar.Size = new System.Drawing.Size(66, 16);
			this.numBar.TabIndex = 16;
			this.numBar.ValueChanged += new System.EventHandler(this.Event_BarLocation);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label6.ForeColor = System.Drawing.Color.Gray;
			this.label6.Location = new System.Drawing.Point(98, 3);
			this.label6.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(17, 9);
			this.label6.TabIndex = 19;
			this.label6.Text = "LEN";
			this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// numLen
			// 
			this.numLen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numLen.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numLen.ForeColor = System.Drawing.Color.Silver;
			this.numLen.Location = new System.Drawing.Point(121, 3);
			this.numLen.Maximum = new decimal(new int[] {
			                                  	1000000000,
			                                  	0,
			                                  	0,
			                                  	0});
			this.numLen.Minimum = new decimal(new int[] {
			                                  	1,
			                                  	0,
			                                  	0,
			                                  	0});
			this.numLen.Name = "numLen";
			this.numLen.Size = new System.Drawing.Size(66, 16);
			this.numLen.TabIndex = 16;
			this.numLen.Value = new decimal(new int[] {
			                                	32,
			                                	0,
			                                	0,
			                                	0});
			this.numLen.ValueChanged += new System.EventHandler(this.Event_BarLocation);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label1.ForeColor = System.Drawing.Color.Gray;
			this.label1.Location = new System.Drawing.Point(193, 3);
			this.label1.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(17, 9);
			this.label1.TabIndex = 21;
			this.label1.Text = "OFF";
			this.toolTip1.SetToolTip(this.label1, "Song Alignment/Start position");
			// 
			// numOff
			// 
			this.numOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numOff.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numOff.ForeColor = System.Drawing.Color.Silver;
			this.numOff.Location = new System.Drawing.Point(216, 3);
			this.numOff.Maximum = new decimal(new int[] {
			                                  	1000000,
			                                  	0,
			                                  	0,
			                                  	0});
			this.numOff.Name = "numOff";
			this.numOff.Size = new System.Drawing.Size(88, 16);
			this.numOff.TabIndex = 20;
			this.numOff.ValueChanged += new System.EventHandler(this.Event_BarLocation);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Consolas", 6F, System.Drawing.FontStyle.Bold);
			this.label2.ForeColor = System.Drawing.Color.Gray;
			this.label2.Location = new System.Drawing.Point(310, 3);
			this.label2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(13, 9);
			this.label2.TabIndex = 22;
			this.label2.Text = "CH";
			this.toolTip1.SetToolTip(this.label2, "Song Alignment/Start position");
			// 
			// numCH
			// 
			this.numCH.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.numCH.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.numCH.ForeColor = System.Drawing.Color.Silver;
			this.numCH.Location = new System.Drawing.Point(329, 3);
			this.numCH.Maximum = new decimal(new int[] {
			                                 	24,
			                                 	0,
			                                 	0,
			                                 	0});
			this.numCH.Name = "numCH";
			this.numCH.Size = new System.Drawing.Size(88, 16);
			this.numCH.TabIndex = 23;
			this.numCH.ValueChanged += new System.EventHandler(this.Event_BarLocation);
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			                               	this.tsChannelsMenuItem});
			this.toolStrip1.Location = new System.Drawing.Point(420, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.Size = new System.Drawing.Size(63, 25);
			this.toolStrip1.TabIndex = 24;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// tsChannelsMenuItem
			// 
			this.tsChannelsMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsChannelsMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("tsChannelsMenuItem.Image")));
			this.tsChannelsMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsChannelsMenuItem.Name = "tsChannelsMenuItem";
			this.tsChannelsMenuItem.Size = new System.Drawing.Size(29, 22);
			this.tsChannelsMenuItem.Text = "Channels";
			// 
			// MidiPianoView
			// 
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.vScrollBar1);
			this.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.Name = "MidiPianoView";
			this.Size = new System.Drawing.Size(803, 314);
			((System.ComponentModel.ISupportInitialize)(this.vScrollBar1)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numLen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numOff)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCH)).EndInit();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.ToolStripDropDownButton tsChannelsMenuItem;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.NumericUpDown numCH;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.NumericUpDown numOff;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numLen;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numBar;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.TrackBar vScrollBar1;
		#endregion
		
	}

}
