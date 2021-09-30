using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Display_Streamer
{

    public partial class Dashboard : Form
    {
        List<DeviceInfo> screens = MonitorHelper.getScreenDevices();

        public Dashboard()
        {
            InitializeComponent();

            
            for (int i = 0; i < screens.Count; i++)
            {
                comboBox1.Items.Add(screens[i].DeviceName);
            }
            comboBox1.SelectedIndex = 0;

            comboBox2.Items.Add("1/2 fps");
            comboBox2.Items.Add("1 fps");
            comboBox2.Items.Add("1.5 fps");
            comboBox2.Items.Add("2 fps");
            comboBox2.SelectedIndex = 1;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Config.refresh_rate = 500 * (comboBox2.SelectedIndex + 1);
            //Hide the Form
            this.Hide();
            System.Threading.Thread.Sleep(250);
            Config.main_form = this;

            Capture capture = new Capture(screens[comboBox1.SelectedIndex]);
            capture.Show();

        }
    }
}
