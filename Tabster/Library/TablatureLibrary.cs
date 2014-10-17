﻿#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using Tabster.Core.Data;
using Tabster.Core.Data.Processing;
using Tabster.Core.Types;
using SearchOption = System.IO.SearchOption;

#endregion

namespace Tabster.Library
{
    public class TablatureLibrary : ITablatureLibrary
    {
        private static readonly Version INDEX_VERSION = new Version("1.0");

        private readonly TabsterDocumentProcessor<TablatureDocument> _documentProcessor = new TabsterDocumentProcessor<TablatureDocument>(TablatureDocument.FILE_VERSION, true);
        private readonly TabsterXmlDocument _indexDoc = new TabsterXmlDocument("library");
        private readonly string _indexPath;
        private readonly List<LibraryItem> _libraryItems = new List<LibraryItem>();
        private readonly TabsterDocumentProcessor<TablaturePlaylistDocument> _playlistProcessor = new TabsterDocumentProcessor<TablaturePlaylistDocument>(TablaturePlaylistDocument.FILE_VERSION, true);

        private readonly List<TablaturePlaylistDocument> _playlists = new List<TablaturePlaylistDocument>();

        public TablatureLibrary(string indexPath, string libraryDirectory, string playlistDirectory)
        {
            _indexPath = indexPath;

            LibraryDirectory = libraryDirectory;
            PlaylistDirectory = playlistDirectory;

            if (!Directory.Exists(LibraryDirectory))
                Directory.CreateDirectory(LibraryDirectory);

            if (!Directory.Exists(PlaylistDirectory))
                Directory.CreateDirectory(PlaylistDirectory);
        }

        public ReadOnlyCollection<TablaturePlaylistDocument> Playlists
        {
            get { return _playlists.AsReadOnly(); }
        }

        public string LibraryDirectory { get; private set; }
        public string PlaylistDirectory { get; private set; }

        #region Index File

        public void Load()
        {
            _libraryItems.Clear();
            _playlists.Clear();

            var loadFromCache = File.Exists(_indexPath);

            if (loadFromCache)
            {
                _indexDoc.Load(_indexPath);

                var itemNodes = _indexDoc.ReadChildNodes("tabs");

                if (itemNodes != null)
                {
                    foreach (var itemNode in itemNodes)
                    {
                        var path = itemNode.InnerText;

                        if (File.Exists(path) && itemNode.Attributes != null)
                        {
                            var artist = string.Empty;
                            if (itemNode.Attributes["artist"] != null)
                                artist = itemNode.Attributes["artist"].Value;

                            var title = string.Empty;
                            if (itemNode.Attributes["title"] != null)
                                title = itemNode.Attributes["title"].Value;

                            var type = default(TablatureType);
                            if (itemNode.Attributes["type"] != null)
                                type = (TablatureType) Enum.Parse(typeof (TablatureType), itemNode.Attributes["type"].Value);

                            var favorited = itemNode.Attributes["favorite"] != null && bool.Parse(itemNode.Attributes["favorite"].Value);

                            var views = 0;
                            if (itemNode.Attributes["views"] != null)
                                int.TryParse(itemNode.Attributes["views"].Value, out views);

                            DateTime? lastViewed = null;
                            DateTime dt;
                            if (itemNode.Attributes["last_viewed"] != null && DateTime.TryParse(itemNode.Attributes["last_viewed"].Value, out dt))
                                lastViewed = dt;

                            var fi = new FileInfo(path);

                            var entry = new LibraryItem(fi, artist, title, type) {Favorited = favorited, Views = views, LastViewed = lastViewed};

                            _libraryItems.Add(entry);
                        }
                    }
                }

                var playlistPaths = _indexDoc.ReadChildNodeValues("playlists");

                if (playlistPaths != null)
                {
                    foreach (var file in playlistPaths)
                    {
                        if (File.Exists(file))
                        {
                            var playlist = _playlistProcessor.Load(file);

                            if (playlist != null)
                            {
                                _playlists.Add(playlist);
                            }
                        }
                    }
                }
            }

            else
            {
                foreach (var doc in LoadTablatureDocumentsFromDisk())
                {
                    Add(doc);
                }

                foreach (var playlist in LoadTablaturePlaylistsFromDisk())
                {
                    _playlists.Add(playlist);
                }

                Save();
            }
        }

        public void Save()
        {
            _indexDoc.Version = INDEX_VERSION;

            _indexDoc.WriteNode("tabs");

            foreach (var entry in _libraryItems.Where(entry => File.Exists(entry.FileInfo.FullName)))
            {
                _indexDoc.WriteNode("tab",
                                    entry.FileInfo.FullName,
                                    "tabs",
                                    new SortedDictionary<string, string>
                                        {
                                            {"artist", entry.Artist},
                                            {"title", entry.Title},
                                            {"type", entry.Type.ToString()},
                                            {"favorite", entry.Favorited.ToString().ToLower()},
                                            {"views", entry.Views.ToString()},
                                            {"last_viewed", entry.LastViewed == null ? string.Empty : entry.LastViewed.Value.ToString()}
                                        }, false);
            }

            _indexDoc.WriteNode("playlists");

            foreach (var playlist in _playlists)
            {
                if (File.Exists(playlist.FileInfo.FullName))
                {
                    _indexDoc.WriteNode("playlist", playlist.FileInfo.FullName, "playlists", preventNodeDuplication: false);
                }
            }

            _indexDoc.Save(_indexPath);
        }

