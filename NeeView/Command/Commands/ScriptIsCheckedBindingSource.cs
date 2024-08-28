using NeeLaboratory.Generators;
using System;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// ScriptCommandParameter.IsChecked のバインドソース
    /// </summary>
    /// <remarks>
    /// パラメーターオブジェクトそのものの変更に対応するためにコマンドインスタンスレベルのイベントを監視している。
    /// </remarks>
    [NotifyPropertyChanged]
    public partial class ScriptIsCheckedBindingSource : INotifyPropertyChanged, IDisposable
    {
        private bool _disposedValue;
        private readonly ScriptCommand _command;

        public ScriptIsCheckedBindingSource(ScriptCommand command)
        {
            _command = command;
            _command.ParameterChanged += ParameterSource_ParameterChanged;
        }

        private void ParameterSource_ParameterChanged(object? sender, ParameterChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ScriptCommandParameter.IsChecked))
            {
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsChecked
        {
            get { return _command.GetScriptCommandParameter().IsChecked; }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _command.ParameterChanged -= ParameterSource_ParameterChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
