using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils
{
    public class ProgressBar : MonoBehaviour
    {
        public Text progBarTitle;
        public Text progBarDescription;
        public Image progBarImage;

        private int totalCnt;
        private int currCnt;

        public void Initialize(int totalCnt = 10, string title = "Progress Bar", string description = "start.")
        {
            this.progBarTitle.text = title;
            this.progBarDescription.text = description;
            this.totalCnt = totalCnt;
            this.currCnt = 0;
            this.progBarImage.fillAmount = 0f;

            this.gameObject.SetActive(true);
        }

        public void IncreaseCnt(string description = null)
        {
            if (description != null)
            {
                this.progBarDescription.text = description;
            }
            this.currCnt += 1;
            this.progBarImage.fillAmount = (float)this.currCnt / this.totalCnt;
        }

        public void SetPercentage(float ratio, string description = null)
        {
            if (description != null)
            {
                this.progBarDescription.text = description;
            }
            this.currCnt = (int)(this.totalCnt * ratio);
            this.progBarImage.fillAmount = (float)this.currCnt / this.totalCnt;
        }

        public void Finish(string description = "finished.")
        {
            if (description != null)
            {
                this.progBarDescription.text = description;
            }
            this.currCnt = this.totalCnt;
            this.progBarImage.fillAmount = 1f;
        }

        public void Close()
        {
            this.gameObject.SetActive(false);
        }

    }
}