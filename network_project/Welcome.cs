using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace network_project
{
    public partial class Welcome : Form
    {
        public Welcome()
        {
            InitializeComponent();
        }
        //Tạo game mới
        private void btnNewgame_Click(object sender, EventArgs e)
        {
            Form1 newForm = new Form1();           
            newForm.Show();
        }
        //Thoát game
        private void btnQuit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
