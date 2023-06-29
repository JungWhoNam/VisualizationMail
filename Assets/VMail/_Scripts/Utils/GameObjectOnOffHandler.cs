using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VMail.Utils
{
    public class GameObjectOnOffHandler : MonoBehaviour
    {
        public UnityEvent onOn;
        public UnityEvent onOff;

        private void OnEnable()
        {
            this.onOn.Invoke();
        }

        private void OnDisable()
        {
            this.onOff.Invoke();
        }

    }
}