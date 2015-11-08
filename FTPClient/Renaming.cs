using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTPClient
{
    public partial class Renaming : Form
    {
        public FTPViewer f;
        public Renaming()
        {
            InitializeComponent();
        }

        private void newNameBttn_Click(object sender, EventArgs e)
        {
            if (newNameTB.Text != string.Empty)
            {
                f.name = newNameTB.Text;
                Close();
            }
            else
                MessageBox.Show("Enter the name!", "Error", MessageBoxButtons.OK);
        }
    }
}
