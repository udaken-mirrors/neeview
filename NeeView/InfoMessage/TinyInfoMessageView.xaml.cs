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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// TinyInfoMessageView.xaml の相互作用ロジック
    /// </summary>
    public partial class TinyInfoMessageView : UserControl, INotifyPropertyChanged
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


        public TinyInfoMessage Source
        {
            get { return (TinyInfoMessage)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(TinyInfoMessage), typeof(TinyInfoMessageView), new PropertyMetadata(null, SourceChanged));

        private static void SourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TinyInfoMessageView control && control.Source != null)
            {
                control.VM = new TinyInfoMessageViewModel(control.Source);
            }
        }

        /// <summary>
        /// VM property.
        /// </summary>
        public TinyInfoMessageViewModel? VM
        {
            get { return _vm; }
            private set { if (_vm != value) { _vm = value; RaisePropertyChanged(); } }
        }

        private TinyInfoMessageViewModel? _vm;


        /// <summary>
        /// constructor
        /// </summary>
        public TinyInfoMessageView()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }
    }
}
