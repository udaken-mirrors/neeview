using NeeView.Runtime.LayoutPanel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public static class UserSettingValidator
    {
#pragma warning disable CS0612, CS0618 // 型またはメンバーが旧型式です

        /// <summary>
        /// 互換性処理
        /// </summary>
        public static UserSetting Validate(this UserSetting self)
        {
            if (self is null) throw new ArgumentNullException(nameof(self));
            if (self.Format is null) throw new FormatException("UserSetting.Format must not be null.");
            if (self.Config is null) throw new FormatException("UserSetting.Config must not be null.");
            if (self.Commands is null) throw new FormatException("UserSetting.Commands must not be null.");

            // 画像拡張子初期化
            if (self.Config.Image.Standard.SupportFileTypes is null)
            {
                self.Config.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupportedFileTypes(self.Config.Image.Standard.UseWicInformation);
            }

            // 現在のバージョンであればチェック不要
            if (self.Format.Equals(new FormatVersion(Environment.SolutionName)))
            {
                return self;
            }

            // ver.38
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 38, 0, 0)) < 0)
            {
                Debug.WriteLine($"ValidateShortCutKey...");
                foreach (var command in self.Commands.Values)
                {
                    command.ValidateShortCutKey();
                }
                Debug.WriteLine($"ValidateShortCutKey done.");

                // NOTE: Config.Panels.PanelDocks は継承しません。

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_38_0_0);
            }

            // ver.39
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 39, 0, 0)) < 0)
            {
                if (self.Config.Panels.FontName_Legacy != default)
                {
                    self.Config.Fonts.FontName = self.Config.Panels.FontName_Legacy;
                }
                if (self.Config.Panels.FontSize_Legacy != default)
                {
                    self.Config.Fonts.PanelFontScale = self.Config.Panels.FontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }
                if (self.Config.Panels.FolderTreeFontSize_Legacy != default)
                {
                    self.Config.Fonts.FolderTreeFontScale = self.Config.Panels.FolderTreeFontSize_Legacy / SystemVisualParameters.Current.MessageFontSize;
                }

                self.Config.Panels.Layout?.ValidateRename("PagemarkPanel", nameof(PlaylistPanel));

                switch (self.Config.System.Language)
                {
                    case "English":
                        self.Config.System.Language = "en";
                        break;
                    case "Japanese":
                        self.Config.System.Language = "ja";
                        break;
                }

                if (self.Config.BookSetting.SortMode == PageSortMode.FileName)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.Entry;
                }
                if (self.Config.BookSetting.SortMode == PageSortMode.FileNameDescending)
                {
                    self.Config.BookSetting.SortMode = PageSortMode.EntryDescending;
                }

                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_39_0_0);
            }

            // ver.40
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 40, 0, 0)) < 0)
            {
                switch (self.Config.System.Language)
                {
                    case "zh-TW":
                        self.Config.System.Language = "zh-Hant";
                        break;
                    case "zh-CN":
                        self.Config.System.Language = "zh-Hans";
                        break;
                }

                // 水平ホイールのコマンド入れ替え初期化
                self.Config.Command.IsReversePageMoveHorizontalWheel = self.Config.Command.IsReversePageMoveWheel;

                // コマンド関係
                if (self.Commands != null)
                {
                    // ページモード切替ジェスチャが初期設定ならば切り替える
                    if (self.Commands.TryGetValue("SetPageModeOne", out var setPageModeOne) && setPageModeOne.MouseGesture == "RU"
                        && self.Commands.TryGetValue("SetPageModeTwo", out var setPageModeTwo) && setPageModeTwo.MouseGesture == "RD"
                        && self.Commands.TryGetValue("TogglePageMode", out var togglePageMode) && togglePageMode.MouseGesture == "")
                    {
                        setPageModeOne.MouseGesture = "";
                        setPageModeTwo.MouseGesture = "";
                        togglePageMode.MouseGesture = TogglePageModeCommand.DefaultMouseGesture;

                        var togglePageModeReverseCommand = new TogglePageModeReverseCommand();
                        var togglePageModeReverse = togglePageModeReverseCommand.CreateMemento();
                        self.Commands[togglePageModeReverseCommand.Name] = togglePageModeReverse;
                    }
                    // 変更されているならば新しいコマンドのジェスチャは無効化
                    else
                    {
                        var togglePageModeReverseCommand = new TogglePageModeReverseCommand();
                        var togglePageModeReverse = togglePageModeReverseCommand.CreateMemento();
                        togglePageModeReverse.MouseGesture = "";
                        self.Commands[togglePageModeReverseCommand.Name] = togglePageModeReverse;
                    }

                    // メインビューウィンドウ切り替えコマンドにショートカットキーF12を設定
                    if (self.Commands.TryGetValue("ToggleMainViewFloating", out var toggleMainViewFloating) && toggleMainViewFloating.ShortCutKey == "" && !self.Commands.Any(e => e.Key.Contains("F12")))
                    {
                        toggleMainViewFloating.ShortCutKey = "F12";
                    }
                }
            }

