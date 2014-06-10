﻿#region

using System.Collections.ObjectModel;
using System.Windows.Forms;
using Microsoft.VisualBasic.ApplicationServices;
using Tabster.Core;
using Tabster.Forms;

#endregion

namespace Tabster
{
    public class SingleInstanceController : WindowsFormsApplicationBase
    {
        private static TabFile _queuedTabfile;
        private static bool _isLibraryOpen;
        private static bool _noSplash;

        public SingleInstanceController()
        {
            IsSingleInstance = true;
            StartupNextInstance += this_StartupNextInstance;
        }

        public new Form MainForm
        {
            get { return base.MainForm; }
        }

        public new Form SplashScreen
        {
            get { return base.SplashScreen; }
        }

        private static void ProcessCommandLine(ReadOnlyCollection<string> commandLine)
        {
            if (commandLine.Count > 0)
            {
                if (commandLine.Contains("-nosplash"))
                {
                    _noSplash = true;
                }

                if (Common.IsFilePath(commandLine[0], true))
                {
                    TabFile t;
                    if (TabFile.TryParse(commandLine[0], out t))
                    {
                        _queuedTabfile = t;

                        if (_isLibraryOpen)
                            Program.TabHandler.LoadExternally(t, true);
                    }
                }
            }
        }

        private static void this_StartupNextInstance(object sender, StartupNextInstanceEventArgs e)
        {
            ProcessCommandLine(e.CommandLine);
        }

        protected override bool OnStartup(StartupEventArgs e)
        {
            ProcessCommandLine(e.CommandLine);
            return base.OnStartup(e);
        }

        protected override void OnCreateSplashScreen()
        {
            base.OnCreateSplashScreen();

            if (!_noSplash)
            {
                MinimumSplashScreenDisplayTime = 3500; //seems to make MainForm show prematurely
                base.SplashScreen = new SplashScreen {Cursor = Cursors.AppStarting};
            }
        }

        protected override void OnCreateMainForm()
        {
            base.OnCreateMainForm();
            base.MainForm = _queuedTabfile != null ? new MainForm(_queuedTabfile) : new MainForm();
            _isLibraryOpen = true;
        }
    }
}