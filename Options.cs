using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Edamame.ClipboardPlugin.Properties;
using Edamame.ClipboardPlugin;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;

namespace AudioDataPlugIn
{

    public partial class Options : Form
    {
        public Options()
        {
            this.InitializeComponent();
        }

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start(linkLabel1.Text);
		}

		private void Options_Load(object sender, EventArgs e)
		{
			this.Icon = Resources.ctdb;
		}

    }
}
