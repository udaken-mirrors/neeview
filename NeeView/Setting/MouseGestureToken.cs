namespace NeeView.Setting
{
    public class MouseGestureToken : GestureToken
    {
        public MouseGestureToken(MouseSequence gesture)
        {
            Gesture = gesture;
        }

        public MouseSequence Gesture { get; set; }
    }
}
