using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace VMail.Viewer.Social
{
    public class ViewerComment : MonoBehaviour, IViewer
    {
        public static string Author = "Bob";
        public static float Threshold = 0.00025f;

        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private TMP_Text outputField;
        [SerializeField]
        private Scrollbar scrollbar;
        [SerializeField]
        private Text onOffText;

        private PageInfo currPageInfo = null;

        void OnEnable()
        {
            this.inputField.onSubmit.AddListener(AddComment);
            if (this.onOffText != null)
            {
                this.onOffText.text = ">>";
            }
        }

        void OnDisable()
        {
            this.inputField.onSubmit.RemoveListener(AddComment);
            if (this.onOffText != null)
            {
                this.onOffText.text = "<<";
            }
        }

        // ============================================= INTERFACE =============================================

        public void OpenMessage(Message message)
        {
            // nothing to do
        }

        public void SetState(Page page)
        {
            this.currPageInfo = null;
            if (page != null)
            {
                this.currPageInfo = page.pageInfo;
            }
            this.UpdateState();
        }

        public void SetState(Transition transition)
        {
            this.currPageInfo = null;

            if (transition != null)
            {
                if (transition.amt <= ViewerComment.Threshold)
                {
                    this.currPageInfo = transition.from == null ? null : transition.from.pageInfo;
                }
                else if (transition.amt >= (1f - ViewerComment.Threshold))
                {
                    this.currPageInfo = transition.to == null ? null : transition.to.pageInfo;
                }
            }

            this.UpdateState();
        }

        // ==========================================================================================

        public void ToggleOnOff()
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }

        public void SetInputFieldVisiblity(bool v)
        {
            this.inputField.gameObject.SetActive(v);
        }

        public void AddComment(string comment)
        {
            if (this.currPageInfo == null)
            {
                Debug.LogWarning("the current page is null.");
                return;
            }
            if (string.IsNullOrEmpty(comment))
            {
                Debug.LogWarning("entered an empty string.");
                return;
            }

            // adds a new comment
            Comment c = new Comment()
            {
                author = ViewerComment.Author,
                comment = comment
            };
            this.currPageInfo.comments.Add(c);

            // update the state
            this.UpdateState();

            Utils.CloseWithoutSaveWarning.closeWithoutWarning = false;
        }

        public void Clear()
        {
            this.currPageInfo = null;
            this.inputField.text = string.Empty;
            this.outputField.text = "";
            this.scrollbar.value = 0; // set the scrollbar to the bottom when next text is submitted.
            this.inputField.interactable = false;
        }

        private void UpdateState()
        {
            this.inputField.text = string.Empty;
            this.outputField.text = "";
            if (this.currPageInfo != null)
            {
                foreach (Comment c in this.currPageInfo.comments)
                {
                    this.outputField.text += "<#08003D>[" + c.author + "]</color> " + c.comment + "\n";
                }
            }

#if UNITY_STANDALONE_WIN || UNITY_WEBGL
            this.inputField.ActivateInputField();
#endif

            this.scrollbar.value = 0; // set the scrollbar to the bottom when next text is submitted.

            this.inputField.interactable = (this.currPageInfo != null);
        }

    }
}