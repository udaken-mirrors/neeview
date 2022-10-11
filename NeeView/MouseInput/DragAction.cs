using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class DragAction
    {
        private static readonly Regex _trimCommand = new(@"DragAction$", RegexOptions.Compiled);


        public DragAction()
        {
            Name = _trimCommand.Replace(this.GetType().Name, "");
        }

        public DragAction(string name)
        {
            Name = name;
        }


        public string Name { get; set; }

        public string Note { get; set; } = "";

        public bool IsLocked { get; set; }

        public bool IsDummy { get; set; }

        public DragKey DragKey { get; set; } = new DragKey();

        public DragActionGroup Group { get; set; }

        public DragActionParameterSource? ParameterSource { get; set; }

        public DragActionParameter? Parameter
        {
            get => ParameterSource?.Get();
            set => ParameterSource?.Set(value);
        }


        // グループ判定
        public bool IsGroupCompatible(DragAction target)
        {
            return Group != DragActionGroup.None && Group == target.Group;
        }

        public virtual void Execute(DragTransformControl sender, DragTransformActionArgs e)
        {
        }

        public virtual void ExecuteEnd(DragTransformControl sender, DragTransformActionArgs e)
        {
        }

        #region Memento

        [Memento]
        public class Memento
        {
            public string MouseButton { get; set; } = "";

            public DragActionParameter? Parameter { get; set; }


            public Memento Clone()
            {
                return (Memento)MemberwiseClone();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.MouseButton = DragKey.ToString();
            memento.Parameter = (DragActionParameter?)Parameter?.Clone();
            return memento;
        }

        public void Restore(Memento element)
        {
            DragKey = new DragKey(element.MouseButton);
            Parameter = (DragActionParameter?)element.Parameter?.Clone();
        }

        #endregion
    }



    public enum DragActionGroup
    {
        None, // どのグループにも属さない
        Move,
    };



    public class DragTransformActionArgs
    {
        public DragTransformActionArgs(Point start, Point end)
        {
            Start = start;
            End = end;
        }

        public Point Start { get; }
        public Point End { get; }
    }

}
