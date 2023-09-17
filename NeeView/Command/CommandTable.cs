using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

// TODO: コマンド引数にコマンドパラメータを渡せないだろうか。（現状メニュー呼び出しであることを示すタグが指定されることが有る)

namespace NeeView
{
    // Typo: InputScheme ... 保存データの互換性を確認後に修正
    public enum InputScheme
    {
        TypeA, // 標準
        TypeB, // ホイールでページ送り
        TypeC, // クリックでページ送り
    };


    /// <summary>
    /// コマンド設定テーブル
    /// </summary>
    public partial class CommandTable : BindableBase, IDictionary<string, CommandElement>
    {
        static CommandTable() => Current = new CommandTable();
        public static CommandTable Current { get; }


        private Dictionary<string, CommandElement> _elements;


        private CommandTable()
        {
            InitializeCommandTable();

            Changed += CommandTable_Changed;
        }


        /// <summary>
        /// コマンドテーブルが変更された
        /// </summary>
        [Subscribable]
        public event EventHandler<CommandChangedEventArgs>? Changed;


        public CommandCollection DefaultMemento { get; private set; }

        public int ChangeCount { get; private set; }

        public Dictionary<string, CommandElement> Elements => _elements;

        public Dictionary<string, ObsoleteCommandItem> ObsoleteCommands { get; private set; }

        #region IDictionary Support

        public ICollection<string> Keys => ((IDictionary<string, CommandElement>)_elements).Keys;

        public ICollection<CommandElement> Values => ((IDictionary<string, CommandElement>)_elements).Values;

