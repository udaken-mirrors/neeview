﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NeeView
{
    public class Exporter
    {
        public void Export(string filename)
        {
            SaveDataSync.Current.SaveAll(false);

            try
            {
                // 保存されたファイルをzipにまとめて出力
                using (ZipArchive archive = new ZipArchive(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(App.Current.Option.SettingFilename, SaveData.UserSettingFileName);

                    if (File.Exists(SaveData.Current.HistoryFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.Current.HistoryFilePath, SaveData.HistoryFileName);
                    }
                    if (File.Exists(SaveData.Current.BookmarkFilePath))
                    {
                        archive.CreateEntryFromFile(SaveData.Current.BookmarkFilePath, SaveData.BookmarkFileName);
                    }
                    var playlists = PlaylistHub.GetPlaylistFiles();
                    if (playlists.Any())
                    {
                        foreach (var playlist in playlists)
                        {
                            archive.CreateEntryFromFile(playlist.FullName, LoosePath.Combine("Playlists", playlist.Name));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 中途半端なファイルは削除
                if (File.Exists(filename))
                {
                    Debug.WriteLine($"Delete {filename}");
                    File.Delete(filename);
                }

                throw;
            }
        }

    }
}