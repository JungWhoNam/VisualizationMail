using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail.Utils
{
    public class ToggleOnOff : MonoBehaviour
    {
        public GameObject[] objectsOn;
        public GameObject[] objectsOff;

        private bool mode;

        private void Start()
        {
            this.Toggle();
        }

        public void Toggle()
        {
            foreach (GameObject obj in this.objectsOn)
            {
                obj.SetActive(mode);
            }
            foreach (GameObject obj in this.objectsOff)
            {
                obj.SetActive(!mode);
            }

            mode = !mode;
        }

    }
}