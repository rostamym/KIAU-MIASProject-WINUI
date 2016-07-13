using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DicomImageViewer
{
    public partial class DicInfo : Form
    {
        string[] _info;
        public DicInfo(string[] info)
        {
            InitializeComponent();
            _info = info;
        }

        private void DicInfo_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (string s in _info)
            {
                listBox1.Items.Add(s);
            }
        }
    }
}
