using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace VMail.Utils
{
    public class ObjectDragPointer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onBeginDrag;
        public UnityEvent onDrag;
        public UnityEvent onEndDrag;
        public UnityEvent onPointerDown;
        public UnityEvent onPointerUp;

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("OnBeginDrag");
            this.onBeginDrag.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.onDrag.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("OnEndDrag");
            this.onEndDrag.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log("OnPointerDown");
            this.onPointerDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("OnPointerUp");
            this.onPointerUp.Invoke();
        }
    }

}