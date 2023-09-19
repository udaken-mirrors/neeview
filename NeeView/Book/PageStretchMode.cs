using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    // 画像のストレッチモード
    public enum PageStretchMode
    {
        [AliasName]
        None,

        [AliasName]
        Uniform,

        [AliasName]
        UniformToFill,

        [AliasName]
        UniformToSize,

        [AliasName]
        UniformToVertical,

        [AliasName]
        UniformToHorizontal,
    }
}
