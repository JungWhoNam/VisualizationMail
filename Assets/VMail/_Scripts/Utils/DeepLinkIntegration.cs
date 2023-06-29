using UnityEngine;
using UnityEngine.SceneManagement;

namespace VMail.Utils.Web
{
    public class DeepLinkIntegration : MonoBehaviour
    {
        public VMailWebManager webManager;

        public static DeepLinkIntegration Instance { get; private set; }
        private string deeplinkURL;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Application.deepLinkActivated += onDeepLinkActivated;
                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    onDeepLinkActivated(Application.absoluteURL);
                }
                // Initialize DeepLink Manager global variable.
                else deeplinkURL = "[none]";
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void onDeepLinkActivated(string url)
        {
            // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
            this.deeplinkURL = url;

            if (this.webManager == null)
            {
                return;
            }

            this.webManager.onRefreshed.AddListener(OpenVMailFromURL);
            this.webManager.RefreshAvailableVMails();
        }

        private void OpenVMailFromURL()
        {
            this.webManager.OpenFromURL(this.deeplinkURL);

            this.webManager.onRefreshed.RemoveListener(OpenVMailFromURL);
        }
    }
}