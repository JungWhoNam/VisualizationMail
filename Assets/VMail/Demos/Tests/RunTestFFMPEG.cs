using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunTestFFMPEG : MonoBehaviour
{
    public Text outputText;

    private void Start()
    {
        string args = " -version";
        string results = RunTestFFMPEG.RunFFMPEG(args);
        outputText.text = results;
    }

    static public string RunFFMPEG(string args)
    {
        string results = "";

        string ffmpegPath = Application.streamingAssetsPath + "/ffmpeg/Win/ffmpeg.exe";
#if UNITY_STANDALONE_OSX
        // https://stackoverflow.com/questions/35174612/c-sharp-start-process-on-mac-ffmpeg-exit-code-1
        ffmpegPath = Application.streamingAssetsPath + "/ffmpeg/Mac/ffmpeg";
#endif

        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo.FileName = ffmpegPath;
        p.StartInfo.Arguments = args;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;//no new windows
        p.Start();//begin

        string input = p.StandardOutput.ReadToEnd();
        if (!string.IsNullOrEmpty(input))
        {
            results += "Running ffmpeg\n" + input + "\n";
        }

        string err = p.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(err))
        {
            results += "Running ffmpeg errors\n" + err + "\n";
        }

        // Wait for ffmpeg to finish
        while (!p.HasExited) ;

        p.Close();//step new line
        p.Dispose();//free memory
        Debug.Log("Finished running a ffmpeg command... " + args);

        return results;
    }
}
