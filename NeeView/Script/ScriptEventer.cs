using NeeLaboratory.Threading.Jobs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// ブック関連のスクリプトイベント発行用
    /// </summary>
    public class ScriptEventer : IDisposable
    {
        private PageFrameBoxPresenter _presenter;
        private bool _disposedValue;


        public ScriptEventer()
        {
            _presenter = PageFrameBoxPresenter.Current;
            _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
            _presenter.ViewPageChanged += Presenter_ViewPageChanged;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _presenter.PageFrameBoxChanged += Presenter_PageFrameBoxChanged;
                    _presenter.ViewPageChanged += Presenter_ViewPageChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Presenter_PageFrameBoxChanged(object? sender, PageFrameBoxChangedEventArgs e)
        {
            NVDebug.AssertSTA();
            if (e.Box is null) return;

            // Script: OnBookLoaded
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnBookLoaded, null, CommandOption.None);
        }

        private void Presenter_ViewPageChanged(object? sender, ViewPageChangedEventArgs e)
        {
            NVDebug.AssertSTA();
            if (!e.Pages.Any()) return;

            // Script: OnPageChanged
            CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);
        }
    }
}
