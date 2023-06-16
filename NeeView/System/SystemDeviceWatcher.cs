﻿using NeeLaboratory.ComponentModel;
using NeeView.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NeeView
{

    public class DriveChangedEventArgs : EventArgs
    {
        public DriveChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }
    }

    public class MediaChangedEventArgs : EventArgs
    {
        public MediaChangedEventArgs(string driveName, bool isAlive)
        {
            Name = driveName;
            IsAlive = isAlive;
        }

        public string Name { get; set; }
        public bool IsAlive { get; set; }
    }

    public enum DirectoryChangeType
    {
        Created = 1,
        Deleted = 2,
        Changed = 4,
        Renamed = 8,
        All = 15
    }

    public class DirectoryChangedEventArgs : EventArgs
    {
        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath, string? oldFullpath)
        {
            if (changeType == DirectoryChangeType.All) throw new ArgumentOutOfRangeException(nameof(changeType));

            ChangeType = changeType;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));

            if (changeType == DirectoryChangeType.Renamed)
            {
                OldFullPath = oldFullpath ?? throw new ArgumentNullException(nameof(oldFullpath));

                if (Path.GetDirectoryName(OldFullPath) != Path.GetDirectoryName(FullPath))
                {
                    throw new ArgumentException("Not same directory");
                }
            }
        }

        public DirectoryChangedEventArgs(DirectoryChangeType changeType, string fullPath) : this(changeType, fullPath, null)
        {
            if (changeType == DirectoryChangeType.Renamed) throw new InvalidOperationException();
        }


        public DirectoryChangeType ChangeType { get; set; }
        public string FullPath { get; set; }
        public string? OldFullPath { get; set; }
    }

    public class SettingChangedEventArgs : EventArgs
    {
        public SettingChangedEventArgs(uint action, string? message)
        {
            Action = action;
            Message = message;
        }

        public uint Action { get; private set; }
        public string? Message { get; private set; }
    }

    public class SystemDeviceWatcher
    {
        static SystemDeviceWatcher() => Current = new SystemDeviceWatcher();
        public static SystemDeviceWatcher Current { get; }


        private Window? _window;

        public SystemDeviceWatcher()
        {
        }

        public event EventHandler<DriveChangedEventArgs>? DriveChanged;

        public IDisposable SubscribeDriveChanged(EventHandler<DriveChangedEventArgs> handler)
        {
            DriveChanged += handler;
            return new AnonymousDisposable(() => DriveChanged -= handler);
        }

        public event EventHandler<MediaChangedEventArgs>? MediaChanged;

        public IDisposable SubscribeMediaChanged(EventHandler<MediaChangedEventArgs> handler)
        {
            MediaChanged += handler;
            return new AnonymousDisposable(() => MediaChanged -= handler);
        }

        public event EventHandler<DirectoryChangedEventArgs>? DirectoryChanged;
        
        public IDisposable SubscribeDirectoryChanged(EventHandler<DirectoryChangedEventArgs> handler)
        {
            DirectoryChanged += handler;
            return new AnonymousDisposable(() => DirectoryChanged -= handler);
        }

        public event EventHandler<SettingChangedEventArgs>? SettingChanged;

        public IDisposable SubscribeSettingChanged(EventHandler<SettingChangedEventArgs> handler)
        {
            SettingChanged += handler;
            return new AnonymousDisposable(() => SettingChanged -= handler);
        }

        public event EventHandler? EnterSizeMove;

        public IDisposable SubscribeEnterSizeMove(EventHandler handler)
        {
            EnterSizeMove += handler;
            return new AnonymousDisposable(() => EnterSizeMove -= handler);
        }

        public event EventHandler? ExitSizeMove;

        public IDisposable SubscribeExitSizeMove(EventHandler handler)
        {
            ExitSizeMove += handler;
            return new AnonymousDisposable(() => ExitSizeMove -= handler);
        }


        // ウィンドウプロシージャ初期化
        public void Initialize(Window window)
        {
            if (_window != null) throw new InvalidOperationException();

            var hsrc = HwndSource.FromVisual(window) as HwndSource ?? throw new InvalidOperationException("Cannot get window handle");

            _window = window;

            var notifyEntry = new SHChangeNotifyEntry() { pIdl = IntPtr.Zero, Recursively = true };
            var notifyId = NativeMethods.SHChangeNotifyRegister(hsrc.Handle,
                                                  SHChangeNotifyRegisterFlags.SHCNRF_ShellLevel,
                                                  SHChangeNotifyEvents.SHCNE_MEDIAINSERTED | SHChangeNotifyEvents.SHCNE_MEDIAREMOVED
                                                  | SHChangeNotifyEvents.SHCNE_MKDIR | SHChangeNotifyEvents.SHCNE_RMDIR | SHChangeNotifyEvents.SHCNE_RENAMEFOLDER,
                                                  (uint)WindowMessages.WM_SHNOTIFY,
                                                  1,
                                                  ref notifyEntry);

            hsrc.AddHook(WndProc);
        }


        // ウィンドウプロシージャ
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                switch ((WindowMessages)msg)
                {
                    case WindowMessages.WM_ENTERSIZEMOVE:
                        EnterSizeMove?.Invoke(this, EventArgs.Empty);
                        break;
                    case WindowMessages.WM_EXITSIZEMOVE:
                        ExitSizeMove?.Invoke(this, EventArgs.Empty);
                        break;
                    case WindowMessages.WM_DEVICECHANGE:
                        OnDeviceChange(wParam, lParam);
                        break;
                    case WindowMessages.WM_SHNOTIFY:
                        OnSHNotify(wParam, lParam);
                        break;
                    case WindowMessages.WM_SETTINGCHANGE:
                        OnSettingChange(wParam, lParam);
                        break;
                    case WindowMessages.WM_MOUSEACTIVATE:
                        OnMouseActive();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// 設定変更イベント
        /// </summary>
        private void OnSettingChange(IntPtr wParam, IntPtr lParam)
        {
            var action = (uint)wParam;
            string? str = Marshal.PtrToStringAuto(lParam);
            ////Trace.WriteLine($"WM_SETTINGCHANGE: {action:X4}, {str}");

            SettingChanged?.Invoke(this, new SettingChangedEventArgs(action, str));
        }

        /// <summary>
        /// マウスボタンを押すことでウィンドウをアクティブ化するメッセージ処理
        /// </summary>
        private static void OnMouseActive()
        {
            if (Config.Current.Window.MouseActivateAndEat)
            {
                MainWindow.Current.SetMouseActivage();
            }
        }

        private void OnDeviceChange(IntPtr wParam, IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
            {
                return;
            }

            var volume = (DEV_BROADCAST_VOLUME?)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME));
            if (volume is null) return;

            var driveName = UnitMaskToDriveName(volume.Value.dbcv_unitmask);
            if (driveName == null)
            {
                return;
            }

            switch ((DeviceBroadcastTtiggers)wParam.ToInt32())
            {
                case DeviceBroadcastTtiggers.DBT_DEVICEARRIVAL:
                    ////Debug.WriteLine("DBT_DEVICEARRIVAL");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, true));
                    break;
                case DeviceBroadcastTtiggers.DBT_DEVICEREMOVECOMPLETE:
                    ////Debug.WriteLine("DBT_DEVICEREMOVECOMPLETE");
                    DriveChanged?.Invoke(this, new DriveChangedEventArgs(driveName, false));
                    break;
            }
        }

        private static string? UnitMaskToDriveName(uint unitmask)
        {
            for (int i = 0; i < 32; ++i)
            {
                if ((unitmask >> i & 1) == 1)
                {
                    return ((char)('A' + i)).ToString() + ":\\";
                }
            }

            return null;
        }

        // TODO:全てのフォルダーの変更が通知される。これは必要な機能としては過剰すぎる。本当に必要か再検討せよ。
        // TODO: 重い処理が多いので、集積かBeginInvokeかする。
        private void OnSHNotify(IntPtr wParam, IntPtr lParam)
        {
            var shNotify = (SHNOTIFYSTRUCT)Marshal.PtrToStructure(wParam, typeof(SHNOTIFYSTRUCT))!;

            var shcne = (SHChangeNotifyEvents)lParam;

            ////Debug.WriteLine(shcne + ": " + shNotify);

            switch (shcne)
            {
                case SHChangeNotifyEvents.SHCNE_MEDIAINSERTED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (path is not null)
                        {
                            MediaChanged?.Invoke(this, new MediaChangedEventArgs(path, true));
                        }
                    }
                    break;
                case SHChangeNotifyEvents.SHCNE_MEDIAREMOVED:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (path is not null)
                        {
                            MediaChanged?.Invoke(this, new MediaChangedEventArgs(path, false));
                        }
                    }
                    break;

                case SHChangeNotifyEvents.SHCNE_MKDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Created, path));
                        }
                    }
                    break;

                case SHChangeNotifyEvents.SHCNE_RMDIR:
                    {
                        var path = PIDLToString(shNotify.dwItem1);
                        if (!string.IsNullOrEmpty(path))
                        {
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Deleted, path));
                        }
                    }
                    break;

                case SHChangeNotifyEvents.SHCNE_RENAMEFOLDER:
                    {
                        var path1 = PIDLToString(shNotify.dwItem1);
                        var path2 = PIDLToString(shNotify.dwItem2);
                        if (!string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path2) && path1 != path2)
                        {
                            // path1 is new, path2 is old, maybe.
                            DirectoryChanged?.Invoke(this, new DirectoryChangedEventArgs(DirectoryChangeType.Renamed, path2, path1));
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private static string? PIDLToString(IntPtr dwItem)
        {
            if (dwItem == IntPtr.Zero) return null;

            var buff = new StringBuilder(1024);
            var isSuccess = NativeMethods.SHGetPathFromIDList(dwItem, buff);
            if (!isSuccess) return null;

            return buff.ToString(); ;
        }

    }
}