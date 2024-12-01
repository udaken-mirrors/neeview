using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace NeeView
{
    public class FileAssociation : IFileAssociation
    {
        public const string ProgIdPrefix = "NeeView";
        private readonly string _extension;
        private readonly FileAssociationCategory _category;
        private readonly string _progId;
        private bool _isEnabled;

        public FileAssociation(string extension, FileAssociationCategory category)
        {
            if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentNullException(nameof(extension));
            if (extension[0] != '.') throw new ArgumentException($"{nameof(extension)} does not begin with a period.");

            _extension = extension;
            _category = category;

            // ProgID は "NeeView.[ext]" とする
            _progId = ProgIdPrefix + _extension;

            _isEnabled = IsAssociated();
        }


        public string Extension => _extension;

        public FileAssociationCategory Category => _category;

        public string? Description { get; init; }


        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (_isEnabled)
                    {
                        Associate();
                    }
                    else
                    {
                        Unassociate();
                    }
                }
            }
        }

        private static bool IsExistSubKey(RegistryKey reg, string name)
        {
            try
            {
                using var key = reg.OpenSubKey(name, false);
                return key != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        private static RegistryKey? OpenClassesKey(bool writable)
        {
            return Registry.CurrentUser.OpenSubKey(@"Software\Classes", writable);
        }

        public bool IsAssociated()
        {
            using var root = OpenClassesKey(false);
            if (root is null) return false;

            return IsExistSubKey(root, _progId);
        }

        private void Associate()
        {
            var applicationFilePath = Environment.AssemblyLocation;
            var fileIconPath = Category.ToIconPath();

            using var root = OpenClassesKey(true);
            if (root is null) return;

            // program
            // [Software\Classes\NeeView.xxx]
            // [Software\Classes\NeeView.xxx\DefaultIcon]
            // [Software\Classes\NeeView.xxx\Shell\open\command]
            // [Software\Classes\NeeView.xxx\Category] .. for NeeView
            using var prog = root.CreateSubKey(_progId, true);
            if (Description is not null)
            {
                prog.SetValue("", Description);
            }
            prog.SetValue("Category", _category.ToString());
            prog.CreateSubKey(@"DefaultIcon").SetValue("", $"{fileIconPath}");
            prog.CreateSubKey(@"Shell\open\command").SetValue("", $"\"{applicationFilePath}\" \"%1\"");

            // extension
            // [Software\Classes\.xxx]
            using var extKey = root.CreateSubKey(_extension, true);
            extKey.SetValue("", _progId);

            Debug.WriteLine($"FileAssociate: ON: {Extension}");
        }

        private void Unassociate()
        {
            using var root = OpenClassesKey(true);
            if (root is null) return;

            // extension
            if (Category == FileAssociationCategory.ForNeeView)
            {
                // 専用ファイルは拡張子定義ごと削除する
                root.DeleteSubKeyTree(_extension);
            }
            else
            {
                // NeeViewの定義のみ削除する
                using var extKey = root.OpenSubKey(_extension, true);
                if (extKey is not null)
                {
                    var oldProgId = extKey.GetValue("") as string;
                    if (oldProgId == _progId)
                    {
                        extKey.DeleteValue("");
                    }
                }
            }

            // program
            root.DeleteSubKeyTree(_progId);

            Debug.WriteLine($"FileAssociate: OFF: {Extension}");
        }
    }
}