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
        private string demotivatorText = "";
        private string fileToDemotivatePath = "";
        private string resultPath = @$"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\picsoutput\output.jpg";


        public string DemotivatorText { get => DemotivatorText; set => DemotivatorText = value; }
        public string FileToDemotivatePath { get => fileToDemotivatePath; set => fileToDemotivatePath = value; }
        public string ResultPath { get => resultPath; set => resultPath = value; }

        public Demotivator(string text, string file)
        {
            demotivatorText = text;
            fileToDemotivatePath = file;
        }

        public void Demotivate() 
        {
            var background = Brushes.Black;
            var textColor = Brushes.White;

            var gap = 20;
            var fontSize = 70;

            var dpi = 96;

            var font =
                new Typeface(
                    new FontFamily("Times New Roman"), FontStyles.Normal,
                    FontWeights.Normal, FontStretches.SemiExpanded);
            // <--

            var image = BitmapFrame.Create(new Uri("file://" + fileToDemotivatePath));
            var imageWidth = (double)image.PixelWidth;
            var imageHeight = (double)image.PixelHeight;

            var formattedText =
            new FormattedText(
                    demotivatorText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    font, fontSize, textColor, dpi)
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
                    background, null,
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
