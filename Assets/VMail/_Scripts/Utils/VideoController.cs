using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Collections;

namespace VMail.Utils
{
    public class VideoController : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        public enum PlaybackMode { Forward, ForwardTil, Backward, BackwardTil, None };

        [SerializeField]
        private MessageMobile message;
        [Space(10)]
        [SerializeField]
        private VideoPlayer videoPlayer;
        [SerializeField]
        private VideoPlayer videoPlayerBackward;

        [Space(10)]
        public Button forwardBtn;
        public Button forwardTilBtn;
        public Button forwardSkipBtn;

        [Space(10)]
        public Button backwardBtn;
        public Button backwardTilBtn;
        public Button backwardSkipBtn;

        [Space(10)]
        [SerializeField]
        private Image progress;
        [SerializeField]
        public UnityEvent onTimeChanged;

        private float prevProgressRatio = -1;
        private PlaybackMode currMode;
        private PlaybackMode lastMode;
        private bool isSwitchingForward = false;
        private bool isSwitchingBackward = false;


        void Start()
        {
            //this.videoPlayer.loopPointReached += EndReached;
            //this.videoPlayerBackward.loopPointReached += EndReached;
        }

        public void Open(string urlForward, string urlBackward)
        {
            this.prevProgressRatio = -1;

            this.videoPlayer.url = urlForward;
            this.videoPlayer.Play();

            this.videoPlayerBackward.url = urlBackward;
            this.videoPlayerBackward.Play();

            this.videoPlayer.prepareCompleted += OnPrepareCompleted;
        }

        private void OnPrepareCompleted(VideoPlayer vp)
        {
            this.GoToFirstPage();
            this.SetCurrentMode(PlaybackMode.None);
            this.UpdateButtonTexts(PlaybackMode.None);
        }

        public void SetVisibility(bool v)
        {
            this.videoPlayer.targetCameraAlpha = v ? 1f : 0f;
            this.videoPlayerBackward.targetCameraAlpha = v ? 1f : 0f;
        }

        /*public Vector2Int GetVideoSize()
        {
            return new Vector2Int((int)this.videoPlayer.width, (int)this.videoPlayer.height);
        }*/

        private void Update()
        {
            if (this.videoPlayer.frameCount > 0)
            {
                float pct = this.GetCurrentProgressRatio();

                if (this.prevProgressRatio != pct)
                {
                    // check if the end points are reached...
                    bool endReached = false;
                    float currTime = pct * (float)this.videoPlayer.length;
                    float secPerPhaseHalf = VMail.Utils.MessageFile.VideoFileGenerator.secPerPhase * 0.5f;

                    if (currTime < 0f + secPerPhaseHalf) // reached the start
                    {
                        //Debug.Log("reached the start... " + currTime);
                        this.GoToFirstPage();
                        this.SetCurrentMode(PlaybackMode.None);
                        this.UpdateButtonTexts(PlaybackMode.None);
                        endReached = true;
                    }
                    else if (currTime > this.videoPlayer.length - secPerPhaseHalf) // reached the end
                    {
                        //Debug.Log("reached the end... " + currTime);
                        this.GoToLastPage();
                        this.SetCurrentMode(PlaybackMode.None);
                        this.UpdateButtonTexts(PlaybackMode.None);
                        endReached = true;
                    }

                    if (!endReached && this.currMode == PlaybackMode.ForwardTil)
                    {
                        int idxPrev = this.GetPrevPageIndex(this.prevProgressRatio);
                        int idxCurr = this.GetPrevPageIndex(pct);

                        //Debug.Log(this.currMode.ToString() + ": " + idxPrev + " " + idxCurr + " " + pct);
                        if ((idxPrev != idxCurr && idxCurr == -1) || idxPrev < idxCurr) // reached the next page
                        //if (idxPrev < idxCurr) // reached the next page
                        {
                            this.ProcessButtonClick(this.forwardTilBtn);
                        }
                    }
                    else if (!endReached && this.currMode == PlaybackMode.BackwardTil)
                    {
                        int idxPrev = this.GetPrevPageIndex(this.prevProgressRatio);
                        int idxCurr = this.GetPrevPageIndex(pct);

                        //Debug.Log(this.currMode.ToString() + ": " + idxPrev + " " + idxCurr + " " + pct);
                        //if ((idxPrev != idxCurr && idxPrev == -1) || idxPrev > idxCurr) // reached the previous page
                        if (idxPrev > idxCurr) // reached the previous page
                        {
                            this.ProcessButtonClick(this.backwardTilBtn);
                        }
                    }

                    this.onTimeChanged.Invoke();
                    this.prevProgressRatio = pct;
                    this.progress.fillAmount = pct;
                }
            }
        }

