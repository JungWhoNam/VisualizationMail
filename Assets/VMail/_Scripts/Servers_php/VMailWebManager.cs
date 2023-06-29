using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VMail.Utils.MessageFile;

namespace VMail.Utils.Web
{
    public class VMailWebManager : MonoBehaviour
    {
        //Make sure to put "/" at the end.
        public static readonly string ServerDir = "https://jungwhonam.com/VMails_Codes/data/";
        //public static readonly string ServerDir = "http://sculptingvis.tacc.utexas.edu/php/VMails_Codes/data/";

        //public static readonly string rootDirNameServer = "../vmails";
        public static readonly string rootDirNameServer = "data";
        public static readonly string rootDirNameLocal = "vmails";


        [SerializeField]
        private WebIntegration webIntegration;
        [SerializeField]
        private ManagerDesktop managerDesktop;
        [SerializeField]
        private ManagerMobile managerMobile;
        [SerializeField]
        private VMailWebUploader uploader;
        [SerializeField]
        private bool desktopMode = true;

        [Space(10)]
        [SerializeField]
        private GameObject container;
        [SerializeField]
        private VMailWeb baseVMailWeb;

        [Space(10)]
        [SerializeField]
        private Button saveBtn;
        [SerializeField]
        private Button saveAsBtn;

        public List<VMailWeb> vMailWebs = new List<VMailWeb>();
        public VMailData currVMailData { get; private set; }
        private VMailData prevVMailData = null;

        [Space(10)]
        public UnityEvent onRefreshed;

        private void OnEnable()
        {
            this.RefreshAvailableVMails();
        }

        public void SaveCurrentMessage(bool overwrite)
        {
            if (this.currVMailData == null)
            {
                this.uploader.SetUploadMode(false);
            }
            else
            {
                this.uploader.SetUploadMode(overwrite);
            }

            this.uploader.gameObject.SetActive(true);
        }

        public void SetCurrentMessage(VMailData data)
        {
            this.currVMailData = data;

            this.saveBtn.interactable = true;
            if (this.desktopMode)
            {
                this.saveAsBtn.interactable = this.currVMailData != null;
            }
        }

        public void ResetCurrentMessage()
        {
            this.SetCurrentMessage(null);
        }

        public void RefreshAvailableVMails()
        {
            StartCoroutine(this.Refresh());
        }

        private IEnumerator Refresh()
        {
            // clear the existing vmails
            foreach (VMailWeb vMailWeb in this.vMailWebs)
            {
                GameObject.Destroy(vMailWeb.gameObject);
            }
            this.vMailWebs.Clear();

            // populate with the new ones
            bool isDone = false;
            System.Action<List<VMailData>> getVMails = (datas) =>
            {
                isDone = true;

                foreach (VMailData data in datas)
                {
                    VMailWeb vMailWeb = GameObject.Instantiate<VMailWeb>(this.baseVMailWeb);
                    vMailWeb.Init(data);
                    vMailWeb.transform.SetParent(this.container.transform, false);
                    vMailWeb.gameObject.name = "vmail - " + data.name;
                    vMailWeb.gameObject.SetActive(true);
                    this.vMailWebs.Add(vMailWeb);
                }
            };
            StartCoroutine(this.webIntegration.GetVMails(getVMails));

            // wait until the callback is called
            yield return new WaitUntil(() => isDone == true);

            this.onRefreshed.Invoke();
        }

