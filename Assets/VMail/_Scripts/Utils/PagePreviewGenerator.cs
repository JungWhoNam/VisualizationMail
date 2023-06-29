using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VMail.Viewer.Realtime;

namespace VMail.Utils
{
    public class PagePreviewGenerator : MonoBehaviour
    {
        public ViewerExploratoryVis viewer;
        public ProgressBar progressBar;

        public UnityEvent onCompleted;

        public bool isOn { get; private set; }
        private int currStep = 0;
        private List<Page> pages;
        private int stepsInInterval = 3;

        public void Initialize(List<Page> pages)
        {
            this.isOn = true;
            this.currStep = 0;
            this.pages = pages;
            this.progressBar.Initialize(this.pages.Count * this.stepsInInterval, "Updating Preview Images...", "starting...");
        }

        public void ExecuteNextStep()
        {
            if (!this.isOn)
            {
                Debug.LogWarning("this generator is not on.");
                return;
            }

            int idx = (this.currStep / stepsInInterval);
            int remainder = (this.currStep % stepsInInterval);

            if (idx >= this.pages.Count)
            {
                this.OnComplete();
            }
            else if (remainder == 0) // 1) update the state.
            {
                this.progressBar.IncreaseCnt("updating the visualization state... (" + (idx + 1) + "/" + this.pages.Count + ")");

                this.viewer.SetState(this.pages[idx]);
                this.currStep += 1;
            }
            else if (remainder == 1) // 2) wait a frame.
            {
                this.progressBar.IncreaseCnt("waiting a frame... (" + (idx + 1) + "/" + this.pages.Count + ")");

                this.currStep += 1;
            }
            else if (remainder == 2) // 2) capture the images and update the page.
            {
                this.progressBar.IncreaseCnt("updateing the preview image... (" + (idx + 1) + "/" + this.pages.Count + ")");

                this.pages[idx].SetThumbnail(false, this.viewer.GetImage(false));
                this.pages[idx].SetThumbnail(true, this.viewer.GetImage360());

                if (idx < (this.pages.Count - 1))
                {
                    this.pages[idx].SetImageToNextPage(this.viewer.GetImage(this.pages[idx + 1].pageInfo.viewInfo));
                }
                else
                {
                    this.pages[idx].SetImageToNextPage(null);
                }

                this.currStep += 1;
            }
        }

        private void OnComplete()
        {
            this.isOn = false;
            this.currStep = 0;
            this.pages = null;
            this.progressBar.Finish();
            this.progressBar.Close();

            this.onCompleted.Invoke();
        }

    }
}