        public int Count => ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, CommandElement>>)_elements).IsReadOnly;

        public CommandElement this[string key]
        {
            get => ((IDictionary<string, CommandElement>)_elements)[key];
            set => ((IDictionary<string, CommandElement>)_elements)[key] = value;
        }

        public void Add(string key, CommandElement value)
        {
            ((IDictionary<string, CommandElement>)_elements).Add(key, value);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, CommandElement>)_elements).Remove(key);
        }

        public void Add(KeyValuePair<string, CommandElement> item)
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Clear();
        }

        public bool Contains(KeyValuePair<string, CommandElement> item)
        {
            return ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Contains(item);
        }

        public bool ContainsKey(string? key)
        {
            return key != null && _elements.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out CommandElement value)
        {
            return ((IDictionary<string, CommandElement>)_elements).TryGetValue(key, out value);
        }

        public void CopyTo(KeyValuePair<string, CommandElement>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, CommandElement>>)_elements).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, CommandElement> item)
        {
            return ((ICollection<KeyValuePair<string, CommandElement>>)_elements).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, CommandElement>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, CommandElement>>)_elements).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_elements).GetEnumerator();
        }

        #endregion

        #region Methods: Initialize

        /// <summary>
        /// コマンドテーブル初期化
        /// </summary>
        [MemberNotNull(nameof(_elements), nameof(DefaultMemento), nameof(ObsoleteCommands))]
        private void InitializeCommandTable()
        {
            var list = new List<CommandElement>()
            {
                new LoadAsCommand(),
                new ReLoadCommand(),
                new UnloadCommand(),
                new OpenExternalAppCommand(),
                new OpenExplorerCommand(),
                new ExportImageAsCommand(),
                new ExportImageCommand(),
                new PrintCommand(),
                new DeleteFileCommand(),
                new DeleteBookCommand(),
                new CopyFileCommand(),
                new CopyImageCommand(),
                new PasteCommand(),

                new ClearHistoryCommand(),
                new ClearHistoryInPlaceCommand(),
                new RemoveUnlinkedHistoryCommand(),

                new ToggleStretchModeCommand(),
                new ToggleStretchModeReverseCommand(),
                new SetStretchModeNoneCommand(),
                new SetStretchModeUniformCommand(),
                new SetStretchModeUniformToFillCommand(),
                new SetStretchModeUniformToSizeCommand(),
                new SetStretchModeUniformToVerticalCommand(),
                new SetStretchModeUniformToHorizontalCommand(),
                new ToggleStretchAllowScaleUpCommand(),
                new ToggleStretchAllowScaleDownCommand(),
                new ToggleNearestNeighborCommand(),
                new ToggleBackgroundCommand(),
                new SetBackgroundBlackCommand(),
                new SetBackgroundWhiteCommand(),
                new SetBackgroundAutoCommand(),
                new SetBackgroundCheckCommand(),
                new SetBackgroundCheckDarkCommand(),
                new SetBackgroundCustomCommand(),
                new ToggleTopmostCommand(),
                new ToggleVisibleAddressBarCommand(),
                new ToggleHideMenuCommand(),
                new ToggleVisibleSideBarCommand(),
                new ToggleHidePanelCommand(),
                new ToggleVisiblePageSliderCommand(),
                new ToggleHidePageSliderCommand(),

                new ToggleVisibleBookshelfCommand(),
                new ToggleVisiblePageListCommand(),
                new ToggleVisibleBookmarkListCommand(),
                new ToggleVisiblePlaylistCommand(),
                new ToggleVisibleHistoryListCommand(),
                new ToggleVisibleFileInfoCommand(),
                new ToggleVisibleNavigatorCommand(),
                new ToggleVisibleEffectInfoCommand(),
                new ToggleVisibleFoldersTreeCommand(),
                new FocusFolderSearchBoxCommand(),
                new FocusBookmarkListCommand(),
                new FocusMainViewCommand(),
                new ToggleVisibleThumbnailListCommand(),
                new ToggleHideThumbnailListCommand(),
                new ToggleMainViewFloatingCommand(),

                new ToggleFullScreenCommand(),
                new SetFullScreenCommand(),
                new CancelFullScreenCommand(),
                new ToggleWindowMinimizeCommand(),
                new ToggleWindowMaximizeCommand(),
                new ShowHiddenPanelsCommand(),

                new ToggleSlideShowCommand(),
                new ToggleHoverScrollCommand(),

                new ViewScrollNTypeUpCommand(),
                new ViewScrollNTypeDownCommand(),
                new ViewScrollUpCommand(),
                new ViewScrollDownCommand(),
                new ViewScrollLeftCommand(),
                new ViewScrollRightCommand(),
                new ViewScaleUpCommand(),
                new ViewScaleDownCommand(),
                new ViewRotateLeftCommand(),
                new ViewRotateRightCommand(),
                new ToggleIsAutoRotateLeftCommand(),
                new ToggleIsAutoRotateRightCommand(),

                new ToggleViewFlipHorizontalCommand(),
                new ViewFlipHorizontalOnCommand(),
                new ViewFlipHorizontalOffCommand(),

                new ToggleViewFlipVerticalCommand(),
                new ViewFlipVerticalOnCommand(),
                new ViewFlipVerticalOffCommand(),
                new ViewResetCommand(),

                new PrevPageCommand(),
                new NextPageCommand(),
                new PrevOnePageCommand(),
                new NextOnePageCommand(),

                new PrevScrollPageCommand(),
                new NextScrollPageCommand(),
                new JumpPageCommand(),
                new JumpRandomPageCommand(),
                new PrevSizePageCommand(),
                new NextSizePageCommand(),
                new PrevFolderPageCommand(),
                new NextFolderPageCommand(),
                new FirstPageCommand(),
                new LastPageCommand(),
                new PrevHistoryPageCommand(),
                new NextHistoryPageCommand(),

                new PrevBookCommand(),
                new NextBookCommand(),
                new RandomBookCommand(),
                new PrevHistoryCommand(),
                new NextHistoryCommand(),

                new PrevBookHistoryCommand(),
                new NextBookHistoryCommand(),
                new MoveToParentBookCommand(),
                new MoveToChildBookCommand(),

                new ToggleMediaPlayCommand(),
                new ToggleBookOrderCommand(),
                new SetBookOrderByFileNameACommand(),
                new SetBookOrderByFileNameDCommand(),
                new SetBookOrderByPathACommand(),
                new SetBookOrderByPathDCommand(),
                new SetBookOrderByFileTypeACommand(),
                new SetBookOrderByFileTypeDCommand(),
                new SetBookOrderByTimeStampACommand(),
                new SetBookOrderByTimeStampDCommand(),
                new SetBookOrderByEntryTimeACommand(),
                new SetBookOrderByEntryTimeDCommand(),
                new SetBookOrderBySizeACommand(),
                new SetBookOrderBySizeDCommand(),
                new SetBookOrderByRandomCommand(),
                new TogglePageModeCommand(),
                new SetPageModeOneCommand(),
                new SetPageModeTwoCommand(),
                new TogglePageOrientationCommand(),
                new ToggleBookReadOrderCommand(),
                new SetBookReadOrderRightCommand(),
                new SetBookReadOrderLeftCommand(),
                new ToggleIsSupportedDividePageCommand(),
                new ToggleIsSupportedWidePageCommand(),
                new ToggleIsSupportedSingleFirstPageCommand(),
                new ToggleIsSupportedSingleLastPageCommand(),
                new ToggleIsRecursiveFolderCommand(),
                new ToggleSortModeCommand(),
                new SetSortModeFileNameCommand(),
                new SetSortModeFileNameDescendingCommand(),
                new SetSortModeTimeStampCommand(),
                new SetSortModeTimeStampDescendingCommand(),
                new SetSortModeSizeCommand(),
                new SetSortModeSizeDescendingCommand(),
                new SetSortModeEntryCommand(),
                new SetSortModeEntryDescendingCommand(),
                new SetSortModeRandomCommand(),
                new SetDefaultPageSettingCommand(),

                new ToggleBookmarkCommand(),
                new TogglePlaylistItemCommand(),
                new PrevPlaylistItemCommand(),
                new NextPlaylistItemCommand(),
                new PrevPlaylistItemInBookCommand(),
                new NextPlaylistItemInBookCommand(),

                new ToggleCustomSizeCommand(),

                new ToggleResizeFilterCommand(),
                new ToggleGridCommand(),
                new ToggleEffectCommand(),

                new ToggleIsLoupeCommand(),
                new LoupeOnCommand(),
                new LoupeOffCommand(),
                new LoupeScaleUpCommand(),
                new LoupeScaleDownCommand(),
                new OpenOptionsWindowCommand(),
                new OpenSettingFilesFolderCommand(),
                new OpenScriptsFolderCommand(),
                new OpenVersionWindowCommand(),
                new CloseApplicationCommand(),

                new TogglePermitFileCommand(),

                new HelpCommandListCommand(),
                new HelpScriptCommand(),
                new HelpMainMenuCommand(),
                new HelpSearchOptionCommand(),
                new OpenContextMenuCommand(),

                new ExportBackupCommand(),
                new ImportBackupCommand(),
                new ReloadSettingCommand(),
                new SaveSettingCommand(),
                new TouchEmulateCommand(),

                new FocusPrevAppCommand(),
                new FocusNextAppCommand(),

                new StretchWindowCommand(),

                new OpenConsoleCommand(),

                new CancelScriptCommand()
            };

            // command list order
            foreach (var item in list.GroupBy(e => e.Group).SelectMany(e => e).Select((e, index) => (e, index)))
            {
                item.e.Order = item.index;
            }

            // to dictionary
            _elements = list.ToDictionary(e => e.Name);

            // share
            _elements["NextPage"].SetShare(_elements["PrevPage"]);
            _elements["NextOnePage"].SetShare(_elements["PrevOnePage"]);
            _elements["NextScrollPage"].SetShare(_elements["PrevScrollPage"]);
            _elements["NextSizePage"].SetShare(_elements["PrevSizePage"]);
            _elements["NextFolderPage"].SetShare(_elements["PrevFolderPage"]);
            _elements["LastPage"].SetShare(_elements["FirstPage"]);
            _elements["ToggleStretchModeReverse"].SetShare(_elements["ToggleStretchMode"]);
            _elements["SetStretchModeUniformToFill"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToSize"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToVertical"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["SetStretchModeUniformToHorizontal"].SetShare(_elements["SetStretchModeUniform"]);
            _elements["NextPlaylistItemInBook"].SetShare(_elements["PrevPlaylistItemInBook"]);
            _elements["ViewScrollNTypeDown"].SetShare(_elements["ViewScrollNTypeUp"]);
            _elements["ViewScrollDown"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollLeft"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScrollRight"].SetShare(_elements["ViewScrollUp"]);
            _elements["ViewScaleDown"].SetShare(_elements["ViewScaleUp"]);
            _elements["ViewRotateRight"].SetShare(_elements["ViewRotateLeft"]);

            // TODO: pair...

            // デフォルト設定として記憶
            DefaultMemento = CreateCommandCollectionMemento();

            // 廃棄されたコマンドの情報
            var obsoleteCommands = new List<ObsoleteCommandItem>()
            {
                new ObsoleteCommandItem("ToggleVisibleTitleBar", null, 39),
                new ObsoleteCommandItem("ToggleVisiblePagemarkList", "ToggleVisiblePlaylist", 39),
                new ObsoleteCommandItem("TogglePagemark", "TogglePlaylistMark", 39),
                new ObsoleteCommandItem("PrevPagemark", "PrevPlaylistItem", 39),
                new ObsoleteCommandItem("NextPagemark", "NextPlaylistItem", 39),
                new ObsoleteCommandItem("PrevPagemarkInBook", "PrevPlaylistItemInBook", 39),
                new ObsoleteCommandItem("NextPagemarkInBook", "NextPlaylistItemInBook", 39),
            };
            ObsoleteCommands = obsoleteCommands.ToDictionary(e => e.Obsolete);
        }

        #endregion

        #region Methods

        public CommandElement GetElement(string? key)
        {
            if (key is null) return CommandElement.None;

            if (TryGetValue(key, out CommandElement? command))
            {
                return command;
            }
            else
            {
                return CommandElement.None;
            }
        }

        public CommandElement CreateCloneCommand(CommandElement source)
        {
            var cloneCommand = CloneCommand(source);

            Changed?.Invoke(this, new CommandChangedEventArgs(false));

            return cloneCommand;
        }

        public void RemoveCloneCommand(CommandElement command)
        {
            if (command.IsCloneCommand())
            {
                _elements.Remove(command.Name);

                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        private CommandElement CloneCommand(CommandElement source)
        {
            var cloneCommandName = CreateUniqueCommandName(source.NameSource);
            return CloneCommand(source, cloneCommandName);
        }

        private CommandElement CloneCommand(CommandElement source, CommandNameSource name)
        {
            var cloneCommand = source.CloneCommand(name);
            _elements.Add(cloneCommand.Name, cloneCommand);
            ValidateOrder();
            return cloneCommand;
        }

        private CommandNameSource CreateUniqueCommandName(CommandNameSource name)
        {
            if (!_elements.ContainsKey(name.FullName))
            {
                return name;
            }

            for (int id = 2; ; id++)
            {
                var newName = new CommandNameSource(name.Name, id);
                if (!_elements.ContainsKey(newName.FullName))
                {
                    return newName;
                }
            }
        }

        private void ValidateOrder()
        {
            var sorted = _elements.Values
                .OrderBy(e => e.Order)
                .GroupBy(e => e.GetType())
                .Select(group => group.OrderBy(e => e.NameSource))
                .SelectMany(e => e)
                .ToList();

            foreach (var item in sorted.Select((e, i) => (e, i)))
            {
                item.e.Order = item.i;
            }
        }

        /// <summary>
        /// テーブル更新イベントを発行
        /// </summary>
        public void RaiseChanged()
        {
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        private void CommandTable_Changed(object? sender, CommandChangedEventArgs e)
        {
            ChangeCount++;
            ClearInputGestureDirty();
        }

        /// <summary>
        /// 初期設定生成
        /// </summary>
        /// <param name="type">入力スキーム</param>
        /// <returns></returns>
        public static CommandCollection CreateDefaultMemento(InputScheme type)
        {
            var memento = CommandTable.Current.DefaultMemento.Clone();

            // Type.M
            switch (type)
            {
                case InputScheme.TypeA: // default
                    break;

                case InputScheme.TypeB: // wheel page, right click context menu
                    memento["NextScrollPage"].ShortCutKey = "";
                    memento["PrevScrollPage"].ShortCutKey = "";
                    memento["NextPage"].ShortCutKey = "Left,WheelDown";
                    memento["PrevPage"].ShortCutKey = "Right,WheelUp";
                    memento["OpenContextMenu"].ShortCutKey = "RightClick";
                    break;

                case InputScheme.TypeC: // click page
                    memento["NextScrollPage"].ShortCutKey = "";
                    memento["PrevScrollPage"].ShortCutKey = "";
                    memento["NextPage"].ShortCutKey = "Left,LeftClick";
                    memento["PrevPage"].ShortCutKey = "Right,RightClick";
                    memento["ViewScrollUp"].ShortCutKey = "WheelUp";
                    memento["ViewScrollDown"].ShortCutKey = "WheelDown";
                    break;
            }

            return memento;
        }

        public bool TryExecute(object sender, string commandName, object[]? args, CommandOption option)
        {
            if (TryGetValue(commandName, out CommandElement? command))
            {
                var arguments = new CommandArgs(args, option);
                if (command.CanExecute(sender, arguments))
                {
                    command.Execute(sender, arguments);
                }
            }

            return false;
        }

        /// <summary>
        /// 入力ジェスチャーが変更されていたらテーブル更新イベントを発行する
        /// </summary>
        public void FlushInputGesture()
        {
            if (_elements.Values.Any(e => e.IsInputGestureDirty))
            {
                Changed?.Invoke(this, new CommandChangedEventArgs(false));
            }
        }

        /// <summary>
        /// 入力ジェスチャー変更フラグをクリア
        /// </summary>
        public void ClearInputGestureDirty()
        {
            foreach (var command in _elements.Values)
            {
                command.IsInputGestureDirty = false;
            }
        }


        // ショートカット重複チェック
        public List<string> GetOverlapShortCut(string shortcut)
        {
            var overlaps = _elements
                .Where(e => !string.IsNullOrEmpty(e.Value.ShortCutKey) && e.Value.ShortCutKey.Split(',').Contains(shortcut))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
        }

        // マウスジェスチャー重複チェック
        public List<string> GetOverlapMouseGesture(string gesture)
        {
            var overlaps = _elements
                .Where(e => !string.IsNullOrEmpty(e.Value.MouseGesture) && e.Value.MouseGesture.Split(',').Contains(gesture))
                .Select(e => e.Key)
                .ToList();

            return overlaps;
        }

        // コマンドリストをブラウザで開く
        public void OpenCommandListHelp()
        {
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "CommandList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView Command List"));
                writer.WriteLine($"<body><h1>{Properties.Resources.HelpCommandList_Title}</h1>");
                writer.WriteLine($"<p>{Properties.Resources.HelpCommandList_Message}</p>");
                writer.WriteLine("<table class=\"table-slim table-topless\">");
                writer.WriteLine($"<tr><th>{Properties.Resources.Word_Group}</th><th>{Properties.Resources.Word_Command}</th><th>{Properties.Resources.Word_Shortcut}</th><th>{Properties.Resources.Word_Gesture}</th><th>{Properties.Resources.Word_Touch}</th><th>{Properties.Resources.Word_Summary}</th></tr>");
                foreach (var command in _elements.Values.OrderBy(e => e.Order))
                {
                    writer.WriteLine($"<tr><td>{command.Group}</td><td>{command.Text}</td><td>{command.ShortCutKey}</td><td>{new MouseGestureSequence(command.MouseGesture).ToDispString()}</td><td>{command.TouchGesture}</td><td>{command.Remarks}</td></tr>");
                }
                writer.WriteLine("</table>");
                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            ExternalProcess.Start(fileName);
        }

        #endregion

        #region Scripts

        /// <summary>
        /// スクリプトコマンド更新要求
        /// </summary>
        /// <param name="commands">新しいスクリプトコマンド群</param>
        /// <param name="isReplace">登録済コマンドも置き換える</param>
        public void SetScriptCommands(IEnumerable<ScriptCommand> commands, bool isReplace)
        {
            var news = (commands ?? new List<ScriptCommand>())
                .ToList();

            var oldies = _elements.Values
                .OfType<ScriptCommand>()
                .ToList();

            // 入れ替えの場合は既存の設定をすべて削除
            if (isReplace)
            {
                foreach (var command in oldies)
                {
                    _elements.Remove(command.Name);
                }
                oldies = new List<ScriptCommand>();
            }

            // 存在しないものは削除
            var newPaths = news.Select(e => e.Path).ToList();
            var excepts = oldies.Where(e => !newPaths.Contains(e.Path)).ToList();
            foreach (var command in excepts)
            {
                _elements.Remove(command.Name);
            }

            // 既存のものは情報更新
            var updates = oldies.Except(excepts).ToList();
            foreach (var command in updates)
            {
                command.UpdateDocument(false);
            }

            // 新規のものは追加
            var overwritesPaths = updates.Select(e => e.Path).Distinct().ToList();
            var newcomer = news.Where(e => !overwritesPaths.Contains(e.Path)).ToList();
            foreach (var command in newcomer)
            {
                _elements.Add(command.Name, command);
            }

            // re order
            var scripts = _elements.Values.OfType<ScriptCommand>().OrderBy(e => e.NameSource.Name).ThenBy(e => e.NameSource.Number);
            var offset = _elements.Count;
            foreach (var item in scripts.Select((e, i) => (e, i)))
            {
                item.e.Order = offset + item.i;
            }

            Debug.Assert(_elements.Values.GroupBy(e => e.Order).All(e => e.Count() == 1));
            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        #endregion Scripts

        #region Memento CommandCollection

        public CommandCollection CreateCommandCollectionMemento()
        {
            var collection = new CommandCollection();
            foreach (var item in _elements)
            {
                collection.Add(item.Key, item.Value.CreateMemento());
            }
            return collection;
        }

        public void RestoreCommandCollection(CommandCollection? collection)
        {
            if (collection == null) return;

            ScriptManager.Current.UpdateScriptCommands(isForce: false, isReplace: false);

            foreach (var pair in collection)
            {
                if (_elements.ContainsKey(pair.Key))
                {
                    _elements[pair.Key].Restore(pair.Value);
                }
                else
                {
                    var cloneName = CommandNameSource.Parse(pair.Key);
                    if (cloneName.IsClone)
                    {
                        if (_elements.TryGetValue(cloneName.Name, out var source))
                        {
                            var command = CloneCommand(source, cloneName);
                            Debug.Assert(command.Name == pair.Key);
                            command.Restore(pair.Value);
                        }
                        else
                        {
                            Debug.WriteLine($"Warning: No such clone source command '{cloneName.Name}'");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Warning: No such command '{pair.Key}'");
                    }
                }
            }

            Changed?.Invoke(this, new CommandChangedEventArgs(false));
        }

        #endregion
    }
 

    /// <summary>
    /// 保存用コマンドコレクション
    /// </summary>
    public class CommandCollection : Dictionary<string, CommandElement.Memento>
    {
        public CommandCollection Clone()
        {
            var clone = new CommandCollection();
            foreach (var item in this)
            {
                clone.Add(item.Key, (CommandElement.Memento)item.Value.Clone());
            }
            return clone;
        }
    }
}
