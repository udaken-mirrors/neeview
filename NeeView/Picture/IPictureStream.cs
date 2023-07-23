namespace NeeView
{
    /// <summary>
    /// PictureStream Interface
    /// </summary>
    public interface IPictureStream
    {
        /// <summary>
        /// 画像ストリームを取得
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        NamedStream? Create(ArchiveEntry entry);
    }

}