        #endregion

        #region Tablature Methods

        private IEnumerable<TablatureDocument> LoadTablatureDocumentsFromDisk()
        {
            return Directory.GetFiles(LibraryDirectory, string.Format("*{0}", TablatureDocument.FILE_EXTENSION), SearchOption.AllDirectories).Select(file => _documentProcessor.Load(file)).Where(doc => doc != null).ToList();
        }

        public bool Remove(LibraryItem item, bool diskDelete, bool saveIndex = false)
        {
            var result = false;
            try
            {
                var success = _libraryItems.Remove(item);

                if (success)
                {
                    if (File.Exists(item.FileInfo.FullName) && diskDelete)
                    {
                        FileSystem.DeleteFile(item.FileInfo.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }

                    if (saveIndex)
                        Save();

                    result = true;
                }
            }

            catch
            {
                //unhandled
            }

            return result;
        }

        #endregion

        #region Playlist Methods

        private IEnumerable<TablaturePlaylistDocument> LoadTablaturePlaylistsFromDisk()
        {
            return Directory.GetFiles(PlaylistDirectory, string.Format("*{0}", TablaturePlaylistDocument.FILE_EXTENSION), SearchOption.TopDirectoryOnly).Select(file => _playlistProcessor.Load(file)).Where(doc => doc != null).ToList();
        }

        public void Add(TablaturePlaylistDocument playlist)
        {
            if (playlist.FileInfo == null)
            {
                var uniqueName = GenerateUniqueFilename(PlaylistDirectory, playlist.Name);
                playlist.SaveAs(uniqueName);
            }

            _playlists.Add(playlist);

            Save();
        }

        public bool Remove(TablaturePlaylistDocument playlist, bool diskDelete)
        {
            var result = _playlists.Remove(playlist);

            if (result && diskDelete && File.Exists(playlist.FileInfo.FullName))
                File.Delete(playlist.FileInfo.FullName);

            Save();

            return result;
        }

        public TablaturePlaylistDocument FindPlaylist(Predicate<TablaturePlaylistDocument> match)
        {
            return _playlists.FirstOrDefault(playlist => match(playlist));
        }

        public TablaturePlaylistDocument FindPlaylistByPath(string path)
        {
            return FindPlaylist(x => x.FileInfo.FullName.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Implementation of ITablatureLibrary

        public int TotalItems
        {
            get { return _libraryItems.Count; }
        }

        public void Add(LibraryItem item)
        {
            _libraryItems.Add(item);
        }

        public LibraryItem Add(TablatureDocument doc)
        {
            if (doc.FileInfo == null)
            {
                var filename = string.Format("{0} - {1} ({2})", doc.Artist, doc.Title, doc.Type.ToFriendlyString());
                var uniqueName = GenerateUniqueFilename(LibraryDirectory, filename + TablatureDocument.FILE_EXTENSION);
                doc.SaveAs(uniqueName);
            }

            var item = new LibraryItem(doc);

            Add(item);

            return item;
        }

        public bool Remove(LibraryItem item)
        {
            return _libraryItems.Remove(item);
        }

        public LibraryItem GetLibraryItem(TablatureDocument doc)
        {
            return _libraryItems.Find(x => x.Document.Equals(doc));
        }

        public LibraryItem Find(Predicate<LibraryItem> match)
        {
            return _libraryItems.Find(match);
        }

        public List<LibraryItem> FindAll(Predicate<LibraryItem> match)
        {
            return _libraryItems.FindAll(match);
        }

        public LibraryItem Find(string path)
        {
            return _libraryItems.Find(x => x.FileInfo.FullName.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<LibraryItem> GetEnumerator()
        {
            return ((IEnumerable<LibraryItem>) _libraryItems).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private static string GenerateUniqueFilename(string directory, string filename)
        {
            //remove invalid file path characters
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var sanitized = new Regex(String.Format("[{0}]", Regex.Escape(regexSearch))).Replace(filename, "");

            var fileName = Path.GetFileNameWithoutExtension(sanitized);
            var fileExt = Path.GetExtension(sanitized);

            var firstTry = Path.Combine(directory, String.Format("{0}{1}", fileName, fileExt));
            if (!File.Exists(firstTry))
                return firstTry;

            for (var i = 1;; ++i)
            {
                var appendedPath = Path.Combine(directory, String.Format("{0} ({1}){2}", fileName, i, fileExt));

                if (!File.Exists(appendedPath))
                    return appendedPath;
            }
        }
    }
}