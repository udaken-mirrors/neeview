using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class MouseGestureSettingViewModel : BindableBase
    {
        private readonly IDictionary<string, CommandElement> _commandMap;
        private readonly string _key;
        private readonly TouchInputForGestureEditor _touchGesture;
        private readonly MouseInputForGestureEditor _mouseGesture;
        private StringGestureToken _gestureToken = new();
        private string _newGesture = "";

        
        public MouseGestureSettingViewModel(IDictionary<string, CommandElement> commandMap, string key, FrameworkElement gestureSender)
        {
            _commandMap = commandMap;
            _key = key;

            _touchGesture = new TouchInputForGestureEditor(gestureSender);
            _touchGesture.Gesture.GestureProgressed += Gesture_MouseGestureProgressed;

            _mouseGesture = new MouseInputForGestureEditor(gestureSender);
            _mouseGesture.Gesture.GestureProgressed += Gesture_MouseGestureProgressed;

            OriginalGesture = NewGesture = _commandMap[_key].MouseGesture;
            UpdateGestureToken(NewGesture);
        }


        public StringGestureToken GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        public string OriginalGesture { get; set; }

        public string NewGesture
        {
            get { return _newGesture; }
            set { if (_newGesture != value) { _newGesture = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Gesture Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gesture_MouseGestureProgressed(object? sender, MouseGestureEventArgs e)
        {
            NewGesture = e.Sequence.ToString();
            UpdateGestureToken(NewGesture);
        }

        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="gesture"></param>
        public void UpdateGestureToken(string gesture)
        {
            // Check Conflict
            var token = new StringGestureToken(gesture);

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _commandMap
                    .Where(i => i.Key != _key && i.Value.MouseGesture == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Format(Properties.TextResources.GetString("Notice.Conflict"), ResourceService.Join(token.Conflicts.Select(i => CommandTable.Current.GetElement(i).Text)));
                }
            }

            GestureToken = token;
        }

        /// <summary>
        /// 決定
        /// </summary>
        public void Flush()
        {
            _commandMap[_key].MouseGesture = NewGesture;
        }

        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        private RelayCommand? _clearCommand;
        public RelayCommand ClearCommand
        {
            get { return _clearCommand = _clearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            _commandMap[_key].MouseGesture = "";
            _mouseGesture.Gesture.Reset();
        }
    }


    public class StringGestureToken
    {
        public StringGestureToken()
        {
            Gesture = "";
        }

        public StringGestureToken(string gesture)
        {
            Gesture = gesture;
        }


        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<string>? Conflicts { get; set; }

        // 競合メッセージ
        public string? OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;

        public bool IsExist => this.Gesture is not null;
    }
}
