using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail
{
    public class MessageMobile : MonoBehaviour
    {
        [SerializeField]
        private GameObject markerContainer;
        [SerializeField]
        private Page basePage;
        [SerializeField]
        private GameObject baseSpacer;

        public MessageInfo info { get; private set; }
        public List<Page> pages = new List<Page>();
        public List<GameObject> spacers = new List<GameObject>();

        public GameObject blockLeft;
        public GameObject blockRight;

        private Vector2 initOffsetMin;
        private Vector2 initOffsetMax;

        public void Awake()
        {
            this.initOffsetMin = this.GetComponent<RectTransform>().offsetMin;
            this.initOffsetMax = this.GetComponent<RectTransform>().offsetMax;
        }

        public void Initialize(MessageInfo info, List<Texture2D> previews, List<Texture2D> img360s, List<Texture2D> imgAnns)
        {
            this.Clear();
            this.info = info;

            for (int i = 0; i < info.pageInfos.Count; i++)
            {
                PageInfo pageInfo = info.pageInfos[i];
                this.AddPageMarker(pageInfo, previews == null ? null : previews[i], img360s == null ? null : img360s[i], imgAnns == null ? null : imgAnns[i]);
            }

            if (this.blockLeft != null && this.blockRight != null)
            {
                int numOfMarkers = info.pageInfos.Count + (info.pageInfos.Count - 1);
                float w = this.GetComponent<RectTransform>().rect.width / (float)numOfMarkers;
                {
                    RectTransform rt = this.GetComponent<RectTransform>();
                    //rt.offsetMin = new Vector2(rt.offsetMin.x - w * 0.5f, rt.offsetMin.y);
                    //rt.offsetMax = new Vector2(rt.offsetMax.x + w * 0.5f, rt.offsetMax.y);
                    rt.offsetMin = new Vector2(initOffsetMin.x - w * 0.5f, initOffsetMin.y);
                    rt.offsetMax = new Vector2(initOffsetMax.x + w * 0.5f, initOffsetMax.y);
                }

                w = this.GetComponent<RectTransform>().rect.width / (float)numOfMarkers;
                {
                    RectTransform rt = this.blockLeft.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(w * 0.5f + 2f, rt.sizeDelta.y);
                    rt.anchoredPosition = new Vector2(-2, rt.anchoredPosition.y);
                }

                {
                    RectTransform rt = this.blockRight.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(w * 0.5f + 2f, rt.sizeDelta.y);
                    rt.anchoredPosition = new Vector2(-1f * w * 0.5f, rt.anchoredPosition.y);
                }
            }
        }

        public void Clear()
        {
            this.info = null;
            if (this.pages != null)
            {
                foreach (Page page in this.pages)
                {
                    Destroy(page.gameObject);
                }
                pages.Clear();
            }
            if (this.spacers != null)
            {
                foreach (GameObject spacer in this.spacers)
                {
                    Destroy(spacer.gameObject);
                }
                spacers.Clear();
            }
        }

        public List<PageInfo> GetPageInfos()
        {
            List<PageInfo> infos = new List<PageInfo>();
            foreach (Page page in this.pages)
            {
                infos.Add(page.pageInfo);
            }
            return infos;
        }

        private void AddPageMarker(PageInfo info, Texture2D thumbnail, Texture2D img360, Texture2D annImg)
        {
            // add the spacer
            if (this.pages.Count > 0)
            {
                GameObject spacer = Instantiate(this.baseSpacer);
                spacer.transform.SetParent(this.markerContainer.transform, false);
                spacer.gameObject.name = "spacer-" + this.spacers.Count;
                spacer.gameObject.SetActive(true);
                this.spacers.Add(spacer);
            }

            // add the marker
            {
                Page page = Instantiate(this.basePage);
                page.Initialize(info, thumbnail, img360, annImg);
                page.transform.SetParent(this.markerContainer.transform, false);
                page.gameObject.name = "marker-" + this.pages.Count;
                page.gameObject.SetActive(true);
                page.SetMode(false);
                this.pages.Add(page);
            }
        }

    }
}