using System.Windows.Media;

namespace NeeView
{
    public class AudioInfo
    {
        public AudioInfo(ArchiveEntry archiveEntry, string title, string album, string artist, ImageSource? coverImage)
        {
            ArchiveEntry = archiveEntry;
            Title = title;
            Album = album;
            Artist = artist;
            CoverImage = coverImage;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public string Title { get; }
        public string Album { get; }
        public string Artist { get; }
        public ImageSource? CoverImage { get; }
    }
}
