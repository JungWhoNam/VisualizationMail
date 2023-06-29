using System.IO;

using UnityEngine;
using UnityEngine.Events;

using VMail.Viewer.Realtime;

namespace VMail.Utils.MessageFile
{
    public class MessageFileGenerator : MonoBehaviour
    {
        public ViewerExploratoryVis viewer;
        public ProgressBar progressBar;
        [Space(10)]
        public UnityEvent onCompleteEvent;

        public bool isOn { get; private set; }
        private Message message;
        private string dirPath;
        private int currStep = 0;

        private int numOfPhase;
        private int numOfImgsPerPhase;
        private int numOfStepsPerImage = 3;
        private int numOfSteps;

        private string tempDirPath;


        public void Initialize(Message message, string dirPath)
        {
            // initialize variables
            this.isOn = true;
            this.message = message;
            this.dirPath = dirPath;
            this.currStep = 0;

            this.numOfPhase = message.pages.Count + (message.pages.Count - 1);
            this.numOfImgsPerPhase = (int)(VideoFileGenerator.fps * VideoFileGenerator.secPerPhase);
            this.numOfSteps = numOfPhase * numOfImgsPerPhase * this.numOfStepsPerImage;

            // initialize the progress bar
            this.progressBar.Initialize(numOfSteps, "Saving a video file...", "starting...");

            // set up the directories for saving the results
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            // for the temp directory to save out the image files for the video file generation
            this.tempDirPath = dirPath + "/" + VideoFileGenerator.TempImgDirName;
            if (Directory.Exists(tempDirPath)) // delete the temp directory, if already exists.
            {
                Tools.DeleteDirectory(tempDirPath);
            }
            if (!Directory.Exists(tempDirPath)) // create the temp directory.
            {
                if (Directory.CreateDirectory(tempDirPath) == null)
                {
                    Debug.LogError("problem creating a temporary directory for the images for the video... " + tempDirPath);
                    return;
                }
            }
        }

        public void ExecuteNextStep()
        {
            if (!this.isOn)
            {
                Debug.LogWarning("this generator is not on.");
                return;
            }

            if (this.currStep >= this.numOfSteps)
            {
                this.OnComplete();
                return;
            }

            int currPhaseIdx = this.currStep / (this.numOfImgsPerPhase * this.numOfStepsPerImage);

            int fromPageIdx = currPhaseIdx / 2;
            int toPageIdx = fromPageIdx + 1;
            float currTransitionAmt = 0f; // for the page phase

            if ((currPhaseIdx % 2 == 1)) // for the transition phase
            {
                currTransitionAmt =
                    ((this.currStep - (this.numOfImgsPerPhase * this.numOfStepsPerImage * currPhaseIdx))
                    / this.numOfStepsPerImage)
                    / (float)this.numOfImgsPerPhase;
            }

            if (currPhaseIdx % 2 == 0 && toPageIdx >= this.message.pages.Count) // for the last page phase
            {
                fromPageIdx -= 1;
                toPageIdx -= 1;
                currTransitionAmt = 1f;
            }

            int remainder = this.currStep % this.numOfStepsPerImage;
            if (remainder == 0) // 1) update the state.
            {
                //Debug.Log("Transition " + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3") + " - step #1");

                if (this.message.pages.Count > 1) // use the set transition
                {
                    Transition t = new Transition();
                    t.from = this.message.pages[fromPageIdx];
                    t.to = this.message.pages[toPageIdx];
                    t.amt = currTransitionAmt;
                    this.viewer.SetState(t);
                }
                else // use the set page
                {
                    this.viewer.SetState(this.message.pages[0]);
                }

                this.currStep += 1;
                string msg = "from a transition," + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3");
                msg += " - update the visualization state (1/3)";
                this.progressBar.IncreaseCnt(msg);
            }
            else if (remainder == 1) // 2) wait a frame.
            {
                //Debug.Log("Transition " + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3") + " - step #2");

                // nothing else to do.

                this.currStep += 1;
                string msg = "from a transition," + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3");
                msg += " - wait a frame before capturing the image (2/3)";
                this.progressBar.IncreaseCnt(msg);
            }
            else if (remainder == 2) // 3) save the images.
            {
                //Debug.Log("Transition " + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3") + " - step #3");

                Texture2D tex = this.viewer.GetImage(true);
                TextureScale.Bilinear(tex, VideoFileGenerator.width, tex.height * VideoFileGenerator.width / tex.width);
                tex.Apply();
                int decAmtWidth = tex.width % 2 == 0 ? 0 : -1;
                int decAmtHeight = tex.height % 2 == 0 ? 0 : -1;
                if (decAmtWidth != 0 || decAmtHeight != 0) // the images for the video have to be divisible by 2.
                {
                    tex = Tools.Crop(tex, 0, 0, tex.width + decAmtWidth, tex.height + decAmtHeight);
                }
                byte[] bytes = tex.EncodeToJPG();
                File.WriteAllBytes(tempDirPath + "/" + VideoFileGenerator.ImgNamePrefix + (this.currStep / this.numOfStepsPerImage) + ".jpg", bytes);

                this.currStep += 1;
                string msg = "from a transition," + fromPageIdx + "~" + toPageIdx + "," + currTransitionAmt.ToString("f3");
                msg += " - capure and save the image (3/3)";
                this.progressBar.IncreaseCnt(msg);
            }
        }

        private void OnComplete()
        {
            // save out the thumbnails, JSON, and the video, in this order.
            // save out the thumbnails before saving out the JSON file, since the pages will be updated with the saved out image paths
            // save out the video last, since the lastly modified time inidicates whether the video should be updated or not.
            ThumbnailFileGenerator.SaveThumbnailFiles(this.message, this.dirPath, true);

            // save out the JSON file
            JSONFileGenerator.SaveJSONFile(this.message, this.dirPath);

            //if (MessageFileGenerator.SaveVideo)
            {
                VideoFileGenerator.SaveVideoFile(this.message, this.dirPath, this.dirPath + "/" + VideoFileGenerator.TempImgDirName);
            }

            {
                try
                {
                    string src = Path.Combine(Application.streamingAssetsPath, "VMail", "index.html");
                    File.Copy(src, this.dirPath + "/index.html");
                }
                catch (System.UnauthorizedAccessException e)
                {
                    Debug.LogError(e);
                }
            }

            this.isOn = false;
            this.message = null;
            this.dirPath = string.Empty;
            this.currStep = 0;

            this.numOfPhase = 0;
            this.numOfImgsPerPhase = 0;
            this.numOfSteps = 0;

            this.tempDirPath = string.Empty;

            this.progressBar.Finish();
            this.progressBar.Close();

            if (this.onCompleteEvent != null)
            {
                this.onCompleteEvent.Invoke();
            }
        }

    }
}