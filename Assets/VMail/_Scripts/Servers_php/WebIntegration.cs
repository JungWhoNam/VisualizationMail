using SimpleJSON;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using VMail.Utils.Web;
using System.Collections.Generic;
using System.Linq;

namespace VMail.Utils.Web
{
    public class WebIntegration : MonoBehaviour
    {
        //Make sure to put "/" at the end.
        public static readonly string CodeDirURL = "http://localhost/";

        [SerializeField]
        private ProgressBar progressBar;

        public MyEventIntString onInsertedVMailsTable;
        public MyEventInt onUpdatedVMailsTable;
        public MyEventString onUploadedCompleted;
        public MyEventString onDownloadedCompleted;
        public MyEventInt onCopiedCompleted;

        void Start()
        {
            //StartCoroutine(GetVMails());


            // -------------------------------------- download test --------------------------------------

            /*{ // !!! DownloadFile a file
                string filePathInLocal = Path.Combine(Application.persistentDataPath, "vmails", "2shots", "Thumbnails", "video.mp4");
                string filePathInServer = "../vmails/7shots/Thumbnails/video.mp4";

                StartCoroutine(DownloadFile(filePathInServer, filePathInLocal));
            }*/

            /*{ // !!! Download files in the direcrory
                string dirPathInLocal = Path.Combine(Application.persistentDataPath, "vmails", "2shots");
                string dirPathInServer = "../vmails/2shots";

                Action<int> successCallback = (cnt) => { 
                    Debug.Log("- downloaded " + cnt + " file(s).");
                };
                StartCoroutine(DownloadFilesInDir(dirPathInServer, dirPathInLocal, successCallback));
            }*/

            /*{ // !!! Download a vmail
                string rootDirNameServer = "../vmails";
                string rootDirNameLocal = "vmails";
                string vmailID = "2shots";
                string thumbnailDirName = "Thumbnails";

                StartCoroutine(DownloadVMail(rootDirNameServer, rootDirNameLocal, vmailID, thumbnailDirName));
            }*/


            // -------------------------------------- upload test --------------------------------------

            /*{ // !!! upload a file (create folders in the process)
                string filePathInLocal = Path.Combine(Application.persistentDataPath, "vmails", "2shots", "video.mp4");
                string dirPathInServer ="../vmails/7shots/Thumbnails";

                Action<bool> successCallback = (success) => { };
                StartCoroutine(UploadFile(dirPathInServer, filePathInLocal, successCallback));
            }*/

            /*{ // !!! upload files(s) in the directory (create folders in the process)
                string dirPathInLocal = Path.Combine(Application.persistentDataPath, "vmails", "2shots", "Thumbnails");
                string dirPathInServer = "../vmails/5shots/Thumbnails";

                Action<int> uploadCallback = (cnt) =>
                {
                    Debug.Log("- uploaded " + cnt + " file(s).");
                };
                StartCoroutine(UploadFilesInDir(dirPathInServer, dirPathInLocal, uploadCallback));
            }*/

            /*{ // !!! upload a vmail
                string rootDirNameServer = "../vmails";
                string rootDirNameLocal = "vmails";
                string vmailID = "3shots";
                string thumbnailDirName = "Thumbnails";

                StartCoroutine(UploadVMail(rootDirNameServer, rootDirNameLocal, vmailID, thumbnailDirName));
            }*/

            /*{ // !!! insert into the vmails table
                StartCoroutine(CreateVMail("testshots"));
            }*/

            /*{ // !!! update the vmails table
                VMailData vMailData = new VMailData(1, "2shots");

                StartCoroutine(UpdateVMail(vMailData));
            }*/

            /*{ // !!! copy an existing vmail data
                VMailData vMailData = new VMailData(1, "2shots");

                StartCoroutine(CopyExistingVMailData(vMailData));
            }*/
        }

        public void OpenVMailDummy(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                Debug.LogWarning("errors downloading the directory.");
            }
            else
            {
                Debug.Log("Opening a vmail from the local drive..." + dirPath);
            }
        }

