using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DemotivatorBot
{
    internal class Demotivator
    {
        public string DemotivatorText { get; set; } = "";
        public string FileToDemotivatePath { get; set; } = "";
        public string FontStyle { get; set; } = "Times New Roman";
        public SolidColorBrush Background { get; set; } = Brushes.Black;
        public SolidColorBrush TextColor { get; set; } = Brushes.White;
        public string ResultPath { get; set; } = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\picsoutput\output.jpg";


        public Demotivator(string text, string file)
        {
            DemotivatorText = text;
            FileToDemotivatePath = file;
        }

        public void Demotivate()
        {
            var gap = 20;
            var fontSize = 70;

            var dpi = 96;

            var font =
                new Typeface(
                    new FontFamily(FontStyle), FontStyles.Normal,
                    FontWeights.Normal, FontStretches.SemiExpanded);
            

            var image = BitmapFrame.Create(new Uri("file://" + FileToDemotivatePath));
            var imageWidth = (double)image.PixelWidth;
            var imageHeight = (double)image.PixelHeight;

            var formattedText =
            new FormattedText(
                    DemotivatorText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    font, fontSize, TextColor, dpi)
                {
                    MaxTextWidth = imageWidth,
                    TextAlignment = TextAlignment.Center
                };

            var textWidth = formattedText.Width;
            var textHeight = formattedText.Height;

            var totalWidth = (int)Math.Ceiling(imageWidth + 2 * gap);
            var totalHeight = (int)Math.Ceiling(imageHeight + 3 * gap + textHeight);

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    Background, null,
                    new Rect(0, 0, totalWidth, totalHeight));

                drawingContext.DrawImage(
                    image,
                    new Rect(gap, gap, imageWidth, imageHeight));
                drawingContext.DrawText(
                    formattedText,
                    new Point(gap, imageHeight + 2 * gap));
            }

            var bmp =
                new RenderTargetBitmap(
                    totalWidth, totalHeight, dpi, dpi,
                    PixelFormats.Pbgra32);
            bmp.Render(drawingVisual);

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (var stream = System.IO.File.Create(ResultPath))
                encoder.Save(stream);

            return;
        } 
    }
}
