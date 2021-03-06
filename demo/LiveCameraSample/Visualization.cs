﻿// 
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
using System.Windows.Media.Effects;

namespace LiveCameraSample
{

    public class PlayerScoreAndImage
    {
        public Guid PlayerId { get; set; }
        public int Score { get; set; }
        public CroppedBitmap Image { get; set; }
    }

    public class Visualization
    {
        private static Random rnd = new Random();

        private static SolidColorBrush s_lineBrush = new SolidColorBrush(new System.Windows.Media.Color { R = 255, G = 185, B = 0, A = 255 });
        private static SolidColorBrush s_lineBrush1 = new SolidColorBrush(new System.Windows.Media.Color { R = 26, G = 130, B = 196, A = 255 });
        private static SolidColorBrush s_lineBrush2 = new SolidColorBrush(new System.Windows.Media.Color { R = 255, G = 203, B = 57, A = 255});
        private static SolidColorBrush s_lineBrush3 = new SolidColorBrush(new System.Windows.Media.Color { R = 241, G = 91, B = 96, A = 255});
        private static SolidColorBrush s_lineBrush4 = new SolidColorBrush(new System.Windows.Media.Color { R = 40, G = 40, B = 50, A = 255});
        private static SolidColorBrush s_lineBrush5 = new SolidColorBrush(new System.Windows.Media.Color { R = 229, G = 47, B = 137, A = 255 });
        private static SolidColorBrush[] brushes = new SolidColorBrush[5] { s_lineBrush1, s_lineBrush2, s_lineBrush3, s_lineBrush4, s_lineBrush5 }; 
        private static Dictionary<Guid, SolidColorBrush> colorsForPlayers = new Dictionary<Guid, SolidColorBrush>();

        private static Typeface s_typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        private static int latestBrushIndex = 0;

        private static SolidColorBrush GetLatestBrush()
        {
            if (latestBrushIndex == 5)
            {
                latestBrushIndex = 0;
            }

            return brushes[latestBrushIndex++];
        }

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
                baseImage.DpiX, baseImage.DpiY, PixelFormats.Default);

            
            outputBitmap.Render(visual);

