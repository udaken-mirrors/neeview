using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    public class PictureProfile : BindableBase
    {
        static PictureProfile() => Current = new PictureProfile();
        public static PictureProfile Current { get; }


        private PictureProfile()
        {
        }


        [PropertyMember]
        public FileTypeCollection SupportFileTypes
        {
            get
            {
                if (Config.Current.Image.Standard.SupportFileTypes is null)
                {
                    // NOTE: fall through. don't come here!
                    Config.Current.Image.Standard.SupportFileTypes = PictureFileExtensionTools.CreateDefaultSupportedFileTypes(Config.Current.Image.Standard.UseWicInformation);
                }
                return Config.Current.Image.Standard.SupportFileTypes;
            }
            set { Config.Current.Image.Standard.SupportFileTypes = value; }
        }


        // 対応拡張子判定 (ALL)
        public bool IsSupported(string fileName, bool includeMedia)
        {
            string ext = LoosePath.GetExtension(fileName);

            if (SupportFileTypes.Contains(ext)) return true;

            if (Config.Current.Susie.IsEnabled)
            {
                if (SusiePluginManager.Current.ImageExtensions.Contains(ext)) return true;
            }

            if (Config.Current.Image.Svg.IsEnabled)
            {
                if (Config.Current.Image.Svg.SupportFileTypes.Contains(ext)) return true;
            }

            if (Config.Current.Image.IsMediaEnabled && includeMedia)
            {
                if (Config.Current.Archive.Media.SupportFileTypes.Contains(ext)) return true;
            }

            return false;
        }

        // 対応拡張子判定 (標準)
        public bool IsDefaultSupported(string fileName)
        {
            string ext = LoosePath.GetExtension(fileName);
            return SupportFileTypes.Contains(ext);
        }

        // 対応拡張子判定 (Susie)
        public bool IsSusieSupported(string fileName)
        {
            if (!Config.Current.Susie.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return SusiePluginManager.Current.ImageExtensions.Contains(ext);
        }

        // 対応拡張子判定 (Svg)
        public bool IsSvgSupported(string fileName)
        {
            if (!Config.Current.Image.Svg.IsEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return Config.Current.Image.Svg.SupportFileTypes.Contains(ext);
        }

        // 対応拡張子判定 (Media)
        public bool IsMediaSupported(string fileName)
        {
            if (!Config.Current.Image.IsMediaEnabled) return false;

            string ext = LoosePath.GetExtension(fileName);
            return Config.Current.Archive.Media.SupportFileTypes.Contains(ext);
        }

        // 最大サイズ内におさまるサイズを返す
        public static Size CreateFixedSize(Size size)
        {
            if (size.IsEmpty) return size;

            return size.Limit(Config.Current.Performance.MaximumSize);
        }

    }
}
