#region User/License
// oio * 10/20/2012 * 8:50 AM

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
namespace modest100.Views
{
	partial class TimeConfigurationView
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the control.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.labelMs = new System.Windows.Forms.Label();
			this.numSamples = new System.Windows.Forms.NumericUpDown();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.comboSampleRate = new System.Windows.Forms.ComboBox();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numSamples)).BeginInit();
			this.groupBox3.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.labelMs);
			this.groupBox2.Controls.Add(this.numSamples);
			this.groupBox2.Location = new System.Drawing.Point(123, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(222, 46);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Samples";
			// 
			// labelMs
			// 
			this.labelMs.AutoSize = true;
			this.labelMs.Location = new System.Drawing.Point(119, 22);
			this.labelMs.Name = "labelMs";
			this.labelMs.Size = new System.Drawing.Size(0, 13);
			this.labelMs.TabIndex = 1;
			// 
			// numSamples
			// 
			this.numSamples.Increment = new decimal(new int[] {
									64,
									0,
									0,
									0});
			this.numSamples.Location = new System.Drawing.Point(6, 19);
			this.numSamples.Maximum = new decimal(new int[] {
									9000000,
									0,
									0,
									0});
			this.numSamples.Minimum = new decimal(new int[] {
									256,
									0,
									0,
									0});
			this.numSamples.Name = "numSamples";
			this.numSamples.Size = new System.Drawing.Size(102, 20);
			this.numSamples.TabIndex = 0;
			this.numSamples.Value = new decimal(new int[] {
									1024,
									0,
									0,
									0});
			this.numSamples.ValueChanged += new System.EventHandler(this.Event_ValueChanged);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.comboSampleRate);
			this.groupBox3.Location = new System.Drawing.Point(3, 3);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(114, 46);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Sample-Rate";
			// 
			// comboSampleRate
			// 
			this.comboSampleRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboSampleRate.FormattingEnabled = true;
			this.comboSampleRate.Location = new System.Drawing.Point(7, 19);
			this.comboSampleRate.Name = "comboSampleRate";
			this.comboSampleRate.Size = new System.Drawing.Size(101, 21);
			this.comboSampleRate.TabIndex = 0;
			// 
			// TimeConfigurationView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox3);
			this.Name = "TimeConfigurationView";
			this.Size = new System.Drawing.Size(352, 184);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numSamples)).EndInit();
			this.groupBox3.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.Label labelMs;
		private System.Windows.Forms.ComboBox comboSampleRate;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.NumericUpDown numSamples;
		private System.Windows.Forms.GroupBox groupBox2;
	}
}
