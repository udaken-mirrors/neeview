using System.Threading;

namespace NeeView
{
    public class PictureNamedStreamSource : PictureStreamSource
    {
        private static readonly PictureStream _pictureStream = new();


        public PictureNamedStreamSource(ArchiveEntry entry) : base(entry)
        {
        }


        public string? Decoder { get; private set; }

        public override void Initialize(CancellationToken token)
        {
            if (IsInitialized()) return;

            using (var namedStream = _pictureStream.Create(this.ArchiveEntry))
            {
                InitializeCore(namedStream.Stream, namedStream.RawData, token);
                Decoder = namedStream.Name;
            }
        }
    }


}