        // process the button click 
        public void ProcessButtonClick(Button clicked)
        {
            PlaybackMode mode = PlaybackMode.None;

            if (clicked == this.forwardBtn)
            {
                mode = this.currMode == PlaybackMode.Forward ? PlaybackMode.None : PlaybackMode.Forward;
            }
            else if (clicked == this.forwardTilBtn)
            {
                mode = this.currMode == PlaybackMode.ForwardTil ? PlaybackMode.None : PlaybackMode.ForwardTil;
            }
            else if (clicked == this.backwardBtn)
            {
                mode = this.currMode == PlaybackMode.Backward ? PlaybackMode.None : PlaybackMode.Backward;
            }
            else if (clicked == this.backwardTilBtn)
            {
                mode = this.currMode == PlaybackMode.BackwardTil ? PlaybackMode.None : PlaybackMode.BackwardTil;
            }

            this.SetCurrentMode(mode);
            this.UpdateButtonTexts(mode);
        }

        // play or pause the corresponding video player
        // sync the video time
        private void SetCurrentMode(PlaybackMode mode)
        {
            // show and hide corresponding video player
            if (mode == PlaybackMode.Forward || mode == PlaybackMode.ForwardTil)
            {
                this.videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                this.videoPlayerBackward.renderMode = VideoRenderMode.CameraFarPlane;
            }
            else if (mode == PlaybackMode.Backward || mode == PlaybackMode.BackwardTil)
            {
                this.videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
                this.videoPlayerBackward.renderMode = VideoRenderMode.CameraNearPlane;
            }

            float ratio = this.GetCurrentProgressRatio();
            this.isSwitchingForward = true;
            this.isSwitchingBackward = true;
            this.videoPlayer.seekCompleted += OnSeekCompleted;
            this.videoPlayerBackward.seekCompleted += OnSeekCompleted;
            this.SkipToPercent(ratio);

            if (mode == PlaybackMode.None)
            {
                this.videoPlayer.Pause();
                this.videoPlayerBackward.Pause();
            }
            else if (mode == PlaybackMode.Forward || mode == PlaybackMode.ForwardTil)
            {
                this.videoPlayer.Play();
                this.videoPlayerBackward.Pause();
            }
            else if (mode == PlaybackMode.Backward || mode == PlaybackMode.BackwardTil)
            {
                this.videoPlayer.Pause();
                this.videoPlayerBackward.Play();
            }

            this.lastMode = this.currMode;
            this.currMode = mode;
        }

        // update the button texts depending on the playback mode
        private void UpdateButtonTexts(PlaybackMode mode)
        {
            this.forwardBtn.GetComponentInChildren<Text>().text = mode == PlaybackMode.Forward ? "||" : ">";
            this.forwardTilBtn.GetComponentInChildren<Text>().text = mode == PlaybackMode.ForwardTil ? "||" : ">|";

            this.backwardBtn.GetComponentInChildren<Text>().text = mode == PlaybackMode.Backward ? "||" : "<";
            this.backwardTilBtn.GetComponentInChildren<Text>().text = mode == PlaybackMode.BackwardTil ? "||" : "|<";
        }

        // returns 0~1
        private float GetCurrentProgressRatio()
        {
            float pct = (float)this.videoPlayer.frame / (float)this.videoPlayer.frameCount;

            // switching from the forward to the backward video
            // and the backward video is still swtiching
            if (this.isSwitchingBackward || this.isSwitchingForward)
            {
                if (this.lastMode == PlaybackMode.Backward || this.lastMode == PlaybackMode.BackwardTil)
                {
                    pct = 1f - ((float)this.videoPlayerBackward.frame / (float)this.videoPlayerBackward.frameCount);
                }
            }
            else
            {
                if (this.currMode == PlaybackMode.Backward || this.currMode == PlaybackMode.BackwardTil)
                {
                    pct = 1f - ((float)this.videoPlayerBackward.frame / (float)this.videoPlayerBackward.frameCount);
                }

                if (this.currMode == PlaybackMode.None)
                {
                    if (this.lastMode == PlaybackMode.Backward || this.lastMode == PlaybackMode.BackwardTil)
                    {
                        pct = 1f - ((float)this.videoPlayerBackward.frame / (float)this.videoPlayerBackward.frameCount);
                    }
                }
            }

            pct = Mathf.Clamp(pct, 0f, 1f);

            return pct;
        }

        private void OnSeekCompleted(VideoPlayer vp)
        {
            vp.seekCompleted -= this.OnSeekCompleted;
            StartCoroutine(this.WaitForEndOfFrameCoroutine(vp));
        }

