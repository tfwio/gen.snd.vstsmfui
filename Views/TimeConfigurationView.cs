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
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using modest100.Forms;

namespace modest100.Views
{
	/// <summary>
	/// Description of TimeConfigurationControl.
	/// </summary>
	public partial class TimeConfigurationView : MidiControlBase
	{
		LatencyCalculator setting = new LatencyCalculator();
		double SampleRate { get { return double.Parse(comboSampleRate.Text ?? "0"); } }
		
		public TimeConfigurationView()
		{
			InitializeComponent();
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.comboSampleRate.Items.AddRange(
				new object[] {
					22050,
					44100,
					48000,
					96000});
			comboSampleRate.SelectedValue = 48000;
			this.comboSampleRate.SelectedIndexChanged += new System.EventHandler(this.Event_ValueChanged);
		}
		
		void Event_ValueChanged(object sender, EventArgs e)
		{
			bool wasPlaying = false;
			if (SampleRate != DspAudio.TimeConfiguration.Instance.Rate)
			{
				
				if (this.UserInterface.VstContainer.VstPlayer.IsRunning) {
					this.UserInterface.VstContainer.VstPlayer.Stop();
					wasPlaying = true;
				}
			}
			
			DspAudio.TimeConfiguration.Instance.Rate = Convert.ToInt32(SampleRate);
			DspAudio.TimeConfiguration.Instance.Latency = Convert.ToInt32(setting.LatencyInMilliseconds);
			
			setting.ResetValue(int.Parse(comboSampleRate.Text),Convert.ToInt32(numSamples.Value));
			labelMs.Text = string.Format("{0:N} ms",setting.LatencyInMilliseconds);
			if (wasPlaying) this.UserInterface.VstContainer.VstPlayer.Play();
		}
	}


}
