using System.Collections.Generic;
using UnityEngine;

namespace VMail.Viewer.Social
{
    public class ViewerAnnotationSpatial : MonoBehaviour, IViewer
    {
        public static float Threshold = 0.00025f; // 0.025

        [SerializeField]
        private ManagerDesktop managerDesktop;
        [SerializeField]
        private AnnotationSpatial baseAnnotation;

        [Space(10)]
        [SerializeField]
        private BrushPallette toolBoxUI;
        [SerializeField]
        private float distFromNearPlane = 1f;

        private bool isEnabled = false;
        private Message currMessage;
        private AnnotationSpatial currAnnotation;
        private List<AnnotationSpatial> anns = new List<AnnotationSpatial>();
        private bool isExplorationMode;

        private void Update()
        {
            if (this.managerDesktop.GetCurrentMode() != Realtime.ViewerExploratoryVis.VisMode.Exploratory)
                return;

            // only if the mouse is within the camera's viewport.
            Vector3 viewPort = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if ((viewPort.x >= 0f) && (viewPort.x <= 1f) && (viewPort.y >= 0f) && (viewPort.y <= 1f))
            {
                AnnotationSpatial annHit = null;

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.parent != null)
                    {
                        annHit = hit.transform.parent.GetComponent<AnnotationSpatial>();
                    }
                }

                this.HighlightAnnotation(annHit == null ? null : annHit.page);

                if (annHit != null && Input.GetMouseButtonDown(0))
                {
                    this.managerDesktop.UpdateState(annHit.page);
                }
            }
        }

        public void HighlightAnnotation(Page page)
        {
            foreach (AnnotationSpatial ann in this.anns)
            {
                ann.SetSelected(page != null && ann.page == page);
                ann.page.SetSelected(page != null && ann.page == page);
            }
        }

        public void UnHighlightAnnotations()
        {
            this.HighlightAnnotation(null);
        }

        // ============================================= INTERFACE =============================================

        public void OpenMessage(Message message)
        {
            this.Clear();

            for (int i = 0; i < message.pages.Count; i++)
            {
                // create a new annotation widget.
                Page page = message.pages[i];
                AnnotationSpatial ann = Instantiate(this.baseAnnotation);
                ann.gameObject.name = "ann_" + i;
                ann.SetPage(page);
                this.anns.Add(ann);
                ann.transform.SetParent(this.transform);
                ann.gameObject.SetActive(true);

                // move and orient accordingly.
                Vector3 pos = page.pageInfo.viewInfo.pos;
                Quaternion rot = page.pageInfo.viewInfo.rot;
                {
                    float dist = this.distFromNearPlane;
                    float h = Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * dist * 2f;
                    float w = h * Camera.main.aspect;
                    // for the full-scale aspect ration... use the below instead of // float w = h * this.cam.aspect; 
                    // float a = (this.cam.pixelWidth / this.cam.rect.width) / (this.cam.pixelHeight / this.cam.rect.height);
                    // float w = h * a;
                    ann.transform.position = pos + (rot * Vector3.forward * dist);
                    ann.transform.localScale = new Vector3(w, h, 1f);
                    ann.transform.rotation = rot;
                }
            }
        }

        public void SetState(Page page)
        {
            if (!this.isEnabled)
            {
                return;
            }

            if (page == null)
            {
                this.currAnnotation = null;
                this.toolBoxUI.SetInteractable(false);
                return;
            }
            else
            {
                // turn on and off the colliders
                foreach (AnnotationSpatial ann in this.anns)
                {
                    if (ann.page == page)
                    {
                        this.currAnnotation = ann;
                    }
                    ann.SetInteractable(ann.page == page);
                    ann.SetBackgroundImageVisibility(ann.page == page);
                }
                // turn on/off the toolbox UI
                this.toolBoxUI.SetInteractable(page != null);
            }
        }

        public void SetState(Transition transition)
        {
            if (!this.isEnabled)
            {
                return;
            }
            if (transition == null)
            {
                this.currAnnotation = null;
                this.toolBoxUI.SetInteractable(false);
                return;
            }

            // get the current page
            Page currPage = null;
            if (transition.amt <= ViewerAnnotationSpatial.Threshold)
            {
                currPage = transition.from;
            }
            else if (transition.amt >= (1f - ViewerAnnotationSpatial.Threshold))
            {
                currPage = transition.to;
            }

            // turn on and off the colliders
            foreach (AnnotationSpatial ann in this.anns)
            {
                if (ann.page == currPage)
                {
                    this.currAnnotation = ann;
                }
                ann.SetInteractable(ann.page == currPage);
                ann.SetBackgroundImageVisibility(ann.page == currPage);
            }

            // turn on/off the toolbox UI
            this.toolBoxUI.SetInteractable(currPage != null);
        }

        // ==========================================================================================

        public void SetEnable(bool enable)
        {
            this.isEnabled = enable;

            this.toolBoxUI.gameObject.SetActive(this.isEnabled);

            if (!this.isEnabled)
            {
                for (int i = 0; i < this.anns.Count; i++)
                {
                    this.anns[i].SetInteractable(false);
                    this.anns[i].SetBackgroundImageVisibility(false);
                }
            }
        }

        public void ClearCurrentAnnotation()
        {
            if (this.currAnnotation != null)
            {
                this.currAnnotation.Clear();
            }
        }

        public void Clear()
        {
            foreach (AnnotationSpatial ann in this.anns)
            {
                DestroyImmediate(ann.gameObject);
            }
            this.anns.Clear();
        }

    }
}