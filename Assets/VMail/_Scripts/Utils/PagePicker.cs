using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace VMail.Utils
{
    public class PagePicker : MonoBehaviour
    {
        [SerializeField]
        private Button from;
        [SerializeField]
        private Button to;

        [Space(10)]
        [SerializeField]
        private ManagerDesktop managerDesktop;

        [Space(10)]
        [SerializeField]
        private UnityEvent onClick;

        public void PickPage(Page from, Page to)
        {
            this.gameObject.SetActive(true);

            {
                // set the button image
                Texture2D tex = from.GetThumbnailImage(false);
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                this.from.image.sprite = sprite;

                // update the ui size
                Vector2 uiSize = this.from.gameObject.GetComponent<RectTransform>().sizeDelta;
                int h = (int)(uiSize.x * tex.height / tex.width);
                uiSize.y = h;
                this.from.gameObject.GetComponent<RectTransform>().sizeDelta = uiSize;

                // add the on click events
                this.from.onClick.AddListener(delegate { this.managerDesktop.UpdateState(from); });
                this.from.onClick.AddListener(delegate { this.gameObject.SetActive(false); });
            }

            {
                // set the button image
                Texture2D tex = to.GetThumbnailImage(false);
                Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                this.to.image.sprite = sprite;

                // update the ui size
                Vector2 uiSize = this.to.gameObject.GetComponent<RectTransform>().sizeDelta;
                int h = (int)(uiSize.x * tex.height / tex.width);
                uiSize.y = h;
                this.to.gameObject.GetComponent<RectTransform>().sizeDelta = uiSize;

                // add the on click events
                this.to.onClick.AddListener(delegate { this.managerDesktop.UpdateState(to); });
                this.to.onClick.AddListener(delegate { this.gameObject.SetActive(false); });

                this.onClick.Invoke();
            }
        }

    }
}