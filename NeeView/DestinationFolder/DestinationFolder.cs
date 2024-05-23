using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class DestinationFolder : ICloneable, IEquatable<DestinationFolder>
    {
        private string _name = "";
        private string _path = "";

        public DestinationFolder()
        {
        }

        public DestinationFolder(string name, string path)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }


        public string Name
        {
            get { return string.IsNullOrWhiteSpace(_name) ? LoosePath.GetFileName(_path) : _name; }
            set { _name = value; }
        }

        public string Path
        {
            get => _path;
            set => _path = value;
        }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(_path);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(DestinationFolder? other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name && this.Path == other.Path)
                return true;
            else
                return false;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as DestinationFolder);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Path.GetHashCode();
        }

        public void Copy(IEnumerable<string> paths)
        {
            if (!paths.Any()) return;

            if (!Directory.Exists(this.Path))
            {
                throw new DirectoryNotFoundException();
            }

            FileIO.CopyToFolder(paths, this.Path);
        }

        public async Task MoveAsync(IEnumerable<string> paths, CancellationToken token)
        {
            if (!paths.Any()) return;

            if (!Directory.Exists(this.Path))
            {
                throw new DirectoryNotFoundException();
            }

            await FileIO.MoveToFolderAsync(paths, this.Path, token);
        }

        public void Move(IEnumerable<string> paths)
        {
            if (!paths.Any()) return;

            if (!Directory.Exists(this.Path))
            {
                throw new DirectoryNotFoundException();
            }

            FileIO.MoveToFolder(paths, this.Path);
        }
    }
}
