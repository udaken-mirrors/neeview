using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    // タッチアクション
    public enum TouchAction
    {
        [AliasName]
        None,

        [AliasName]
        Drag,

        [AliasName]
        MouseDrag,

        [AliasName]
        Gesture,

        [AliasName]
        Loupe,
    }


    public enum TouchActionClass
    {
        Drag,
        Hold,
    }

    public static class TouchActionClassExtension
    {
        private static readonly Dictionary<TouchAction, string> _mapHold;
        private static readonly Dictionary<TouchAction, string> _mapDrag;

        static TouchActionClassExtension()
        {
            _mapHold = AliasNameExtensions.GetAliasNameDictionary<TouchAction>();

            _mapDrag = _mapHold
                .Where(e => e.Key != TouchAction.Loupe)
                .ToDictionary(e => e.Key, e => e.Value);
        }

        public static Dictionary<TouchAction, string> GetAliasNameMap(this TouchActionClass self)
        {
            return self switch
            {
                TouchActionClass.Drag => _mapDrag,
                _ => _mapHold,
            };
        }
    }
}
