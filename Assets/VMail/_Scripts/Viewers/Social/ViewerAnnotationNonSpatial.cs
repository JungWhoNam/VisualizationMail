using System.Collections.Generic;
using UnityEngine;

namespace VMail.Viewer.Social
{
    public class ViewerAnnotationNonSpatial : MonoBehaviour, IViewer
    {
        public static float Threshold = 0.00025f;

        [Space(10)]
        [SerializeField]
        private BrushPallette toolBoxUI;
        [SerializeField]
        private float distFromNearPlane = 0.8f;
        [SerializeField]
        private bool ignoreAspect = true;
        [SerializeField]
        private AnnotationSpatial annotation;

        private bool isEnabled = true;

        void Awake()
        {
            // place the annotation plane within the camera view frustum.
            //if (this.cam != null)
            {
                this.transform.SetParent(Camera.main.transform);
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
                this.transform.localScale = Vector3.one;

                float dist = this.distFromNearPlane;
                float h = Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * dist * 2f;
                float w = h * Camera.main.aspect;

                if (!this.ignoreAspect)
                {
                    //h = Mathf.Tan(this.cam.fieldOfView * this.cam.rect.height * Mathf.Deg2Rad * 0.5f) * dist * 2f;


                    // for the full-scale aspect ration... use the below instead of // float w = h * this.cam.aspect; 
                    w = h * Camera.main.aspect;
                   float a = (Camera.main.pixelWidth / Camera.main.rect.width) / (Camera.main.pixelHeight / Camera.main.rect.height);
                    w = h * a;
                }

                Vector3 pos = this.annotation.transform.position;
                Quaternion rot = this.annotation.transform.rotation;
                this.annotation.transform.position = pos + (rot * Vector3.forward * dist);
                this.annotation.transform.localScale = new Vector3(w, h, 1f);
                this.annotation.transform.rotation = rot;

                this.gameObject.SetActive(false);
            }
        }

        // ============================================= INTERFACE =============================================

        public void OpenMessage(Message message)
        {
            // nothing to do
        }

        public void SetState(Page page)
        {
            if (!this.isEnabled)
            {
                return;
            }

            if (page == null)
            {
                this.annotation.SetInteractable(false);
                this.toolBoxUI.SetInteractable(false);
                this.annotation.gameObject.SetActive(false);
            }
            else
            {
                this.annotation.SetInteractable(true);
                this.toolBoxUI.SetInteractable(true);
                this.annotation.gameObject.SetActive(true);

                this.annotation.SetPage(page);
            }
        }

        public void SetState(Transition transition)
        {
            if (transition == null)
            {
                return;
            }

            if (!this.isEnabled)
            {
                return;
            }

            if (transition.amt < 0.5f)
            {
                this.annotation.SetInteractable(false);
                this.annotation.gameObject.SetActive(false);
            }
            else
            {
                this.annotation.gameObject.SetActive(true);
                this.annotation.SetPage(transition.to);

                if (transition.amt > (1f - ViewerAnnotationNonSpatial.Threshold))
                {
                    this.annotation.SetInteractable(true);
                    this.toolBoxUI.SetInteractable(true);
                }
                else
                {
                    this.annotation.SetInteractable(false);
                    this.toolBoxUI.SetInteractable(false);
                }
            }
        }

        // ==========================================================================================

        public void ToggleOnOff()
        {
            this.SetEnable(!this.isEnabled);
        }

        public void SetEnable(bool enable)
        {
            this.isEnabled = enable;
            this.toolBoxUI.gameObject.SetActive(this.isEnabled);
        }

        public void Clear()
        {
            this.annotation.Clear();
        }

    }
}