﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    public record class PageFrameTransformKey(Page? Page, PagePart Part)
    {
        public static PageFrameTransformKey Dummy { get; } = new PageFrameTransformKey(null, PagePart.All);
    }

    public static class PageFrameTransformTool
    {
        /// <summary>
        /// PageFrame から PageFrameTransformKey を生成する
        /// </summary>
        /// <param name="pageFrame"></param>
        /// <returns></returns>
        public static PageFrameTransformKey CreateKey(PageFrame pageFrame)
        {
            Debug.Assert(pageFrame.Elements.Any());
            var element = pageFrame.Elements.First();
            return new PageFrameTransformKey(element.Page, element.PagePart);
        }
    }

    // TODO: Lock用Transform は専用のものでよいのでは？
    [NotifyPropertyChanged]
    public partial class PageFrameTransformMap : INotifyPropertyChanged
    {
        private readonly Dictionary<PageFrameTransformKey, PageFrameTransform> _map = new();
        private PageFrameTransform _share = new();

        private IShareTransformContext _shareContext;
        private bool _isFlipLocked;
        private bool _isScaleLocked;
        private bool _isAngleLocked;


        public event PropertyChangedEventHandler? PropertyChanged;


        public PageFrameTransformMap(IShareTransformContext shareTransformContext)
        {
            _shareContext = shareTransformContext;

            _isScaleLocked = _shareContext.IsScaleLocked;
            _isAngleLocked = _shareContext.IsAngleLocked;
            _isFlipLocked = _shareContext.IsFlipLocked;

            if (_isScaleLocked && _shareContext.IsKeepScaleBooks)
            {
                _share.SetScale(_shareContext.ShareScale, TimeSpan.Zero);
            }
            if (_isAngleLocked && _shareContext.IsKeepAngleBooks)
            {
                _share.SetAngle(_shareContext.ShareAngle, TimeSpan.Zero);
            }
            if (_isFlipLocked && _shareContext.IsKeepFlipBooks)
            {
                _share.SetFlipHorizontal(_shareContext.ShareFlipHorizontal, TimeSpan.Zero);
                _share.SetFlipVertical(_shareContext.ShareFlipVertical, TimeSpan.Zero);
            }

            _share.TransformChanged += Share_TransformChanged;
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


        private void Share_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Action)
            {
                case TransformAction.Scale:
                    _shareContext.ShareScale = _share.Scale;
                    break;
                case TransformAction.Angle:
                    _shareContext.ShareAngle = _share.Angle;
                    break;
                case TransformAction.FlipHorizontal:
                    _shareContext.ShareFlipHorizontal = _share.IsFlipHorizontal;
                    break;
                case TransformAction.FlipVertical:
                    _shareContext.ShareFlipVertical = _share.IsFlipVertical;
                    break;
            }
        }

        // NOTE: Disposable
        public PageFrameTransformAccessor CreateAccessor(PageFrameTransformKey position)
        {
            return new PageFrameTransformAccessor(this, ElementAt(position));
        }

        public PageFrameTransform ElementAt(PageFrameTransformKey position)
        {
            if (_map.TryGetValue(position, out var transform))
                return transform;
            else
                return _map[position] = new PageFrameTransform();
        }

        public bool ContainsKey(PageFrameTransformKey position)
        {
            return _map.ContainsKey(position);
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
            Clear(TransformMask.All);
        }

        public void Clear(TransformMask mask)
        {
            if (mask.HasFlag(TransformMask.Flip))
            {
                ClearFlip(false, false);
            }
            if (mask.HasFlag(TransformMask.Scale))
            {
                ClearScale(1.0);
            }
            if (mask.HasFlag(TransformMask.Angle))
            {
                ClearAngle(0.0);
            }
            if (mask.HasFlag(TransformMask.Point))
            {
                ClearPoint(default);
            }

            _share.Clear(mask);
        }

        /// <summary>
        /// Flip取得 (非同期スレッド用)
        /// </summary>
        /// <param name="position">ページ位置</param>
        /// <returns></returns>
        public (bool IsFlipHorizontal, bool IsFlipVertical) GetFlip(PageFrameTransformKey position)
        {
            if (IsFlipLocked)
            {
                return (_share.IsFlipHorizontal, _share.IsFlipVertical);
            }
            else if (_map.TryGetValue(position, out var transform))
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
        /// <param name="position">ページ位置</param>
        /// <returns></returns>
        public double GetScale(PageFrameTransformKey position)
        {
            if (IsScaleLocked)
            {
                return _share.Scale;
            }
            else if (_map.TryGetValue(position, out var transform))
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
        /// <param name="position">ページ位置</param>
        /// <returns></returns>
        public double GetAngle(PageFrameTransformKey position)
        {
            if (IsAngleLocked)
            {
                return _share.Angle;
            }
            else if (_map.TryGetValue(position, out var transform))
            {
                return transform.Angle;
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Point取得 (非同期スレッド用)
        /// </summary>
        /// <param name="position">ページ位置</param>
        /// <returns></returns>
        public Point GetPoint(PageFrameTransformKey position)
        {
            if (_map.TryGetValue(position, out var transform))
            {
                return transform.Point;
            }
            else
            {
                return default;
            }
        }

    }


    [Flags]
    public enum TransformMask
    {
        None = 0,
        Flip = (1 << 0),
        Scale = (1 << 1),
        Angle = (1 << 2),
        Point = (1 << 3),

        All = Flip | Scale | Angle | Point
    }
}