﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

namespace NeeView
{
    /// <summary>
    /// コマンドの種類
    /// </summary>
    public enum CommandType
    {
        OpenSettingWindow,
        CloseApplication,

        LoadAs,
        OpenFilePlace,

        ClearHistory,

        PrevPage,
        NextPage,
        PrevOnePage,
        NextOnePage,
        PrevScrollPage,
        NextScrollPage,

        FirstPage,
        LastPage,

        PrevFolder,
        NextFolder,

        ToggleFolderOrder,
        SetFolderOrderByFileName,
        SetFolderOrderByTimeStamp,
        SetFolderOrderByRandom,

        ToggleTopmost,
        ToggleHideMenu,

        ToggleFullScreen,
        SetFullScreen,
        CancelFullScreen,

        ToggleSlideShow,

        ToggleStretchMode,
        SetStretchModeNone,
        SetStretchModeInside,
        SetStretchModeOutside,
        SetStretchModeUniform,
        SetStretchModeUniformToFill,
        SetStretchModeUniformToVertical,

        ToggleIsEnabledNearestNeighbor,

        ToggleBackground,
        SetBackgroundBlack,
        SetBackgroundWhite,
        SetBackgroundAuto,
        SetBackgroundCheck,

        TogglePageMode,
        SetPageMode1,
        SetPageMode2,

        ToggleBookReadOrder,
        SetBookReadOrderRight,
        SetBookReadOrderLeft,

        ToggleIsSupportedDividePage,
        ToggleIsSupportedWidePage,
        ToggleIsSupportedSingleFirstPage,
        ToggleIsSupportedSingleLastPage,

        ToggleIsRecursiveFolder,

        ToggleSortMode,
        SetSortModeFileName,
        SetSortModeFileNameDescending,
        SetSortModeTimeStamp,
        SetSortModeTimeStampDescending,
        SetSortModeRandom,

        ToggleIsReverseSort, // 欠番

        ViewScrollUp,
        ViewScrollDown,
        ViewScaleUp,
        ViewScaleDown,
        ViewRotateLeft,
        ViewRotateRight,
        ViewReset,
    }

    public static class CommandTypeExtensions
    {
        public static bool IsDisable(this CommandType type)
        {
            return (type == CommandType.ToggleIsReverseSort);
        }
    }
}
