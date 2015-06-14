﻿#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Tabster.Data;
using Tabster.Data.Processing;

#endregion

namespace PngFile
{
    internal class PngFileExporter : ITablatureFileExporter
    {
        public PngFileExporter()
        {
            FileType = new FileType("Portable Netork Graphics", ".png");
        }

        #region Implementation of ITablatureFileExporter

        public FileType FileType { get; private set; }

        public Version Version
        {
            get { return new Version("1.0"); }
        }

        public void Export(ITablatureFile file, string fileName)
        {
            using (var fontDialog = new FontSizeDialog {})
            {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    var font = new Font("Courier New", fontDialog.FontSize);

                    SizeF size = new Size();

                    //measure size of text
                    using (var bmp = new Bitmap(1, 1))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            size = g.MeasureString(file.Contents, font);
                        }
                    }

                    const int verticalPadding = 10;
                    const int horizontalPadding = 10;

                    size.Width += (horizontalPadding*2);
                    size.Height += (verticalPadding*2);

                    using (var bmp = new Bitmap((int) size.Width, (int) size.Height))
                    {
                        using (var g = Graphics.FromImage(bmp))
                        {
                            g.Clear(Color.White);
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.TextRenderingHint = TextRenderingHint.AntiAlias;
                            g.DrawString(file.Contents, font, Brushes.Black, verticalPadding, horizontalPadding);
                        }

                        bmp.Save(fileName);
                    }
                }
            }
        }

        #endregion
    }
}