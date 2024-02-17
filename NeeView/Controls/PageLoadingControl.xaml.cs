using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NeeView
{
    /// <summary>
    /// PageLoadingControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageLoadingControl : UserControl
    {
        public PageLoadingControl()
        {
            InitializeComponent();
            this.SetBinding(IsActiveProperty, new Binding(nameof(PageLoadingViewModel.IsActive)));
            this.MessageTextBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(PageLoadingViewModel.Message)));
            Update();
        }


        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(PageLoadingControl), new PropertyMetadata(false, IsActiveProperty_Changed));


        private static void IsActiveProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PageLoadingControl control)
            {
                control.Update();
            }
        }


        private void Update()
        {
            if (IsActive)
            {
                this.Root.Opacity = 0;
                var ani = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5)) { BeginTime = TimeSpan.FromSeconds(0.5) };
                this.Root.BeginAnimation(UIElement.OpacityProperty, ani, HandoffBehavior.SnapshotAndReplace);
                this.Root.Visibility = Visibility.Visible;

                this.ProgressRing.IsActive = true;
            }
            else
            {
                this.Root.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
                this.Root.Visibility = Visibility.Collapsed;

                this.ProgressRing.IsActive = false;
            }
        }
    }



    [NotifyPropertyChanged]
    public partial class PageLoadingViewModel : INotifyPropertyChanged
    {
        private PageLoading _model;

        public PageLoadingViewModel(PageLoading model)
        {
            _model = model;
            _model.SubscribePropertyChanged(nameof(_model.IsActive), (s, e) => RaisePropertyChanged(nameof(IsActive)));
            _model.SubscribePropertyChanged(nameof(_model.Message), (s, e) => RaisePropertyChanged(nameof(Message)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsActive
        {
            get => _model.IsActive;
            set => _model.IsActive = value;
        }

        public string Message
        {
            get => _model.Message;
            set => _model.Message = value;
        }
    }


    [NotifyPropertyChanged]
    public partial class PageLoading : INotifyPropertyChanged
    {
        private Locker _locker;
        private bool _isActive;
        private string _message = "Loading...";

        public PageLoading()
        {
            _locker = new Locker();
            _locker.LockCountChanged += Locker_LockCountChanged;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsActive
        {
            get { return _isActive; }
            set { SetProperty(ref _isActive, value); }
        }

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }


        public Locker.Key Lock() => _locker.Lock();

        private void Locker_LockCountChanged(object? sender, LockCountChangedEventArgs e)
        {
            IsActive = e.IsLocked;
        }
    }
}
