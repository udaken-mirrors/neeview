using NeeView.Effects;
using NeeLaboratory.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace NeeView
{
    public class ExportImageSource
    {
        public ExportImageSource(string? bookAddress, List<Page> pages, FrameworkElement view, Brush? background, Brush? backgroundFront, Transform viewTransform, Effect? viewEffect)
        {
            BookAddress = bookAddress;
            Pages = pages;
            View = view;
            Background = background;
            BackgroundFront = backgroundFront;
            ViewTransform = viewTransform;
            ViewEffect = viewEffect;
        }

        public string? BookAddress { get; private set; }

        public List<Page> Pages { get; private set; }

        public FrameworkElement View { get; private set; }

        public Brush? Background { get; private set; }

        public Brush? BackgroundFront { get; private set; }


        public Transform ViewTransform { get; private set; }

        public Effect? ViewEffect { get; private set; }


        public static ExportImageSource Create()
        {
            var viewComponent = MainViewComponent.Current;

            var element = viewComponent.MainView.PageContents;

            var rotateTransform = new RotateTransform(viewComponent.DragTransform.Angle);
            var scaleTransform = new ScaleTransform(viewComponent.DragTransform.ScaleX, viewComponent.DragTransform.ScaleY);
            var transform = new TransformGroup();
            transform.Children.Add(scaleTransform);
            transform.Children.Add(rotateTransform);

            var context = new ExportImageSource(
                bookAddress: BookOperation.Current.Address,
                pages: viewComponent.ContentCanvas.CloneContents.Select(e => e.Page).WhereNotNull().ToList(),
                view: element,
                viewTransform: transform,
                viewEffect: ImageEffect.Current.Effect,
                background: viewComponent.ContentCanvasBrush.CreateBackgroundBrush(),
                backgroundFront: viewComponent.ContentCanvasBrush.CreateBackgroundFrontBrush(new DpiScale(1, 1))
            );

            return context;
        }

    }
}