        private IEnumerator WaitForEndOfFrameCoroutine(VideoPlayer vp)
        {
            yield return new WaitForEndOfFrame();
            if (vp == this.videoPlayer)
            {
                this.isSwitchingForward = false;
            }
            else if (vp == this.videoPlayerBackward)
            {
                this.isSwitchingBackward = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TrySkip(eventData);
        }

        public Page GetCurrentPage()
        {
            return this.GetPage(this.GetCurrentProgressRatio(), true);
        }

        public void GoToPage(Page page)
        {
            for (int i = 0; i < this.message.pages.Count; i++)
            {
                if (page == this.message.pages[i])
                {
                    int cnt = (this.message.pages.Count - 1) * 2 + 1;
                    float pct = ((1f / cnt) * (i * 2)) + (1f / cnt * 0.5f);
                    this.SkipToPercent(pct);
                    break;
                }
            }
        }

        public void GoToNextPage()
        {
            Page pg = this.GetPage(this.GetCurrentProgressRatio(), false);
            for (int i = 0; i < this.message.pages.Count - 1; i++)
            {
                if (pg == this.message.pages[i])
                {
                    this.GoToPage(this.message.pages[i + 1]);
                }
            }
        }

        public void GoToPreviousPage()
        {
            Page pg = this.GetPage(this.GetCurrentProgressRatio(), false);
            for (int i = 1; i < this.message.pages.Count; i++)
            {
                if (pg == this.message.pages[i])
                {
                    this.GoToPage(this.message.pages[i - 1]);
                }
            }
        }

        public void GoToFirstPage()
        {
            if (this.message != null && this.message.pages != null && this.message.pages.Count > 0)
            {
                this.GoToPage(this.message.pages[0]);
            }
        }

        public void GoToLastPage()
        {
            if (this.message != null && this.message.pages != null && this.message.pages.Count > 0)
            {
                this.GoToPage(this.message.pages[this.message.pages.Count - 1]);
            }
        }

        private Page GetPage(float progressRatio, bool includeEmptySpot)
        {
            int cnt = (this.message.pages.Count - 1) * 2 + 1;
            float pct = Mathf.Clamp(progressRatio, 0f, 0.99f);
            //int idx = (int)pct.LinearRemap(0f, 1f, 0f, cnt);
            int idx = (int)(pct * cnt);

            if (idx % 2 == 0 || !includeEmptySpot)
            {
                return this.message.pages[idx / 2];
            }
            return null;
        }

        private void SkipToPercent(float pct)
        {
            { // check if the end points are reached...
                float currTime = pct * (float)this.videoPlayer.length;
                float secPerPhaseHalf = VMail.Utils.MessageFile.VideoFileGenerator.secPerPhase * 0.5f;

                if (currTime < 0f + secPerPhaseHalf) // reached the start
                {
                    //Debug.Log("SkpToPercent... reached the start... " + currTime);
                    pct = (0f + secPerPhaseHalf) / (float)this.videoPlayer.length;
                }
                else if (currTime > this.videoPlayer.length - secPerPhaseHalf) // reached the end
                {
                    //Debug.Log("SkpToPercent... reached the end... " + currTime);
                    pct = ((float)this.videoPlayer.length - secPerPhaseHalf) / (float)this.videoPlayer.length;
                }
            }

            { // update the forward video player
                var frame = this.videoPlayer.frameCount * pct;
                this.videoPlayer.frame = (long)frame;
            }
            { // update the backward video player
                var frame = this.videoPlayerBackward.frameCount * (1f - pct);
                this.videoPlayerBackward.frame = (long)frame;
            }

            this.prevProgressRatio = pct;
            this.progress.fillAmount = pct;
        }

        private void TrySkip(PointerEventData eventData)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                progress.rectTransform, eventData.position, null, out localPoint))
            {
                float pct = Mathf.InverseLerp(progress.rectTransform.rect.xMin, progress.rectTransform.rect.xMax, localPoint.x);
                this.SkipToPercent(pct);
            }
        }

        private int GetPrevPageIndex(float pct)
        {
            for (int i = 0; i < this.message.pages.Count - 1; i++)
            {
                int cnt = (this.message.pages.Count - 1) * 2 + 1;
                float pct0 = ((1f / cnt) * (i * 2)) + (1f / cnt * 0.5f);
                float pct1 = ((1f / cnt) * ((i + 1) * 2)) + (1f / cnt * 0.5f);

                if (pct0 <= pct && pct < pct1)
                {
                    return i;
                }
            }

            return -1;
        }

        /*private void EndReached(VideoPlayer vp)
        {
            this.forwardBtn.GetComponentInChildren<Text>().text = ">";
            this.forwardTilBtn.GetComponentInChildren<Text>().text = ">|";

            this.backwardBtn.GetComponentInChildren<Text>().text = "<";
            this.backwardTilBtn.GetComponentInChildren<Text>().text = "|<";

            this.SetCurrentMode(PlaybackMode.None);

            if (vp == this.videoPlayer)
            {
                this.progress.fillAmount = 1f;
            }
            else if (vp == this.videoPlayerBackward)
            {
                this.progress.fillAmount = 0f;
            }
        }*/

    }
}