using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VMail.Viewer.Social;

namespace VMail.Utils.Web
{
    public class UserManager : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TMP_InputField userNameText;

        public static readonly string FileName = "userInfo.txt";

        public static readonly string DefaultUserName = "UserName - Device Type";

        private void Awake()
        {
            this.LoadUserInfo();

            ViewerComment.Author = this.userNameText.text;

            // if the user has not set his or her user name... keep this panel opended.
            this.gameObject.SetActive(UserManager.DefaultUserName == this.userNameText.text);
        }

        public void LoadUserInfo()
        {
            string fPath = Path.Combine(Application.persistentDataPath, UserManager.FileName);

            string userName = UserManager.DefaultUserName;
            if (File.Exists(fPath))
            {
                System.IO.StreamReader file = new System.IO.StreamReader(fPath);
                string line = file.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    userName = line;
                }

                file.Close();
            }

            this.userNameText.text = userName;
        }

        public void SaveUserName()
        {
            if (string.IsNullOrEmpty(this.userNameText.text))
            {
                return;
            }

            string fPath = Path.Combine(Application.persistentDataPath, UserManager.FileName);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fPath))
            {
                file.WriteLine(this.userNameText.text);
            }

            ViewerComment.Author = this.userNameText.text;
            this.gameObject.SetActive(false);
        }

    }
}