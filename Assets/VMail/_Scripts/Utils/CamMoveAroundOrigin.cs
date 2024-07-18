// mostly copied from Jibran I. Syed from https://jibransyed.wordpress.com/2013/02/22/rotating-panning-and-zooming-a-camera-in-unity/
// changed his code for a global movemement and work around origin.

using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

namespace VMail.Utils
{
    public class CamMoveAroundOrigin : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        [Space(10)]
        [SerializeField]
        private EventSystem es;
        [SerializeField]
        private List<GraphicRaycaster> grs;

        [Space(10)]
        public AnimationCurve speedCurve;   // to change the speed when the camera is close to 0 in y. 
        public float turnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
        public float panSpeed = 0.05f;      // Speed of the camera when being panned
        public float zoomSpeed = 2f;        // Speed of the camera going back and forth
        public bool rotWorldY = true;
        public bool panWorldY = false;
        public bool rotAroundOrigin = true;

        [Space(10)]
        public UnityEvent onInteracted;

        private Vector3 mouseOrigin;        // Position of cursor when mouse dragging starts
        private bool isPanning;             // Is the camera being panned?
        private bool isRotating;            // Is the camera being rotated?
        private bool isZooming;             // Is the camera zooming?
        private Vector3 initPosCam;
        private Quaternion initRotCam;

        void Start()
        {
            this.initPosCam = this.cam.transform.position;
            this.initRotCam = this.cam.transform.rotation;
        }

        public void ResetState()
        {
            this.cam.transform.position = this.initPosCam;
            this.cam.transform.rotation = this.initRotCam;
        }

        void Update()
        {
            if (this.IsInViewPort(Input.mousePosition))
            {
                if (Input.GetMouseButtonDown(0)) // Get the left mouse button
                {
                    this.mouseOrigin = this.cam.ScreenToViewportPoint(Input.mousePosition);
                    this.isRotating = true;
                }
                if (Input.GetMouseButtonDown(2)) // Get the right mouse button
                {
                    this.mouseOrigin = this.cam.ScreenToViewportPoint(Input.mousePosition);
                    this.isPanning = true;
                }
                if (Input.GetAxis("Mouse ScrollWheel") != 0) // Get the mouse wheel
                {
                    this.mouseOrigin = this.cam.ScreenToViewportPoint(Input.mousePosition);
                    this.isZooming = true;
                }
            }

            // Disable movements on button release
            if (!Input.GetMouseButton(0)) isRotating = false;
            if (!Input.GetMouseButton(2)) isPanning = false;
            if (Input.GetAxis("Mouse ScrollWheel") == 0) isZooming = false;


            if (isRotating || isPanning || isZooming)
            {
                this.onInteracted.Invoke();
            }

            // Determine the speed factor, varied by how y position value is close to 0.
            float remapped = Mathf.Clamp(this.cam.transform.position.y, 0, 1.5f) / 1.5f;
            float speedFactor = this.speedCurve.Evaluate(remapped);
            if (speedFactor > this.speedCurve.keys[1].value)
            { // clamp to the max ASSUME the second key point is the max
                speedFactor = this.speedCurve.keys[1].value;
            }
            if (speedFactor < this.speedCurve.keys[0].value)
            { // clamp to the min ASSUME the first key point is the max
                speedFactor = this.speedCurve.keys[0].value;
            }

            Vector3 pos = this.cam.ScreenToViewportPoint(Input.mousePosition) - mouseOrigin;

            // Rotate camera along X and world Y axis
            if (isRotating)
            {
                float speed = speedFactor * turnSpeed;
                this.cam.transform.RotateAround(this.rotAroundOrigin ? Vector3.zero : this.cam.transform.position, this.cam.transform.right, -pos.y * speed);
                this.cam.transform.RotateAround(this.rotAroundOrigin ? Vector3.zero : this.cam.transform.position, this.rotWorldY ? Vector3.up : this.cam.transform.up, pos.x * speed);
            }
            if (isPanning)
            {
                float speed = speedFactor * panSpeed;
                this.cam.transform.Translate(this.cam.transform.right * pos.x * speed, Space.World);
                this.cam.transform.Translate((this.panWorldY ? Vector3.up : this.cam.transform.up) * pos.y * speed, Space.World);
            }
            // Move the camera linearly along world Y axis
            if (isZooming)
            {
                float step = Input.GetAxis("Mouse ScrollWheel") * speedFactor * zoomSpeed;
                /*
                // move towards a fixed point
                Vector3 moveTo = Vector3.MoveTowards(this.cam.transform.position, Vector3.zero, step);
                if (!moveTo.Equals(Vector3.zero))
                {
                    this.cam.transform.position = Vector3.MoveTowards(this.cam.transform.position, Vector3.zero, step);
                }*/
                this.cam.transform.position = this.cam.transform.position + this.cam.transform.forward * step;
            }
        }

        private bool IsInViewPort(Vector3 mousePos)
        {
            // for checking if it hits UI components.
            if (this.es != null && this.grs != null && this.grs.Count > 0)
            {
                PointerEventData ped = new PointerEventData(this.es);
                ped.position = Input.mousePosition;

                foreach (GraphicRaycaster gr in grs)
                {
                    List<RaycastResult> results = new List<RaycastResult>();
                    gr.Raycast(ped, results);
                    if (results.Count > 0)
                        return false;
                }
            }

            // check if the mouse is within the camera's viewport.
            Vector3 viewPort = this.cam.ScreenToViewportPoint(mousePos);
            if ((viewPort.x >= 0f) && (viewPort.x <= 1f) && (viewPort.y >= 0f) && (viewPort.y <= 1f))
                return true;

            return false;
        }

    }
}