        public void FilterMessages(string filter)
        {
            string input = filter.Trim();

            foreach (VMailWeb vMailWeb in this.vMailWebs)
            {
                if (input.Trim() == "")
                {
                    vMailWeb.gameObject.SetActive(true);
                }
                else // check for matches
                {
                    if (vMailWeb.vMailData.name.IndexOf(input, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vMailWeb.gameObject.SetActive(true);
                    }
                    else if (vMailWeb.vMailData.GetVideoURL().IndexOf(input, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vMailWeb.gameObject.SetActive(true);
                    }
                    else if (vMailWeb.vMailData.GetDirectoryURL().IndexOf(input, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vMailWeb.gameObject.SetActive(true);
                    }
                    else
                    {
                        vMailWeb.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void TryDownloadMessage(VMailWeb vMailWeb)
        {
            string rootDirNameServer = VMailWebManager.rootDirNameServer;
            string rootDirNameLocal = VMailWebManager.rootDirNameLocal;
            string vmailID = vMailWeb.vMailData.ID.ToString();
            string thumbnailDirName = ThumbnailFileGenerator.ThumbnailDirName;

            StartCoroutine(this.webIntegration.DownloadVMail(rootDirNameServer, rootDirNameLocal, vmailID, thumbnailDirName));

            this.prevVMailData = this.currVMailData;
            this.SetCurrentMessage(vMailWeb.vMailData);
        }

        public void OpenFromURL(string openingURL)
        {
            Debug.Log("Openning from an URL: " + openingURL);
            // vmail://https://jungwhonam.com/VMails_Codes/data/23/

            // check if there is a match
            foreach (VMailWeb vMailWeb in this.vMailWebs)
            {
                string url = "vmail://" + vMailWeb.vMailData.GetDirectoryURL();
                if (openingURL.Equals(url))
                {
                    this.TryDownloadMessage(vMailWeb);
                }
            }
        }

        public void OpenVMail(string dirPathInLocal)
        {
            if (string.IsNullOrEmpty(dirPathInLocal)) // failed to download the vmail web
            {
                Debug.LogWarning("Failed to download... " + this.currVMailData.ToString());
                this.SetCurrentMessage(this.prevVMailData);
            }
            else if (Directory.Exists(dirPathInLocal))
            {
                Debug.Log("Opening... " + dirPathInLocal);
                if (this.desktopMode)
                {
                    this.managerDesktop.OpenFrom(dirPathInLocal);
                }
                else
                {
                    this.managerMobile.Open(dirPathInLocal);
                }
            }
        }



        public void SaveVMail()
        {
            if (this.currVMailData == null)
            {
                return;
            }

            string dirPath = Path.Combine(Application.persistentDataPath, VMailWebManager.rootDirNameLocal);
            string msgDirInLocal = Path.Combine(dirPath, this.currVMailData.ID.ToString());

            if (this.desktopMode)
            {
                // create the root folder and the message folder in the local drive
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                // update an existing one
                // delete the old message folder
                if (Directory.Exists(msgDirInLocal))
                {
                    Directory.Delete(msgDirInLocal, true);
                }
                // create the folder
                Directory.CreateDirectory(msgDirInLocal);

                // save to the folder
                this.managerDesktop.SaveTo(msgDirInLocal);

                // 3) upload the message to the Google Drive.
                // UploadVMail() will be called after saving the message folder (see MessageFileGenerator.cs).

            }
            else
            {
                this.managerMobile.SaveTo(msgDirInLocal);
                // UploadVMail() will be called after saving the message folder (see MessageFileGenerator.cs).
            }
        }

        public void StartUpdateSequence()
        {
            this.SaveVMail();
            // public void UploadVMail()
            // public void UpdateVMailTable()
            // setactive(false)
        }

        public void StartCreateSequence(string name)
        {
            StartCoroutine(this.webIntegration.CreateVMail(name));
            // public void SetCurrentVMailAndUpdate(int id, string name)
            // public void StartUpdateSequence()
        }

        public void UploadVMail()
        {
            if (this.currVMailData == null)
            {
                return;
            }

            string rootDirNameServer = VMailWebManager.rootDirNameServer;
            string rootDirNameLocal = VMailWebManager.rootDirNameLocal;
            string vmailID = this.currVMailData.ID.ToString();
            string thumbnailDirName = ThumbnailFileGenerator.ThumbnailDirName;

            StartCoroutine(this.webIntegration.UploadVMail(rootDirNameServer, rootDirNameLocal, vmailID, thumbnailDirName, this.desktopMode));
        }

        public void UpdateVMailTable(string dirPathInLocal)
        {
            if (string.IsNullOrEmpty(dirPathInLocal))
            {
                Debug.LogWarning("failed to upload the folder.");
                return;
            }

            if (this.currVMailData == null)
            {
                Debug.LogWarning("currVMailData is null.");
                return;
            }

            if (this.desktopMode)
            {
                this.currVMailData.lastModifiedDesktop = DateTime.UtcNow;
            }
            else
            {
                this.currVMailData.lastModifiedMobile = DateTime.UtcNow;
            }

            StartCoroutine(this.webIntegration.UpdateVMail(this.currVMailData));
        }

        public void SetCurrentVMailAndUpdate(int id, string name)
        {
            if (id <= 0)
            {
                return;
            }

            this.SetCurrentMessage(new VMailData(id, name));

            this.StartUpdateSequence();
        }

        public void SavePreviousVMailData()
        {
            if (this.currVMailData == null)
            {
                return;
            }

            StartCoroutine(this.webIntegration.CopyExistingVMailData(this.currVMailData));
        }

    }
}