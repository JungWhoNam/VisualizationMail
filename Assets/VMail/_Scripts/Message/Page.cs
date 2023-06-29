using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using VMail.Utils;

namespace VMail
{
    public class Page : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public PageInfo pageInfo; // { get; private set; }

        [SerializeField]
        private Image viewImage;
        [SerializeField]
        private Image viewImageAnn;
        public Texture2D img; // { get; private set; }
        private Texture2D img360;
        public Texture2D imgToNext; // { get; private set; }
        public Texture2D annTex { get; private set; }

        [Space(10)]
        [SerializeField]
        private RectTransform canvasRT;
        [SerializeField]
        private Message message;
        [SerializeField]
        private Button closeButton;
        [SerializeField]
        private Color selectedColor = Color.magenta;

        [Space(10)]
        [SerializeField]
        public UnityEngine.Events.UnityEvent onEndDragEvent;

        private Color initColor;
        private Vector3 initPos;
        private Vector3 initOffset;
        private bool isValidDrag = false;
        private bool isEditMode = true;


        private void Awake()
        {
            // update the size
            Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
            this.GetComponent<RectTransform>().sizeDelta = new Vector2((int)(Screen.width * size.y / (Screen.height * 0.7407f)), size.y);
        }

        public void Initialize(PageInfo pageInfo, Texture2D thumbnail, Texture2D img360, Texture2D annTex)
        {
            this.pageInfo = pageInfo;
            this.initColor = this.GetComponent<Image>().color;
            this.SetView(pageInfo.viewInfo);
            this.SetThumbnail(false, thumbnail);
            this.SetThumbnail(true, img360);
            this.annTex = annTex;

            if (this.viewImageAnn != null)
            {
                Sprite sprite = Sprite.Create(annTex, new Rect(0.0f, 0.0f, annTex.width, annTex.height), new Vector2(0.5f, 0.5f), 100);
                this.viewImageAnn.sprite = sprite;
            }
        }

        public void SetMode(bool editMode)
        {
            if (this.closeButton != null)
            {
                this.closeButton.gameObject.SetActive(editMode);
            }
            this.isEditMode = editMode;
        }

        public Texture2D GetThumbnailImage(bool get360)
        {
            return get360 ? this.img360 : this.img;
        }

        public void SetImageToNextPage(Texture2D tex)
        {
            this.imgToNext = tex;
        }

        public Texture2D GetImageToNextPage()
        {
            return this.imgToNext;
        }

        public void SetSelected(bool selected)
        {
            this.GetComponent<Image>().color = selected ? this.selectedColor : this.initColor;
        }

        private void SetView(ViewInfo viewInfo)
        {
            this.pageInfo.viewInfo = viewInfo;
        }
        
        public void SetThumbnail(bool is360, Texture2D tex)
        {
            if (tex == null)
            {
                return;
            }
            if (!is360)
            {
                this.img = tex;
                if (this.viewImage != null)
                {
                    Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100);
                    this.viewImage.sprite = sprite;
                }
            }
            else
            {
                this.img360 = tex;
            }
        }

        //
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!this.isEditMode)
            {
                return;
            }

            // initialize variables
            this.isValidDrag = false;
            this.initPos = this.GetComponent<RectTransform>().position;
            this.initOffset = this.GetGlobalMousePos(eventData) - this.initPos;
        }

        private Vector3 GetGlobalMousePos(PointerEventData eventData)
        {
            Vector3 globalMousePos;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(this.canvasRT, eventData.position, eventData.pressEventCamera, out globalMousePos))
            {
                // TODO handle the case where it returns false.
            }
            return globalMousePos;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!this.isEditMode)
            {
                return;
            }

            RectTransform rt = this.GetComponent<RectTransform>();
            rt.position = this.GetGlobalMousePos(eventData) - this.initOffset;

            Message msg = Tools.FindInParents<Message>(eventData.pointerCurrentRaycast.gameObject);
            if (msg == null)
            {
                this.isValidDrag = false;
            }
            else
            {
                if (this.message == msg) // move within the message
                {
                    this.isValidDrag = true;
                }
                else if (this.message != msg) // moved to another message
                {
                    Debug.Log("moving to another message");
                    this.isValidDrag = true;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!this.isEditMode)
            {
                return;
            }

            if (!this.isValidDrag) // if it wasn't a succesfull drag, bring it back to its original place. 
            {
                // TODO
            }

            this.message.UpdateOrderOfPages();

            this.message.RefreshLayout();
            // on click it has the same effect as clikcing on the thumbnail view
            this.onEndDragEvent.Invoke();
        }

    }

    [System.Serializable]
    public class PageInfo
    {
        public ViewInfo viewInfo;

        public ImageThumbnail img;
        public ImageThumbnail img360;
        public ImageThumbnail imgToNext;
        public VideoThumbnail vid;
        public AnnotationThumbnail ann;

        public List<Comment> comments;

        public PageInfo()
        {
            this.viewInfo = new ViewInfo();

            this.img = new ImageThumbnail(false);
            this.img360 = new ImageThumbnail(true);
            this.imgToNext = new ImageThumbnail(false);
            this.vid = new VideoThumbnail();
            this.ann = new AnnotationThumbnail();

            this.comments = new List<Comment>();
        }
    }

    [System.Serializable]
    public class Comment
    {
        public string author = "Bob";
        public string comment;
    }

    [System.Serializable]
    public class ImageThumbnail
    {
        public string fPath;
        public bool is360;

        public ImageThumbnail(bool is360)
        {
            this.fPath = string.Empty;
            this.is360 = is360;
        }
    }

    [System.Serializable]
    public class AnnotationThumbnail
    {
        public string imgFilePath;

        public AnnotationThumbnail()
        {
            this.imgFilePath = string.Empty;
        }
    }

    [System.Serializable]
    public class VideoThumbnail
    {
        public float from; // in ms
        public float to; // in ms

        public VideoThumbnail()
        {
            this.from = -1f;
            this.to = -1f;
        }
    }

    [System.Serializable]
    public class ViewInfo
    {
        public Vector3 pos;
        public Quaternion rot;
        public string json;

        public ViewInfo()
        {
        }
    }

}