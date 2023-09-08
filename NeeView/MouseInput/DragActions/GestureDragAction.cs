namespace NeeView
{
    // TODO:
    public class GestureDragAction : DragAction
    {
        public GestureDragAction(string name) : base(name)
        {
            Note = Properties.Resources.DragActionType_Gesture;
            IsLocked = true;
            IsDummy = true;
            DragKey = new DragKey("RightButton");
            DragActionCategory = DragActionCategory.None;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            throw new System.NotImplementedException();
        }
    }

}
