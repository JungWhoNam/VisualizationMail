using UnityEngine;
using UnityEngine.UI;

using VMail.Utils;

namespace VMail.Viewer.Baked
{
    public class ViewerSphericalImage : MonoBehaviour, IViewer
    {
        [SerializeField]
        private CamMoveAroundOrigin nav;
        [SerializeField]
        private Toggle btn;

        private Page currPage;

        public void SetState(Page page)
        {
            if (page == null)
            {
                this.gameObject.SetActive(false);
            }
            else if (page != this.currPage) // update the texture
            {
                this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", page.GetThumbnailImage(true));

                this.transform.rotation = Quaternion.Inverse(page.pageInfo.viewInfo.rot);

                if (this.nav != null)
                {
                    this.nav.ResetState();
                }
            }

            if (this.btn != null)
            {
                this.btn.interactable = page != null;
            }

            this.currPage = page;
        }

        public void OpenMessage(Message message)
        {
            // nothing to do
        }

        public void SetState(Transition t)
        {
            // nothing to do
        }

        public void ToggleOnOff()
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }

    }
}