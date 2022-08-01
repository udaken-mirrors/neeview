using NeeView.Text;

namespace NeeView.UnitTest
{
    public class NatualComparer
    {
        [Fact]
        public void NormalDigitCharacter()
        {
            var nums = new List<string> { "9", "8", "7", "6", "5", "4", "3", "2", "1", "0" };
            nums.Sort(new NaturalComparer());
            Assert.Equal(new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" }, nums);
        }

        [Fact]
        public void KanjiDigitCharacter()
        {
            var nums = new List<string> { "十", "九", "八", "七", "六", "五", "四", "三", "二", "一", "零" };
            nums.Sort(new NaturalComparer());
            Assert.Equal(new List<string> { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" }, nums);
        }

        /// <summary>
        /// アラビア数字外で数字と認識される文字で問題が発生しないかをチェック。
        /// 基本的にアラビア数字以外は普通の文字として処理します。
        /// </summary>
        [Fact]
        public void SpecialDigitCharacter()
        {
            string a = @"߁";
            string b = @"߂";
            Assert.True(char.IsDigit(a[0]));
            Assert.True(char.IsDigit(b[0]));

            var comp = new NaturalComparer();
            var result = comp.Compare(a, b);
            Assert.True(result < 0);
        }
    }
}