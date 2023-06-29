using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils.Web
{
    public class VMailWebSharer : MonoBehaviour
    {
        [SerializeField]
        private VMailWebManager manager;

        [SerializeField]
        private TMP_InputField dirUrlText;
        [SerializeField]
        private Button dirCopyBtn;

        [SerializeField]
        private TMP_InputField videoUrlText;
        [SerializeField]
        private Button videoCopyBtn;

        [Space(10)]
        [SerializeField]
        private VMail.Utils.Android.AndroidShare androidSharer;

        private void OnEnable()
        {
            this.dirUrlText.text = this.manager.currVMailData == null ?
                "Please open and edit a message first." : this.manager.currVMailData.GetDirectoryURL();
            this.videoUrlText.text = this.manager.currVMailData == null ?
                "Please open and edit a message first." : this.manager.currVMailData.GetVideoURL();

            this.dirCopyBtn.interactable = this.manager.currVMailData != null;
            this.videoCopyBtn.interactable = this.manager.currVMailData != null;
        }

        public void CopyDirectoryURL()
        {
            GUIUtility.systemCopyBuffer = this.dirUrlText.text;
        }

        public void CopyVideoURL()
        {
            GUIUtility.systemCopyBuffer = this.videoUrlText.text;
        }

        public void ShareDirectoryURL()
        {
            if (this.androidSharer != null && this.manager.currVMailData != null)
            {
                string subject = this.manager.currVMailData.name;
                string message = this.dirUrlText.text;
                this.androidSharer.ShareText(subject, message);
            }
        }

    }
}