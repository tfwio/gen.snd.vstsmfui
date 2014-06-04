using System;
using System.Collections.Generic;
using DspAudio;
using DspAudio.Midi;
using DspAudio.Vst;
using DspAudio.Vst.Module;
using Jacobi.Vst.Core;

namespace modest100.Forms
{
	// Cayete la boca!
	public class MidiVstPluginListView : MidiControlBase
	{
		const string label_format = "Rate: {0:N0}, Tempo: {1}, MBQT: {2}, Time: {3:hh:mm:ss:ttt}, Quarters: {4:N4}";
		
		#region Properties
		
		VstPluginManager PluginManager { get { return UserInterface.VstContainer.PluginManager; } }
		INaudioVstContainer VstContainer { get { return UserInterface.VstContainer; } }
		HostCommandStub Host { get { return VstContainer.VstHost; } }
		bool HasPlayer { get { return VstContainer.VstPlayer != null; } }
		
		readonly object threadlock = new object();
		
		SampleClock s2mbt = new SampleClock();
		
		#endregion
		
		public MidiVstPluginListView()
		{
			InitializeComponent();
			WindowsInterop.WindowsTheme.HandleTheme(this.PluginListVw);
		}
		
		#region .
		#endregion
		#region .
		#endregion
  #region .....
  
  	/* 
  	 * All of these methods are re-directed to the VstListView class.
  	 */
		void Event_PluginSelected(object sender, EventArgs e)  { VstListView.HandlePluginSelected(PluginListVw,PluginManager); }
		void Event_PluginReload(object sender, EventArgs e)    { VstListView.HandlePluginReload(PluginListVw,PluginManager); }
		void Event_PluginListReset(object sender, EventArgs e) { VstListView.ItemsRefresh(PluginListVw, PluginManager); }
		
		#endregion
	  #region .
		#endregion
	  #region .
		#endregion
		
		/// <inheritdoc/>
		public override void SetUI(IMidiParserUI ui)
		{
			// 
			base.SetUI(ui);
			// 
			// PluginManager = new VstPluginManager(VstContainer);
			PluginListVw.ItemSelectionChanged += Event_PluginSelected;
			PluginManager.PluginListRefreshed += Event_PluginListReset;
			PluginManager.ActivePluginReset += Event_PluginReload;
		}
		
