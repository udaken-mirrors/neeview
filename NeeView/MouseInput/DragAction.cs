using System.Text.RegularExpressions;

namespace NeeView
{
    public abstract class DragAction
    {
        public delegate DragActionControl CreateDragAction(DragTransformContext context, DragAction? source);

        private static readonly Regex _trimCommand = new(@"DragAction$", RegexOptions.Compiled);

        public DragAction()
        {
            Name = _trimCommand.Replace(GetType().Name, "");
        }

        public DragAction(string name)
        {
            Name = name;
        }


        public string Name { get; }

        public string Note { get; init; } = "";

        public bool IsDummy { get; init; }

        public bool IsLocked { get; init; }

        public DragKey DragKey { get; set; } = DragKey.Empty;

        public DragActionParameterSource? ParameterSource { get; init; }

        public DragActionParameter? Parameter
        {
            get => ParameterSource?.Get();
            set => ParameterSource?.Set(value);
        }

        public DragActionCategory DragActionCategory { get; protected set; }


        public abstract DragActionControl CreateControl(DragTransformContext context);


        #region Memento

        public class Memento
        {
            public DragKey MouseButton { get; set; } = DragKey.Empty;

            public DragActionParameter? Parameter { get; set; }


            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.MouseButton = DragKey;
            memento.Parameter = (DragActionParameter?)Parameter?.Clone();
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            DragKey = memento.MouseButton;
            Parameter = (DragActionParameter?)memento.Parameter?.Clone();
        }

        #endregion
    }

}
