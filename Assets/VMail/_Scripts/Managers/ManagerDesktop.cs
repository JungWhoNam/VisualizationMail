using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using VMail.Viewer;
using VMail.Utils;
using VMail.Utils.MessageFile;
using VMail.Viewer.Social;
using VMail.Viewer.Realtime;

using UnityEngine.Events;

namespace VMail
{
    public class ManagerDesktop : MonoBehaviour
    {
        [SerializeField]
        private Button navButton;
        [SerializeField]
        private GameObject navOnImage;
        [SerializeField]
        private Button annButton;
        [SerializeField]
        private GameObject annOnImage;
        [SerializeField]
        private Text commButton;
        [SerializeField]
        public CamMoveAroundOrigin nav;
        [SerializeField]
        private ViewerAnnotationNonSpatial viewerAnnNonSpatial;

        [Header("Viewers")]
        public ViewerExploratoryVis viewer;
        public ViewerComment viewerComment;
        public ViewerAnnotationSpatial viewerAnnotation;
        public Message message;
        public MessagePlayer messagePlayer;

        [Header("Generators")]
        [SerializeField]
        private PagePreviewGenerator previewGenOnAdd;
        [SerializeField]
        private PagePreviewGenerator previewGenOnOpen;
        [SerializeField]
        private MessageFileGenerator messageGen;

        private bool isEditMode = true;

        private int prevScreenWidth;
        private int prevScreenHeight;

        bool addNewPageThisFrame = false;
        bool saveMessageThisFrame = false;
        bool openNextFrame = false; // for the volume rendering (need to take the screen shot after the rendering is updated - a frame after). 
        bool openMessageThisFrame = false;
        string dirPath;

        private void Awake()
        {
            this.prevScreenWidth = Screen.width;
            this.prevScreenHeight = Screen.height;
        }

        private void Start()
        {
            this.UpdatePageSizes();
            this.UpdateMessagePlayer();
        }

        private void Update()
        {
            // if the screen size has been changed...
            if (this.prevScreenWidth != Screen.width || this.prevScreenHeight != Screen.height)
            {
                Debug.Log(Screen.width + " " + Screen.height);
                this.UpdatePageSizes();
                this.UpdateMessagePlayer();
                this.prevScreenWidth = Screen.width;
                this.prevScreenHeight = Screen.height;
            }
        }


        private void LateUpdate()
        {
            if (this.messageGen.isOn)
            {
                this.viewerAnnotation.UnHighlightAnnotations();

                this.messageGen.ExecuteNextStep();
            }
            else if (this.previewGenOnAdd.isOn)
            {
                this.viewerAnnotation.UnHighlightAnnotations();

                this.previewGenOnAdd.ExecuteNextStep();
            }
            else if (this.previewGenOnOpen.isOn)
            {
                this.viewerAnnotation.UnHighlightAnnotations();

                this.previewGenOnOpen.ExecuteNextStep();
            }
            else if (this.addNewPageThisFrame)
            {
                PageInfo pageInfo = new PageInfo();
                pageInfo.viewInfo = viewer.GetView();
                Page page = this.message.AddPage(pageInfo, viewer.GetImage(false), viewer.GetImage360(pageInfo.viewInfo), Tools.CreateAnnotationTexture(this.viewer.GetViewingAreaSize()));

                this.viewerAnnotation.OpenMessage(this.message);
                this.UpdateMessagePlayer();
                this.messagePlayer.SetTransitionValueAsRatio(1f);
                this.UpdateStateFromCurrentTransition();

                this.addNewPageThisFrame = false;

                this.previewGenOnAdd.Initialize(this.message.pages);
            }
            else if (this.saveMessageThisFrame)
            {
                if (this.message.pages.Count <= 0)
                {
                    Debug.LogWarning("error on saving... the message has no page.");
                }
                else if (dirPath == null)
                {
                    Debug.LogWarning("error on saving...");
                }
                else
                {
                    this.SetStoryMode(true);
                    this.messageGen.Initialize(this.message, this.dirPath);
                }

                this.saveMessageThisFrame = false;
            }
            else if (this.openNextFrame)
            {
                this.openMessageThisFrame = true;
                this.openNextFrame = false;
            }
            else if (this.openMessageThisFrame)
            {
                if (dirPath == null || !Directory.Exists(dirPath))
                {
                    Debug.LogWarning("error on opening... " + dirPath);
                }
                else
                {
                    Debug.Log("opening " + dirPath);
                    this.Open(dirPath);

                    this.UpdatePageSizes();
                    this.UpdateMessagePlayer();
                    this.messagePlayer.SetTransitionValueAsRatio(0f);
                    this.UpdateStateFromCurrentTransition();
                }

                this.openMessageThisFrame = false;

                // re-generate preview images
                if (this.dirPath != null && Directory.Exists(this.dirPath))
                {
                    this.SetStoryMode(false);
                    this.previewGenOnOpen.Initialize(this.message.pages);
                }
            }
        }

