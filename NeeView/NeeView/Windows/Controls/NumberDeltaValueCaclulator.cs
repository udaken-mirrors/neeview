namespace NeeView.Windows.Controls
{
    public class NumberDeltaValueCaclulator : IValueDeltaCalculator
    {
        public double Scale { get; set; } = 1.0;

        public object Calc(object value, int delta)
        {
            return value switch
            {
                int n => (int)(n + delta * Scale),
                double n => n + delta * Scale,
                _ => value,
            };
        }
    }
}