        public void PrintVMailUploadMessage(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath))
            {
                Debug.LogWarning("errors uploading the directory.");
            }
            else
            {
                Debug.Log("Uploaded a vmail from the local drive..." + dirPath);
            }
        }

        public void PrintUpdatedVMail(int id)
        {
            if (id < 0)
            {
                Debug.LogWarning("errors updating the vmails table.");
            }
            else
            {
                Debug.Log("Updated the vmails table... " + id);

                //StartCoroutine(GetVMails());
            }
        }

        public IEnumerator GetVMails(System.Action<List<VMailData>> callback)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(WebIntegration.CodeDirURL + "GetVMails.php"))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // show results as text
                    Debug.Log(www.downloadHandler.text);

                    JSONArray jsonArray = JSON.Parse(www.downloadHandler.text) as JSONArray;

                    List<VMailData> vmails = new List<VMailData>();
                    if (jsonArray != null)
                    {
                        for (int i = 0; i < jsonArray.Count; i++)
                        {
                            VMailData vMailData = new VMailData(
                                jsonArray[i].AsObject["ID"],
                                jsonArray[i].AsObject["name"],
                                jsonArray[i].AsObject["lastModifiedDesktop"],
                                jsonArray[i].AsObject["lastModifiedMobile"],
                                jsonArray[i].AsObject["lastModifiedServer"]
                                );

                            vmails.Add(vMailData);
                        }
                    }

                    callback(vmails);
                }
            }
        }

        // ASSUMES there is "Thumbnails" sub-directory and there is at least a file in each directory (the vmail dir and its "Thumbnails" dir). 
        public IEnumerator DownloadVMail(string rootDirNameServer, string rootDirNameLocal, string vmailID, string thumbnailDirName)
        {
            string dirPathInLocal = Path.Combine(Application.persistentDataPath, rootDirNameLocal, vmailID);
            string subDirPathInLocal = Path.Combine(dirPathInLocal, thumbnailDirName);
            string dirPathInServer = rootDirNameServer + "/" + vmailID;
            string subDirPathInServer = dirPathInServer + "/" + thumbnailDirName;

            /// 0: delete and create, 1: download one, 2: download two, 3: finish
            if (this.progressBar != null)
            {
                this.progressBar.Initialize(3, "Opening the VMail.. ", "Deleting the old folder...");
            }

            // 1) delete the old vmail directory, if exists, and create new one and its "Thumbnails" directory (in the local drive)
            if (Directory.Exists(dirPathInLocal))
            {
                Directory.Delete(dirPathInLocal, true);
            }
            // create the directories
            Directory.CreateDirectory(dirPathInLocal);
            Directory.CreateDirectory(subDirPathInLocal);

            // 2) download the files in the vmail folder
            /// 0: delete and create, 1: download one, 2: download two, 3: finish
            if (this.progressBar != null)
            {
                this.progressBar.IncreaseCnt("Downloading the folder...");
            }
            ///
            bool isDoneDownloadingDir = false;
            int downloadedCntTotalDir = 0;
            Action<int> getDirCallback = (downloadedCnt) =>
            {
                isDoneDownloadingDir = true;
                downloadedCntTotalDir = downloadedCnt;
                Debug.Log("dir... downloaded.. " + downloadedCnt + " file(s)");
            };
            StartCoroutine(DownloadFilesInDir(dirPathInServer, dirPathInLocal, getDirCallback));

            // wait until the callback is called
            yield return new WaitUntil(() => isDoneDownloadingDir == true);

            // check if there were any file donwloaded (if not indicate the error)
            if (downloadedCntTotalDir <= 0)
            {
                /// 0: delete and create, 1: download one, 2: download two, 3: finish
                if (this.progressBar != null)
                {
                    this.progressBar.Finish("Failed...");
                    this.progressBar.Close();
                }
                ///
                Debug.LogWarning("- no files downloaded...");
                this.onDownloadedCompleted.Invoke(null);
            }
            else
            {
                // 3) download the files in its "Thumbnails" folder
                /// 0: delete and create, 1: download one, 2: download two, 3: finish
                if (this.progressBar != null)
                {
                    this.progressBar.IncreaseCnt("Downloading the Thumbnails folder...");
                }
                ///
                bool isDoneDownloadingSubDir = false;
                int downloadedCntTotalSubDir = 0;
                Action<int> getSubDirCallback = (downloadedCnt) =>
                {
                    isDoneDownloadingSubDir = true;
                    downloadedCntTotalSubDir = downloadedCnt;
                    Debug.Log("subDir... downloaded.. " + downloadedCnt + " file(s)");
                };
                StartCoroutine(DownloadFilesInDir(subDirPathInServer, subDirPathInLocal, getSubDirCallback));

                // wait until the callback is called
                yield return new WaitUntil(() => isDoneDownloadingSubDir == true);

                // check if there were any file donwloaded (if not indicate the error)
                if (downloadedCntTotalSubDir <= 0)
                {
                    /// 0: delete and create, 1: download one, 2: download two, 3: finish
                    if (this.progressBar != null)
                    {
                        this.progressBar.Finish("Failed...");
                        this.progressBar.Close();
                    }
                    ///
                    Debug.LogWarning("- no files downloaded...");
                    this.onDownloadedCompleted.Invoke(null);
                }
                else
                {
                    /// 0: delete and create, 1: download one, 2: download two, 3: finish
                    if (this.progressBar != null)
                    {
                        this.progressBar.Finish("Donwloaded the folder...");
                        this.progressBar.Close();
                    }
                    // on finished downloading all the files
                    this.onDownloadedCompleted.Invoke(dirPathInLocal);
                }
            }
        }

        // assume the vmail and its "Thumbnails" folder are already existed in the server
        public IEnumerator UploadVMail(string rootDirNameServer, string rootDirNameLocal, string vmailID, string thumbnailDirName, bool desktopMode = true)
        {
            string dirPathInLocal = Path.Combine(Application.persistentDataPath, rootDirNameLocal, vmailID);
            string subDirPathInLocal = Path.Combine(dirPathInLocal, thumbnailDirName);
            string dirPathInServer = rootDirNameServer + "/" + vmailID;
            string subDirPathInServer = dirPathInServer + "/" + thumbnailDirName;

            // 1) upload the files in the vmail folder
            /// 0: upload one, 1: upload two, 3: finish
            if (this.progressBar != null)
            {
                this.progressBar.Initialize(3, "Uploading the VMail.. ", "Uploading the folder...");
            }
            ///
            bool isDoneUploadingDir = false;
            int uploadedCntTotalDir = 0;
            Action<int> uploadDirCallback = (uploadedCnt) =>
            {
                isDoneUploadingDir = true;
                uploadedCntTotalDir += uploadedCnt;
                Debug.Log("dir... uploaded.. " + uploadedCnt + " file(s)");
            };
            if (desktopMode)
            {
                StartCoroutine(UploadFilesInDir(dirPathInServer, dirPathInLocal, uploadDirCallback));
            }
            else
            {
                //StartCoroutine(UploadFilesInDir(dirPathInServer, dirPathInLocal, uploadDirCallback, "msg.json"));
                StartCoroutine(UploadFilesInDir(dirPathInServer, dirPathInLocal, uploadDirCallback, new string[] { ".vtt", ".json" }));
            }
            

            // wait until the callback is called
            yield return new WaitUntil(() => isDoneUploadingDir == true);


            // check if there were any file donwloaded (if not indicate the error)
            if (uploadedCntTotalDir <= 0)
            {
                /// 0: upload one, 1: upload two, 3: finish
                if (this.progressBar != null)
                {
                    this.progressBar.Finish("Failed");
                    this.progressBar.Close();
                }
                ///
                Debug.LogWarning("- no files uploaded...");
                this.onUploadedCompleted.Invoke(null);
            }
            else
            {
                /// 0: upload one, 1: upload two, 3: finish
                if (this.progressBar != null)
                {
                    this.progressBar.IncreaseCnt("Uploading the Thumbnails folder...");
                }
                ///
                bool isDoneUploadingSubDir = false;
                int uploadedCntTotalSubDir = 0;
                Action<int> uploadSubDirCallback = (uploadedCnt) =>
                {
                    isDoneUploadingSubDir = true;
                    uploadedCntTotalSubDir += uploadedCnt;
                    Debug.Log("subDir... uploaded.. " + uploadedCnt + " file(s)");
                };
                if (desktopMode)
                {
                    StartCoroutine(UploadFilesInDir(subDirPathInServer, subDirPathInLocal, uploadSubDirCallback));
                }
                else
                {
                    StartCoroutine(UploadFilesInDir(subDirPathInServer, subDirPathInLocal, uploadSubDirCallback, MessageFile.ThumbnailFileGenerator.AnnImgNamePrefix + "*.png"));
                }

                // wait until the callback is called
                yield return new WaitUntil(() => isDoneUploadingSubDir == true);

                // check if there were any file uploaded (if not indicate the error)
                if (uploadedCntTotalSubDir <= 0)
                {
                    /// 0: upload one, 1: upload two, 3: finish
                    if (this.progressBar != null)
                    {
                        this.progressBar.Finish("Failed");
                        this.progressBar.Close();
                    }
                    ///
                    Debug.LogWarning("- no files uploaded...");
                    this.onUploadedCompleted.Invoke(null);
                }
                else
                {
                    /// 0: upload one, 1: upload two, 3: finish
                    if (this.progressBar != null)
                    {
                        this.progressBar.Finish("Uploaded the folder...");
                        this.progressBar.Close();
                    }
                    ///
                    // on finished uploading all the files
                    this.onUploadedCompleted.Invoke(dirPathInLocal);
                }
            }
        }

        public IEnumerator CreateVMail(string name)
        {
            WWWForm form = new WWWForm();
            form.AddField("name", name);
            form.AddField("lastModifiedDesktop", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedMobile", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedServer", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "CreateVMail.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);
                    this.onInsertedVMailsTable.Invoke(-1, name);
                }
                else
                {
                    int id = -1;
                    if (int.TryParse(www.downloadHandler.text, out id))
                    {
                        Debug.Log("CreateVMail... " + www.downloadHandler.text);
                        this.onInsertedVMailsTable.Invoke(id, name);
                    }
                    else
                    {
                        Debug.LogWarning("CreateVMail... " + www.downloadHandler.text);
                        this.onInsertedVMailsTable.Invoke(-1, name);
                    }
                }
            }
        }

        public IEnumerator UpdateVMail(VMailData vMailData)
        {
            WWWForm form = new WWWForm();
            form.AddField("ID", vMailData.ID.ToString());
            form.AddField("name", vMailData.name);
            form.AddField("lastModifiedDesktop", vMailData.lastModifiedDesktop.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedMobile", vMailData.lastModifiedMobile.ToString("yyyy-MM-dd HH:mm:ss"));
            form.AddField("lastModifiedServer", vMailData.lastModifiedServer.ToString("yyyy-MM-dd HH:mm:ss"));

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "UpdateVMail.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);

                    this.onUpdatedVMailsTable.Invoke(-1);
                }
                else
                {
                    if (www.downloadHandler.text.Equals(vMailData.ID.ToString()))
                    {
                        Debug.Log("UpdateVMail... ");
                        this.onUpdatedVMailsTable.Invoke(vMailData.ID);
                    }
                    else
                    {
                        Debug.LogWarning("UpdateVMail... " + www.downloadHandler.text);
                        this.onUpdatedVMailsTable.Invoke(-1);
                    }
                }
            }
        }

        public IEnumerator CopyExistingVMailData(VMailData vMailData)
        {
            if (this.progressBar != null)
            {
                this.progressBar.Initialize(1, "Uploading the VMail.. ", "Saving existing the folder...");
            }

            WWWForm form = new WWWForm();
            form.AddField("ID", vMailData.ID.ToString());
            form.AddField("trackingID", DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss"));

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "CopyExistingVMailData.php", form))
            {
                yield return www.SendWebRequest();

                if (this.progressBar != null)
                {
                    this.progressBar.Finish();
                    this.progressBar.Close();
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);
                    this.onCopiedCompleted.Invoke(-1);
                }
                else
                {
                    if (www.downloadHandler.text.StartsWith("-1"))
                    {
                        Debug.LogWarning("CopyExistingVMailData... Failed... " + www.downloadHandler.text);
                        this.onCopiedCompleted.Invoke(-1);
                    }
                    else
                    {
                        Debug.Log("CopyExistingVMailData... Success... " + vMailData.ID);
                        this.onCopiedCompleted.Invoke(vMailData.ID);
                    }
                }
            }
        }

        // ---------------------------------------------------------------- basics ----------------------------------------------------------------
        private IEnumerator GetFileNames(string dirPath, System.Action<string> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("dirPath", dirPath);

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "GetFileNames.php", form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);
                }
                else
                {
                    Debug.Log("GetFileNames... " + www.downloadHandler.text);

                    callback(www.downloadHandler.text);
                }
            }
        }

        private IEnumerator GetFile(string filePath, System.Action<byte[]> callback)
        {
            WWWForm form = new WWWForm();
            form.AddField("filePath", filePath);

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "GetFile.php", form))
            {
                yield return www.SendWebRequest();

                // check for errors
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);
                }
                else
                {
                    Debug.Log("GetFile... " + filePath);

                    // results as a byte array
                    callback(www.downloadHandler.data);
                }
            }
        }

        public IEnumerator PrintFileNames(string dirPath)
        {
            // get the list of files
            bool isDoneGettingFileNames = false;
            string[] fileNames = null;
            Action<string> getFileNamesCallback = (fileList) =>
            {
                isDoneGettingFileNames = true;

                if (!string.IsNullOrEmpty(fileList))
                {
                    fileNames = fileList.Split(new char[] { ',' });
                }
            };
            StartCoroutine(GetFileNames(dirPath, getFileNamesCallback));

            // wait until the callback is called
            yield return new WaitUntil(() => isDoneGettingFileNames == true);

            Debug.Log("Checking the directory... " + dirPath);
            if (fileNames != null)
            {
                foreach (string fileName in fileNames)
                {
                    Debug.Log("- " + fileName);
                }
            }
        }

        public IEnumerator DownloadFile(string filePathInServer, string filePathInLocal)
        {
            bool isDoneGettingFile = false;
            Action<byte[]> getFileCallback = (downloadedBytes) =>
            {
                isDoneGettingFile = true;

                if (downloadedBytes.Length > 0)
                {
                    // save it out to an file in the local drive
                    File.WriteAllBytes(filePathInLocal, downloadedBytes);
                }
                else
                {
                    Debug.LogWarning("failed to download the file " + filePathInLocal);
                }
            };
            StartCoroutine(GetFile(filePathInServer, getFileCallback));

            // wait until the callback is called
            yield return new WaitUntil(() => isDoneGettingFile == true);
        }

        public IEnumerator DownloadFilesInDir(string dirPathInServer, string dirPathInLocal, Action<int> callback)
        {
            // 1) get the list of files in the server directory
            bool isDoneGettingFileNames = false;
            string[] fileNames = null;
            Action<string> getFileNamesCallback = (fileList) =>
            {
                isDoneGettingFileNames = true;

                if (!string.IsNullOrEmpty(fileList))
                {
                    fileNames = fileList.Split(new char[] { ',' });
                }
            };
            StartCoroutine(GetFileNames(dirPathInServer, getFileNamesCallback));

            // wait until the callback is called
            yield return new WaitUntil(() => isDoneGettingFileNames == true);

            // 2) save the files into the local directory
            int downloadedCnt = 0;

            if (fileNames != null)
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    if (fileNames[i].Contains(".")) // skip the folder
                    {
                        string fPathInServer = dirPathInServer + "/" + fileNames[i];
                        string fPathInLocal = Path.Combine(dirPathInLocal, fileNames[i]);

                        Debug.Log(fileNames[i]);
                        Debug.Log("Downloading... " + fPathInServer + " and saving it to... " + fPathInLocal);

                        bool isDoneGettingFile = false;
                        Action<byte[]> getFileCallback = (downloadedBytes) =>
                        {
                            isDoneGettingFile = true;

                            if (downloadedBytes.Length > 0)
                            {
                                File.WriteAllBytes(fPathInLocal, downloadedBytes);
                                Debug.Log("- saved it ot the local directory");
                                downloadedCnt += 1;
                            }
                            else
                            {
                                Debug.LogWarning("- failed to download the file " + fPathInServer);
                            }
                        };
                        StartCoroutine(GetFile(fPathInServer, getFileCallback));

                        // wait until the callback is called
                        yield return new WaitUntil(() => isDoneGettingFile == true);
                    }
                }
            }

            callback(downloadedCnt);
        }

        public IEnumerator UploadFile(string dirPathInServer, string filePathInLocal, System.Action<bool> callback)
        {
            byte[] bytes = File.ReadAllBytes(filePathInLocal);

            WWWForm form = new WWWForm();
            form.AddBinaryData("file", bytes, filePathInLocal);
            form.AddField("dirPath", dirPathInServer);

            using (UnityWebRequest www = UnityWebRequest.Post(WebIntegration.CodeDirURL + "UploadFile.php", form))
            {
                yield return www.SendWebRequest();

                // check for errors
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning(www.error);
                    callback(false);
                }
                else
                {
                    if (www.downloadHandler.text.Equals("0"))
                    {
                        Debug.Log("Uploaded the file: " + filePathInLocal);
                        callback(true);
                    }
                    else
                    {
                        Debug.LogWarning(www.downloadHandler.text);
                        Debug.LogWarning("Failed to upload the file: " + filePathInLocal);
                    }
                }
            }
        }

        public IEnumerator UploadFilesInDir(string dirPathInServer, string dirPathInLocal, Action<int> callback, string searchPattern = "*.*")
        {
            string[] filesInLocalDir = Directory.GetFiles(dirPathInLocal, searchPattern);

            int uploadedCnt = 0;
            foreach (string fileInLocalDir in filesInLocalDir)
            {
                bool isDone = false;
                Action<bool> uploadCallback = (success) =>
                {
                    isDone = true;
                    if (success)
                    {
                        uploadedCnt += 1;
                    }
                };
                StartCoroutine(UploadFile(dirPathInServer, fileInLocalDir, uploadCallback));

                // wait until the callback is called
                yield return new WaitUntil(() => isDone == true);
            }

            callback(uploadedCnt);
        }

        public IEnumerator UploadFilesInDir(string dirPathInServer, string dirPathInLocal, Action<int> callback, string[] extensions)
        {
            DirectoryInfo dInfo = new DirectoryInfo(dirPathInLocal);

            FileInfo[] filesInLocalDir = dInfo.GetFiles().Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();

            int uploadedCnt = 0;
            foreach (FileInfo fileInfo in filesInLocalDir)
            {
                string fileInLocalDir = fileInfo.FullName;

                bool isDone = false;
                Action<bool> uploadCallback = (success) =>
                {
                    isDone = true;
                    if (success)
                    {
                        uploadedCnt += 1;
                    }
                };
                StartCoroutine(UploadFile(dirPathInServer, fileInLocalDir, uploadCallback));

                // wait until the callback is called
                yield return new WaitUntil(() => isDone == true);
            }

            callback(uploadedCnt);
        }

        [Serializable]
        public class MyEventInt : UnityEvent<int> { }
        [Serializable]
        public class MyEventString : UnityEvent<string> { }
        [Serializable]
        public class MyEventIntString : UnityEvent<int, string> { }
    }
}