﻿#region

using System;
using Tabster.Core.Plugins;

#endregion

namespace Songsterr
{
    public class SongsterrPlugin : TabsterPluginBase, ITabsterPluginAttributes
    {
        #region Implementation of ITabsterPluginAttributes

        public string Author
        {
            get { return "Nate Shoffner"; }
        }

        public string Copyright
        {
            get { return "Copyright © Nate Shoffner 2014"; }
        }

        public string Description
        {
            get { return "Supports songsterr.com tab searching and downloading."; }
        }

        public string DisplayName
        {
            get { return "Songsterr"; }
        }

        public Version Version
        {
            get { return new Version("1.0"); }
        }

        public Uri Website
        {
            get { return new Uri("http://nateshoffner.com"); }
        }

        public Type[] Types
        {
            get { return new[] {typeof (SongsterrSearch), typeof (SongsterrParser)}; }
        }

        #endregion
    }
}