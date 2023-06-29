using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail.Utils
{
    public class CloseWithoutSaveWarningUiPanel : MonoBehaviour
    {
        public void Quit()
        {
            CloseWithoutSaveWarning.closeWithoutWarning = true;
            Application.Quit();
        }

        public void SetCloseWithoutWarning(bool closeWithoutWarning)
        {
            Utils.CloseWithoutSaveWarning.closeWithoutWarning = closeWithoutWarning;
        }

    }
}