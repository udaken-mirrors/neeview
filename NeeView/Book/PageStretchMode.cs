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
    
    #region Obsolete

    // 旧・画像のストレッチモード
    [Obsolete("use PageStretchMode")]
    public enum PageStretchModeV1
    {
        None,
        Inside,
        Outside,
        Uniform,
        UniformToFill,
        UniformToSize,
        UniformToVertical,
        UniformToHorizontal,
    }

    [Obsolete("no used")]
    public static class PageStretchModeV1Extension
    {
        public static PageStretchMode ToPageStretchMode(this PageStretchModeV1 self)
        {
            return self switch
            {
                PageStretchModeV1.Uniform => PageStretchMode.Uniform,
                PageStretchModeV1.UniformToFill => PageStretchMode.UniformToFill,
                PageStretchModeV1.UniformToSize => PageStretchMode.UniformToSize,
                PageStretchModeV1.UniformToVertical => PageStretchMode.UniformToVertical,
                PageStretchModeV1.UniformToHorizontal => PageStretchMode.UniformToHorizontal,
                _ => PageStretchMode.None,
            };
        }
    }

    #endregion Obsolete
}
