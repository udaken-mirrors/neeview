using System;

namespace NeeView.Interop
{
    [Flags]
    public enum ShopObjectTypes : uint
    {
        SHOP_PRINTERNAME = 0x1,
        SHOP_FILEPATH = 0x2,
        SHOP_VOLUMEGUID = 0x4,
    }
}