#if false
            // ver.99 (バージョン変更処理テスト)
            if (self.Format.CompareTo(new FormatVersion(Environment.SolutionName, 99, 0, 0)) < 0)
            {
                self.Commands?.ValidateRename(CommandNameValidator.RenameMap_99_0_0);
                self.ContextMenu?.ValidateRename(CommandNameValidator.RenameMap_99_0_0);
            }
#endif

            return self;
        }

#pragma warning restore CS0612, CS0618 // 型またはメンバーが旧型式です

    }


    /// <summary>
    /// コマンド名変更対応
    /// </summary>
    public static class CommandNameValidator
    {
        public static Dictionary<string, string> RenameMap_38_0_0 { get; } = new Dictionary<string, string>()
        {
            ["TogglePermitFileCommand"] = "TogglePermitFile",
            ["FocusPrevAppCommand"] = "FocusPrevApp",
            ["FocusNextAppCommand"] = "FocusNextApp",
        };

        public static Dictionary<string, string> RenameMap_39_0_0 { get; } = new Dictionary<string, string>()
        {
            ["ToggleVisiblePagemarkList"] = "ToggleVisiblePlaylist",
            ["TogglePagemark"] = "TogglePlaylistMark",
            ["PrevPagemark"] = "PrevPlaylistItem",
            ["NextPagemark"] = "NextPlaylistItem",
            ["PrevPagemarkInBook"] = "PrevPlaylistItemInBook",
            ["NextPagemarkInBook"] = "NextPlaylistItemInBook",
        };

#if false
        // バージョン変更処理テスト用
        public static Dictionary<string, string> RenameMap_99_0_0 { get; } = new Dictionary<string, string>()
        {
            ["ExportImageAs"] = "XXX_ExportImageAs",
        };
#endif


        /// <summary>
        /// CommandCollection のコマンド名変更
        /// </summary>
        public static void ValidateRename(this CommandCollection commandCollection, Dictionary<string, string> renameMap)
        {
            foreach (var pair in renameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            void Rename(string oldName, string newName)
            {
                foreach (var command in commandCollection.ToList())
                {
                    var nameSource = CommandNameSource.Parse(command.Key);
                    if (nameSource.Name == oldName)
                    {
                        commandCollection.TryAdd(new CommandNameSource(newName, nameSource.Number).FullName, command.Value);
                        commandCollection.Remove(command.Key);
                    }
                }
            }
        }

        /// <summary>
        /// ContextMenu のコマンド名変更
        /// </summary>
        public static void ValidateRename(this MenuNode contextMenu, Dictionary<string, string> renameMap)
        {
            foreach (var pair in renameMap)
            {
                Rename(pair.Key, pair.Value);
            }

            void Rename(string oldName, string newName)
            {
                foreach (var node in contextMenu.GetEnumerator().Where(e => e.MenuElementType == MenuElementType.Command))
                {
                    Debug.Assert(node.CommandName != null);
                    var nameSource = CommandNameSource.Parse(node.CommandName);
                    if (nameSource.Name == oldName)
                    {
                        node.CommandName = new CommandNameSource(newName, nameSource.Number).FullName;
                    }
                }
            }
        }
    }

    /// <summary>
    /// LayoutPanel 名前変更対応
    /// </summary>
    public static class LayoutPanelValidator
    {
        public static void ValidateRename(this LayoutPanelManager.Memento self, string oldName, string newName)
        {
            if (self is null) return;

            if (self.Panels != null && self.Panels.TryGetValue(oldName, out var value))
            {
                self.Panels.Remove(oldName);
                self.Panels.Add(newName, value);
            }

            if (self.Docks != null)
            {
                foreach (var dock in self.Docks.Values)
                {
                    dock.SelectedItem = dock.SelectedItem == oldName ? newName : dock.SelectedItem;

#pragma warning disable CS0612 // 型またはメンバーが旧型式です
                    if (dock.Panels is not null)
                    {
                        dock.Panels = dock.Panels
                            .Select(e => e.Select(x => x == oldName ? newName : x).ToList())
                            .ToList();
                    }
#pragma warning restore CS0612 // 型またはメンバーが旧型式です

                    foreach(var panelLayout in dock.PanelLayout)
                    {
                        panelLayout.Panels = panelLayout.Panels
                            .Select(x => x == oldName ? newName : x).ToList()
                            .ToList();
                    }
                }
            }

            if (self.Windows?.Panels != null)
            {
                self.Windows.Panels = self.Windows.Panels
                    .Select(x => x == oldName ? newName : x)
                    .ToList();
            }
        }

    }

}