		#region Windows Form Designer generated code
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			PluginManager.ReleaseAllPlugins();
//			PluginManager = null;
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PluginListVw = new System.Windows.Forms.ListView();
			this.NameHdr = new System.Windows.Forms.ColumnHeader();
			this.ProductHdr = new System.Windows.Forms.ColumnHeader();
			this.VendorHdr = new System.Windows.Forms.ColumnHeader();
			this.VersionHdr = new System.Windows.Forms.ColumnHeader();
			this.AssemblyHdr = new System.Windows.Forms.ColumnHeader();
			this.OpenFileDlg = new System.Windows.Forms.OpenFileDialog();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this.numBank = new System.Windows.Forms.NumericUpDown();
			this.numPatch = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.cbActiveParams = new System.Windows.Forms.ComboBox();
			this.numParamValue = new System.Windows.Forms.NumericUpDown();
			this.lblParamValue = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.numCC01 = new System.Windows.Forms.NumericUpDown();
			this.numCC02 = new System.Windows.Forms.NumericUpDown();
			this.numCC03 = new System.Windows.Forms.NumericUpDown();
			this.btnCC = new System.Windows.Forms.Button();
			this.label5 = new System.Windows.Forms.Label();
			this.numNrpnMsb = new System.Windows.Forms.NumericUpDown();
			this.numD1 = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.btnSendRpn = new System.Windows.Forms.Button();
			this.btnSendNrpn = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.ckIgnoreMidiPatch = new System.Windows.Forms.CheckBox();
			this.flowLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBank)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numPatch)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numParamValue)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC01)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC02)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC03)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numNrpnMsb)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numD1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// PluginListVw
			// 
			this.PluginListVw.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.PluginListVw.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			                                   	this.NameHdr,
			                                   	this.ProductHdr,
			                                   	this.VendorHdr,
			                                   	this.VersionHdr,
			                                   	this.AssemblyHdr});
			this.PluginListVw.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PluginListVw.FullRowSelect = true;
			this.PluginListVw.HideSelection = false;
			this.PluginListVw.Location = new System.Drawing.Point(0, 0);
			this.PluginListVw.MultiSelect = false;
			this.PluginListVw.Name = "PluginListVw";
			this.PluginListVw.Size = new System.Drawing.Size(491, 420);
			this.PluginListVw.TabIndex = 0;
			this.PluginListVw.UseCompatibleStateImageBehavior = false;
			this.PluginListVw.View = System.Windows.Forms.View.Details;
			// 
			// NameHdr
			// 
			this.NameHdr.Text = "Name";
			this.NameHdr.Width = 146;
			// 
			// ProductHdr
			// 
			this.ProductHdr.DisplayIndex = 2;
			this.ProductHdr.Text = "Product";
			this.ProductHdr.Width = 120;
			// 
			// VendorHdr
			// 
			this.VendorHdr.DisplayIndex = 3;
			this.VendorHdr.Text = "Vendor";
			this.VendorHdr.Width = 120;
			// 
			// VersionHdr
			// 
			this.VersionHdr.DisplayIndex = 1;
			this.VersionHdr.Text = "Version";
			// 
			// AssemblyHdr
			// 
			this.AssemblyHdr.Text = "Assemlby";
			this.AssemblyHdr.Width = 120;
			// 
			// OpenFileDlg
			// 
			this.OpenFileDlg.Filter = "Plugins (*.dll)|*.dll|All Files (*.*)|*.*";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.flowLayoutPanel1.SetFlowBreak(this.comboBox1, true);
			this.comboBox1.Font = new System.Drawing.Font("Consolas", 8F);
			this.comboBox1.Location = new System.Drawing.Point(11, 110);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(283, 21);
			this.comboBox1.TabIndex = 1;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.AutoScroll = true;
			this.flowLayoutPanel1.Controls.Add(this.label2);
			this.flowLayoutPanel1.Controls.Add(this.numBank);
			this.flowLayoutPanel1.Controls.Add(this.numPatch);
			this.flowLayoutPanel1.Controls.Add(this.ckIgnoreMidiPatch);
			this.flowLayoutPanel1.Controls.Add(this.label4);
			this.flowLayoutPanel1.Controls.Add(this.numParamValue);
			this.flowLayoutPanel1.Controls.Add(this.lblParamValue);
			this.flowLayoutPanel1.Controls.Add(this.button1);
			this.flowLayoutPanel1.Controls.Add(this.cbActiveParams);
			this.flowLayoutPanel1.Controls.Add(this.comboBox1);
			this.flowLayoutPanel1.Controls.Add(this.label1);
			this.flowLayoutPanel1.Controls.Add(this.numCC01);
			this.flowLayoutPanel1.Controls.Add(this.numCC02);
			this.flowLayoutPanel1.Controls.Add(this.numCC03);
			this.flowLayoutPanel1.Controls.Add(this.btnCC);
			this.flowLayoutPanel1.Controls.Add(this.label5);
			this.flowLayoutPanel1.Controls.Add(this.numNrpnMsb);
			this.flowLayoutPanel1.Controls.Add(this.numD1);
			this.flowLayoutPanel1.Controls.Add(this.btnSendRpn);
			this.flowLayoutPanel1.Controls.Add(this.btnSendNrpn);
			this.flowLayoutPanel1.Controls.Add(this.label6);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Font = new System.Drawing.Font("Consolas", 8F);
			this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(8);
			this.flowLayoutPanel1.Size = new System.Drawing.Size(306, 420);
			this.flowLayoutPanel1.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Consolas", 8F);
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Margin = new System.Windows.Forms.Padding(0);
			this.label2.Name = "label2";
			this.label2.Padding = new System.Windows.Forms.Padding(4, 6, 0, 0);
			this.label2.Size = new System.Drawing.Size(29, 19);
			this.label2.TabIndex = 1;
			this.label2.Text = "BNK";
			// 
			// numBank
			// 
			this.numBank.Location = new System.Drawing.Point(40, 11);
			this.numBank.Maximum = new decimal(new int[] {
			                                   	255,
			                                   	0,
			                                   	0,
			                                   	0});
			this.numBank.Name = "numBank";
			this.numBank.Size = new System.Drawing.Size(40, 20);
			this.numBank.TabIndex = 2;
			this.numBank.Value = new decimal(new int[] {
			                                 	127,
			                                 	0,
			                                 	0,
			                                 	0});
			// 
			// numPatch
			// 
			this.numPatch.Font = new System.Drawing.Font("Consolas", 8F);
			this.numPatch.Location = new System.Drawing.Point(86, 11);
			this.numPatch.Maximum = new decimal(new int[] {
			                                    	255,
			                                    	0,
			                                    	0,
			                                    	0});
			this.numPatch.Name = "numPatch";
			this.numPatch.Size = new System.Drawing.Size(39, 20);
			this.numPatch.TabIndex = 0;
			this.numPatch.Value = new decimal(new int[] {
			                                  	127,
			                                  	0,
			                                  	0,
			                                  	0});
			// 
			// label4
			// 
			this.flowLayoutPanel1.SetFlowBreak(this.label4, true);
			this.label4.Font = new System.Drawing.Font("Consolas", 8F);
			this.label4.Location = new System.Drawing.Point(8, 34);
			this.label4.Margin = new System.Windows.Forms.Padding(0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(229, 19);
			this.label4.TabIndex = 1;
			this.label4.Text = "(program name)";
			// 
			// cbActiveParams
			// 
			this.cbActiveParams.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.flowLayoutPanel1.SetFlowBreak(this.cbActiveParams, true);
			this.cbActiveParams.Font = new System.Drawing.Font("Consolas", 8F);
			this.cbActiveParams.Location = new System.Drawing.Point(68, 82);
			this.cbActiveParams.Name = "cbActiveParams";
			this.cbActiveParams.Size = new System.Drawing.Size(226, 21);
			this.cbActiveParams.TabIndex = 0;
			this.cbActiveParams.SelectedIndexChanged += new System.EventHandler(this.Event_ComboChanged);
			// 
			// numParamValue
			// 
			this.numParamValue.DecimalPlaces = 5;
			this.numParamValue.Font = new System.Drawing.Font("Consolas", 8F);
			this.numParamValue.Increment = new decimal(new int[] {
			                                           	1,
			                                           	0,
			                                           	0,
			                                           	131072});
			this.numParamValue.Location = new System.Drawing.Point(11, 56);
			this.numParamValue.Name = "numParamValue";
			this.numParamValue.Size = new System.Drawing.Size(81, 20);
			this.numParamValue.TabIndex = 4;
			this.numParamValue.ThousandsSeparator = true;
			// 
			// lblParamValue
			// 
			this.lblParamValue.AutoSize = true;
			this.flowLayoutPanel1.SetFlowBreak(this.lblParamValue, true);
			this.lblParamValue.Font = new System.Drawing.Font("Consolas", 8F);
			this.lblParamValue.Location = new System.Drawing.Point(95, 53);
			this.lblParamValue.Margin = new System.Windows.Forms.Padding(0);
			this.lblParamValue.Name = "lblParamValue";
			this.lblParamValue.Padding = new System.Windows.Forms.Padding(4, 6, 0, 0);
			this.lblParamValue.Size = new System.Drawing.Size(125, 19);
			this.lblParamValue.TabIndex = 1;
			this.lblParamValue.Text = "(value description)";
			// 
			// button1
			// 
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.button1.Font = new System.Drawing.Font("Consolas", 8F);
			this.button1.Location = new System.Drawing.Point(11, 82);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(51, 22);
			this.button1.TabIndex = 6;
			this.button1.Text = "check";
			this.button1.UseVisualStyleBackColor = false;
			this.button1.Click += new System.EventHandler(this.Button1Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.flowLayoutPanel1.SetFlowBreak(this.label1, true);
			this.label1.Font = new System.Drawing.Font("Consolas", 8F);
			this.label1.Location = new System.Drawing.Point(8, 134);
			this.label1.Margin = new System.Windows.Forms.Padding(0);
			this.label1.Name = "label1";
			this.label1.Padding = new System.Windows.Forms.Padding(4, 6, 0, 0);
			this.label1.Size = new System.Drawing.Size(83, 19);
			this.label1.TabIndex = 1;
			this.label1.Text = "Send MIDI CC";
			// 
			// numCC01
			// 
			this.numCC01.Font = new System.Drawing.Font("Consolas", 8F);
			this.numCC01.Location = new System.Drawing.Point(11, 163);
			this.numCC01.Maximum = new decimal(new int[] {
			                                   	255,
			                                   	0,
			                                   	0,
			                                   	0});
			this.numCC01.Name = "numCC01";
			this.numCC01.Size = new System.Drawing.Size(47, 20);
			this.numCC01.TabIndex = 4;
			this.numCC01.ValueChanged += new System.EventHandler(this.BtnCCClick);
			// 
			// numCC02
			// 
			this.numCC02.Font = new System.Drawing.Font("Consolas", 8F);
			this.numCC02.Location = new System.Drawing.Point(64, 163);
			this.numCC02.Maximum = new decimal(new int[] {
			                                   	255,
			                                   	0,
			                                   	0,
			                                   	0});
			this.numCC02.Name = "numCC02";
			this.numCC02.Size = new System.Drawing.Size(47, 20);
			this.numCC02.TabIndex = 4;
			this.numCC02.ValueChanged += new System.EventHandler(this.BtnCCClick);
			// 
			// numCC03
			// 
			this.numCC03.Font = new System.Drawing.Font("Consolas", 8F);
			this.numCC03.Location = new System.Drawing.Point(117, 163);
			this.numCC03.Maximum = new decimal(new int[] {
			                                   	255,
			                                   	0,
			                                   	0,
			                                   	0});
			this.numCC03.Name = "numCC03";
			this.numCC03.Size = new System.Drawing.Size(47, 20);
			this.numCC03.TabIndex = 4;
			// 
			// btnCC
			// 
			this.btnCC.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.flowLayoutPanel1.SetFlowBreak(this.btnCC, true);
			this.btnCC.Font = new System.Drawing.Font("Consolas", 8F);
			this.btnCC.Location = new System.Drawing.Point(170, 163);
			this.btnCC.Name = "btnCC";
			this.btnCC.Size = new System.Drawing.Size(51, 20);
			this.btnCC.TabIndex = 7;
			this.btnCC.Text = "send";
			this.btnCC.UseVisualStyleBackColor = false;
			this.btnCC.Click += new System.EventHandler(this.BtnCCClick);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.flowLayoutPanel1.SetFlowBreak(this.label5, true);
			this.label5.Font = new System.Drawing.Font("Consolas", 8F);
			this.label5.Location = new System.Drawing.Point(8, 186);
			this.label5.Margin = new System.Windows.Forms.Padding(0);
			this.label5.Name = "label5";
			this.label5.Padding = new System.Windows.Forms.Padding(4, 6, 0, 0);
			this.label5.Size = new System.Drawing.Size(35, 19);
			this.label5.TabIndex = 1;
			this.label5.Text = "[cc]";
			// 
			// numNrpnMsb
			// 
			this.numNrpnMsb.Font = new System.Drawing.Font("Consolas", 8F);
			this.numNrpnMsb.Location = new System.Drawing.Point(11, 215);
			this.numNrpnMsb.Maximum = new decimal(new int[] {
			                                      	65535,
			                                      	0,
			                                      	0,
			                                      	0});
			this.numNrpnMsb.Name = "numNrpnMsb";
			this.numNrpnMsb.Size = new System.Drawing.Size(47, 20);
			this.numNrpnMsb.TabIndex = 4;
			// 
			// numD1
			// 
			this.numD1.Font = new System.Drawing.Font("Consolas", 8F);
			this.numD1.Location = new System.Drawing.Point(64, 215);
			this.numD1.Maximum = new decimal(new int[] {
			                                 	65535,
			                                 	0,
			                                 	0,
			                                 	0});
			this.numD1.Name = "numD1";
			this.numD1.Size = new System.Drawing.Size(47, 20);
			this.numD1.TabIndex = 4;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.flowLayoutPanel1.SetFlowBreak(this.label6, true);
			this.label6.Font = new System.Drawing.Font("Consolas", 8F);
			this.label6.Location = new System.Drawing.Point(8, 238);
			this.label6.Margin = new System.Windows.Forms.Padding(0);
			this.label6.Name = "label6";
			this.label6.Padding = new System.Windows.Forms.Padding(4, 6, 0, 0);
			this.label6.Size = new System.Drawing.Size(35, 19);
			this.label6.TabIndex = 8;
			this.label6.Text = "[cc]";
			// 
			// btnSendRpn
			// 
			this.btnSendRpn.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnSendRpn.Font = new System.Drawing.Font("Consolas", 8F);
			this.btnSendRpn.Location = new System.Drawing.Point(117, 215);
			this.btnSendRpn.Name = "btnSendRpn";
			this.btnSendRpn.Size = new System.Drawing.Size(74, 20);
			this.btnSendRpn.TabIndex = 9;
			this.btnSendRpn.Text = "rpn:send";
			this.btnSendRpn.UseVisualStyleBackColor = false;
			this.btnSendRpn.Click += new System.EventHandler(this.GetRpnMessage);
			// 
			// btnSendNrpn
			// 
			this.btnSendNrpn.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.flowLayoutPanel1.SetFlowBreak(this.btnSendNrpn, true);
			this.btnSendNrpn.Font = new System.Drawing.Font("Consolas", 8F);
			this.btnSendNrpn.Location = new System.Drawing.Point(197, 215);
			this.btnSendNrpn.Name = "btnSendNrpn";
			this.btnSendNrpn.Size = new System.Drawing.Size(74, 20);
			this.btnSendNrpn.TabIndex = 9;
			this.btnSendNrpn.Text = "nrpn:send";
			this.btnSendNrpn.UseVisualStyleBackColor = false;
			this.btnSendNrpn.Click += new System.EventHandler(this.GetNrpnMessage);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.PluginListVw);
			this.splitContainer1.Size = new System.Drawing.Size(801, 420);
			this.splitContainer1.SplitterDistance = 306;
			this.splitContainer1.TabIndex = 4;
			// 
			// ckIgnoreMidiPatch
			// 
			this.ckIgnoreMidiPatch.AutoSize = true;
			this.ckIgnoreMidiPatch.Location = new System.Drawing.Point(131, 11);
			this.ckIgnoreMidiPatch.Name = "ckIgnoreMidiPatch";
			this.ckIgnoreMidiPatch.Size = new System.Drawing.Size(128, 17);
			this.ckIgnoreMidiPatch.TabIndex = 10;
			this.ckIgnoreMidiPatch.Text = "Ignore MIDI Patch";
			this.ckIgnoreMidiPatch.UseVisualStyleBackColor = true;
			this.ckIgnoreMidiPatch.CheckedChanged += new System.EventHandler(this.Event_IgnoreMidiPatchChange);
			// 
			// VstPluginList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "VstPluginList";
			this.Size = new System.Drawing.Size(801, 420);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.flowLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numBank)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numPatch)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numParamValue)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC01)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC02)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCC03)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numNrpnMsb)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numD1)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.CheckBox ckIgnoreMidiPatch;
		private System.Windows.Forms.Button btnSendNrpn;
		private System.Windows.Forms.Button btnSendRpn;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numD1;
		private System.Windows.Forms.NumericUpDown numNrpnMsb;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numCC03;
		private System.Windows.Forms.Button btnCC;
		private System.Windows.Forms.NumericUpDown numCC02;
		private System.Windows.Forms.NumericUpDown numCC01;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblParamValue;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.NumericUpDown numParamValue;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.ComboBox cbActiveParams;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.NumericUpDown numPatch;
		private System.Windows.Forms.NumericUpDown numBank;
		private System.Windows.Forms.ListView PluginListVw;
		private System.Windows.Forms.ColumnHeader NameHdr;
		private System.Windows.Forms.ColumnHeader VersionHdr;
		private System.Windows.Forms.ColumnHeader ProductHdr;
		private System.Windows.Forms.ColumnHeader VendorHdr;
		private System.Windows.Forms.ColumnHeader AssemblyHdr;
		private System.Windows.Forms.OpenFileDialog OpenFileDlg;
		
		#endregion
		
		#region PARA
		
		int currentProgram = 0, currentParam = 0;
		bool NoActivePlugin { get { return PluginManager.ActivePlugin==null; } }
		
		void Event_ComboChanged(object sender, EventArgs e)
		{
			if (NoActivePlugin) return;
			this.numParamValue.ValueChanged -= Event_ParamGetSet;
			currentProgram = (cbActiveParams.SelectedItem as VstCCParam).ProgramID;
			currentParam = (cbActiveParams.SelectedItem as VstCCParam).ID;
			Param2UpDown();
			this.numParamValue.ValueChanged += Event_ParamGetSet;
		}
		void Button1Click(object sender, EventArgs e)
		{
			if (NoActivePlugin) return;
			var nfo=new MidiVstPluginListView.ParamInfo();
			this.numParamValue.ValueChanged -= Event_ParamGetSet;
			nfo.ParamTest(cbActiveParams,PluginManager.ActivePlugin);
			try { Param2UpDown(); } catch {}
			this.numParamValue.ValueChanged += Event_ParamGetSet;
			nfo = null;
		}
		void Event_ParamGetSet(object sender, EventArgs e)
		{
			if (NoActivePlugin) return;
			try { UpDownToParam(); } catch {}
		}
		
		/// from parameter to updown
		void Param2UpDown()
		{
			var param = cbActiveParams.SelectedItem as VstCCParam;
			// set updown value
			numParamValue.Value = Convert.ToDecimal(param.Value);
			// set label text
			lblParamValue.Text = string.Format("{0}, {1}",param.Display,param.Label);
		}
		
		/// from numeric updown to parameter
		void UpDownToParam()
		{
			var param = cbActiveParams.SelectedItem as VstCCParam;
			// set param value
			param.Value = Convert.ToSingle(numParamValue.Value);
			// set label text
			lblParamValue.Text = string.Format("{0}, {1}",param.Display,param.Label);
		}
		
		#endregion
		
		#region ParameterInfo Class
		class ParamInfo
		{
			IEnumerable<int> ParameterCount(VstPlugin plugin) {
				for (int i = 0; i < plugin.PluginInfo.ParameterCount; i++)
					yield return i;
			}
			public void ParamTest(System.Windows.Forms.ComboBox cbActiveParams, VstPlugin plugin)
			{
				cbActiveParams.Items.Clear();
				var ccp = new VstCCPgm(plugin,plugin.ProgramIndex);
				foreach (int i in ParameterCount(plugin))
				{
					VstCCParam para = ccp[i];
					cbActiveParams.Items.Add(para);
				}
			}
		}
		#endregion

		#region Paremeter Event Handlers
		void BtnCCClick(object sender, EventArgs e)
		{
			if (PluginManager.ActivePlugin==null) return;
			byte[] testvalue = new byte[2];
//			testvalue = BitConverter.GetBytes(Convert.ToSingle(numCC02.Value),);
			byte[] bitevent = new byte[4]{ 0xB0, Convert.ToByte(numCC01.Value), Convert.ToByte( numCC02.Value ), 0 };
			label5.Text = string.Format("{0:X2}{1:X2}{2:X2}{3:X2}",0xB0, Convert.ToByte(numCC01.Value), Convert.ToByte(  numCC02.Value), 0);
			PluginManager.ActivePlugin.PluginCommandStub.ProcessEvents(new VstEvent[]{new VstMidiEvent(0,0,0,bitevent,0,0)}
			                                                          );
		}
		void GetRpnMessage(object sender, EventArgs e)
		{
			var msg = RpnNrpn.FromInt(numNrpnMsb.Value.ToInt32(),numD1.Value.ToInt32());
			PluginManager.ActivePlugin.PluginCommandStub.ProcessEvents(msg.GetRpnEvents());
			label6.Text = string.Format("{0}\n{1}\n{2}\n{3}",msg.StrRpnLsb,msg.StrRpnMsb,msg.StrRpnLsbData,msg.StrRpnMsbData);
		}
		void GetNrpnMessage(object sender, EventArgs e)
		{
			var msg = RpnNrpn.FromInt(numNrpnMsb.Value.ToInt32(),numD1.Value.ToInt32());
			PluginManager.ActivePlugin.PluginCommandStub.ProcessEvents(msg.GetNrpnEvents());
			label6.Text = string.Format("{0}\n{1}\n{2}\n{3}",msg.StrNrpnLsb,msg.StrNrpmMsb,msg.StrNrpnLsbData,msg.StrNrpnMsbData);
		}
		#endregion
		
		void Event_IgnoreMidiPatchChange(object sender, EventArgs e)
		{
			if (PluginManager.MasterPluginInstrument!=null)
			{
				PluginManager.MasterPluginInstrument.IgnoreMidiProgramChange = ckIgnoreMidiPatch.Checked;
			}
		}
		
	}

}
