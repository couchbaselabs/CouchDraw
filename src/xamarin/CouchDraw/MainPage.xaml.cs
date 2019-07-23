using System.Collections.Generic;
using Xamarin.Forms;
using TouchTracking;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using CouchDraw.Core.ViewModels;
using System.ComponentModel;
using System;

namespace CouchDraw
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        MainViewModel ViewModel { get; set; } 

        public MainPage()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MainViewModel(UpdateCanvas);
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            var point = new Models.Point
            {
                X = args.Location.X,
                Y = args.Location.Y
            };

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    ViewModel.CreatePath(point);
                    canvasView.InvalidateSurface();
                    break;
                case TouchActionType.Moved:
                    ViewModel.AddPoint(point);
                    canvasView.InvalidateSurface();
                    break;
            }
        }

        List<SKPath> GetSKPaths(List<Models.Path> paths)
        {
            var skPaths = new List<SKPath>();

            foreach (var path in paths)
            {
                var point = path.Points[0];

                if (point != null)
                {
                    var skPath = new ColorSKPath(path.Color);

                    // The path needs to be started from the first touch point
                    skPath.MoveTo(GetToSKPoint(new Point(point.X, point.Y)));

                    for (int i = 1; i < path.Points.Count; i++)
                    {
                        point = path.Points[i];

                        // Connect the last point in the path to the new point
                        skPath.LineTo(GetToSKPoint(new Point(point.X, point.Y)));
                    }

                    skPaths.Add(skPath);
                }
            }

            return skPaths;
        }

        SKPoint GetToSKPoint(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            foreach (ColorSKPath path in GetSKPaths(ViewModel.Paths))
            {
                paint.Color = path.Color;
                canvas.DrawPath(path, paint);
            }

            foreach (ColorSKPath path in GetSKPaths(ViewModel.ExternalPaths))
            {
                paint.Color = path.Color;
                canvas.DrawPath(path, paint);
            }
        }

        void UpdateCanvas() => Device.BeginInvokeOnMainThread(canvasView.InvalidateSurface);
    }

    public class ColorSKPath : SKPath
    {
        public SKColor Color { get; set; }

        public ColorSKPath(string hexColor)
        {
            Color = SKColor.Parse(hexColor);
        }
    }
}
