using NeeLaboratory;
using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// 指定ページ数移動コマンド用パラメータ
    /// </summary>
    public class MoveSizePageCommandParameter : ReversibleCommandParameter
    {
        private int _size = 10;

        [PropertyMember]
        public int Size
        {
            get { return _size; }
            set { SetProperty(ref _size, MathUtility.Clamp(value, 0, 1000)); }
        }
    }
}
