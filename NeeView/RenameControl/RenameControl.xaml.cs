using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// RenameControl.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Support

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            PropertyChanged += (s, e) => { if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == propertyName) handler?.Invoke(s, e); };
        }

        #endregion


        private static readonly char[] _invalidChars = System.IO.Path.GetInvalidFileNameChars();
        private readonly RenameManager _manager;
        private int _keyCount;
        private bool _closed;
        private bool _isInvalidFileNameChars;
        private bool _isInvalidSeparatorChars;
        private bool _isSeleftFileNameBody;
        private bool _isHideExtension;
        private readonly string _oldValue;
        private string _extension = "";
        private string _text;
        private string _oldText;


        public RenameControl(RenameControlSource source)
        {
            InitializeComponent();

            _manager = RenameManager.GetRenameManager(source.Target)
                ?? throw new InvalidOperationException("RenameManager must not be null.");

            this.Target = source.Target;
            this.StoredFocusTarget = source.TargetContainer;
            this.RenameTextBox.FontFamily = this.Target.FontFamily;
            this.RenameTextBox.FontSize = this.Target.FontSize;
            _text = source.Text; // GetFixedText(source.Text, false);
            _oldText = _text;
            _oldValue = _text;

            this.RenameTextBox.DataContext = this;
        }


        /// <summary>
        /// 終了時イベント
        /// </summary>
        public event EventHandler<RenameClosedEventArgs>? Closed;


        // リネームを行うTextBlock
        public TextBlock Target { get; private set; }

        // ファイル名禁則文字制御
        public bool IsInvalidFileNameChars
        {
            get { return _isInvalidFileNameChars; }
            set { SetProperty(ref _isInvalidFileNameChars, value); }
        }

        // パス区切り文字制御
        public bool IsInvalidSeparatorChars
        {
            get { return _isInvalidSeparatorChars; }
            set { SetProperty(ref _isInvalidSeparatorChars, value); }
        }

        // 拡張子を除いた部分を選択
        public bool IsSeleftFileNameBody
        {
            get { return _isSeleftFileNameBody && !_isHideExtension; }
            set { SetProperty(ref _isSeleftFileNameBody, value); }
        }

        // 拡張子を非表示
        public bool IsHideExtension
        {
            get { return _isHideExtension; }
            set
            {
                if (SetProperty(ref _isHideExtension, value))
                {
                    if (_isHideExtension)
                    {
                        _extension = System.IO.Path.GetExtension(_oldValue);
                        Text = System.IO.Path.GetFileNameWithoutExtension(_oldValue);
                        _oldText = Text;
                    }
                    else
                    {
                        _extension = "";
                        Text = _oldValue;
                        _oldText = Text;
                    }
                }
            }
        }

        // 編集文字列
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, GetFixedText(value, true)); }
        }

        // フォーカスを戻すコントロール
        public UIElement? StoredFocusTarget { get; set; }


        private async void Description_StateChanged(object? sender, RenameControlStateChangedEventArgs e)
        {
            switch (e.Action)
            {
                case RenameControlStateChangedAction.LayoutChanged:
                    SyncLayout();
                    break;
                case RenameControlStateChangedAction.ScrollChanged:
                    await CloseAsync(true, true);
                    break;
                default:
                    await CloseAsync(false, false);
                    break;
            }
        }

        private string GetFixedText(string source, bool withToast)
        {
            if (_isInvalidFileNameChars)
            {
                return GetFixedInvalidFileNameCharsText(source, withToast);
            }
            else if (_isInvalidSeparatorChars)
            {
                return GetFixedInvalidSeparatorCharsText(source, withToast);
            }
            else
            {
                return source;
            }
        }

        private static string GetFixedInvalidFileNameCharsText(string source, bool withToast)
        {
            var text = new string(source.Where(e => !_invalidChars.Contains(e)).ToArray());
            if (withToast && text != source)
            {
                ToastService.Current.Show(new Toast(Properties.Resources.Notice_InvalidFileNameChars, "", ToastIcon.Information));
            }
            return text;
        }

        private static string GetFixedInvalidSeparatorCharsText(string source, bool withToast)
        {
            var text = new string(source.Where(e => !LoosePath.Separators.Contains(e)).ToArray());
            if (withToast && text != source)
            {
                ToastService.Current.Show(new Toast(Properties.Resources.Notice_InvalidSeparatorChars, "", ToastIcon.Information));
            }
            return text;
        }

        public async Task<RenameControlResult> ShowAsync()
        {
            var tcs = new TaskCompletionSource<RenameControlResult>();
            Closed += RenameControl_Closed;
            _manager.Add(this);
            var result = await tcs.Task;
            Closed -= RenameControl_Closed;
            return result;

            void RenameControl_Closed(object? sender, RenameClosedEventArgs e)
            {
                tcs.TrySetResult(new RenameControlResult(e.OldValue, e.NewValue, e.MoveRename, e.IsRestoreFocus));
            }
        }

        public static async Task<RenameControlResult> ShowAsync(RenameControlSource source)
        {
            var renameControl = new RenameControl(source);
            return await renameControl.ShowAsync();
        }

        /// <summary>
        /// Rename開始
        /// </summary>
        public void Open()
        {
            _manager.Add(this);
        }

        /// <summary>
        /// Rename終了
        /// </summary>
        /// <param name="isSuccess">名前変更成功</param>
        /// <param name="isRestoreFocus">元のコントロールにフォーカスを戻す要求</param>
        /// <param name="moveRename">次の項目に名前変更を要求</param>
        public async Task CloseAsync(bool isSuccess, bool isRestoreFocus = true, int moveRename = 0)
        {
            Debug.Assert(-1 <= moveRename && moveRename <= 1);

            if (_closed) return;
            _closed = true;
            this.IsHitTestVisible = false;

            var newText = Text.Trim();
            var newValue = isSuccess ? newText + _extension : _oldValue;
            var restoreFocus = isRestoreFocus && this.RenameTextBox.IsFocused;

            if (_oldValue != newValue)
            {
                this.Text = isSuccess ? newText : _oldText;
                await OnRenameAsync(_oldValue, newValue);
            }

            _manager.Remove(this);

            if (restoreFocus && StoredFocusTarget != null)
            {
                FocusTools.FocusIfWindowActived(StoredFocusTarget);
            }

            var args = new RenameClosedEventArgs(_oldValue, newValue, moveRename, restoreFocus);
            Closed?.Invoke(this, args);
        }

        protected virtual async Task<bool> OnRenameAsync(string oldValue, string newValue)
        {
            return await Task.FromResult(true);
        }

        private async void RenameTextBox_LostFocus(object? sender, RoutedEventArgs e)
        {
            await CloseAsync(true);
        }

        private void RenameTextBox_Loaded(object? sender, RoutedEventArgs e)
        {
            // 拡張子以外を選択状態にする
            string name = this.IsSeleftFileNameBody ? LoosePath.GetFileNameWithoutExtension(Text) : Text;
            this.RenameTextBox.Select(0, name.Length);

            // 表示とともにフォーカスする
            this.RenameTextBox.Focus();
        }

        private void RenameTextBox_Unloaded(object? sender, RoutedEventArgs e)
        {
        }

        private void RenameTextBox_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            // 最初の方向入力に限りカーソル位置を固定する
            if (_keyCount == 0 && (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right))
            {
                this.RenameTextBox.Select(this.RenameTextBox.SelectionStart + this.RenameTextBox.SelectionLength, 0);
                _keyCount++;
            }
        }

        private async void RenameTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                await CloseAsync(false);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                await CloseAsync(true);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                var moveRename = (Keyboard.Modifiers == ModifierKeys.Shift) ? -1 : +1;
                await CloseAsync(true, true, moveRename);
                e.Handled = true;
            }
        }

        private async void RenameTextBox_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
        {
            await CloseAsync(true);
            e.Handled = true;
        }

        private void MeasureText_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            this.RenameTextBox.MinWidth = Math.Min(this.MeasureText.ActualWidth + 30, this.MaxWidth);
        }

        // 単キーコマンド無効
        private void Control_KeyDown_IgnoreSingleKeyGesture(object? sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        /// <summary>
        /// renameコントロールをターゲットの位置に合わせる
        /// </summary>
        public void SyncLayout()
        {
            if (this.Target is null) throw new InvalidOperationException();

            var pos = this.Target.TranslatePoint(new Point(0, 0), _manager) - new Vector(3, 2);
            Canvas.SetLeft(this, pos.X);
            Canvas.SetTop(this, pos.Y);

            this.MaxWidth = _manager.ActualWidth - pos.X - 8;
        }
    }
}
