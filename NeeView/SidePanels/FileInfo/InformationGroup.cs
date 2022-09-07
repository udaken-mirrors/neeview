using System;

namespace NeeView
{
    public enum InformationGroup
    {
        File,
        Image,
        Description,
        Origin,
        Camera,
        AdvancedPhoto,
        Gps,
    }

    public enum InformationCategory
    {
        File,
        Image,
        Metadata,
    }

    public static class InformationGroupExtensions
    {
        public static InformationCategory ToInformationCategory(this InformationGroup self)
        {
            return self switch
            {
                InformationGroup.File
                    => InformationCategory.File,
                InformationGroup.Image
                    => InformationCategory.Image,
                InformationGroup.Description or InformationGroup.Origin or InformationGroup.Camera or InformationGroup.AdvancedPhoto or InformationGroup.Gps
                    => InformationCategory.Metadata,
                _
                    => throw new NotSupportedException(),
            };
        }
    }
}
