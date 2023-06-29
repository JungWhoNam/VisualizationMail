using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail.Utils
{
    // constrains the screen in 19:6...
    public class ScreenWidthAspectRatio : MonoBehaviour
    {
        private readonly int aspectWidth = 16;
        private readonly int aspectHeight = 9;

        private readonly int minWidth = 1920;

        public UnityEngine.UI.Text textForTesting;

        private int prevWidth;
        private int prevHeight;

        private void Start()
        {
            this.SetRatio();

            this.prevWidth = Screen.width;
        }

        private void Update()
        {
            this.SetRatio();
        }

        private void SetRatio()
        {
            // if not in 19:6
            if (this.prevWidth != Screen.width || this.prevHeight != Screen.height)
            {
                //Debug.Log("screen 1... " + this.GetCurrentAspectHeight());
                //Debug.Log("screen 2... " + "w and h: " + Screen.width + " x " + Screen.height);

                // constrains in 19:6...
                // 16:9 = w:h ==> h = 9 * w / 16
                int newWidth = Screen.width < this.minWidth ? this.minWidth : Screen.width;
                int newHeight = Mathf.RoundToInt((float) this.aspectHeight * newWidth / this.aspectWidth);
                Screen.SetResolution(newWidth, newHeight, false);

                if (this.textForTesting != null)
                {
                    this.textForTesting.text += "\tw and h: " + Screen.width + " x " + Screen.height;
                    this.textForTesting.text += "\tratio: 16 x " + this.GetCurrentAspectHeight();
                }

                this.prevWidth = Screen.width;
                this.prevHeight = Screen.height;
            }
        }

        private int GetCurrentAspectHeight()
        {
            // 16:? = w:h ==> ? = 16 * h / w
            return Mathf.RoundToInt((float) this.aspectWidth * Screen.height / Screen.width);
        }

    }
}