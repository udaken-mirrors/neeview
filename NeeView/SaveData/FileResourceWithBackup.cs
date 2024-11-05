using System;
using System.Collections.Generic;
using System.Linq;


namespace NeeView
{
    public class FileResourceWithBackup
    {
        private readonly List<FileResource> _resources;

        public FileResourceWithBackup(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _resources = [];
            }
            else
            {
                _resources = [
                    new FileResource(path),
                    new FileResource(path + SaveData.BackupExtension) { IsBackup = true }
                    ];
            }
        }

        /// <summary>
        /// 有効な FileResource を取得
        /// </summary>
        /// <returns>有効な FileResource が存在しない場合は null</returns>
        public FileResource? GetValidResource()
        {
            foreach (var resource in _resources)
            {
                if (resource.IsValid())
                {
                    return resource;
                }
            }
            return null;
        }

        /// <summary>
        /// 最も適切な例外を取得
        /// </summary>
        /// <returns>リソースが有効な場合は null</returns>
        public Exception? GetException()
        {
            foreach (var resource in _resources)
            {
                if (resource.IsValid())
                {
                    return null;
                }
                if (resource.Exception is not null)
                {
                    return resource.Exception;
                }
            }
            return null;
        }

        /// <summary>
        /// ファイル存在チェック
        /// </summary>
        /// <returns></returns>
        public bool FileExists()
        {
            var resource = GetValidResource();
            if (resource is not null) return true;
            if (_resources.Count == 0) return false;

            return _resources.All(e => e.State != FileResourceState.FileNotFound);
        }
    }
}
