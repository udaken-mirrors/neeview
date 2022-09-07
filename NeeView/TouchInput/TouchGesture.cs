using System.Windows;

namespace NeeView
{
    public enum TouchGesture
    {
        None,
        TouchL1,
        TouchL2,
        TouchR1,
        TouchR2,
        TouchCenter,
    }


    public static class TouchGestureExtensions
    {
        //
        public static TouchGesture GetTouchGesture(double xRate, double yRate)
        {
            return TouchGesture.TouchCenter.IsTouched(xRate, yRate)
                ? TouchGesture.TouchCenter
                : GetTouchGestureLast(xRate, yRate);
        }

        //
        public static TouchGesture GetTouchGestureLast(double xRate, double yRate)
        {
            return xRate < 0.5
                ? yRate < 0.5 ? TouchGesture.TouchL1 : TouchGesture.TouchL2
                : yRate < 0.5 ? TouchGesture.TouchR1 : TouchGesture.TouchR2;
        }

        //
        public static bool IsTouched(this TouchGesture self, double xRate, double yRate)
        {
            return self switch
            {
                TouchGesture.TouchCenter => 0.33 < xRate && xRate < 0.66 && yRate < 0.75,
                TouchGesture.TouchL1 => xRate < 0.5 && yRate < 0.5,
                TouchGesture.TouchL2 => xRate < 0.5 && !(yRate < 0.5),
                TouchGesture.TouchR1 => !(xRate < 0.5) && yRate < 0.5,
                TouchGesture.TouchR2 => !(xRate < 0.5) && !(yRate < 0.5),
                _ => false,
            };
        }
    }

}
