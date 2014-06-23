﻿#region

using System.Diagnostics;
using System.Windows.Forms;
using Tabster.Properties;

#endregion

namespace Tabster.Forms
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            Text = string.Format("Tabster v{0}", Common.TruncateVersion(Application.ProductVersion));
            lblname.Text = string.Format("Tabster {0}", Common.TruncateVersion(Application.ProductVersion));
            pictureBox1.Image = Resources.guitar128;
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(((LinkLabel) sender).Text);
        }
    }
}