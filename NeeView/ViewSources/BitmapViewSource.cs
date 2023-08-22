using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NeeView.ComponentModel;
using NeeView.Media.Imaging;

namespace NeeView
{
    public class BitmapViewSource : PictureViewSource
    {
        public BitmapViewSource(BitmapPageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, new BitmapPictureSource(pageContent), bookMemoryService)
        {
        }
    }
}