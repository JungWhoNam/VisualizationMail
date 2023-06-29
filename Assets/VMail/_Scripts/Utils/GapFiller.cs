using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils
{
    public class GapFiller : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;
        [SerializeField]
        private RectTransform botGap;
        [SerializeField]
        private RectTransform topGap;

        private void Start()
        {
            this.botGap.sizeDelta = new Vector2(this.botGap.sizeDelta.x, Screen.height * this.cam.rect.y);
            this.topGap.sizeDelta = new Vector2(this.topGap.sizeDelta.x, Screen.height * this.cam.rect.y);
        }

    }
}
