using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    // TODO: Lock用Transform は専用のものでよいのでは？
    [NotifyPropertyChanged]
    public partial class PageFrameTransformMap : INotifyPropertyChanged
    {

        private readonly Dictionary<PageRange, PageFrameTransform> _map = new();
        private PageFrameTransform _share = new();

        private bool _isFlipLocked;
        private bool _isScaleLocked;
        private bool _isAngleLocked;


        public event PropertyChangedEventHandler? PropertyChanged;


        public PageFrameTransformMap()
        {
        }


        public bool IsFlipLocked
        {
            get { return _isFlipLocked; }
            set
            {
                if (SetProperty(ref _isFlipLocked, value))
                {
                    ClearFlip(false, false);
                }
            }
        }

        public bool IsScaleLocked
        {
            get { return _isScaleLocked; }
            set
            {
                if (SetProperty(ref _isScaleLocked, value))
                {
                    ClearScale(1.0);
                }
            }
        }

        public bool IsAngleLocked
        {
            get { return _isAngleLocked; }
            set
            {
                if (SetProperty(ref _isAngleLocked, value))
                {
                    ClearAngle(0.0);
                }
            }
        }


        public PageFrameTransform Share => _share;


        // NOTE: Disposable
        public PageFrameTransformAccessor CreateAccessor(PageRange range)
        {
            return new PageFrameTransformAccessor(this, ElementAt(range));
        }

        public PageFrameTransform ElementAt(PageRange range)
        {
            if (_map.TryGetValue(range, out var transform))
                return transform;
            else
                return _map[range] = new PageFrameTransform();
        }

        public bool ContainsKey(PageRange range)
        {
            return _map.ContainsKey(range);
        }

        public void ClearFlip(bool isFlipHorizontal, bool isFlipVertical)
        {
            _share.SetFlipHorizontal(isFlipHorizontal, TimeSpan.Zero);
            _share.SetFlipVertical(isFlipVertical, TimeSpan.Zero);

            foreach (var transform in _map.Values)
            {
                transform.SetFlipHorizontal(false, TimeSpan.Zero);
                transform.SetFlipVertical(false, TimeSpan.Zero);
            }
        }

        public void ClearScale(double scale)
        {
            _share.SetScale(scale, TimeSpan.Zero);
            foreach (var transform in _map.Values)
            {
                transform.SetScale(1.0, TimeSpan.Zero);
            }
        }

        public void ClearAngle(double angle)
        {
            _share.SetAngle(angle, TimeSpan.Zero);
            foreach (var transform in _map.Values)
            {
                transform.SetAngle(0.0, TimeSpan.Zero);
            }
        }

        public void ClearPoint(Point point)
        {
            _share.SetPoint(point, TimeSpan.Zero);
            foreach (var transform in _map.Values)
            {
                transform.SetPoint(default, TimeSpan.Zero);
            }
        }

        public void Clear()
        {
            ClearFlip(false, false);
            ClearScale(1.0);
            ClearAngle(0.0);
            ClearPoint(default);

            //_map.Clear();
            _share.Clear();
        }


        /// <summary>
        /// Flip取得 (非同期スレッド用)
        /// </summary>
        /// <param name="range">対象範囲</param>
        /// <returns></returns>
        public (bool IsFlipHorizontal, bool IsFlipVertical) GetFlip(PageRange range)
        {
            if (IsFlipLocked)
            {
                return (_share.IsFlipHorizontal, _share.IsFlipVertical);
            }
            else if (_map.TryGetValue(range, out var transform))
            {
                return (transform.IsFlipHorizontal, transform.IsFlipVertical);
            }
            else
            {
                return (false, false);
            }
        }

        /// <summary>
        /// Scale取得 (非同期スレッド用)
        /// </summary>
        /// <param name="range">対象範囲</param>
        /// <returns></returns>
        public double GetScale(PageRange range)
        {
            if (IsScaleLocked)
            {
                return _share.Scale;
            }
            else if (_map.TryGetValue(range, out var transform))
            {
                return transform.Scale;
            }
            else
            {
                return 1.0;
            }
        }

        /// <summary>
        /// Angle取得 (非同期スレッド用)
        /// </summary>
        /// <param name="range">対象範囲</param>
        /// <returns></returns>
        public double GetAngle(PageRange range)
        {
            if (IsAngleLocked)
            {
                return _share.Angle;
            }
            else if (_map.TryGetValue(range, out var transform))
            {
                return transform.Angle;
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Poiont取得 (非同期スレッド用)
        /// </summary>
        /// <param name="range">対象範囲</param>
        /// <returns></returns>
        public Point GetPoint(PageRange range)
        {
            if (_map.TryGetValue(range, out var transform))
            {
                return transform.Point;
            }
            else
            {
                return default;
            }
        }

    }
}
