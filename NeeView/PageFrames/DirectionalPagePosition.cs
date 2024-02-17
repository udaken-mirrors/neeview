//#define LOCAL_DEBUG

using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 指向性ページ位置
    /// </summary>
    /// <param name="Position"></param>
    /// <param name="Direction"></param>
    public record struct DirectionalPagePosition(PagePosition Position, LinkedListDirection Direction)
    {
        public int Value => Position.Value;
        public int Index => Position.Index;
        public int Part => Position.Part;

        public void Deconstruct(out PagePosition position, out LinkedListDirection direction)
        {
            position = Position;
            direction = Direction;
        }
    }
}
