﻿using System;
using System.Diagnostics;

namespace NeeView
{
    public class ScaleSliderCenteredDragAction : DragAction
    {
        public ScaleSliderCenteredDragAction()
        {
            Note = Properties.Resources.DragActionType_ScaleSliderCentered;
            ParameterSource = new DragActionParameterSource(typeof(SensitiveDragActionParameter));
            DragActionCategory = DragActionCategory.Scale;
        }

        public override DragActionControl CreateControl(DragTransformContext context)
        {
            return new ScaleSliderCenteredActionControl(context, this, ScaleType.TransformScale);
        }
    }

}
