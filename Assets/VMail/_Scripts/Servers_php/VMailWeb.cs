using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VMail.Utils.MessageFile;

namespace VMail.Utils.Web
{
    public class VMailWeb : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text description;
        [SerializeField]
        private TMP_Text dirUrlText;
        [SerializeField]
        private TMP_Text videoUrlText;

        public VMailData vMailData { get; private set; }

        [Space(10)]
        [SerializeField]
        private VMail.Utils.Android.AndroidShare androidSharer;

        public void Init(VMailData vMailData)
        {
            this.vMailData = vMailData;

            this.description.text = vMailData.name + ", " + vMailData.GetLatestModified().ToString("yyyy-MM-dd HH:mm:ss");
            this.dirUrlText.text = VMailWebManager.ServerDir + vMailData.ID;
            this.videoUrlText.text = VMailWebManager.ServerDir + vMailData.ID + "/" + VideoFileGenerator.VideoFileName;
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
            if (this.androidSharer != null && this.vMailData != null)
            {
                string subject = this.vMailData.name;
                string message = this.dirUrlText.text;
                this.androidSharer.ShareText(subject, message);
            }
        }

    }
}