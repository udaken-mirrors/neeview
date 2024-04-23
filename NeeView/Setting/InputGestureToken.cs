namespace NeeView.Setting
{
    public class InputGestureToken : GestureToken
    {
        public InputGestureToken(InputGestureSource gesture)
        {
            Gesture = gesture;
        }

        public InputGestureSource Gesture { get; set; }
    }
}
