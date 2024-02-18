using System;

namespace NeeView
{
    public interface IBookPlaylistControl
    {
        bool IsMarked { get; }

        event EventHandler? MarkersChanged;

        bool CanMark();
        bool CanMark(Page page);
        bool CanNextMarkInPlace(MovePlaylistItemInBookCommandParameter param);
        bool CanPrevMarkInPlace(MovePlaylistItemInBookCommandParameter param);
        void NextMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param);
        void PrevMarkInPlace(object? sender, MovePlaylistItemInBookCommandParameter param);
        PlaylistItem? SetMark(bool isMark);
        PlaylistItem? ToggleMark();
        void UpdateMarkers();
    }
}