        /*public void Open()
        {
            string dirPath = Tools.OpenDirPanel(Application.dataPath + "/Resources");
            this.OpenFrom(dirPath);
        }*/

        public void OpenFrom(string dirPath)
        {
            this.openNextFrame = true;
            this.dirPath = dirPath;
        }

        private void Open(string dirPath)
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
            List<Texture2D> nextImages = new List<Texture2D>();
            List<Texture2D> anns = new List<Texture2D>();
            foreach (PageInfo page in info.pageInfos)
            {
                previews.Add(Tools.LoadTexture2D(dirPath + "/" + page.img.fPath));
                img360s.Add(Tools.LoadTexture2D(dirPath + "/" + page.img360.fPath));
                nextImages.Add(page.imgToNext.fPath == "" ? null : Tools.LoadTexture2D(dirPath + "/" + page.imgToNext.fPath));
                anns.Add(Tools.LoadTexture2D(dirPath + "/" + page.ann.imgFilePath));
            }

            // initialize the message with the new info.
            this.message.Initialize(info, previews, img360s, anns);

            //
            for (int i = 0; i < this.message.pages.Count; i++)
            {
                this.message.pages[i].SetImageToNextPage(nextImages[i]);
            }

            // 
            this.viewerAnnotation.OpenMessage(this.message);

            this.UpdateMessagePlayer();
            this.messagePlayer.SetTransitionValueAsRatio(0f);
            this.UpdateStateFromCurrentTransition();

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = true;
        }

        /*public void Save()
        {
            string dirPath = Tools.OpenDirPanel(Application.dataPath + "/Resources");
            this.SaveTo(dirPath);
        }*/

        public void SaveTo(string dirPath)
        {
            this.saveMessageThisFrame = true;
            this.dirPath = dirPath;
        }

        public void SetStoryMode(bool enableStoryMode)
        {
            this.nav.enabled = !enableStoryMode;
            this.viewerAnnotation.SetEnable(enableStoryMode);
            //this.viewerAnnNonSpatial.SetEnable(enableStoryMode);
            this.viewerComment.SetInputFieldVisiblity(enableStoryMode);
            this.navButton.interactable = enableStoryMode;
            this.navOnImage.gameObject.SetActive(!enableStoryMode);
            this.annButton.interactable = !enableStoryMode;
            this.annOnImage.gameObject.SetActive(enableStoryMode);
            this.message.SetMode(!enableStoryMode);
            this.messagePlayer.PausePlaying();

            this.viewer.SetCurrentMode(enableStoryMode ? ViewerExploratoryVis.VisMode.Story : ViewerExploratoryVis.VisMode.Exploratory);

            this.UpdateStateFromCurrentTransition();
        }

        public ViewerExploratoryVis.VisMode GetCurrentMode()
        {
            return this.viewer.currMode;
        }

        public void AddNewPage()
        {
            this.addNewPageThisFrame = true;
        }

        public void RemovePage(Page page)
        {
            this.message.Remove(page);

            this.viewerAnnotation.OpenMessage(this.message);

            this.UpdateMessagePlayer();

            this.UpdateStateFromCurrentTransition();

            if (this.message.pages.Count <= 0)
            {
                this.SetStoryMode(false);
            }
        }

