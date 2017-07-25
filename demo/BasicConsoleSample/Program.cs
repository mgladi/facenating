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
using VideoFrameAnalyzer;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;
using System.Linq;

namespace BasicConsoleSample
{
    internal class Program
    {

        private static void Regular()
        {
            // Create grabber. 
            FrameGrabber<Face[]> grabber = new FrameGrabber<Face[]>();

            // Create Face API Client.
            FaceServiceClient faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            // Set up a listener for when we acquire a new frame.
            grabber.NewFrameProvided += (s, e) =>
            {
                Console.WriteLine("New frame acquired at {0}", e.Frame.Metadata.Timestamp);
            };

            // Set up Face API call.
            grabber.AnalysisFunction = async frame =>
            {
                Console.WriteLine("Submitting frame acquired at {0}", frame.Metadata.Timestamp);
                // Encode image and submit to Face API. 
                return await faceClient.DetectAsync(frame.Image.ToMemoryStream(".jpg"));
            };

            // Set up a listener for when we receive a new result from an API call. 
            grabber.NewResultAvailable += (s, e) =>
            {
                if (e.TimedOut)
                    Console.WriteLine("API call timed out.");
                else if (e.Exception != null)
                    Console.WriteLine("API call threw an exception.");
                else
                    Console.WriteLine("New result received for frame acquired at {0}. {1} faces detected", e.Frame.Metadata.Timestamp, e.Analysis.Length);
            };

            // Tell grabber when to call API.
            // See also TriggerAnalysisOnPredicate
            grabber.TriggerAnalysisOnInterval(TimeSpan.FromMilliseconds(3000));

            // Start running in the background.
            grabber.StartProcessingCameraAsync().Wait();

            // Wait for keypress to stop
            Console.WriteLine("Press any key to stop...");
            Console.ReadKey();

            // Stop, blocking until done.
            grabber.StopProcessingAsync().Wait();

        }
        private static void Main(string[] args)
        {
            FaceServiceClient faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
            FindInGroup();
            //AddFaces();
        }

        private static void FindInGroup()
        {
            // Create Face API Client.
            FaceServiceClient faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");

            string testImageFile = @"C:\Users\zivci\Documents\Students\both.png";
            string personGroupId = "zivandrazgroupid";
            using (Stream s = File.OpenRead(testImageFile))
            {
                var faces = faceClient.DetectAsync(s).Result;
                var faceIds = faces.Select(face => face.FaceId).ToArray();


                var results = faceClient.IdentifyAsync(personGroupId, faceIds).Result;
                foreach (var identifyResult in results)
                {
                    Console.WriteLine("Result of face: {0}", identifyResult.FaceId);
                    if (identifyResult.Candidates.Length == 0)
                    {
                        Console.WriteLine("No one identified");
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = faceClient.GetPersonAsync(personGroupId, candidateId).Result;
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }
        }
        private static void AddFaces()
        {
            // Create Face API Client.
            FaceServiceClient faceClient = new FaceServiceClient("3b6c7018fa594441b2465d5d8652526a", "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
            faceClient.CreatePersonGroupAsync("zivandrazgroupid", "zivandrazgroup").Wait();
            using (Stream s = File.OpenRead(@"C:\Users\zivci\Documents\Students\zivcizer.png"))
            {
                var zivPerson = faceClient.CreatePersonAsync("zivandrazgroupid", "zivPerson").Result;
                var persistedZiv = faceClient.AddPersonFaceAsync("zivandrazgroupid", zivPerson.PersonId, s).Result;
            }
            using (Stream s = File.OpenRead(@"C:\Users\zivci\Documents\Students\raz.png"))
            {
                var razPerson = faceClient.CreatePersonAsync("zivandrazgroupid", "razPerson").Result;
                var persistedRaz = faceClient.AddPersonFaceAsync("zivandrazgroupid", razPerson.PersonId, s).Result;
            }
            faceClient.TrainPersonGroupAsync("zivandrazgroupid").Wait();

        }
    }
}
