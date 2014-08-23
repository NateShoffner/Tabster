﻿#region

using System;
using System.Windows.Forms;
using Tabster.Core.Data;
using Tabster.Core.Types;
using Tabster.Utilities;

#endregion

namespace Tabster.Forms
{
    public partial class NewTabDialog : Form
    {
        public NewTabDialog()
        {
            InitializeComponent();

            txtArtist.Text = Environment.UserName;
            txtArtist.Select(txtArtist.Text.Length, 0);
        }

        public NewTabDialog(string artist, string song, TabType type)
            : this()
        {
            txtArtist.Text = artist;
            txtTitle.Text = song;
            typeList.SelectedType = type;

            ValidateInput();
        }

        public TablatureDocument Tab { get; private set; }

        private void ValidateInput(object sender = null, EventArgs e = null)
        {
            okbtn.Enabled = okbtn.Enabled = txtArtist.Text.Trim().Length > 0 && txtTitle.Text.Trim().Length > 0 ;
        }

        private void okbtn_Click(object sender, EventArgs e)
        {
            Tab = new TablatureDocument(txtArtist.Text.Trim(), txtTitle.Text.Trim(), typeList.SelectedType, "")
                      {
                          SourceType = TablatureSourceType.UserCreated,
                          Method = Common.GetTablatureDocumentMethodString()
                      };
        }
    }
}