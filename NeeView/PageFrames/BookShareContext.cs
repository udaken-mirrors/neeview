namespace NeeView.PageFrames
{
    /// <summary>
    /// ブック間で共通の設定値
    /// </summary>
    public class BookShareContext : IShareTransformContext
    {
        private readonly Config _config;

        public BookShareContext(Config config)
        {
            _config = config;
        }

        public Config Config => _config;

        public ViewScrollContext ViewScrollContext { get; } = new();

        public bool IsFlipLocked => _config.View.IsKeepFlip;
        public bool IsScaleLocked => _config.View.IsKeepScale;
        public bool IsAngleLocked => _config.View.IsKeepAngle;

        public bool IsKeepFlipBooks => _config.View.IsKeepFlipBooks;
        public bool IsKeepScaleBooks => _config.View.IsKeepScaleBooks;
        public bool IsKeepAngleBooks => _config.View.IsKeepAngleBooks;

        public double ShareScale { get; set; } = 1.0;
        public double ShareAngle { get; set; }
        public bool ShareFlipHorizontal { get; set; }
        public bool ShareFlipVertical { get; set; }
    }
}
