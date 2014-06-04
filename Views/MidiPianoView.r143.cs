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
			SetOfNotes = new List<MidiData>(Parser.Notes.Where(xnote => Convert.ToDouble(xnote.Start) < StopPointTicks));
			ChanelNumbers.Clear();
			foreach (MidiData n in SetOfNotes.Where(nn=> nn is MidiNote))
			{
				if (n.Ch.HasValue) if (!ChanelNumbers.Contains(n.Ch.Value)) ChanelNumbers.Add(n.Ch.Value);
			}
		}
		#endregion
		
		#region Fields: Private
		
		const int shadowx = 2, shadowy = 1;
		SampleClock clock = new SampleClock();
		bool isReady = false;
		bool HasControlKey = false;
		public setting BarPosition { get;set; }
		/// <summary> Positioning for mouse information. </summary>
		Point MouseInfo = new Point( 30,12 );
		Image BackgroundImg = null;
		Binding OffsetBinding;
		
		DictionaryList<int,MidiMessage> notes;
		
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
			public int SongStart { get;set; }
			public int Bar { get;set; }
			public int Length { get;set; }
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
		
		string MouseString { get { return string.Format( "{0:00#.000}x{1:000}", HoverPoint.X / 4, HoverPoint.Y ); } }
		
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
			isMouseDown = true;
			Focused = true;
			MousePoint = PointToClient(MousePosition);
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
			isMouseDown = false;
			MouseTrail = new FloatPoint(0);
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
		
		
		#region Drawing Layers (not in background)
		void GetForeground_NewBars(Graphics g, MidiPianoView.Needs n)
		{
			FloatRect grid = Ren.ui.ClientRect;
//			Ren.ui.NodeMax;
			grid = null;
		}
		
		void GetForeground_Caption(Graphics g, Needs n)
		{
			if (n.HasFlag(Needs.Mouse)) {
				float line = GetLineHeight(g);
				using (SolidBrush b0 = new SolidBrush(Color.FromArgb(80,Color.Black)))
					using (SolidBrush b1 = new SolidBrush(Color.FromArgb(127, Color.DodgerBlue)))
				{
					g.DrawString(MeasureString, Ren.NoteFont, b0, MouseInfo.X+shadowx, GutterYOffset.Y-line-line-line+shadowy);
					g.DrawString(MouseString,   Ren.NoteFont, b0, MouseInfo.X+shadowx, GutterYOffset.Y-line-line+shadowy);
					
					g.DrawString(MeasureString, Ren.NoteFont, b1, MouseInfo.X, GutterYOffset.Y-line-line-line);
					g.DrawString(MouseString,   Ren.NoteFont, b1, MouseInfo.X, GutterYOffset.Y-line-line);
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
		
		class Ren : MidiPianoViewRenderer
		{
			void DrawNote(Graphics g, MidiNote note)
			{
			}
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
			this.vScrollBar1 = new System.Windows.Forms.TrackBar();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label5 = new System.Windows.Forms.Label();
			this.numBar = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.numLen = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.numOff = new System.Windows.Forms.NumericUpDown();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.vScrollBar1)).BeginInit();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numLen)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numOff)).BeginInit();
			this.SuspendLayout();
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			                                                                | System.Windows.Forms.AnchorStyles.Right)));
			this.vScrollBar1.AutoSize = false;
			this.vScrollBar1.LargeChange = 12;
			this.vScrollBar1.Location = new System.Drawing.Point(475, 55);
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
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.flowLayoutPanel1.ForeColor = System.Drawing.Color.Yellow;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(491, 22);
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
			// MidiPianoView
			// 
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this.vScrollBar1);
			this.Font = new System.Drawing.Font("Consolas", 8.25F);
			this.Name = "MidiPianoView";
			this.Size = new System.Drawing.Size(491, 314);
			((System.ComponentModel.ISupportInitialize)(this.vScrollBar1)).EndInit();
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numLen)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numOff)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
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
		
		
		void Event_BarLocation(object sender, EventArgs e)
		{
			BarPosition = new MidiPianoView.setting(){
				SongStart = Convert.ToInt32(Math.Floor(numOff.Value)),
				Bar				= Convert.ToInt32(Math.Floor(numBar.Value)),
				Length		= Convert.ToInt32(Math.Floor(numLen.Value))
			};
		}
	}

}
