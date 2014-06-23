﻿#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Tabster.Core;
using Tabster.UltimateGuitar;
using TabType = Tabster.UltimateGuitar.TabType;

#endregion

namespace Tabster.Forms
{
    partial class MainForm
    {
        private readonly Dictionary<Uri, UltimateGuitarTab> _ugTabCache = new Dictionary<Uri, UltimateGuitarTab>();
        private readonly SearchManager searchManager = new SearchManager();

        private SearchResult SelectedSearchResult()
        {
            var selectedURL = searchDisplay.SelectedRows.Count > 0 ? new Uri(searchDisplay.SelectedRows[0].Tag.ToString()) : null;
            return selectedURL != null ? searchManager.Find(x => x.URL.Equals(selectedURL)) : null;
        }

        private void onlinesearchbtn_Click(object sender, EventArgs e)
        {
            if (txtsearchartist.Text.Trim().Length > 0 || txtsearchsong.Text.Trim().Length > 0)
            {
                pictureBox1.Visible = true;

                searchManager.Artist = txtsearchartist.Text;
                searchManager.Title = txtsearchsong.Text;

                var searchType = TabType.Undefined;

                switch (txtsearchtype.SelectedIndex)
                {
                    case 0:
                        searchType = TabType.Undefined;
                        break;
                    case 1:
                        searchType = TabType.GuitarTab;
                        break;
                    case 2:
                        searchType = TabType.GuitarChords;
                        break;
                    case 3:
                        searchType = TabType.BassTab;
                        break;
                    case 4:
                        searchType = TabType.DrumTab;
                        break;
                    case 5:
                        searchType = TabType.Ukulele;
                        break;
                }

                searchManager.Type = searchType;

                searchManager.Search();
            }
        }

        private void dataGridViewExtended1_MouseClick(object sender, MouseEventArgs e)
        {
            var currentMouseOverRow = searchDisplay.HitTest(e.X, e.Y).RowIndex;

            if (e.Button == MouseButtons.Right && (currentMouseOverRow >= 0 && currentMouseOverRow < searchDisplay.Rows.Count))
            {
                searchDisplay.Rows[currentMouseOverRow].Selected = true;
                SearchMenu.Show(searchDisplay.PointToScreen(e.Location));
            }
        }

        private void dataGridViewExtended1_SelectionChanged(object sender, EventArgs e)
        {
            LoadSelectedPreview();
        }

        private void SaveSelectedTab(object sender, EventArgs e)
        {
            var selectedResult = SelectedSearchResult();

            if (selectedResult != null)
            {
                using (var nt = new NewTabDialog(selectedResult.Artist, selectedResult.Title, UltimateGuitarTab.GetTabType(selectedResult.Type)))
                {
                    if (nt.ShowDialog() == DialogResult.OK)
                    {
                        var ugTab = _ugTabCache.ContainsKey(selectedResult.URL) ? _ugTabCache[selectedResult.URL] : UltimateGuitarTab.Download(selectedResult.URL);

                        if (ugTab != null)
                        {
                            var tab = new Tab(nt.txtartist.Text, nt.txtsong.Text, Tab.GetTabType(nt.txttype.Text), ugTab.ConvertToTab().Contents) {Source = selectedResult.URL, SourceType = TabSource.Download};

                            var tabFile = TabFile.Create(tab, Program.libraryManager.TabsDirectory);
                            Program.libraryManager.AddTab(tabFile, true);
                            UpdateLibraryItem(tabFile);
                        }
                    }
                }
            }
        }

        private void copyURLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = SelectedSearchResult();

            if (result != null)
            {
                Clipboard.SetDataObject(result.URL.ToString());
            }
        }

        private void LoadSelectedPreview()
        {
            var selectedResult = SelectedSearchResult();

            if (selectedResult != null)
            {
                if (SearchPreviewBackgroundWorker.IsBusy)
                    SearchPreviewBackgroundWorker.CancelAsync();

                searchPreviewEditor.SetText("Loading Preview...");

                if (!SearchPreviewBackgroundWorker.IsBusy)
                    SearchPreviewBackgroundWorker.RunWorkerAsync(selectedResult.URL);
            }
        }

        private void previewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (searchSplitContainer.Panel2Collapsed)
                TogglePreviewPane(sender, e);

            LoadSelectedPreview();
        }

        private void searchSession_OnCompleted(object sender, SearchEventArgs e)
        {
            searchDisplay.SuspendLayout();
            searchDisplay.Rows.Clear();

            foreach (var result in searchManager)
            {
                if (searchManager.Type == TabType.Undefined || searchManager.Type == result.Type)
                {
                    var newRow = new DataGridViewRow {Tag = result.URL.ToString()};

                    var ratingString = "";

                    if (result.Rating > 0)
                        ratingString = new string('\u2605', result.Rating).PadRight(5, '\u2606');

                    newRow.CreateCells(searchDisplay, result.Artist, result.Title, Tab.GetTabString(UltimateGuitarTab.GetTabType(result.Type)), ratingString, result.Votes);
                    searchDisplay.Rows.Add(newRow);
                }
            }

            searchDisplay.ResumeLayout();

            lblsearchresults.Visible = true;
            lblsearchresults.Text = string.Format("Results: {0}", searchDisplay.Rows.Count);
            pictureBox1.Visible = false;

            if (e.Error != null)
            {
                MessageBox.Show("An error has occured while searching.", "Search Error");
            }
        }

        private void SearchPreviewBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var url = (Uri) e.Argument;

            e.Result = url;

            if (!_ugTabCache.ContainsKey(url))
            {
                var ugTab = UltimateGuitarTab.Download(url);
                _ugTabCache.Add(url, ugTab);
            }
        }

        private void SearchPreviewBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == false && e.Error == null)
            {
                var url = (Uri) e.Result;

                if (_ugTabCache.ContainsKey(url))
                {
                    var ugTab = _ugTabCache[url];
                    var tab = ugTab.ConvertToTab();
                    searchPreviewEditor.LoadTab(tab);

                    if (!searchhiddenpreviewToolStripMenuItem.Checked && searchSplitContainer.Panel2Collapsed)
                    {
                        searchSplitContainer.Panel2Collapsed = false;
                        previewToolStrip.Enabled = previewToolStripMenuItem.Enabled = searchSplitContainer.Panel2Collapsed;
                    }
                }
            }
        }
    }
}