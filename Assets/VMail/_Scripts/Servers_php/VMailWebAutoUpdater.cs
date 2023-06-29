using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Utils.Web
{
    public class VMailWebAutoUpdater : MonoBehaviour
    {
        public enum Mode { Idle, Updating }

        [SerializeField]
        private ManagerDesktop desktop;
        [SerializeField]
        private VMailWebManager manager;
        [SerializeField]
        private VMailWebUploader uploader;
        [SerializeField]
        private WebIntegration webIntegration;
        [SerializeField]
        private PagePreviewGenerator pagePreviewGenerator; // this is called after downloading and opening a message
        [SerializeField]
        private Text textDebug;

        [Space(10)]
        [SerializeField]
        private float checkInTime = 1 * 60; // in seconds

        private Mode currMode = Mode.Idle;
        private float sinceLastUpdate;
        private List<int> updatedMessages = new List<int>();

        private bool updated = false;
        private bool updateRightAway = false;


        private void Start()
        {
            //this.manager.Init();
            this.manager.onRefreshed.AddListener(this.OnRefreshed);
            this.pagePreviewGenerator.onCompleted.AddListener(this.OnOpened);
            this.webIntegration.onUpdatedVMailsTable.AddListener(this.OnRefreshed);
        }

        private void Update()
        {
            if (this.currMode == Mode.Idle)
            {
                this.sinceLastUpdate += Time.deltaTime;

                if (this.updateRightAway || this.sinceLastUpdate > this.checkInTime)
                {
                    this.StartUpdateSequence();
                    this.updateRightAway = false;
                }
            }
            else if (this.currMode == Mode.Updating)
            {
                // nothing to do
            }

            if (this.textDebug != null)
            {
                string str = this.currMode.ToString();
                if (this.currMode == Mode.Idle)
                {
                    str += "\n" + this.sinceLastUpdate.ToString("f1") + " " + this.checkInTime.ToString("f1");
                }
                this.textDebug.text = str;
            }
        }

        public void SetCheckInTime(float time)
        {
            if (time > 0)
            {
                this.checkInTime = time;
            }
        }

        public void UpdateNow()
        {
            this.updateRightAway = true;
        }

        public void StartUpdateSequence()
        {
            this.currMode = Mode.Updating;

            this.manager.RefreshAvailableVMails();
        }

        public void OnRefreshed(int updatedVMail)
        {
            Debug.Log("Updater: updated the vmail... " + updatedVMail);

            this.OnRefreshed();
        }

        public void OnRefreshed()
        {
            VMailWeb msg = this.GetMessageToUpdate();

            // all the new messages are updated to date.
            if (msg == null)
            {
                this.OnDoneUpdating();
            }
            // still there are messages to update
            else
            {
                Debug.Log("Updater: downloading and opening... " + msg.vMailData.ID);
                this.manager.TryDownloadMessage(msg);
                this.updatedMessages.Add(msg.vMailData.ID);
                this.updated = true;
            }
        }

        public void OnOpened()
        {
            // start the upload
            this.uploader.SetUploadMode(true);
            this.uploader.CreateOrUpload();

            Debug.Log("Updater: uploading... " + this.manager.currVMailData.ID);
        }

        private void OnDoneUpdating()
        {
            this.currMode = Mode.Idle;
            this.sinceLastUpdate = 0f;
            this.updatedMessages.Clear();

            if (this.updated)
            {
                this.desktop.RemoveAllPages();
                this.manager.ResetCurrentMessage();
                this.desktop.ClearVisState();
            }
            this.updated = false;

            Debug.Log("Updater: finished updating... ");
        }

        private VMailWeb GetMessageToUpdate()
        {
            foreach (VMailWeb msg in this.manager.vMailWebs)
            {
                // skip if modified by the desktop or the server the latest
                if (msg.vMailData.lastModifiedMobile <= msg.vMailData.lastModifiedDesktop ||
                    msg.vMailData.lastModifiedMobile <= msg.vMailData.lastModifiedServer)
                {
                    continue;
                }

                if (!this.updatedMessages.Contains(msg.vMailData.ID))
                {
                    return msg;
                }
            }
            return null;
        }

    }
}
