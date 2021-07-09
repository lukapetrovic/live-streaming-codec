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
                comboBox1.Items.Add(screen.DeviceName);
                comboBox1.SelectedIndex = 0;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Hide the Form
            this.Hide();
            System.Threading.Thread.Sleep(250);

            Capture capture = new Capture(Screen.AllScreens[comboBox1.SelectedIndex]);
            capture.Show();

        }
    }
}
