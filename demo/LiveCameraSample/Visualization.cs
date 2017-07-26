// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.ProjectOxford.Common.Contract;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Vision.Contract;
using Point = System.Windows.Point;
using GameSystem;

namespace LiveCameraSample
{
    public class Visualization
    {
        private static SolidColorBrush s_lineBrush = new SolidColorBrush(new System.Windows.Media.Color { R = 255, G = 185, B = 0, A = 255 });
        private static Typeface s_typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        
        private static BitmapSource DrawOverlay(BitmapSource baseImage, Action<DrawingContext, double> drawAction, bool drawVideo = false)
        {
            double annotationScale = baseImage.PixelHeight / 320;

            DrawingVisual visual = new DrawingVisual();
            DrawingContext drawingContext = visual.RenderOpen();

            if (drawVideo)
            {
                drawingContext.DrawImage(baseImage, new Rect(0, 0, baseImage.Width, baseImage.Height));
            }

            drawAction(drawingContext, annotationScale);

            drawingContext.Close();

            RenderTargetBitmap outputBitmap = new RenderTargetBitmap(
                baseImage.PixelWidth, baseImage.PixelHeight,
                baseImage.DpiX, baseImage.DpiY, PixelFormats.Pbgra32);

            outputBitmap.Render(visual);

            lastBitmap = outputBitmap;
            return outputBitmap;
        }

