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

using Microsoft.ProjectOxford.Emotion;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Vision;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using VideoFrameAnalyzer;
using GameSystem;
using System.Windows.Media;
using Point = System.Windows.Point;
using System.IO;
using System.Timers;
using System.Windows.Threading;

namespace LiveCameraSample
{

    public enum GameState
    {
        Participants,
        Explain,
        RoundBegin,
        Game,
        RoundEnd,
        GameEnd
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private static Random rnd = new Random();

        private EmotionServiceClient _emotionClient = null;
        private FaceServiceClient _faceClient = null;
        private VisionServiceClient _visionClient = null;
        private readonly FrameGrabber<LiveCameraResult> _grabber = null;
        private static readonly ImageEncodingParam[] s_jpegParams = {
            new ImageEncodingParam(ImwriteFlags.JpegQuality, 60)
        };
        private readonly CascadeClassifier _localFaceDetector = new CascadeClassifier();
        private bool _fuseClientRemoteResults;
        private LiveCameraResult _latestResultsToDisplay = null;
        private AppMode _mode;
        private const int NumOfRounds = 1;
        private IRound round = null;
        private int roundNumber = 0;

        public enum AppMode
        {
            Participants,
            Faces,
            Emotions,
            EmotionsWithClientFaceDetect,
        }

        private string currentGroupName = Guid.NewGuid().ToString();
        private string currentGroupId;
        private TimeSpan currentTimerTask = TimeSpan.FromSeconds(6);
        private DateTime currentTimeTaskStart;
        private GameState gameState = GameState.Participants;
        private ScoringSystem scoringSystem; 

        private DispatcherTimer timer;
        private DateTime roundStart;
        private string timerText = "00:00";
        private MediaPlayer backgroundMusic;
        private MediaPlayer sound;

        private Dictionary<Guid, List<CroppedBitmap>> playerImages = new Dictionary<Guid, List<CroppedBitmap>>();
        private List<BitmapSource> groupImages = new List<BitmapSource>();
        private DateTime lastPlayerImagesTime = DateTime.MinValue;
        private int playerImagesTimeOffsetSec = 2;

        public MainWindow()
        {
            currentGroupId = currentGroupName;
            InitializeComponent();
            StartTimer();
            this.backgroundMusic = SoundProvider.Ukulele;
            this.backgroundMusic.Volume = 0.05;
            this.backgroundMusic.MediaEnded += new EventHandler((object sender, EventArgs e) => {
                this.backgroundMusic.Position = TimeSpan.Zero;
                this.backgroundMusic.Play();
            });
            this.backgroundMusic.Play();
            t.Elapsed += T_Elapsed;

            // Create grabber. 
            _grabber = new FrameGrabber<LiveCameraResult>();

            updateMode(AppMode.Participants);

            // Set up a listener for when the client receives a new frame.
            _grabber.NewFrameProvided += (s, e) =>
            {
                
                if (_mode == AppMode.EmotionsWithClientFaceDetect)
                {
                    // Local face detection. 
                    var rects = _localFaceDetector.DetectMultiScale(e.Frame.Image);
                    // Attach faces to frame. 
                    e.Frame.UserData = rects;
                }

                // The callback may occur on a different thread, so we must use the
                // MainWindow.Dispatcher when manipulating the UI. 
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    // Display the image in the left pane.
                    LeftImage.Source = e.Frame.Image.ToBitmapSource();

                    
                    // If we're fusing client-side face detection with remote analysis, show the
                    // new frame now with the most recent analysis available. 
                    if (_fuseClientRemoteResults)
                    {
                        RightImage.Source = VisualizeResult(e.Frame);
                    }

                    
                }));

