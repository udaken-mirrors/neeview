using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    public class ModifierRepeatButton : RepeatButton
    {
        private Window? _window;


        public ModifierRepeatButton()
        {
            this.Loaded += ModifierRepeatButton_Loaded;
            this.Unloaded += ModifierRepeatButton_Unloaded;
            this.MouseEnter += (s, e) => UpdateIsCtrlPressed();
            this.MouseLeave += (s, e) => UpdateIsCtrlPressed();
            this.GotKeyboardFocus += (s, e) => UpdateIsCtrlPressed();
            this.LostKeyboardFocus += (s, e) => UpdateIsCtrlPressed();
        }


        public bool IsCtrlPressed
        {
            get { return (bool)GetValue(IsCtrlPressedProperty); }
            set { SetValue(IsCtrlPressedProperty, value); }
        }

        public static readonly DependencyProperty IsCtrlPressedProperty =
            DependencyProperty.Register("IsCtrlPressed", typeof(bool), typeof(ModifierRepeatButton), new PropertyMetadata(false));


        private void ModifierRepeatButton_Loaded(object sender, RoutedEventArgs e)
        {
            _window = Window.GetWindow(this);
            if (_window is null) return;

            _window.PreviewKeyDown += Window_PreviewKeyDown;
            _window.PreviewKeyUp += Window_PreviewKeyUp;
        }

        private void ModifierRepeatButton_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_window is null) return;

            _window.PreviewKeyDown -= Window_PreviewKeyDown;
            _window.PreviewKeyUp -= Window_PreviewKeyUp;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UpdateIsCtrlPressed();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            UpdateIsCtrlPressed();
        }

        private void UpdateIsCtrlPressed()
        {
            IsCtrlPressed = (IsMouseOver || IsKeyboardFocused) && Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        }

    }
}