            lastBitmap = outputBitmap;
            return outputBitmap;
        }

        public static BitmapSource DrawParticipants(BitmapSource baseImage, Microsoft.ProjectOxford.Face.Contract.Face[] faces)
        {
            var fullRect = new Rect(0, 0, baseImage.PixelWidth, baseImage.PixelHeight);
            var rect1 = new Rect(0, 0, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect2 = new Rect(0, baseImage.PixelHeight/ 2, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect3 = new Rect(baseImage.PixelWidth / 2, 0, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rect4 = new Rect(baseImage.PixelWidth / 2, baseImage.PixelHeight / 2, baseImage.PixelWidth / 2, baseImage.PixelHeight / 2);
            var rects = new Rect[] { rect1, rect2, rect3, rect4 };
            var ratio = ((1.0) * baseImage.PixelHeight) / ((1.0) * baseImage.PixelWidth); 
            if (faces == null)
            {
                return baseImage;
            }

            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {

                if (faces.Length == 0)
                {
                    drawingContext.DrawImage(ImageProvider.WaitingForPlayers, fullRect);
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
        public static ImageSource DrawTime(string showTime, bool drawIndicator, IRound round)
        {

            if (lastBitmap == null)
            {
                return lastBitmap;
            }
            DrawingVisual visual = new DrawingVisual();

           
           
            DrawingContext drawingContext = visual.RenderOpen();

            FormattedText ft = new FormattedText(showTime,
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                50, Brushes.White);

            drawingContext.DrawImage(lastBitmap, new Rect(0, 0, lastBitmap.Width, lastBitmap.Height));

            drawingContext.DrawText(ft, new Point(550, 0));

            if (drawIndicator)
            {
                drawingContext.DrawImage(round.GetRoundIndicator(), new Rect(20, 20, 100, 100));

                FormattedText targetText = new FormattedText(round.GetRoundImageText(),
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface,
                    20, Brushes.White);

                drawingContext.DrawText(targetText, new Point(55, 75));
            }
            drawingContext.Close();

            RenderTargetBitmap outputBitmap = new RenderTargetBitmap(
                (int)lastBitmap.Width, (int)lastBitmap.Height,
                0, 0, PixelFormats.Pbgra32);

            outputBitmap.Render(visual);

            return outputBitmap;
        }


        public static BitmapSource DrawRoundStart(BitmapSource baseImage, IRound round, int roundNum)
        {
            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                var image = round.GetRoundTemplateImage();
                var faceRect = new Rect(0, 0, baseImage.Width, baseImage.Height);
                //faceRect.Inflate(6 * annotationScale, 6 * annotationScale);

                drawingContext.DrawImage(image, faceRect);

                FormattedText targetText = new FormattedText(round.GetRoundImageText(),
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 30, Brushes.White);

                var targetPoint = new System.Windows.Point(70,160);
                drawingContext.DrawText(targetText, targetPoint);

                FormattedText roundNmberText = new FormattedText(roundNum.ToString(),
                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 40, Brushes.White);

                var roundNumberPoint = new System.Windows.Point(200, 16);
                drawingContext.DrawText(roundNmberText, roundNumberPoint);

            };

            return DrawOverlay(baseImage, drawAction);
        }


        public static BitmapSource DrawRoundEnd(BitmapSource baseImage,
            Dictionary<Guid, int> playerScore, 
            Dictionary<Guid, List<CroppedBitmap>> playerImages = null, 
            Dictionary<Guid, int> playerFinalScore = null, 
            List<BitmapSource> groupImages = null)
        {
            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {
                var image = ImageProvider.EndRound;
                var faceRect = new Rect(0, 0, baseImage.Width, baseImage.Height);
                drawingContext.DrawImage(image, faceRect);

                /*
                var playerRoundScore = playerScore.ToList();

                if (playerRoundScore != null && playerImages != null)
                {
                    int i = 0;
                    var enumerator = playerRoundScore.GetEnumerator();

                    enumerator.MoveNext();
                    var player = enumerator.Current;
                    Rect rect = new Rect(20, 160, 150, 150);
                    drawingContext.DrawImage(playerImages[player.Key][0], rect);
                    FormattedText scoreText = new FormattedText(player.Value.ToString() + "Pts",
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 60, Brushes.White);

                    var scorePoint = new System.Windows.Point(180, 250);
                    drawingContext.DrawText(scoreText, scorePoint);
                    while(enumerator.MoveNext())
                    {
                        player = enumerator.Current;
                        if (playerImages.ContainsKey(player.Key))
                        {
                            rect = new Rect(20 + 180 * i, 320, 70, 70);
                            drawingContext.DrawImage(playerImages[player.Key][0], rect);
                            scoreText = new FormattedText(player.Value.ToString() + "Pts",
                                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 30, Brushes.White);

                            scorePoint = new System.Windows.Point(100 + 180 * i, 360);
                            drawingContext.DrawText(scoreText, scorePoint);

                            if(playerFinalScore != null && playerFinalScore.ContainsKey(player.Key))
                            {
                                scoreText = new FormattedText("Total: " + playerFinalScore[player.Key].ToString(),
                                    CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 30, Brushes.Red);

                                scorePoint = new System.Windows.Point(18 + 300 * (i % 2), 282 + 160 * (i / 2));
                                drawingContext.DrawText(scoreText, scorePoint);
                            }
                            i++;
                        }
                    }
                }
                */

                Guid winnerGuid = playerScore.FirstOrDefault().Key;

                var winnerValue = playerScore[winnerGuid];
                foreach (var item in playerScore)
                {
                    if (playerScore[item.Key] > winnerValue)
                    {
                        winnerGuid = item.Key;
                        winnerValue = playerScore[item.Key];
                    }
                }
                var winnerImages = playerImages[winnerGuid];
                var r = rnd.Next(winnerImages.Count);
                var winnerImage = winnerImages[r];

                Dictionary<Guid, List<CroppedBitmap>> losersImages = new Dictionary<Guid, List<CroppedBitmap>>();
                foreach (var item in playerImages)
                {
                    if (item.Key != winnerGuid)
                    {
                        losersImages[item.Key] = item.Value;
                    }
                }

                List<PlayerScoreAndImage> losersList = new List<PlayerScoreAndImage>();
                foreach (var item in losersImages)
                {
                    var images = item.Value;
                    r = rnd.Next(images.Count);
                    var loserImage = images[r];
                    losersList.Add(new PlayerScoreAndImage()
                    {
                        PlayerId = item.Key,
                        Score = playerScore[item.Key],
                        Image = loserImage
                    });
                }

                drawingContext.DrawImage(winnerImage, new Rect(240, 110, 160, 160));
                FormattedText scoreText = new FormattedText(winnerValue.ToString(),
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.White);
                drawingContext.DrawText(scoreText, new Point(300,275));

                int i = 0;
                foreach (var item in losersList)
                {
                    drawingContext.DrawImage(item.Image, new Rect(40+230*i, 340, 100, 100));
                    scoreText = new FormattedText(item.Score.ToString(),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.White);
                        drawingContext.DrawText(scoreText, new Point(70+230*i, 440));
                    i++;
                }
            };

            return DrawOverlay(baseImage, drawAction);
        }

        public static BitmapSource DrawGameEnd(BitmapSource baseImage, 
            Dictionary<Guid, int> playerScore, 
            Dictionary<Guid, List<CroppedBitmap>> playerImages = null,
            List<BitmapSource> groupImages = null)
        {
            Action<DrawingContext, double> drawAction = (drawingContext, annotationScale) =>
            {

                var image = ImageProvider.GameOver;
                var faceRect = new Rect(0, 0, baseImage.Width, baseImage.Height);
                drawingContext.DrawImage(image, faceRect);

                //FormattedText titleText = new FormattedText("End Game!",
                //CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.Purple);
                //var titlePoint = new System.Windows.Point(20, 20);

                //var contentPoint = new System.Windows.Point(20, 60);

                //drawingContext.DrawText(titleText, titlePoint);

                Guid winnerGuid = playerScore.FirstOrDefault().Key;

                var winnerValue = playerScore[winnerGuid];
                foreach (var item in playerScore)
                {
                    if (playerScore[item.Key] > winnerValue)
                    {
                        winnerGuid = item.Key;
                        winnerValue = playerScore[item.Key];
                    }
                }
                var winnerImages = playerImages[winnerGuid];
                var r = rnd.Next(winnerImages.Count);
                var winnerImage = winnerImages[r];

                Dictionary<Guid, List<CroppedBitmap>> losersImages = new Dictionary<Guid, List<CroppedBitmap>>();
                foreach (var item in playerImages)
                {
                    if (item.Key != winnerGuid)
                    {
                        losersImages[item.Key] = item.Value;
                    }
                }

                List<PlayerScoreAndImage> losersList = new List<PlayerScoreAndImage>();
                foreach (var item in losersImages)
                {
                    var images = item.Value;
                    r = rnd.Next(images.Count);
                    var loserImage = images[r];
                    losersList.Add(new PlayerScoreAndImage()
                    {
                        PlayerId = item.Key,
                        Score = playerScore[item.Key],
                        Image = loserImage
                    });
                }

                drawingContext.DrawImage(winnerImage, new Rect(240, 110, 160, 160));
                FormattedText scoreText = new FormattedText(winnerValue.ToString(),
                CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.White);
                drawingContext.DrawText(scoreText, new Point(300, 275));

                int i = 0;
                foreach (var item in losersList)
                {
                    drawingContext.DrawImage(item.Image, new Rect(40 + 230 * i, 340, 100, 100));
                    scoreText = new FormattedText(item.Score.ToString(),
                        CultureInfo.CurrentCulture, FlowDirection.LeftToRight, s_typeface, 25, Brushes.White);
                    drawingContext.DrawText(scoreText, new Point(70 + 230 * i, 440));
                    i++;
                }
            };

            return DrawOverlay(baseImage, drawAction);
        }




        public static BitmapSource DrawFaces(BitmapSource baseImage, IRound round, Dictionary<Guid, Microsoft.ProjectOxford.Face.Contract.Face> identities, ScoringSystem scoring, MainWindow.AppMode mode)
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

                     SolidColorBrush brush;
                                        if (!colorsForPlayers.TryGetValue(personId, out brush))
                                        {
                                            colorsForPlayers[personId] = GetLatestBrush();
                                        }
                                        brush = colorsForPlayers[personId];
                    
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
                           new Pen(brush, lineThickness),
                           faceRect);


                    //drawingContext.DrawImage(ImageProvider.Frame, faceRect);

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

                        drawingContext.DrawRectangle(brush, null, rect);
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