                if (DateTime.Now - currentTimeTaskStart > currentTimerTask)
                {
                    if (gameState == GameState.Explain)
                    {
                        roundStart = DateTime.Now;
                        nextRound();
                    }
                    else if (gameState == GameState.RoundBegin)
                    {
                        currentTimerTask = TimeSpan.FromSeconds(15);
                        currentTimeTaskStart = DateTime.Now;
                        gameState = GameState.Game;
                        roundStart = DateTime.Now;
                    }
                    else if (gameState == GameState.Game)
                    {
                        currentTimerTask = TimeSpan.FromSeconds(6);
                        currentTimeTaskStart = DateTime.Now;
                        gameState = GameState.RoundEnd;
                        scoringSystem.AddRoundToGameScore();
                    }
                    else if (gameState == GameState.RoundEnd)
                    {
                        if (roundNumber == NumOfRounds)
                        {
                            this.sound = SoundProvider.TheWinner;
                            this.sound.Play();
                            currentTimerTask = TimeSpan.FromSeconds(3);
                            gameState = GameState.GameEnd;
                            this.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                StartEndImages();

                                button.Visibility = Visibility.Visible;
                            }));
                           
                        }
                        else
                        {
                            nextRound();
                            roundStart = DateTime.Now;
                        }
                    }
                }
            };

            // Set up a listener for when the client receives a new result from an API call. 
            _grabber.NewResultAvailable += (s, e) =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (e.TimedOut)
                    {
                        MessageArea.Text = "API call timed out.";
                    }
                    else if (e.Exception != null)
                    {
                        string apiName = "";
                        string message = e.Exception.Message;
                        var faceEx = e.Exception as FaceAPIException;
                        var emotionEx = e.Exception as Microsoft.ProjectOxford.Common.ClientException;
                        var visionEx = e.Exception as Microsoft.ProjectOxford.Vision.ClientException;
                        if (faceEx != null)
                        {
                            apiName = "Face";
                            message = faceEx.ErrorMessage;
                        }
                        else if (emotionEx != null)
                        {
                            apiName = "Emotion";
                            message = emotionEx.Error.Message;
                        }
                        else if (visionEx != null)
                        {
                            apiName = "Computer Vision";
                            message = visionEx.Error.Message;
                        }
                        MessageArea.Text = string.Format("{0} API call failed on frame {1}. Exception: {2}", apiName, e.Frame.Metadata.Index, message);
                    }
                    else
                    {
                        _latestResultsToDisplay = e.Analysis;

                        // Display the image and visualization in the right pane. 
                        if (!_fuseClientRemoteResults)
                        {
                            RightImage.Source = VisualizeResult(e.Frame);
                        }
                        if (gameState == GameState.Game || gameState == GameState.RoundBegin)
                        {
                            bool drawIndicator = false;
                            if (gameState == GameState.Game)
                            {
                                drawIndicator = true;
                            }
                            RightImage.Source = VisualizeTimer(drawIndicator);
                        }
                    }
                }));
            };

            // Create local face detector. 
            _localFaceDetector.Load("Data/haarcascade_frontalface_alt2.xml");
        }

        List<BitmapSource> images = new List<BitmapSource>();

        System.Timers.Timer t = new System.Timers.Timer(2000);
        private void StartEndImages()
        {
            images.Clear();
            if (groupImages != null)
            {
                foreach (var item in groupImages)
                {
                    //                        CroppedBitmap cropped = new CroppedBitmap(item, new Int32Rect(0, 0, 480, 640));
                    images.Add(item);
                }
                t.Start();

            }
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (images.Count > 0)
            {
            int r = rnd.Next(images.Count);
            var image = images[r];
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                LeftImage.Source = image;
            }));
        }
        }

        private VideoFrame lastFrame;
        private Face[] currentParticipants;

        /// <summary> Function which submits a frame to the Face API. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the faces returned by the API. </returns>
        private async Task<LiveCameraResult> ParticipantsAnalysisFunction(VideoFrame frame)
        {
            lastFrame = frame;
            // Encode image. 
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            lastFrame = frame;
            //otherJpg = new MemoryStream();
            //await jpg.CopyToAsync(otherJpg);
            // Submit image to API. 
            var attrs = new List<FaceAttributeType> { FaceAttributeType.Age,
                FaceAttributeType.Gender, FaceAttributeType.HeadPose };
            var faces = await _faceClient.DetectAsync(jpg, returnFaceAttributes: attrs);
            currentParticipants = faces;

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                button.Visibility = currentParticipants.Length > 1 && button_mode == "startGame" ? Visibility.Visible : Visibility.Hidden;
            }));
            // Count the API call. 
            Properties.Settings.Default.FaceAPICallCount++;
            // Output. 
            return new LiveCameraResult { Faces = faces };
        }

        /// <summary> Function which submits a frame to the Emotion API. </summary>
        /// <param name="frame"> The video frame to submit. </param>
        /// <returns> A <see cref="Task{LiveCameraResult}"/> representing the asynchronous API call,
        ///     and containing the emotions returned by the API. </returns>
        private async Task<LiveCameraResult> AnalysisFunction(VideoFrame frame)
        {
            var jpg = frame.Image.ToMemoryStream(".jpg", s_jpegParams);
            var attrs = new List<FaceAttributeType> { FaceAttributeType.Age, FaceAttributeType.Emotion };
            Face[] faces = await _faceClient.DetectAsync(jpg, returnFaceAttributes: attrs);
            Guid[] faceIds = faces.Select(face => face.FaceId).ToArray();

            
            var liveCameraResult = new LiveCameraResult
            {
                Faces = faces,
                EmotionScores = faces.Select(f => f.FaceAttributes.Emotion).ToArray()
            };

            try
            {
                IdentifyResult[] identities = await _faceClient.IdentifyAsync(currentGroupId, faceIds);
                var identityDict = new Dictionary<Guid, Face>();

                foreach (var identity in identities)
                {
                    if (identity.Candidates.Length > 0 && identity.Candidates[0].Confidence > 0.6)
                    {
                        identityDict[identity.Candidates[0].PersonId] = faces.First(f => f.FaceId == identity.FaceId);
                    }
                }

                liveCameraResult.Identities = identityDict;
            }
            catch (Exception e)
            {

            }

            return liveCameraResult;
        }

        private BitmapSource VisualizeResult(VideoFrame frame)
        {
            // Draw any results on top of the image. 
            BitmapSource visImage = frame.Image.ToBitmapSource();

            LiveCameraResult result = _latestResultsToDisplay;

            if (result != null)
            {
                // See if we have local face detections for this image.
                var clientFaces = (OpenCvSharp.Rect[])frame.UserData;
                if (clientFaces != null && result.Faces != null)
                {
                    // If so, then the analysis results might be from an older frame. We need to match
                    // the client-side face detections (computed on this frame) with the analysis
                    // results (computed on the older frame) that we want to display. 
                    MatchAndReplaceFaceRectangles(result.Faces, clientFaces);
                }

                if (this.gameState == GameState.Explain)
                {
                    visImage = Visualization.DrawExplain(visImage);
                }
                else if (this.gameState == GameState.RoundBegin)
                {
                    visImage = VisualizeStartRound(frame);
                }
                else if (this.gameState == GameState.RoundEnd)
                {
                    visImage = VisualizeEndRound(frame);
                }
                else if (this.gameState == GameState.Game)
                {
                    // Compute round score
                    Dictionary<Guid, int> scores = round.ComputeFrameScorePerPlayer(result);
                    scoringSystem.AddToCurrentRound(scores);
                    visImage = Visualization.DrawFaces(visImage, round, result.Identities, scoringSystem, _mode);

                    SavePlayerImages(frame.Image.ToBitmapSource(), result);
                }
                else if (this.gameState == GameState.Participants)
                {
                    visImage = Visualization.DrawParticipants(visImage, result.Faces);
                }
                else if (this.gameState == GameState.GameEnd)
                {
                    _grabber.StopProcessingAsync();
                    visImage = VisualizeEndGame(frame);
                }
            }

            return visImage;
        }


        private ImageSource VisualizeTimer(bool drawIndicator)
        {
            // Draw any results on top of the image. 

            return Visualization.DrawTime(timerText, drawIndicator, round);

        }

        private void SavePlayerImages(BitmapSource image, LiveCameraResult result)
        {
            if (result == null || result.Identities == null || this.gameState != GameState.Game)
            {
                return;
            }

            if (DateTime.Now.AddSeconds(-playerImagesTimeOffsetSec) > this.lastPlayerImagesTime)
            {
                this.groupImages.Add(image);
                SaveImageToFile(image);

                foreach (var player in result.Identities)
                {
                    int offset = 0;
                    Int32Rect faceRectangle = new Int32Rect(player.Value.FaceRectangle.Left + offset, player.Value.FaceRectangle.Top + offset, player.Value.FaceRectangle.Width + offset, player.Value.FaceRectangle.Height + offset);
                    CroppedBitmap playerImage = new CroppedBitmap(image, faceRectangle);

                    if (playerImages.ContainsKey(player.Key))
                    {                      
                        playerImages[player.Key].Add(playerImage);
                    }
                    else
                    {
                        playerImages[player.Key] = new List<CroppedBitmap>() { playerImage };
                    }

                    lastPlayerImagesTime = DateTime.Now;
                }
            }       
        }

        private void SaveImageToFile(BitmapSource image)
        {
            if (!Directory.Exists("images"))
            {
                System.IO.Directory.CreateDirectory("images");
            }


            var date = DateTime.Now;
            string filePath = @"images\" + date.ToFileTime() + ".png";
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }       
        }

        private BitmapSource VisualizeStartRound(VideoFrame frame)
        {

            var bitmap = VisualizeRound(frame);
            
            return Visualization.DrawRoundStart(bitmap, round, roundNumber);
        }

        private BitmapSource VisualizeEndRound(VideoFrame frame)
        {
            var bitmap = VisualizeRound(frame);
            string s = "Round Score:\n";

            return Visualization.DrawRoundEnd(bitmap, scoringSystem.CurrentRoundScore, playerImages, scoringSystem.TotalScore);

        }
        private BitmapSource VisualizeEndGame(VideoFrame frame)
        {
            var bitmap = VisualizeRound(frame);
            Dictionary<Guid,int> winners = scoringSystem.GameWinner();
            return Visualization.DrawGameEnd(bitmap, winners, playerImages, groupImages);

        }
        private BitmapSource VisualizeRound(VideoFrame frame)
        {
            PixelFormat pf = PixelFormats.Rgba64;

            // Define parameters used to create the BitmapSource.
            int width = frame.Image.Width;
            int height = frame.Image.Height;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            byte[] rawImage = new byte[rawStride * height];

            return BitmapSource.Create(frame.Image.Width, frame.Image.Height, 0, 0, pf, null, rawImage, rawStride);
        }

        private void updateMode(AppMode newMode)
        {
            this._mode = newMode;
            switch (_mode)
            {
                case AppMode.Participants:
                    _grabber.AnalysisFunction = ParticipantsAnalysisFunction;
                    break;
                case AppMode.Faces:
                    _grabber.AnalysisFunction = AnalysisFunction;
                    break;
                case AppMode.Emotions:
                    _grabber.AnalysisFunction = AnalysisFunction;
                    break;
                case AppMode.EmotionsWithClientFaceDetect:
                    // Same as Emotions, except we will display the most recent faces combined with
                    // the most recent API results. 
                    _grabber.AnalysisFunction = AnalysisFunction;
                    _fuseClientRemoteResults = true;
                    break;
                default:
                    _grabber.AnalysisFunction = null;
                    break;
            }
        }

        private void StartButton()
        {

            // Clean leading/trailing spaces in API keys. 
            Properties.Settings.Default.FaceAPIKey = Properties.Settings.Default.FaceAPIKey.Trim();
            Properties.Settings.Default.EmotionAPIKey = Properties.Settings.Default.EmotionAPIKey.Trim();
            Properties.Settings.Default.VisionAPIKey = Properties.Settings.Default.VisionAPIKey.Trim();

            // Create API clients. 

            _faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            //_faceClient = new FaceServiceClient(Properties.Settings.Default.FaceAPIKey, Properties.Settings.Default.FaceAPIHost);
            _emotionClient = new EmotionServiceClient(Properties.Settings.Default.EmotionAPIKey, Properties.Settings.Default.EmotionAPIHost);
            _visionClient = new VisionServiceClient(Properties.Settings.Default.VisionAPIKey, Properties.Settings.Default.VisionAPIHost);

            // How often to analyze. 
            _grabber.TriggerAnalysisOnInterval(Properties.Settings.Default.AnalysisInterval);

            // Reset message. 
            MessageArea.Text = "";

            _grabber.StartProcessingCameraAsync(0).Wait();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = 1 - SettingsPanel.Visibility;
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel.Visibility = Visibility.Hidden;
            Properties.Settings.Default.Save();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void MatchAndReplaceFaceRectangles(Face[] faces, OpenCvSharp.Rect[] clientRects)
        {
            // Use a simple heuristic for matching the client-side faces to the faces in the
            // results. Just sort both lists left-to-right, and assume a 1:1 correspondence. 

            // Sort the faces left-to-right. 
            var sortedResultFaces = faces
                .OrderBy(f => f.FaceRectangle.Left + 0.5 * f.FaceRectangle.Width)
                .ToArray();

            // Sort the clientRects left-to-right.
            var sortedClientRects = clientRects
                .OrderBy(r => r.Left + 0.5 * r.Width)
                .ToArray();

            // Assume that the sorted lists now corrrespond directly. We can simply update the
            // FaceRectangles in sortedResultFaces, because they refer to the same underlying
            // objects as the input "faces" array. 
            for (int i = 0; i < Math.Min(faces.Length, clientRects.Length); i++)
            {
                // convert from OpenCvSharp rectangles
                OpenCvSharp.Rect r = sortedClientRects[i];
                sortedResultFaces[i].FaceRectangle = new FaceRectangle { Left = r.Left, Top = r.Top, Width = r.Width, Height = r.Height };
            }
        }

        private string button_mode = "startGame";
        private async void button_Click(object sender, RoutedEventArgs e)
        {
            if (button_mode == "startGame")
            {
                button.Content = "Start Again";
                button_mode = "restartGame";
                var otherJpg = lastFrame.Image.Clone().ToMemoryStream(".jpg", s_jpegParams);
                byte[] streamBytes = ReadFully(otherJpg);

                this.sound = SoundProvider.PrepareYourself;
                this.sound.Play();
                this.gameState = GameState.Explain;
                this.currentTimerTask = TimeSpan.FromSeconds(15);
                this.currentTimeTaskStart = DateTime.Now;
                button.Visibility = Visibility.Hidden;
                this.currentTimeTaskStart = DateTime.Now;

                //FaceServiceClient faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
                await _faceClient.CreatePersonGroupAsync(currentGroupId, currentGroupName);
                Face[] clonedCurrentParticipants = (Face[])currentParticipants.Clone();
                List<Guid> personids = new List<Guid>();
                if (clonedCurrentParticipants.Length > 1)
                {

                    var bitmap = lastFrame.Image.ToBitmapSource();
                    for (int i = 0; i < clonedCurrentParticipants.Length; i++)
                    {
                        CreatePersonResult person = await _faceClient.CreatePersonAsync(currentGroupId, i.ToString());
                        personids.Add(person.PersonId);
                        MemoryStream s = new MemoryStream(streamBytes);
                        var rect = clonedCurrentParticipants[i].FaceRectangle;
                        var int32Rect = new Int32Rect(rect.Left, rect.Top, rect.Width, rect.Height);
                        CroppedBitmap playerBitmap = new CroppedBitmap(bitmap, int32Rect);
                        playerImages[person.PersonId] = new List<CroppedBitmap>();
                        playerImages[person.PersonId].Add(playerBitmap);
                        var addedPersistedPerson = await _faceClient.AddPersonFaceAsync(currentGroupId, person.PersonId, s, "userData", clonedCurrentParticipants[i].FaceRectangle);
                    }
                    scoringSystem = new ScoringSystem(personids.ToArray());
                    await _faceClient.TrainPersonGroupAsync(currentGroupId);
                }
            }
            else if (button_mode == "restartGame")
            {
                currentGroupId = Guid.NewGuid().ToString();
                currentGroupName = currentGroupId;
                roundNumber = 0;
                t.Stop();
                groupImages = new List<BitmapSource>();
                playerImages = new Dictionary<Guid, List<CroppedBitmap>>();
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    button.Content = "Start Game";
                    button.Visibility = Visibility.Hidden;
                    this.gameState = GameState.Participants;
                    button_mode = "startGame";
                    updateMode(AppMode.Participants);
                    _grabber.StartProcessingCameraAsync(0);
                }));
            }
        }

        private void nextRound()
        {
            if (this.round == null)
            {
                roundNumber = 1;
            }
            else
            {
                roundNumber++;
            }

            this.sound = SoundProvider.Round(roundNumber);
            this.sound.Play();
            round = getRandomRound();
            scoringSystem.CreateNewRound();
            this.gameState = GameState.RoundBegin;
            this.currentTimerTask = TimeSpan.FromSeconds(6);
            this.currentTimeTaskStart = DateTime.Now;
        }

        private IRound getRandomRound()
        {
            if (roundNumber == 1)
            {
                updateMode(AppMode.Emotions);
                return new RoundEmotion(EmotionType.Surprise, 0.9);
            }
            if (roundNumber == 2)
            {
                updateMode(AppMode.Faces);
                return new RoundAge();
            }

            EmotionType[] ProbabilityTable =
            {
                EmotionType.Anger, EmotionType.Anger, EmotionType.Anger,
                EmotionType.Fear, EmotionType.Fear, EmotionType.Fear,
                EmotionType.Sadness, EmotionType.Sadness,
                EmotionType.Disgust,
                EmotionType.Happiness,
            };

            int rand = new Random().Next(10);
            updateMode(AppMode.Emotions);
            return new RoundEmotion(ProbabilityTable[rand]);
        }

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StartButton();
        }

        private void StartTimer()
        {
            timer = new DispatcherTimer(
               new TimeSpan(0, 0, 0, 0, 50),
               DispatcherPriority.Background,
               t_Tick,
               Dispatcher.CurrentDispatcher);

            timer.IsEnabled = true;
        }

        private void t_Tick(object sender, EventArgs e)
        {
            TimeSpan timeSpan = currentTimerTask - (DateTime.Now - roundStart);
            timerText = timeSpan.ToString(@"ss");
        }
    }
}
