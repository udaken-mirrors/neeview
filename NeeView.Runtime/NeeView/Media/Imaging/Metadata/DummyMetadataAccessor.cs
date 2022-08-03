namespace NeeView.Media.Imaging.Metadata
{
    public class DummyMetadataAccessor : BitmapMetadataAccessor
    {
        public override string GetFormat()
        {
            return "(Dummy)";
        }

        public override object? GetValue(BitmapMetadataKey key)
        {
            return null;
        }
    }

}
