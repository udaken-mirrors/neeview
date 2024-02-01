using NeeView.PageFrames;

namespace NeeView
{
    public class DragActionFactory
    {
        private DragActionTable _table;
        private IDragTransformContextFactory _contextFactory;


        public DragActionFactory(DragActionTable table, IDragTransformContextFactory contextFactory)
        {
            _table = table;
            _contextFactory = contextFactory;
        }


        public DragActionControl? Create(DragKey dragKey)
        {
            // get DragAction from dragKey
            // get context from DragAction.Category
            // get

            if (!_table.TryGetValue(dragKey, out var source)) return null;

            bool isLoupeTransform = source is LoupeDragAction;
            DragTransformContext? context = isLoupeTransform
                ? _contextFactory.CreateLoupeDragTransformContext()
                : _contextFactory.CreateContentDragTransformContext(IsPointed(source.DragActionCategory));
            if (context is null) return null;
            return source.CreateControl(context);
        }


        public bool IsPointed(DragActionCategory category)
        {
            switch (category)
            {
                case DragActionCategory.Angle:
                    return Config.Current.View.RotateCenter == DragControlCenter.Cursor;
                case DragActionCategory.Scale:
                    return Config.Current.View.ScaleCenter == DragControlCenter.Cursor;
                case DragActionCategory.Flip:
                    return Config.Current.View.FlipCenter == DragControlCenter.Cursor;
                default:
                    return false;
            }
        }
    }

}
