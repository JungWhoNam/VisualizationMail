using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VMail.Utils;
using VMail.Utils.MessageFile;
using VMail.Viewer.Social;
using VMail.Viewer.Baked;
using UnityEngine.Events;

namespace VMail
{
    public class ManagerMobile : MonoBehaviour
    {
        [SerializeField]
        private VideoController videoController;
        public MessageMobile message;

        [Space(10)]
        [SerializeField]
        private ViewerBackgrondAnntateImages pageViewer;
        [SerializeField]
        private ViewerComment commentViewer;
        [SerializeField]
        private ViewerSphericalImage sphereViewer;

        [Space(10)]
        public UnityEvent onSaveCompleted;

        private int currStoryModeIndex = 0; // 0: View, 1: Annotate, 2: Explore


        private void Start()
        {
            // this.Open("C://Users/JungWhoNam/AppData/LocalLow/DefaultCompany/ABREngine/VisMessages/1n5bMuEAAhZNv3fE399wdMTV_Qs8L_6mk");
            // this.Open("C://Users/JungWhoNam/AppData/LocalLow/IVLab/ABREngine/vmails/39"); //34
        }


        public void UpdateStateFromCurrentTransition()
        {
            Page page = this.videoController.GetCurrentPage();

            if (this.currStoryModeIndex == 1) // Annotate Mode
            {
                this.pageViewer.gameObject.SetActive(true);
                this.pageViewer.SetInteractable(true);
                this.sphereViewer.gameObject.SetActive(false);
                this.videoController.SetVisibility(true);
            }
            else if (this.currStoryModeIndex == 2) // Explore Mode
            {
                this.pageViewer.gameObject.SetActive(false);
                this.pageViewer.SetInteractable(false);
                this.sphereViewer.gameObject.SetActive(page != null);
                this.videoController.SetVisibility(page == null);
            }
            else // View Mode
            {
                this.pageViewer.gameObject.SetActive(true);
                this.pageViewer.SetInteractable(false);
                this.sphereViewer.gameObject.SetActive(false);
                this.videoController.SetVisibility(true);
            }

            if (this.pageViewer != null)
            {
                this.pageViewer.SetState(page);
            }
            if (this.commentViewer != null)
            {
                this.commentViewer.SetState(page);
            }
            if (this.sphereViewer != null)
            {
                this.sphereViewer.SetState(page);
            }
        }

        public void SetStoryMode(int modeIndex)
        {
            this.currStoryModeIndex = modeIndex;
        }

        public void Open(string dirPath)
        {
            // create a MessageInfo from the JSON file
            string str = Tools.ReadJsonFile(dirPath + "/" + JSONFileGenerator.JSONName);
            if (string.IsNullOrEmpty(str))
            {
                Debug.LogWarning("error on opening...");
                return;
            }
            MessageInfo info = JsonUtility.FromJson<MessageInfo>(str);

            // get preview images
            List<Texture2D> previews = new List<Texture2D>();
            List<Texture2D> img360s = new List<Texture2D>();
            List<Texture2D> anns = new List<Texture2D>();
            foreach (PageInfo page in info.pageInfos)
            {
                {
                    string fPath = dirPath + "/" + page.img.fPath;
                    previews.Add(Tools.LoadTexture2D(fPath));
                }
                {
                    string fPath = dirPath + "/" + page.img360.fPath;
                    img360s.Add(Tools.LoadTexture2D(fPath));
                }
                {
                    string fPath = dirPath + "/" + page.ann.imgFilePath;
                    anns.Add(Tools.LoadTexture2D(fPath));
                }
            }

            // initialize the message with the new info.
            this.message.Initialize(info, previews, img360s, anns);

            // open the video
            this.videoController.Open(dirPath + "/" + VideoFileGenerator.VideoFileName, dirPath + "/" + VideoFileGenerator.VideoBackwardFileName);
            //this.videoPlayer.Play();

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = true;
        }

        public void SaveTo(string msgDirPath)
        {
            if (this.message.pages.Count <= 0)
            {
                Debug.LogWarning("error on saving... the message has no page.");
                return;
            }
            else if (!Directory.Exists(msgDirPath))
            {
                Debug.LogWarning("error on saving... the directory does not exist: " + msgDirPath);
                return;
            }

            VideoFileGenerator.UpdatePageInfo(this.message);

            // create the thumbnail direction, if it does not exist.
            string dirPath = msgDirPath + "/" + ThumbnailFileGenerator.ThumbnailDirName;
            if (!Directory.Exists(dirPath))
            {
                Debug.LogWarning("the directory does not exists: " + dirPath);
                return;
            }

            // Save the JSON file
            JSONFileGenerator.SaveJSONFile(this.message, msgDirPath);

            // Save the VTT file
            string vttFilePath = Path.Combine(msgDirPath, VideoFileGenerator.VtttFileName);
            VideoFileGenerator.CreateVttFile(this.message, vttFilePath);

            // save the annotation images
            for (int i = 0; i < this.message.pages.Count; i++)
            {
                Page page = this.message.pages[i];

                Texture2D tex = page.annTex;
                if (tex == null)
                {
                    page.pageInfo.ann.imgFilePath = string.Empty;
                }
                else
                {
                    string relPath = ThumbnailFileGenerator.ThumbnailDirName + "/" + ThumbnailFileGenerator.AnnImgNamePrefix + i + ".png";
                    Tools.SaveTexture2D(msgDirPath + "/" + relPath, tex);
                    page.pageInfo.ann.imgFilePath = relPath;
                }
            }

            this.onSaveCompleted.Invoke();
        }

    }

}
