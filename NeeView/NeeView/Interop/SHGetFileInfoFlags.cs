using System;

namespace NeeView.Interop
{
    [Flags]
    public enum SHGetFileInfoFlags : uint
    {
        // get icon
        SHGFI_ICON = 0x000000100,

        // get display name
        SHGFI_DISPLAYNAME = 0x000000200,

        // get type name
        SHGFI_TYPENAME = 0x000000400,

        // get attributes
        SHGFI_ATTRIBUTES = 0x000000800,

        // get icon location
        SHGFI_ICONLOCATION = 0x000001000,

        // return exe type
        SHGFI_EXETYPE = 0x000002000,

        // get system icon index
        SHGFI_SYSICONINDEX = 0x000004000,

        // put a link overlay on icon
        SHGFI_LINKOVERLAY = 0x000008000,

        // show icon in selected state
        SHGFI_SELECTED = 0x000010000,

        // get only specified attributes
        SHGFI_ATTR_SPECIFIED = 0x000020000,

        // get large icon
        SHGFI_LARGEICON = 0x000000000,

        // get small icon
        SHGFI_SMALLICON = 0x000000001,

        // get open icon
        SHGFI_OPENICON = 0x000000002,

        // get shell size icon
        SHGFI_SHELLICONSIZE = 0x000000004,

        // pszPath is a pidl
        SHGFI_PIDL = 0x000000008,

        // use passed dwFileAttribute
        SHGFI_USEFILEATTRIBUTES = 0x000000010,

        // apply the appropriate overlays
        SHGFI_ADDOVERLAYS = 0x000000020,

        // Get the index of the overlay in
        SHGFI_OVERLAYINDEX = 0x000000040,

        // the upper 8 bits of the iIcon
    }
}
