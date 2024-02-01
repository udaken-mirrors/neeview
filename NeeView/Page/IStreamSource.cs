using System;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// Stream 生成可能な要素
    /// </summary>
    public interface IStreamSource
    {
        // TODO: PageContent保持メモリサイズ用。本来の用途ではないのでどうにかする
        long Length => throw new NotSupportedException();

        Stream OpenStream();

        // TODO: PageContent保持メモリサイズ用。本来の用途ではないのでどうにかする
        long GetMemorySize(); //=> 0L;
    }


    public interface IHasStreamSource
    {
        IStreamSource StreamSource { get; }
    }

}
