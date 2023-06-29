using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

namespace VMail.Utils.MessageFile
{
    public class VideoFileGenerator : MonoBehaviour
    {
        public static int width = 1024;
        public static int fps = 10;

        public static float secPerPhase = 2f;

        public static readonly string VideoFileName = "video.mp4";
        public static readonly string VideoBackwardFileName = "video_backward.mp4";
        public static readonly string PlaylistFileName = "playlist.m3u";
        public static readonly string VtttFileName = "subtitle.vtt";

        public static readonly string ImgNamePrefix = "";
        public static readonly string TempImgDirName = "apodifjqlwef";

        public static bool deleteTempDirAfter = true;
        public static bool createPlaylistFile = true;


        public static void SaveVideoFile(Message message, string dirPath, string tempDirPath)
        {
            // create the video and delete the directory
            if (Directory.Exists(tempDirPath))
            {
                string videoFilePath = dirPath + "/" + VideoFileName;
                string chapterFilePath = tempDirPath + "/metadata.txt";
                string subtitleFilePath = tempDirPath + "/subtitle.srt";
                string subtitleVttFilePath = dirPath + "/" + VtttFileName;
                string playlistFilePath = dirPath + "/" + PlaylistFileName;

                // update the page info with the video information.
                UpdatePageInfo(message);

                // create the playlist file.
                if (createPlaylistFile)
                {
                    CreatePlaylistFile(message, playlistFilePath);
                }

                // the overwritting feature (when the input and output videos are the same), there are errors - codec not found.
                string fPath0 = dirPath + "/0.mp4"; // first phase (images to a video)
                string fPath1 = dirPath + "/1.mp4"; // second phase (the video with chapters)
                string fPath2 = dirPath + "/2.mp4"; // third phase (the video with subtitles)
                string fPath3 = dirPath + "/3.mp4"; // fourth phase (the video with a silent audio)

                // first, create a video (including a backward video)
                CreateVideo(tempDirPath, fPath0);
                CreateBackwardVideo(fPath0, dirPath + "/" + VideoBackwardFileName);

                // second, assign a chapter file (overwriting on the existing video file causes errors)
                CreateChapterMetaFile(message, chapterFilePath);
                AssignChapterFile(fPath0, fPath1, chapterFilePath);
                // third, assign a subtitle file (overwriting on the existing video file causes errors)
                CreateSrtFile(message, subtitleFilePath);
                AssignSubtitleFile(fPath1, fPath2, subtitleFilePath);
                // fourth, add a silent audio.
                AddSilentAudio(fPath2, fPath3);
                //
                CreateVttFile(message, subtitleVttFilePath);

                // rename and remove previous phase files
                if (File.Exists(videoFilePath))
                {
                    File.Delete(videoFilePath);
                }
                File.Move(fPath3, videoFilePath);
                if (deleteTempDirAfter && File.Exists(fPath0))
                {
                    File.Delete(fPath0);
                }
                if (deleteTempDirAfter && File.Exists(fPath1))
                {
                    File.Delete(fPath1);
                }
                if (deleteTempDirAfter && File.Exists(fPath2))
                {
                    File.Delete(fPath2);
                }

                if (deleteTempDirAfter)
                {
                    Tools.DeleteDirectory(tempDirPath);
                }
            }
        }

        // save out the video information
        static public void UpdatePageInfo(Message message)
        {
            if (message == null || message.pages == null)
            {
                Debug.LogWarning("UpdatePageInfo... message is null or its pages");
                return;
            }

            UpdatePageInfo(message.pages);
        }

        // save out the video information
        static public void UpdatePageInfo(MessageMobile message)
        {
            if (message == null || message.pages == null)
            {
                Debug.LogWarning("UpdatePageInfo... message is null or its pages");
                return;
            }

            UpdatePageInfo(message.pages);
        }

        static private void UpdatePageInfo(List<Page> pages)
        {
            for (int i = 0; i < pages.Count; i++)
            {
                Page page = pages[i];

                VideoThumbnail thumbnail = page.pageInfo.vid;
                thumbnail.from = i * secPerPhase * 2;
                thumbnail.to = thumbnail.from + secPerPhase;
            }
        }

