using System;
using System.Windows.Forms;

namespace Display_Streamer
{
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
            foreach (Screen screen in Screen.AllScreens)
            {
                // Device name - not in order
                // WorkingArea - excluding taskbars, docked windows, and docked tool bars
                comboBox1.Items.Add(screen.DeviceName);
                comboBox1.SelectedIndex = 0;
            }

            comboBox2.Items.Add("1s");
            comboBox2.Items.Add("2s");
            comboBox2.Items.Add("3s");
            comboBox2.SelectedIndex = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Config.refresh_rate = (comboBox2.SelectedIndex + 1) * 1000;
            //Hide the Form
            this.Hide();
            System.Threading.Thread.Sleep(250);
            Config.main_form = this;

            Capture capture = new Capture(Screen.AllScreens[comboBox1.SelectedIndex]);
            capture.Show();

        }
    }
}
