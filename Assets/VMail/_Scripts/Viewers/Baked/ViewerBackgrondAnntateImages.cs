using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using VMail.Viewer.Social;

namespace VMail.Viewer.Baked
{
    public class ViewerBackgrondAnntateImages : MonoBehaviour, IViewer
    {
        [SerializeField]
        private RawImage bgImage;
        [SerializeField]
        private AnnotationRawImage annImage;
        [SerializeField]
        private BrushPallette toolBoxUI;
        [SerializeField]
        private TMPro.TMP_InputField commentInputField;

        private Page currPage;


        public void SetState(Page page)
        {
            this.bgImage.gameObject.SetActive(page != null);
            this.annImage.gameObject.SetActive(page != null);
            this.toolBoxUI.SetInteractable(page != null);
            this.commentInputField.interactable = page != null;

            if (page != null && page != this.currPage)
            {
                Texture2D tex = page.GetThumbnailImage(false);
                this.bgImage.texture = tex;
                this.bgImage.GetComponent<AspectRatioFitter>().aspectRatio = (float)tex.width / tex.height;

                this.annImage.SetTexture(page.annTex);
            }

            this.currPage = page;
        }

        public void SetInteractable(bool interactable)
        {
            this.annImage.SetInteractable(interactable);
            this.toolBoxUI.gameObject.SetActive(interactable);
            this.commentInputField.gameObject.SetActive(interactable);
        }

        public void Clear()
        {
            this.annImage.Clear();
        }

        public void OpenMessage(Message message)
        {
            // nothing to do 
        }

        public void SetState(Transition t)
        {
            // nothing to do
        }

    }
}