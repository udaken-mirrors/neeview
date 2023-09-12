namespace NeeView.PageFrames
{
    public interface IShareTransformContext
    {
        bool IsFlipLocked { get; }
        bool IsScaleLocked { get; }
        bool IsAngleLocked { get; }
        bool IsKeepAngleBooks { get; }
        bool IsKeepFlipBooks { get; }
        bool IsKeepScaleBooks { get; }
        double ShareAngle { get; set; }
        bool ShareFlipHorizontal { get; set; }
        bool ShareFlipVertical { get; set; }
        double ShareScale { get; set; }
    }
}
