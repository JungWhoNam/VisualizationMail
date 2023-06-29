using UnityEngine;

namespace VMail.Utils
{
    public class CloseWithoutSaveWarning
    {
        public static bool closeWithoutWarning = true;

        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart()
        {
            Application.wantsToQuit += CheckBeforeClosing;
        }

        static bool CheckBeforeClosing()
        {
            if (!CloseWithoutSaveWarning.closeWithoutWarning)
            {
                CloseWithoutSaveWarning.ShowWarningUI();
            }

            return CloseWithoutSaveWarning.closeWithoutWarning;
        }

        public static void ShowWarningUI()
        {
            Object[] obj = Resources.FindObjectsOfTypeAll(typeof(CloseWithoutSaveWarningUiPanel));
            if (obj != null && obj.Length > 0)
            {
                ((CloseWithoutSaveWarningUiPanel)obj[0]).gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Could not find an object of type CloseWithoutSaveWarningUiPanel");
            }
        }

    }
}