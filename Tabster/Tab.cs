﻿#region

using System;

#endregion

namespace Tabster
{
    public enum TabType
    {
        Guitar,
        Chord,
        Bass,
        Drum
    }

    public enum TabSource
    {
        Download = 0,
        FileImport = 1,
        UserCreated = 2,
    }

    public enum Difficulty
    {
        Unknown,
        Novice,
        Intermediate,
        Advanced
    }

    public enum Tuning
    {
        Unknown,
        Standard,
        HalfStepDown,
        BTuning,
        CTuning,
        DTuning,
        DropA,
        DropASharp,
        DropB,
        DropC,
        DropCSharp,
        DropD,
        OpenC,
        OpenD,
        OpenE,
        OpenG
    }

    public class Tab
    {
        public static readonly string[] TabTypes;

        private string _artist = "";
        private string _audio = "";
        private string _comment = "";
        private string _contents = "";
        private string _lyrics = "";
        private string _title = "";

        static Tab()
        {
            TabTypes = new[] {"Guitar Tab", "Guitar Chords", "Bass Tab", "Drum Tab"};
        }

        public Tab(string artist, string title, TabType type, string contents)
        {
            Title = title;
            Artist = artist;
            Type = type;
            Contents = contents;
        }

        /// <summary>
        ///   Gets or sets the title of the tab.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /// <summary>
        ///   Gets or sets the artist of the tab.
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; }
        }

        /// <summary>
        ///   Gets the contents of the tab
        /// </summary>
        public string Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }

        /// <summary>
        ///   Gets or sets the source of the tab.
        /// </summary>
        public TabSource Source { get; set; }

        /// <summary>
        ///   Gets or sets the remote sorce of the tab.
        /// </summary>
        public Uri RemoteSource { get; set; }

        /// <summary>
        ///   Gets or sets the type of tab.
        /// </summary>
        public TabType Type { get; set; }

        public Difficulty Difficulty { get; set; }

        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }


        public string Audio
        {
            get { return _audio; }
            set { _audio = value; }
        }

        public string Lyrics
        {
            get { return _lyrics; }
            set { _lyrics = value; }
        }

        public string GetName()
        {
            return string.Format("{0} - {1} ({2})", Artist, Title, GetTabString(Type));
        }

        #region Overrides

        public override string ToString()
        {
            return GetName();
        }

        #endregion

        #region Static Methods

        public static TabSource GetTabSource(string source)
        {
            switch (source)
            {
                case "UserCreated":
                    return TabSource.UserCreated;
                case "FileImport":
                    return TabSource.FileImport;
                default:
                    return TabSource.Download;
            }
        }

        public static TabType GetTabType(string type)
        {
            switch (type)
            {
                case "Bass Tab":
                    return TabType.Bass;
                case "Guitar Tab":
                    return TabType.Guitar;
                case "Guitar Chords":
                    return TabType.Chord;
                case "Drum Tab":
                    return TabType.Drum;
            }

            return TabType.Guitar;
        }

        public static string GetTabString(TabType type)
        {
            switch (type)
            {
                case TabType.Guitar:
                    return TabTypes[0];
                case TabType.Chord:
                    return TabTypes[1];
                case TabType.Bass:
                    return TabTypes[2];
                case TabType.Drum:
                    return TabTypes[3];
            }

            return TabTypes[0];
        }

        #endregion
    }
}