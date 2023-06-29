using UnityEngine;

namespace VMail.Viewer.Baked
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ViewerAlphaBlendPages : MonoBehaviour, IViewer
    {        
        [SerializeField]
        private float distFromNearPlane;
        
        private Material alphaBlendMat;
        private Texture2D prevFromPageImage;
        private Texture2D prevToPageImage;

        void Awake()
        {
            this.alphaBlendMat = this.GetComponent<MeshRenderer>().material;

            // place the image plane within the camera view frustum.
            //if (this.cam != null)
            {
                this.transform.SetParent(Camera.main.transform);
                this.transform.localPosition = Vector3.zero;
                this.transform.localRotation = Quaternion.identity;
                this.transform.localScale = Vector3.one;

                float dist = this.distFromNearPlane;
                float h = Mathf.Tan(Camera.main.fieldOfView * Mathf.Deg2Rad * 0.5f) * dist * 2f;
                float w = h * Camera.main.aspect;
                // for the full-scale aspect ration... use the below instead of // float w = h * this.cam.aspect; 
                // float a = (this.cam.pixelWidth / this.cam.rect.width) / (this.cam.pixelHeight / this.cam.rect.height);
                // float w = h * a;
                Vector3 pos = this.transform.position;
                Quaternion rot = this.transform.rotation;
                this.transform.position = pos + (rot * Vector3.forward * dist);
                this.transform.localScale = new Vector3(w, h, 1f);
                this.transform.rotation = rot;

                this.gameObject.SetActive(false);
            }
        }

        public void OpenMessage(Message message)
        {
            // nothing to do
        }

        // set the page preview image as the currently showing image.
        // set the preview image as the second image of the blending material. 
        // leave the second image of the blending material as it is. 
        // set the blending amount to zero, meaning showing only the first image.
        public void SetState(Page page)
        {
            if (page == null)
            {
                Debug.LogWarning("this page is null.");
                return;
            }

            Texture2D img = page.GetThumbnailImage(false);
            if (this.prevFromPageImage != img)
            {
                this.prevFromPageImage = img;
                this.alphaBlendMat.SetTexture("_MainTex", img);
            }

            this.alphaBlendMat.SetFloat("_TransitionValue", 0f);
        }

        // blend between two preview images of the pages in the transitions.
        // if one of the pages is null or the transition is null, do nothing and return. 
        public void SetState(Transition transition)
        {
            if (transition == null)
            {
                Debug.LogWarning("this transition is null.");
                return;
            }
            else if (transition.from == null || transition.to == null)
            {
                Debug.LogWarning("from or to Page of this transition are null.");
                return;
            }

            Texture2D img = transition.from.GetImageToNextPage();
            if (this.prevFromPageImage != img)
            {
                this.prevFromPageImage = img;
                this.alphaBlendMat.SetTexture("_MainTex", img);
            }

            Texture2D img2 = transition.to.GetThumbnailImage(false);
            if (this.prevToPageImage  != img2)
            {
                this.prevToPageImage = img2;
                this.alphaBlendMat.SetTexture("_MainTex2", img2);
            }

            this.alphaBlendMat.SetFloat("_TransitionValue", transition.amt);
        }

    }
}