        static public void CreatePlaylistFile(Message message, string fPath)
        {
            using (StreamWriter file = new StreamWriter(fPath))
            {
                file.WriteLine("#EXTM3U");

                for (int i = 0; i < message.pages.Count; i++)
                {
                    Page curr = message.pages[i];
                    float from = curr.pageInfo.vid.from;
                    float to = curr.pageInfo.vid.to;

                    if (i < message.pages.Count - 1)
                    {
                        Page next = message.pages[i + 1];
                        to = next.pageInfo.vid.from;
                    }

                    file.WriteLine("#EXTVLCOPT:start-time=" + from);
                    file.WriteLine("#EXTVLCOPT:stop-time=" + to);
                    file.WriteLine(VideoFileName);
                    file.WriteLine("");
                }

                file.Close();
            }
        }

        static public void CreateSrtFile(Message message, string fPath)
        {
            using (StreamWriter file = new StreamWriter(fPath))
            {
                for (int i = 0; i < message.pages.Count; i++)
                {
                    Page curr = message.pages[i];

                    TimeSpan from = TimeSpan.FromSeconds(curr.pageInfo.vid.from);
                    string fromStr = from.Hours.ToString("00") + ":" + from.Minutes.ToString("00") + ":" + from.Seconds.ToString("00") + "," + from.Milliseconds.ToString("000");
                    TimeSpan to = TimeSpan.FromSeconds(curr.pageInfo.vid.to);
                    string toStr = to.Hours.ToString("00") + ":" + to.Minutes.ToString("00") + ":" + to.Seconds.ToString("00") + "," + to.Milliseconds.ToString("000");

                    // write the index of the subtitle
                    file.WriteLine((i + 1));
                    // write the duration
                    file.Write(fromStr);
                    file.Write(" --> ");
                    file.WriteLine(toStr);
                    // write the message
                    foreach (Comment c in curr.pageInfo.comments)
                    {
                        file.WriteLine("[" + c.author + "] " + c.comment + "\n");
                    }
                    file.WriteLine("");
                }

                file.Close();
            }
        }

        static public void CreateVttFile(Message message, string fPath)
        {
            if (message == null || message.pages == null)
            {
                Debug.LogWarning("UpdatePageInfo... message is null or its pages");
                return;
            }

            CreateVttFile(message.pages, fPath);
        }

        static public void CreateVttFile(MessageMobile message, string fPath)
        {
            if (message == null || message.pages == null)
            {
                Debug.LogWarning("UpdatePageInfo... message is null or its pages");
                return;
            }

            CreateVttFile(message.pages, fPath);
        }

        static private void CreateVttFile(List<Page> pages, string fPath)
        {
            using (StreamWriter file = new StreamWriter(fPath))
            {
                file.WriteLine("WEBVTT\n");

                for (int i = 0; i < pages.Count; i++)
                {
                    Page curr = pages[i];

                    TimeSpan from = TimeSpan.FromSeconds(curr.pageInfo.vid.from);
                    string fromStr = from.Minutes.ToString("00") + ":" + from.Seconds.ToString("00") + "." + from.Milliseconds.ToString("000");
                    TimeSpan to = TimeSpan.FromSeconds(curr.pageInfo.vid.to);
                    string toStr = to.Minutes.ToString("00") + ":" + to.Seconds.ToString("00") + "." + to.Milliseconds.ToString("000");

                    // write the index of the subtitle
                    file.WriteLine((i + 1));
                    // write the duration
                    file.Write(fromStr);
                    file.Write(" --> ");
                    file.WriteLine(toStr);
                    // write the message
                    foreach (Comment c in curr.pageInfo.comments)
                    {
                        file.WriteLine("[" + c.author + "] " + c.comment);
                    }
                    file.WriteLine("");
                }

                file.Close();
            }
        }

        static public void CreateChapterMetaFile(Message message, string fPath)
        {
            string title = "Created by VMails";
            //string author = "Sexy Beast";

            using (StreamWriter file = new StreamWriter(fPath))
            {
                file.WriteLine(";FFMETADATA1");
                file.WriteLine("title=" + title);
                //file.WriteLine("artist=" + author);
                file.WriteLine("");

                for (int i = 0; i < message.pages.Count; i++)
                {
                    Page curr = message.pages[i];
                    long from = (int)(curr.pageInfo.vid.from * 1000f);
                    long to = (int)(curr.pageInfo.vid.to * 1000f);

                    if (i < message.pages.Count - 1)
                    {
                        Page next = message.pages[i + 1];
                        to = (int)(next.pageInfo.vid.from * 1000f) - 1;
                    }

                    file.WriteLine("[CHAPTER]");
                    file.WriteLine("TIMEBASE=1/1000");
                    file.WriteLine("START=" + from);
                    file.WriteLine("END=" + to);
                    file.WriteLine("title=" + "Chapter " + i);
                    file.WriteLine("");
                }

                file.WriteLine("[STREAM]");
                file.WriteLine("title=" + title);

                file.Close();
            }
        }

