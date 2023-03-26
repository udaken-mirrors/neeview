using NeeView.IO;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    public static class ArchiveEntryTools
    { 
        /// <summary>
        /// ArchiveEntry生成。
        /// 簡易生成のため、アーカイブパス等は有効なインスタンスを生成できない。 
        /// </summary>
        public static ArchiveEntry Create(string path)
        {
            return Create(new QueryPath(path));
        }

        /// <summary>
        /// ArchiveEntry生成。
        /// 簡易生成のため、アーカイブパス等は有効なインスタンスを生成できない。 
        /// <para>
        /// 完全な作成には <seealso cref="ArchiveEntryUtility.CreateAsync"/> を使用する。
        /// </para>
        /// </summary>
        public static ArchiveEntry Create(QueryPath query)
        {
            var entry = new ArchiveEntry(FolderArchive.StaticArchiver);

            entry.RawEntryName = query.SimplePath;

            switch (query.Scheme)
            {
                case QueryScheme.File:
                    try
                    {
                        var directoryInfo = new DirectoryInfo(query.SimplePath);
                        if (directoryInfo.Exists)
                        {
                            entry.Length = -1;
                            entry.CreationTime = directoryInfo.CreationTime;
                            entry.LastWriteTime = directoryInfo.LastWriteTime;
                            entry.IsValid = true;
                            return entry;
                        }
                        var fileInfo = new FileInfo(query.SimplePath);
                        if (fileInfo.Exists)
                        {
                            entry.Length = fileInfo.Length;
                            entry.CreationTime = fileInfo.CreationTime;
                            entry.LastWriteTime = fileInfo.LastWriteTime;
                            entry.IsValid = true;
                            if (FileShortcut.IsShortcut(fileInfo.Name))
                            {
                                var shortcut = new FileShortcut(fileInfo);
                                if (shortcut.TryGetTarget(out var target))
                                {
                                    if (target.Attributes.HasFlag(FileAttributes.Directory))
                                    {
                                        var info = (DirectoryInfo)target;
                                        entry.Link = info.FullName;
                                        entry.Length = -1;
                                        entry.CreationTime = info.CreationTime;
                                        entry.LastWriteTime = info.LastWriteTime;
                                    }
                                    else
                                    {
                                        var info = (FileInfo)target;
                                        entry.Link = info.FullName;
                                        entry.Length = info.Length;
                                        entry.CreationTime = info.CreationTime;
                                        entry.LastWriteTime = info.LastWriteTime;
                                    }
                                }
                            }
                            return entry;
                        }
                    }
                    catch
                    {
                        // アーカイブパス等、ファイル名に使用できない文字が含まれている場合がある
                    }
                    break;
            }

            Debug.WriteLine("ArchiveEntry.Create: Not complete.");
            entry.RawEntryName = query.SimplePath;
            entry.IsValid = false;
            return entry;
        }
    }
}

