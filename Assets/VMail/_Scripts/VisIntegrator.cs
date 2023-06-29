using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail
{
    public class VisIntegrator : MonoBehaviour
    {
        public GameObject[] dataVisToOnOff;

        public virtual void SetVisState(string visState)
        {

        }

        public virtual string GetVisState()
        {
            return "";
        }

        public virtual void Clear()
        {

        }

        public void SetVisibility(bool v)
        {
            if (this.dataVisToOnOff == null)
            {
                Debug.LogWarning("data vis to on/off is null.");
                return;
            }

            foreach (GameObject obj in this.dataVisToOnOff)
            {
                if (obj.activeSelf != v)
                {
                    obj.SetActive(v);
                }
            }
        }

    }
}