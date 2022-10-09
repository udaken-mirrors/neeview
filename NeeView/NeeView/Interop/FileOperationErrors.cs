namespace NeeView.Interop
{
    public enum FileOperationErrors : int
    {
        DE_SAMEFILE = 0x71,
        DE_MANYSRC1DEST = 0x72,
        DE_DIFFDIR = 0x73,
        DE_ROOTDIR = 0x74,
        DE_OPCANCELLED = 0x75,
        DE_DESTSUBTREE = 0x76,
        DE_ACCESSDENIEDSRC = 0x78,
        DE_PATHTOODEEP = 0x79,
        DE_MANYDEST = 0x7A,
        DE_INVALIDFILES = 0x7C,
        DE_DESTSAMETREE = 0x7D,
        DE_FLDDESTISFILE = 0x7E,
        DE_FILEDESTISFLD = 0x80,
        DE_FILENAMETOOLONG = 0x81,
        DE_DEST_IS_CDROM = 0x82,
        DE_DEST_IS_DVD = 0x83,
        DE_DEST_IS_CDRECORD = 0x84,
        DE_FILE_TOO_LARGE = 0x85,
        DE_SRC_IS_CDROM = 0x86,
        DE_SRC_IS_DVD = 0x87,
        DE_SRC_IS_CDRECORD = 0x88,
        DE_ERROR_MAX = 0xB7,
        DE_ERROR_UNKNOWN = 0x402,
        ERRORONDEST = 0x10000,
        DE_DESTROOTDIR = 0x10074,

        ERROR_CANCELLED = 0x04C7,
    }
}
