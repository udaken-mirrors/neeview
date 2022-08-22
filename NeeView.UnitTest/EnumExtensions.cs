namespace NeeView.UnitTest
{
    public class EnumExtensions
    {
        private enum EnumA
        {
            A, B, C
        };
        private enum EnumB
        {
            X = 100, Y, Z
        };

        private string nullString = null!;


        [Fact]
        public void ToEnumFunction()
        {
            Assert.Equal(EnumA.C, "C".ToEnum(typeof(EnumA)));
            Assert.Throws<InvalidCastException>(() => "D".ToEnum(typeof(EnumA)));
            Assert.Throws<InvalidCastException>(() => nullString.ToEnum(typeof(EnumA)));
        }

        [Fact]
        public void ToEnumOrDefaultFunction()
        {
            Assert.Equal(EnumA.C, "C".ToEnumOrDefault(typeof(EnumA)));
            Assert.Equal(EnumA.A, "D".ToEnumOrDefault(typeof(EnumA)));
            Assert.Equal(EnumB.Z, "Z".ToEnumOrDefault(typeof(EnumB)));
            Assert.Equal(EnumB.X, "D".ToEnumOrDefault(typeof(EnumB)));
            Assert.Equal(EnumB.X, nullString.ToEnumOrDefault(typeof(EnumB)));
        }

        [Fact]
        public void ToEnumGeneric()
        {
            Assert.Equal(EnumA.C, "C".ToEnum<EnumA>());
            Assert.Throws<InvalidCastException>(() => "D".ToEnum<EnumA>());
            Assert.Throws<InvalidCastException>(() => nullString.ToEnum<EnumA>());
        }

        [Fact]
        public void ToEnumOrDefaultGeneric()
        {
            Assert.Equal(EnumA.C, "C".ToEnumOrDefault<EnumA>());
            Assert.Equal(EnumA.A, "D".ToEnumOrDefault<EnumA>());
            Assert.Equal(EnumB.Z, "Z".ToEnumOrDefault<EnumB>());
            Assert.Equal(EnumB.X, "D".ToEnumOrDefault<EnumB>());
            Assert.Equal(EnumB.X, nullString.ToEnumOrDefault<EnumB>());
        }
    }
}