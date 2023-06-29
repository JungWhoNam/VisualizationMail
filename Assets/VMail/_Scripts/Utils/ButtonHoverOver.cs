using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VMail.Utils
{
    public class ButtonHoverOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.onPointerEnter.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.onPointerExit.Invoke();
        }

    }
}