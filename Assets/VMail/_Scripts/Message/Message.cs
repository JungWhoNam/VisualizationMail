using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VMail.Viewer;

namespace VMail
{
    public class Message : MonoBehaviour
    {
        protected static int SortByPosX(Page p0, Page p1)
        {
            return p0.GetComponent<RectTransform>().anchoredPosition.x.CompareTo(p1.GetComponent<RectTransform>().anchoredPosition.x);
        }

        [SerializeField]
        protected Page basePage;
        [SerializeField]
        protected GameObject pageContainer;
        [SerializeField]
        protected GameObject addPagePanel;

        public List<Page> pages = new List<Page>();

        [Space(10)]
        [SerializeField]
        private Utils.PagePreviewGenerator previewGen;

        public bool isEditMode { get; protected set; }


        private void Awake()
        {
            this.basePage.gameObject.SetActive(false);
        }

        public virtual void Initialize(MessageInfo info, List<Texture2D> previews, List<Texture2D> img360s, List<Texture2D> anns)
        {
            this.Clear();

            for (int i = 0; i < info.pageInfos.Count; i++)
            {
                this.AddPage(info.pageInfos[i], previews == null ? null : previews[i], img360s == null ? null : img360s[i], anns == null ? null : anns[i]);
            }
        }

        public virtual Page AddPage(PageInfo pageInfo, Texture2D preview, Texture2D img360, Texture2D annTex)
        {
            return this.AddPage(this.pages.Count, pageInfo, preview, img360, annTex);
        }

        public virtual Page AddPage(int at, PageInfo pageInfo, Texture2D preview, Texture2D img360, Texture2D annTex)
        {
            if (at < 0 || at > this.pages.Count)
            {
                Debug.LogError("the index is wrong: " + at + " " + this.pages.Count);
                return null;
            }

            // create a page
            Page pg = Instantiate(this.basePage).GetComponent<Page>();
            pg.Initialize(pageInfo, preview, img360, annTex);

            // update the page and add to the list
            pg.gameObject.SetActive(true);
            pg.transform.SetParent(this.pageContainer.transform, false);
            pg.transform.SetSiblingIndex(this.pageContainer.transform.childCount - 2);
            this.pages.Insert(at, pg);

            for (int i = 0; i < this.pages.Count; i++)
            {
                this.pages[i].name = "Page - " + i;
            }

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = false;

            return pg;
        }

        public virtual void Remove(Page page)
        {
            if (this.previewGen.isOn)
            {
                Debug.LogWarning("The page preview generator is currently being used...");
                return;
            }

            this.pages.Remove(page);
            GameObject.Destroy(page.gameObject);

            // update the preview images
            this.previewGen.Initialize(this.pages);

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = false;
        }

        public virtual void Clear()
        {
            foreach (Page page in this.pages)
            {
                GameObject.Destroy(page.gameObject);
            }
            this.pages.Clear();
        }

        public virtual void UpdatePageSizes(float pageAspect)
        {
            foreach (Page page in this.pages)
            {
                Vector2 size = page.GetComponent<RectTransform>().sizeDelta;
                page.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y * pageAspect, size.y);
            }

            if (this.addPagePanel != null) // update the add page panel size
            {
                Vector2 size = this.addPagePanel.GetComponent<RectTransform>().sizeDelta;
                this.addPagePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(size.y * pageAspect, size.y);
            }
        }

        public virtual void RefreshLayout()
        {
            this.pageContainer.GetComponent<HorizontalLayoutGroup>().enabled = false;
            this.pageContainer.GetComponent<HorizontalLayoutGroup>().enabled = true;
        }

        public virtual void UpdateOrderOfPages()
        {
            if (this.previewGen.isOn)
            {
                Debug.LogWarning("The page preview generator is currently being used...");
                return;
            }

            // based on the x-position, order the pages
            this.pages.Sort(SortByPosX);

            // update the order of the Page gameobjects based on the order of the list "pages"
            for (int i = 0; i < this.pages.Count; i++)
            {
                this.pages[i].transform.SetSiblingIndex(i);
                this.pages[i].gameObject.name = "Page - " + i;
            }

            // update the preview images
            this.previewGen.Initialize(this.pages);

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = false;
        }

        public virtual void SetMode(bool editMode)
        {
            this.addPagePanel.gameObject.SetActive(editMode);
            foreach (Page page in this.pages)
            {
                page.SetMode(editMode);
            }
            this.isEditMode = editMode;
        }

        public virtual float GetDistFromFirstToEndPages()
        {
            if (this.pages.Count <= 1)
            {
                return 0f;
            }

            Vector2 pageSize = this.pages.Count <= 0 ? Vector2.zero : this.pages[0].GetComponent<RectTransform>().sizeDelta;
            float spacing = this.pageContainer.GetComponent<HorizontalLayoutGroup>().spacing;
            return (this.pages.Count - 1) * pageSize.x + (this.pages.Count - 1) * spacing;
        }

        public virtual float GetOffsetFirstPage()
        {
            if (this.pages.Count <= 0)
            {
                return 0f;
            }

            Vector2 pageSize = this.pages.Count <= 0 ? Vector2.zero : this.pages[0].GetComponent<RectTransform>().sizeDelta;

            float spacing = this.pageContainer.GetComponent<HorizontalLayoutGroup>().padding.left;
            return spacing + pageSize.x * 0.5f;
        }

    }

    [System.Serializable]
    public class MessageInfo
    {
        public string lastEditedByAuthor;
        public string lastEditedByDate;
        public DeviceType lastEditedByDevice;

        public List<PageInfo> pageInfos = new List<PageInfo>();

        public void SetPages(List<Page> pages)
        {
            this.pageInfos.Clear();
            foreach (Page page in pages)
            {
                this.pageInfos.Add(page.pageInfo);
            }
        }

        public void SetPageInfos(List<PageInfo> pageInfos)
        {
            this.pageInfos.Clear();
            foreach (PageInfo pageInfo in pageInfos)
            {
                this.pageInfos.Add(pageInfo);
            }
        }

    }

    public class Transition
    {
        public Page from;
        public Page to;
        public float amt;

        public Vector3 getCameraPosition()
        {
            Vector3 posFrom = Vector3.zero, posTo = Vector3.zero;

            if (from != null)
            {
                posFrom = from.pageInfo.viewInfo.pos;
            }
            if (to != null)
            {
                posTo = to.pageInfo.viewInfo.pos;
            }

            return Vector3.Lerp(posFrom, posTo, amt);
        }

        public Quaternion getCameraRotation()
        {
            Quaternion rotFrom = Quaternion.identity, rotTo = Quaternion.identity;

            if (from != null)
            {
                rotFrom = from.pageInfo.viewInfo.rot;
            }
            if (to != null)
            {
                rotTo = to.pageInfo.viewInfo.rot;
            }

            return Quaternion.Lerp(rotFrom, rotTo, amt);
        }
    }
}