        public void UpdateState(Transition t)
        {
            if (t == null) return;

            float threshold = 0.00025f; // Threshold

            // a page mode
            if (this.viewer.currMode == ViewerExploratoryVis.VisMode.Exploratory &&
                ((t.from == null || t.to == null) || (t.amt < threshold || t.amt > (1f - threshold))))
            {
                // get the current page
                Page p = null;
                if (t.from == null)
                    p = t.to;
                else if (t.to == null)
                    p = t.from;
                else if (t.amt < threshold)
                    p = t.from;
                else if (t.amt > (1f - threshold))
                    p = t.to;

                // update the viewers accordingly
                if (this.viewer != null)
                    this.viewer.SetState(p);

                if (this.viewerComment != null)
                    this.viewerComment.SetState(p);

                if (this.viewerAnnotation != null)
                    this.viewerAnnotation.SetState(p);

                if (this.viewerAnnNonSpatial != null)
                    this.viewerAnnNonSpatial.SetState(p);
            }
            else // a transition mode
            {
                if (this.viewer != null)
                    this.viewer.SetState(t);
                if (this.viewerComment != null)
                    this.viewerComment.SetState(t);
                if (this.viewerAnnotation != null)
                    this.viewerAnnotation.SetState(t);
                //if (this.viewerAnnNonSpatial != null)
                //    this.viewerAnnNonSpatial.SetState(t);
            }
        }

        public void UpdateStateFromCurrentTransition()
        {
            if (this.messagePlayer == null) return;

            Transition t = this.GetCurrentTransition();
            this.UpdateState(t);
        }

        public void RemoveAllPages()
        {
            this.message.Clear();

            this.viewerAnnotation.Clear();

            this.viewerAnnNonSpatial.Clear();

            this.UpdateMessagePlayer();

            this.UpdateStateFromCurrentTransition();

            this.SetStoryMode(false);

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = true;
        }

        public void ClearVisState()
        {
            this.viewer.ClearVis();
        }

        public void UpdateMessagePlayer()
        {
            if (this.messagePlayer == null)
            {
                return;
            }

            this.messagePlayer.SetTransitionSize(0f, this.message.pages.Count - 1.0001f);
            this.messagePlayer.gameObject.SetActive(this.message.pages.Count > 1);

            // update the size of the player (slider)
            if (this.message.pages.Count > 1)
            {
                RectTransform rt = this.messagePlayer.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(this.message.GetDistFromFirstToEndPages(), rt.sizeDelta.y);

                Vector3 pos = rt.localPosition;
                rt.localPosition = new Vector3(this.message.GetOffsetFirstPage(), pos.y, pos.z);
            }
        }

        // update the message's thumbnail size according to the viewing area
        public void UpdatePageSizes()
        {
            Vector2 size = this.viewer.GetViewingAreaSize();
            this.message.UpdatePageSizes(size.x / size.y);
        }

        public void UpdateState(Page page)
        {
            if (this.message.pages.Count == 1)
            {
                Transition t = this.GetCurrentTransition();
                this.viewer.SetState(t);
            }

            if (this.messagePlayer != null)
            {
                for (int i = 0; i < this.message.pages.Count; i++)
                {
                    if (this.message.pages[i] == page)
                    {
                        this.messagePlayer.SetTransitionValue(i);
                    }
                }
            }

            this.UpdateStateFromCurrentTransition();
        }

        // ============================================= GET =============================================

        public Transition GetCurrentTransition()
        {
            return this.GetTransition(this.messagePlayer.GetTransitionValue());
        }

        public Page GetCurrentPage()
        {
            float v = this.messagePlayer.GetTransitionValue();

            if (v - (int)v >= 0.9999f)  // Threshold
            {
                return this.message.pages[(int)v + 1];
            }
            if (v - (int)v <= 0.0001f)
            {
                return this.message.pages[(int)v];
            }
            return null;
        }

        public Transition GetTransition(float v)
        {
            if (this.messagePlayer == null)
            {
                return null;
            }

            if (this.message.pages.Count <= 0)
            {
                return null;
            }

            Transition currTransition = new Transition();
            if (this.message.pages.Count == 1)
            {
                currTransition.amt = 0.9999f;
                currTransition.to = this.message.pages[0];
            }
            else
            {
                int fromIdx = (int)v;
                currTransition.from = fromIdx < 0 ? null : this.message.pages[fromIdx];
                int toIdx = fromIdx + 1;
                currTransition.to = toIdx >= this.message.pages.Count ? null : this.message.pages[toIdx];
                float amt = v - fromIdx;
                currTransition.amt = amt;
            }

            return currTransition;
        }

        public Transition GetTransitionFromRatio(float zeroToOne)
        {
            if (this.messagePlayer == null)
            {
                return null;
            }

            float min = this.messagePlayer.GetMinValue();
            float max = this.messagePlayer.GetMaxValue();
            float v = (max - min) * Mathf.Clamp01(zeroToOne) + min;

            return this.GetTransition(v);
        }

    }
}