        public static BitmapSource DrawParticipants(BitmapSource baseImage, Microsoft.ProjectOxford.Face.Contract.Face[] faces)
        {

            var rect1 = new Rect(0, 0, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect2 = new Rect(0, baseImage.PixelHeight/ 2, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect3 = new Rect(baseImage.PixelWidth / 2, 0, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect4 = new Rect(baseImage.PixelWidth / 2, baseImage.PixelHeight / 2, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rects = new Rect[] { rect1, rect2, rect3, rect4 };
            var ratio = ((1.0) * baseImage.PixelHeight) / ((1.0) * baseImage.PixelWidth); 
            if (faces == null)
            {
                /*
                */
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {

                if (faces.Length == 0)
                {
                    FormattedText ft = new FormattedText("Please stand\nin front of\nthe camera",
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                        42, Brushes.Black);
                    // Instead of calling DrawText (which can only draw the text in a solid colour), we
                    // convert to geometry and use DrawGeometry, which allows us to add an outline. 
                    drawingContext.DrawText(ft, new Point(130,30));
                    
                }
                for (int i = 0; i < faces.Length; i++)
                {
                    var face = faces[i];
                    if (face.FaceRectangle == null) { continue; }

                    Rect faceRect = new Rect(
                        face.FaceRectangle.Left, face.FaceRectangle.Top,
                        face.FaceRectangle.Width, face.FaceRectangle.Height);
                    
                    if (i < 4)
                    {
                        var newFaceY = faceRect.Y - (faceRect.Height * 0.3);
                        var newFaceHeight = faceRect.Height * 1.6;
                        var newFaceX = faceRect.X - (faceRect.Width * 0.52);
                        var newFaceWidth = faceRect.Width* 2.13;

                        int rectWidth = (int)(baseImage.Width / 2);
                        int rectHeight = (int)(baseImage.Height/ 2);
                        int xOffset = (((int)rectWidth - (int)faceRect.Width) / 2);
                        int yOffset = (((int)rectHeight - (int)faceRect.Height) / 2);

                        if (newFaceX < 0)
                        {
                            newFaceX = 0;
                        }
                        if (newFaceY < 0)
                        {
                            newFaceY = 0;
                        }
                        if (newFaceX + newFaceWidth > baseImage.Width)
                        {
                            newFaceX = baseImage.Width - newFaceWidth;
                        }
                        if (newFaceY + newFaceHeight > baseImage.Height)
                        {
                            newFaceY = baseImage.Height - newFaceHeight;
                        }
                        if (newFaceX < 0)
                        {
                            newFaceX = 0;
                        }
                        if (newFaceY < 0)
                        {
                            newFaceY = 0;
                        }
                        Int32Rect r = new Int32Rect((int)newFaceX, (int)newFaceY, (int)newFaceWidth, (int)newFaceHeight);

                        BitmapSource topHalf = new CroppedBitmap(baseImage, r);
                        drawingContext.DrawImage(topHalf, rects[i]);
                    }
                }
            };
            
            return DrawOverlay(baseImage, drawAction);
        }

        private static ImageSource lastBitmap;
        public static ImageSource DrawTime()
        {

            if (lastBitmap== null)
            {
                return lastBitmap;
            }
            DrawingVisual visual = new DrawingVisual();
            DrawingContext drawingContext = visual.RenderOpen();


            string dt = DateTime.Now.ToString("M/d/yyyy hh:mm:ss");
            FormattedText ft = new FormattedText(dt,
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                20, Brushes.Yellow);

            drawingContext.DrawImage(lastBitmap, new Rect(0, 0, lastBitmap.Width, lastBitmap.Height));

            drawingContext.DrawText(ft, new Point(0, 0));

            drawingContext.Close();

            RenderTargetBitmap outputBitmap = new RenderTargetBitmap(
                (int)lastBitmap.Width, (int)lastBitmap.Height,
                0, 0, PixelFormats.Pbgra32);

            outputBitmap.Render(visual);

            return outputBitmap;
        }


        public static BitmapSource DrawTags(BitmapSource baseImage, Tag[] tags)
        {
            if (tags == null)
            {
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                double y = 0;
                foreach (var tag in tags)
                {
                    // Create formatted text--in a particular font at a particular size
                    FormattedText ft = new FormattedText(tag.Name,
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                        42 * annotationScale, Brushes.Black);
                    // Instead of calling DrawText (which can only draw the text in a solid colour), we
                    // convert to geometry and use DrawGeometry, which allows us to add an outline. 
                    var geom = ft.BuildGeometry(new System.Windows.Point(10 * annotationScale, y));
                    drawingContext.DrawGeometry(s_lineBrush, new Pen(Brushes.Black, 2 * annotationScale), geom);
                    // Move line down
                    y += 42 * annotationScale;
                }
            };

            return DrawOverlay(baseImage, drawAction, true);
        }

        public static BitmapSource DrawScores(BitmapSource baseImage, Dictionary<Guid, int> scores)
        {
            if (scores == null)
            {
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                double y = 0;
                foreach (var score in scores.Values) //TODO
                {
                    // Create formatted text--in a particular font at a particular size
                    FormattedText ft = new FormattedText(score + "pts",
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                        42 * annotationScale, Brushes.Black);
                    // Instead of calling DrawText (which can only draw the text in a solid colour), we
                    // convert to geometry and use DrawGeometry, which allows us to add an outline. 
                    var geom = ft.BuildGeometry(new System.Windows.Point(10 * annotationScale, y));
                    drawingContext.DrawGeometry(s_lineBrush, new Pen(Brushes.Black, 2 * annotationScale), geom);
                    // Move line down
                    y += 42 * annotationScale;
                }
            };

            return DrawOverlay(baseImage, drawAction, true);
        }

        public static BitmapSource DrawRound(BitmapSource baseImage, string title, string content, Dictionary<Guid, CroppedBitmap> playerImages = null)
        {
            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                FormattedText titleText = new FormattedText(title,
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.Purple);
                var titlePoint = new System.Windows.Point(20, 20);

                FormattedText contentText = new FormattedText(content,
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 14, Brushes.Black);

                var contentPoint = new System.Windows.Point(20, 60);

                drawingContext.DrawText(titleText, titlePoint);
                drawingContext.DrawText(contentText, contentPoint);
            };

            return DrawOverlay(baseImage, drawAction);
        }

        public static BitmapSource DrawFaces(BitmapSource baseImage, Dictionary<Guid, Microsoft.ProjectOxford.Face.Contract.Face> identities, ScoringSystem scoring, MainWindow.AppMode mode)
        {
            if (identities == null)
            {
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                foreach (var personId in identities.Keys)
                {
                    var face = identities[personId];

                    if (face.FaceRectangle == null) { continue; }

                    Rect faceRect = new Rect(
                        face.FaceRectangle.Left, face.FaceRectangle.Top,
                        face.FaceRectangle.Width, face.FaceRectangle.Height);
                    string text = "";
                   
                    if (face.FaceAttributes != null && mode == MainWindow.AppMode.Faces)
                    {
                        text += Aggregation.SummarizeFaceAttributes(face.FaceAttributes);
                    }

                    if (face.FaceAttributes.Emotion != null && mode == MainWindow.AppMode.Emotions)
                    {
                        text += Aggregation.SummarizeEmotion(face.FaceAttributes.Emotion);
                    }

                    if (scoring.CurrentRoundScore.ContainsKey(personId))
                    {
                        text += string.Format("  +{0}pts", scoring.CurrentRoundScore[personId]);
                    }

                    faceRect.Inflate(6 * annotationScale, 6 * annotationScale);

                    double lineThickness = 4 * annotationScale;

                    drawingContext.DrawRectangle(
                        Brushes.Transparent,
                        new Pen(s_lineBrush, lineThickness),
                        faceRect);

                    if (text != "")
                    {
                        FormattedText ft = new FormattedText(text,
                            CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                            16 * annotationScale, Brushes.Black);

                        var pad = 3 * annotationScale;

                        var ypad = pad;
                        var xpad = pad + 4 * annotationScale;
                        var origin = new System.Windows.Point(
                            faceRect.Left + xpad - lineThickness / 2,
                            faceRect.Top - ft.Height - ypad + lineThickness / 2);
                        var rect = ft.BuildHighlightGeometry(origin).GetRenderBounds(null);
                        rect.Inflate(xpad, ypad);

                        drawingContext.DrawRectangle(s_lineBrush, null, rect);
                        drawingContext.DrawText(ft, origin);
                    }
                }
            };

            return DrawOverlay(baseImage, drawAction, true);
        }

        public static BitmapSource DrawSomething(BitmapSource baseImage, string text, Point location)
        {
            if (string.IsNullOrEmpty(text))
            {
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                FormattedText ft = new FormattedText(text,
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                    16 * annotationScale, Brushes.Black);

                var pad = 3 * annotationScale;

                var ypad = pad;
                var xpad = pad + 4 * annotationScale;
                var rect = ft.BuildHighlightGeometry(location).GetRenderBounds(null);
                rect.Inflate(xpad, ypad);

                drawingContext.DrawRectangle(s_lineBrush, null, rect);
                drawingContext.DrawText(ft, location);
            };

            return DrawOverlay(baseImage, drawAction, true);
        }

    }
}
