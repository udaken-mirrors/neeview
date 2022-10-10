using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;
using System.IO;
using System.Threading;
using NeeView.Collections.Generic;
using NeeView.Collections;
using System.Text.Json.Serialization;
using System.Text.Json;
using NeeView.Properties;
using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    [Obsolete("no used")]
    public enum PagemarkOrder
    {
        FileName,
        Path,
    }

    [Obsolete("no used")] // ver.39
    public class PagemarkCollection : BindableBase
    {
        public static TreeListNode<IPagemarkEntry> CreateRoot()
        {
            var items = new TreeListNode<IPagemarkEntry>(new PagemarkFolder());

            return items;
        }


        private static TreeListNode<IPagemarkEntry> ConvertToBookUnitFormat(TreeListNode<IPagemarkEntry> source)
        {

            var map = new Dictionary<string, List<Pagemark>>();

            foreach (var pagemark in source.Select(e => e.Value).OfType<Pagemark>())
            {
                var place = pagemark.Path;

                if (!map.ContainsKey(place))
                {
                    map.Add(place, new List<Pagemark>());
                }

                map[place].Add(pagemark);
            }

            var items = CreateRoot();

            foreach (var key in map.Keys.OrderBy(e => LoosePath.GetFileName(e), new NameComparer()))
            {
                var node = new TreeListNode<IPagemarkEntry>(new PagemarkFolder() { Path = key }) { IsExpanded = true };
                items.Add(node);

                foreach (var pagemark in map[key].OrderBy(e => e.DispName, new NameComparer()))
                {
                    node.Add(new TreeListNode<IPagemarkEntry>(pagemark));
                }
            }

            return items;
        }

        #region Memento

        public class Memento : IMemento
        {
            [JsonPropertyName("Format")]
            public FormatVersion? Format { get; set; }

            public PagemarkNode? Nodes { get; set; }


            public Memento()
            {
                Nodes = new PagemarkNode();
            }


            public void Save(string path)
            {
                Format = new FormatVersion(Environment.SolutionName + ".Pagemark", Environment.AssemblyVersion.Major, Environment.AssemblyVersion.Minor, 0);

                var json = JsonSerializer.SerializeToUtf8Bytes(this, UserSettingTools.GetSerializerOptions());
                File.WriteAllBytes(path, json);
            }

            public static Memento? Load(string path)
            {
                var json = File.ReadAllBytes(path);
                return Load(new ReadOnlySpan<byte>(json));
            }

            public static Memento? Load(Stream stream)
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return Load(new ReadOnlySpan<byte>(ms.ToArray()));
                }
            }

            public static Memento? Load(ReadOnlySpan<byte> json)
            {
                return JsonSerializer.Deserialize<Memento>(json, UserSettingTools.GetSerializerOptions())?.Validate();
            }

            /// <summary>
            /// 互換補正処理 (ver38以降)
            /// </summary>
            private Memento Validate()
            {
                return this;
            }

            public bool IsEmpty()
            {
                return (Nodes?.Children is null || Nodes.Children.Count == 0);
            }
        }

        #endregion
    }


    [Obsolete("no used")]
    public class PagemarkNode
    {
        public string? Path { get; set; }

        public string? EntryName { get; set; }

        public string? DispName { get; set; }

        public bool IsExpanded { get; set; }

        public List<PagemarkNode>? Children { get; set; }

        public bool IsFolder => Children != null;

        public IEnumerable<PagemarkNode> GetEnumerator()
        {
            yield return this;

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    foreach (var node in child.GetEnumerator())
                    {
                        yield return node;
                    }
                }
            }
        }
    }

    [Obsolete("no used")]
    public static class PagemarkNodeConverter
    {
        [return: NotNullIfNotNull("source")]
        public static PagemarkNode? ConvertFrom(TreeListNode<IPagemarkEntry> source)
        {
            if (source == null) return null;

            var node = new PagemarkNode();

            if (source.Value is PagemarkFolder folder)
            {
                node.Path = folder.Path;
                node.IsExpanded = source.IsExpanded;
                node.Children = new List<PagemarkNode>();
                foreach (var child in source.Children)
                {
                    node.Children.Add(ConvertFrom(child));
                }
            }
            else if (source.Value is Pagemark pagemark)
            {
                node.Path = pagemark.Path;
                node.EntryName = pagemark.EntryName;
                node.DispName = pagemark.DispNameRaw;
            }
            else
            {
                throw new NotSupportedException();
            }

            return node;
        }

        public static TreeListNode<IPagemarkEntry>? ConvertToTreeListNode(PagemarkNode source)
        {

            if (source.IsFolder)
            {
                var pagemarkFolder = new PagemarkFolder()
                {
                    Path = source.Path
                };
                var node = new TreeListNode<IPagemarkEntry>(pagemarkFolder);
                node.IsExpanded = source.IsExpanded;
                if (source.Children is not null)
                {
                    foreach (var child in source.Children)
                    {
                        var newNode = ConvertToTreeListNode(child);
                        if (newNode != null)
                        {
                            node.Add(newNode);
                        }
                    }
                }
                return node;
            }
            else
            {
                if (source.Path is null || source.EntryName is null) return null;
                var pagemark = new Pagemark(source.Path, source.EntryName)
                {
                    DispName = source.DispName,
                };
                var node = new TreeListNode<IPagemarkEntry>(pagemark);
                return node;
            }
        }
    }


    // ページマークをプレイリストに変換する
    public static class PagemarkToPlaylistConverter
    {
#pragma warning disable CS0612, CS0618 // 型またはメンバーが旧型式です
        public static PlaylistSource ConvertToPlaylist(PagemarkCollection.Memento memento)
        {
            if (memento.Nodes is null) return new PlaylistSource();

            var items = memento.Nodes.GetEnumerator()
                .Where(e => !e.IsFolder)
                .Select(e => new PlaylistSourceItem(LoosePath.Combine(e.Path, e.EntryName), e.DispName));

            return new PlaylistSource(items);
        }

        public static void PagemarkToPlaylist()
        {
            var path = Config.Current.Playlist.PagemarkPlaylist;

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (File.Exists(path))
            {
                return;
            }

            // load pagemark
            var result = LoadPagemark(Config.Current.PagemarkLegacy?.PagemarkFilePath);
            if (result.pagemark is null)
            {
                return;
            }

            if (!result.pagemark.IsEmpty())
            {
                SavePagemarkPlaylist(result.pagemark);
                Config.Current.Playlist.CurrentPlaylist = path;
            }

            // remove
            FileIO.RemoveFile(result.path);
            if (Config.Current.PagemarkLegacy != null)
            {
                Config.Current.PagemarkLegacy.PagemarkFilePath = "";
            }
        }

        public static void SavePagemarkPlaylist(PagemarkCollection.Memento pagemark)
        {
            if (string.IsNullOrEmpty(Config.Current.Playlist.PagemarkPlaylist))
            {
                return;
            }

            // convert
            var playlistSource = ConvertToPlaylist(pagemark);

            // save
            playlistSource.Save(Config.Current.Playlist.PagemarkPlaylist, true, true);
        }


        // ページマーク読み込み
        private static (string path, PagemarkCollection.Memento? pagemark) LoadPagemark(string? filename)
        {
            if (filename is null) return default;

            using (ProcessLock.Lock())
            {
                var extension = Path.GetExtension(filename).ToLower();

                var failedDialog = new LoadFailedDialog(Resources.Notice_LoadPagemarkFailed, Resources.Notice_LoadPagemarkFailedTitle);

                if (extension == ".json" && File.Exists(filename))
                {
                    PagemarkCollection.Memento? memento = Load(PagemarkCollection.Memento.Load, filename, failedDialog);
                    return (filename, memento);
                }
                else
                {
                    return default;
                }
            }

            static PagemarkCollection.Memento? Load(Func<string, PagemarkCollection.Memento?> load, string path, LoadFailedDialog loadFailedDialog)
            {
                try
                {
                    return load(path);
                }
                catch (Exception ex)
                {
                    loadFailedDialog?.ShowDialog(ex);
                    return null;
                }
            }
        }

#pragma warning restore CS0612, CS0618 // 型またはメンバーが旧型式です

    }
}
