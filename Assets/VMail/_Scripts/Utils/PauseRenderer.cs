using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VMail.Viewer.Social;
using VMail.Viewer.Realtime;

namespace VMail.Utils
{
    public class PauseRenderer : MonoBehaviour
    {
        [SerializeField]
        private ManagerDesktop managerDesktop;
        [SerializeField]
        private RawImage imagePanel;

        private Camera snapCamera;

        // whether this script was enabled in this frame
        private bool onEnabled = false;


        private void Awake()
        {
            this.CreateSnapCamera();
        }

        private void CreateSnapCamera()
        {
            if (this.snapCamera != null)
            {
                Debug.LogWarning("the snap camera is not null.");
                return;
            }

            Camera viewCamera = Camera.main;
            int initScreenWidth = 1920;
            int initScreenHeight = 1080;

            GameObject obj = new GameObject();
            obj.name = "SnapCamera - on pause";
            obj.transform.SetParent(viewCamera.transform);
            this.snapCamera = obj.AddComponent<Camera>() as Camera;
            this.snapCamera.targetTexture = new RenderTexture(initScreenWidth, initScreenHeight, 24, RenderTextureFormat.ARGB32);

            { // sync the view and snap cameras
                RenderTexture rt = this.snapCamera.targetTexture;
                this.snapCamera.CopyFrom(viewCamera);
                this.snapCamera.targetTexture = rt;

                // removes the annotation layer
                this.snapCamera.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
            }

            this.snapCamera.enabled = false;
        }

        private void OnEnable()
        {
            // moved the part of capturing a screenshot to Update()
            // since this is called before rendering the objects in the scene.

            this.onEnabled = true;
        }

        private void OnDisable()
        {
            // hide the image
            this.imagePanel.enabled = false;

            // show the data object
            bool dataVisVisibility = true;
            if (this.managerDesktop.GetCurrentMode() == ViewerExploratoryVis.VisMode.Story)
            {
                Page pg = this.managerDesktop.GetCurrentPage();
                if (pg != null)
                {
                    dataVisVisibility = false;
                }
            }
            this.SetDataVisVisibility(dataVisVisibility);
        }


        private void Update()
        {
            if (this.onEnabled)
            {
                // capture the image
                Texture2D tex = this.GetImage();

                // show the image 
                this.imagePanel.enabled = true;
                this.imagePanel.texture = tex;

                // hide the data object
                this.SetDataVisVisibility(false);

                this.onEnabled = false;
            }
        }

        private Texture2D GetImage()
        {
            this.snapCamera.Render();

            Texture2D tex = Tools.GetRTPixels(this.snapCamera.targetTexture);
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
                SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
            {
                tex = Tools.Crop(tex,
                 (int)(tex.width * this.snapCamera.rect.x),
                 (int)(tex.height * this.snapCamera.rect.y),
                 (int)(tex.width * this.snapCamera.rect.width),
                 (int)(tex.height * this.snapCamera.rect.height));
            }
            else
            {
                tex = Tools.Resize(tex, (int)(tex.width * this.snapCamera.rect.width),
                 (int)(tex.height * this.snapCamera.rect.height));
            }

            return tex;
        }

        private void SetDataVisVisibility(bool v)
        {
            if (this.managerDesktop.viewer == null)
            {
                return;
            }

            this.managerDesktop.viewer.visIntegrator.SetVisibility(v);

            if (this.managerDesktop.nav == null)
            {
                return;
            }
            this.managerDesktop.nav.enabled = v;
        }

    }
}