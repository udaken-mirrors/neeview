using System;

namespace NeeView
{
    public interface IBookPlaylistControl
    {
        bool IsMarked { get; }

        event EventHandler? MarkersChanged;

        bool CanMark();
        bool CanMark(Page page);
        bool CanNextMarkInPlace(MovePlaylsitItemInBookCommandParameter param);
        bool CanPrevMarkInPlace(MovePlaylsitItemInBookCommandParameter param);
        void NextMarkInPlace(object? sender, MovePlaylsitItemInBookCommandParameter param);
        void PrevMarkInPlace(object? sender, MovePlaylsitItemInBookCommandParameter param);
        PlaylistItem? SetMark(bool isMark);
        PlaylistItem? ToggleMark();
        void UpdateMarkers();
    }
}