        static private string SurroundsQuotation(string path)
        {
            string path2 = path;

            path2 = path.StartsWith("\"") ? path2 : "\"" + path2;
            path2 = path.EndsWith("\"") ? path2 : path2 + "\"";

            return path2;
        }

        static public void CreateVideo(string imgDirPath, string videoPath)
        {
            imgDirPath = SurroundsQuotation(imgDirPath);
            videoPath = SurroundsQuotation(videoPath);

            // ./ffmpeg.exe -f image2 -r 30 -i ../imgs/%d.jpg -vcodec libx264 ../imgs/video.mp4
            string args = "-f image2";
            args += " -r " + fps; // set the framerate
            args += " -i " + imgDirPath + "/%d.jpg -vcodec libx264 " + videoPath;
            args += " -y"; // overwrite without asking

            RunFFMPEG(args);
        }

        static public void CreateBackwardVideo(string inputFilePath, string outputFilePath)
        {
            inputFilePath = SurroundsQuotation(inputFilePath);
            outputFilePath = SurroundsQuotation(outputFilePath);

            // ./ffmpeg -i video.mp4 -vf reverse -af areverse reversed.mp4
            string args = " -i " + inputFilePath; // set input video file
            args += " -vf reverse";
            args += " -af areverse ";
            args += outputFilePath;
            args += " -y"; // overwrite without asking

            RunFFMPEG(args);
        }

        static public void AssignChapterFile(string inputFilePath, string outputFilePath, string chapterFilePath)
        {
            inputFilePath = SurroundsQuotation(inputFilePath);
            outputFilePath = SurroundsQuotation(outputFilePath);
            chapterFilePath = SurroundsQuotation(chapterFilePath);

            // ./ffmpeg -i video.mp4 -i metadata.txt -map_metadata 1 -codec copy video-out.mp4
            string args = " -i " + inputFilePath; // set input video file
            args += " -i " + chapterFilePath; // set input chapter meta file
            args += " -map_metadata 1 -codec copy ";
            args += outputFilePath;
            args += " -y"; // overwrite without asking

            RunFFMPEG(args);
        }

        static public void AssignSubtitleFile(string inputFilePath, string outputFilePath, string subtitleFilePath)
        {
            inputFilePath = SurroundsQuotation(inputFilePath);
            outputFilePath = SurroundsQuotation(outputFilePath);
            subtitleFilePath = SurroundsQuotation(subtitleFilePath);

            string args = " -i " + inputFilePath; // set input video file
            args += " -i " + subtitleFilePath; // set input chapter meta file
            args += " -map 0 -map 1 -c copy -c:s mov_text ";
            args += outputFilePath;
            args += " -y"; // overwrite without asking

            RunFFMPEG(args);
        }

        static public void AddSilentAudio(string inputFilePath, string outputFilePath)
        {
            inputFilePath = SurroundsQuotation(inputFilePath);
            outputFilePath = SurroundsQuotation(outputFilePath);

            // ./ffmpeg -i ../imgs/2.mp4 -f lavfi -i anullsrc -c:v copy -c:a aac -c:s copy -shortest ../imgs/video.mp4
            string args = " -i " + inputFilePath; // set input video file
            args += " -f lavfi -i anullsrc";
            args += " -c:v copy -c:a aac -c:s copy -shortest ";
            args += outputFilePath;
            args += " -y"; // overwrite without asking

            RunFFMPEG(args);
        }

        static public void RunFFMPEG(string args)
        {
            string ffmpegPath = Application.streamingAssetsPath + "/ffmpeg/Win/ffmpeg.exe";

#if UNITY_STANDALONE_OSX
            // https://stackoverflow.com/questions/35174612/c-sharp-start-process-on-mac-ffmpeg-exit-code-1
            ffmpegPath = Application.streamingAssetsPath + "/ffmpeg/Mac/ffmpeg";
#endif

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ffmpegPath;
            p.StartInfo.Arguments = args;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;//no new windows
            p.Start();//begin

            string errFileCollection = p.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(errFileCollection))
            {
                Debug.Log("Running ffmpeg errors\n" + errFileCollection);
            }

            // Wait for ffmpeg to finish
            while (!p.HasExited) ;

            p.Close();//step new line
            p.Dispose();//free memory
            Debug.Log("Finished running a ffmpeg command... " + args);
        }

    }
}