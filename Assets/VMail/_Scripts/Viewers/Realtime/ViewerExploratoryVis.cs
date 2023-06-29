using System.Collections.Generic;
using UnityEngine;

using VMail.Utils;
using VMail.Viewer.Baked;
using VMail.Viewer.Social;

namespace VMail.Viewer.Realtime
{
    public class ViewerExploratoryVis : MonoBehaviour, IViewer
    {
        public enum VisMode { Exploratory, Story };

        [Header("Visualization Integrations")]
        [SerializeField]
        public VisIntegrator visIntegrator;
        [SerializeField]
        public VisStateComparator visStateComparator;

        protected Camera viewCamera;
        protected Camera snapCamera;

        [Space(10)]
        [SerializeField]
        protected RenderTexture cubemap;
        [SerializeField]
        protected RenderTexture equirect;
        //[Range(0.1f, 1.0f)]
        //public float thumbnailScaleRatio = 0.5f;
        [SerializeField]
        protected string annotationLayerName = "Annotation";

        [Header("UI Integrations")]
        [SerializeField]
        private PagePicker pagePicker;
        [SerializeField]
        private MessagePlayer messagePlayer;
        [SerializeField]
        private ViewerAlphaBlendPages viewerBaked;
        [SerializeField]
        private ViewerAnnotationSpatial viewerAnnSpatial;
        [SerializeField]
        private ViewerAnnotationNonSpatial viewerAnnNonSpatial;
        [SerializeField]
        private ViewerRenderOnOff viewerRenderOnOff;


        public VisMode currMode { get; private set; }

        public int initScreenWidth { get; protected set; } // Screen.width is buggy except for the start
        public int initScreenHeight { get; protected set; } // Screen.height is buggy except for the start

        private void Awake()
        {
            this.viewCamera = Camera.main;

            this.initScreenWidth = 1920;
            this.initScreenHeight = 1080;
            Screen.SetResolution(this.initScreenWidth, this.initScreenHeight, false);

            this.CreateSnapCamera();
        }

        private void CreateSnapCamera()
        {
            if (this.snapCamera != null)
            {
                Debug.LogWarning("the snap camera is not null.");
                return;
            }

            GameObject obj = new GameObject();
            obj.name = "SnapCamera";
            obj.transform.SetParent(this.viewCamera.transform);
            this.snapCamera = obj.AddComponent<Camera>() as Camera;
            //this.snapCamera.targetTexture = new RenderTexture(
            //    (int)(Screen.width * this.thumbnailScaleRatio),
            //    (int)(Screen.height * this.thumbnailScaleRatio), 24, RenderTextureFormat.ARGB32);
            this.snapCamera.targetTexture = new RenderTexture(this.initScreenWidth, this.initScreenHeight, 24, RenderTextureFormat.ARGB32);

            { // sync the view and snap cameras
                RenderTexture rt = this.snapCamera.targetTexture;
                this.snapCamera.CopyFrom(this.viewCamera);
                this.snapCamera.targetTexture = rt;

                // removes the annotation layer
                this.snapCamera.cullingMask = ~(1 << LayerMask.NameToLayer(this.annotationLayerName));
            }

            this.snapCamera.enabled = false;
        }

        public void SetCurrentMode(VisMode mode)
        {
            if (mode == VisMode.Exploratory)
            {
                this.viewerBaked.gameObject.SetActive(false);
                this.viewerAnnNonSpatial.gameObject.SetActive(false);
                this.viewerAnnSpatial.gameObject.SetActive(true);
            }

            this.viewerRenderOnOff.SetStoryMode(mode == VisMode.Story);

            this.currMode = mode;
        }

        public void HideUnhideBakedViewer(bool hide)
        {
            this.viewerBaked.gameObject.SetActive(!hide);
            this.viewerAnnSpatial.gameObject.SetActive(hide);
            this.viewerAnnNonSpatial.gameObject.SetActive(!hide);
        }

        // ============================================= INTERFACE =============================================

        public void OpenMessage(Message info)
        {
            // nothing to do.
            // TODO download the resources if needed
        }

        public void SetState(Page page)
        {
            if (page == null) return;

            this.viewCamera.transform.position = page.pageInfo.viewInfo.pos;
            this.viewCamera.transform.rotation = page.pageInfo.viewInfo.rot;
            this.UpdateVisState(page.pageInfo.viewInfo.json);

            this.viewerRenderOnOff.SetState(page);
            this.viewerAnnSpatial.SetState(page);
        }

        private void UpdateVisState(string visState)
        {
            string currVisState = this.visIntegrator.GetVisState();

            List<KeyValuePair<string, string>> diffNodes = this.visStateComparator.GetVisDiffs(currVisState, visState);

            if (diffNodes == null || diffNodes.Count > 0)
            {
                this.visIntegrator.SetVisState(visState);
            }
        }

        public void SetState(Transition transition)
        {
            if (transition == null)
            {
                Debug.LogWarning("this transition is null.");
                return;
            }
            else if (transition.from == null || transition.to == null)
            {
                Debug.LogWarning("from or to Page of this transition are null.");
                return;
            }

            this.viewerRenderOnOff.SetState(transition);
            this.viewerAnnSpatial.SetState(transition);

            // compare the two states
            bool isCamDiff = this.visStateComparator.IsCameraLocationDiff(
                            transition.from.pageInfo.viewInfo.pos, transition.to.pageInfo.viewInfo.pos,
                            transition.from.pageInfo.viewInfo.rot, transition.to.pageInfo.viewInfo.rot);
            List<KeyValuePair<string, string>> nodes = this.visStateComparator.GetVisDiffs(
                transition.from.pageInfo.viewInfo.json, transition.to.pageInfo.viewInfo.json);

            if (nodes != null && nodes.Count == 0) // two vis states are the same.
            {
                // update the state
                this.UpdateVisState(transition.from.pageInfo.viewInfo.json);

                // turn off and on viewers
                this.viewerBaked.gameObject.SetActive(false);
                this.viewerAnnNonSpatial.gameObject.SetActive(false);
                this.viewerAnnSpatial.gameObject.SetActive(true);

                // update the transition in the viewer
                this.visStateComparator.SetCameraTransition(
                            transition.from.pageInfo.viewInfo.pos,
                            transition.to.pageInfo.viewInfo.pos,
                            transition.from.pageInfo.viewInfo.rot,
                            transition.to.pageInfo.viewInfo.rot,
                            transition.amt);
            }
            else if (nodes != null && nodes.Count > 0) // two vis states are different and there is a valid transition. 
            {
                // update the state
                this.UpdateVisState(transition.from.pageInfo.viewInfo.json);

                // turn off and on viewers
                this.viewerBaked.gameObject.SetActive(false);
                this.viewerAnnNonSpatial.gameObject.SetActive(false);
                this.viewerAnnSpatial.gameObject.SetActive(true);

                if (isCamDiff)
                {
                    // update the transition in the viewer
                    // map the transition amount from (0.0~0.5) => (0.0~1.0)
                    Transition t = new Transition();
                    t.from = transition.from;
                    t.to = transition.to;
                    t.amt = (transition.amt) * 2f;

                    if (transition.amt < 0.5f)
                    {
                        this.visStateComparator.SetCameraTransition(
                                 t.from.pageInfo.viewInfo.pos,
                                 t.to.pageInfo.viewInfo.pos,
                                 t.from.pageInfo.viewInfo.rot,
                                 t.to.pageInfo.viewInfo.rot,
                                 t.amt);

                        this.visStateComparator.SetVisTransition(nodes, 0f);
                    }
                    else
                    {
                        this.visStateComparator.SetCameraTransition(
                                t.from.pageInfo.viewInfo.pos,
                                t.to.pageInfo.viewInfo.pos,
                                t.from.pageInfo.viewInfo.rot,
                                t.to.pageInfo.viewInfo.rot,
                                t.amt);

                        // map the transition amount from (0.5~1.0) => (0.0~1.0)
                        t.amt = (transition.amt - 0.5f) * 2f;
                        this.visStateComparator.SetVisTransition(nodes, t.amt);
                    }
                }
                else
                {
                    this.visStateComparator.SetVisTransition(nodes, transition.amt);
                }
            }
            else if (nodes == null) // two vis states are different and there is no valid transition. 
            {
                if (this.currMode == VisMode.Exploratory)
                {
                    this.pagePicker.PickPage(transition.from, transition.to);
                    this.messagePlayer.PausePlaying();
                }
                else if (this.currMode == VisMode.Story)
                {
                    if (isCamDiff)
                    {
                        if (transition.amt < 0.5f)
                        {
                            // update the state
                            UpdateVisState(transition.from.pageInfo.viewInfo.json);

                            // turn off and on viewers
                            this.viewerBaked.gameObject.SetActive(false);
                            this.viewerAnnSpatial.gameObject.SetActive(true);
                            this.viewerAnnNonSpatial.gameObject.SetActive(false);

                            // update the transition in the viewer
                            // map the transition amount from (0.0~0.5) => (0.0~1.0)
                            Transition t = new Transition();
                            t.from = transition.from;
                            t.to = transition.to;
                            t.amt = (transition.amt) * 2f;
                            this.visStateComparator.SetCameraTransition(
                                t.from.pageInfo.viewInfo.pos,
                                t.to.pageInfo.viewInfo.pos,
                                t.from.pageInfo.viewInfo.rot,
                                t.to.pageInfo.viewInfo.rot,
                                t.amt);
                        }
                        else
                        {
                            // turn off and on viewers
                            this.viewerBaked.gameObject.SetActive(true);
                            this.viewerAnnSpatial.gameObject.SetActive(false);
                            this.viewerAnnNonSpatial.gameObject.SetActive(true);

                            // update the transition in the viewer
                            // map the transition amount from (0.5~1.0) => (0.0~1.0)
                            Transition t = new Transition();
                            t.from = transition.from;
                            t.to = transition.to;
                            t.amt = (transition.amt - 0.5f) * 2f;
                            this.viewerBaked.SetState(t);
                            this.viewerAnnNonSpatial.SetState(transition);
                        }
                    }
                    else
                    {
                        // turn off and on viewers
                        this.viewerBaked.gameObject.SetActive(true);
                        this.viewerAnnSpatial.gameObject.SetActive(false);
                        this.viewerAnnNonSpatial.gameObject.SetActive(true);

                        // update the transition in the viewer
                        this.viewerBaked.SetState(transition);
                        this.viewerAnnNonSpatial.SetState(transition);
                    }
                }
            }
        }

        // ============================================= GET IMAGE =============================================

        public Texture2D GetImage(bool showAnnotations)
        {
            int initMask = this.snapCamera.cullingMask;
            this.snapCamera.cullingMask = showAnnotations ?
                this.snapCamera.cullingMask | (1 << LayerMask.NameToLayer(this.annotationLayerName)) :
                ~(1 << LayerMask.NameToLayer(this.annotationLayerName));
            this.snapCamera.Render();
            this.snapCamera.cullingMask = initMask;

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

        public Texture2D GetImage(ViewInfo info)
        {
            // save the current states to go back after taking a photo
            Vector3 initPos = this.snapCamera.transform.position;
            Quaternion initRot = this.snapCamera.transform.rotation;

            // view transition
            this.snapCamera.transform.position = info.pos;
            this.snapCamera.transform.rotation = info.rot;

            // save the screen 
            Texture2D tex = this.GetImage(false);

            // bring back to the original states
            this.snapCamera.transform.position = initPos;
            this.snapCamera.transform.rotation = initRot;

            return tex;
        }

        public Texture2D GetImage(Transition transition)
        {
            // save the current states to go back after taking a photo
            Vector3 initPos = this.snapCamera.transform.position;
            Quaternion initRot = this.snapCamera.transform.rotation;

            // view transition
            Vector3 posFrom = transition.from.pageInfo.viewInfo.pos;
            Vector3 posTo = transition.to.pageInfo.viewInfo.pos;
            this.snapCamera.transform.position = Vector3.Lerp(posFrom, posTo, transition.amt);

            Quaternion rotFrom = transition.from.pageInfo.viewInfo.rot;
            Quaternion rotTo = transition.to.pageInfo.viewInfo.rot;
            this.snapCamera.transform.rotation = Quaternion.Lerp(rotFrom, rotTo, transition.amt);

            // save the screen 
            Texture2D tex = this.GetImage(false);

            // bring back to the original states
            this.snapCamera.transform.position = initPos;
            this.snapCamera.transform.rotation = initRot;

            return tex;
        }

        public Texture2D GetImage360()
        {
            Rect initRect = this.snapCamera.rect;
            int initMask = this.snapCamera.cullingMask;

            this.snapCamera.rect = new Rect(0, 0, 1, 1);
            // adds the annotation layer
            this.snapCamera.cullingMask |= (1 << LayerMask.NameToLayer(this.annotationLayerName));

            this.snapCamera.RenderToCubemap(this.cubemap, 63, Camera.MonoOrStereoscopicEye.Mono);
            this.cubemap.ConvertToEquirect(this.equirect, Camera.MonoOrStereoscopicEye.Mono);
            Texture2D tex = Tools.GetRTPixels(this.equirect);

            this.snapCamera.rect = initRect;
            this.snapCamera.cullingMask = initMask;

            return tex;
        }

        public Texture2D GetImage360(ViewInfo info)
        {
            // save the current states to go back after taking a photo
            Vector3 initPos = this.snapCamera.transform.position;
            Quaternion initRot = this.snapCamera.transform.rotation;

            // view transition
            this.snapCamera.transform.position = info.pos;
            this.snapCamera.transform.rotation = info.rot;

            // save the screen 
            Texture2D tex = this.GetImage360();

            // bring back to the original states
            this.snapCamera.transform.position = initPos;
            this.snapCamera.transform.rotation = initRot;

            return tex;
        }

        // ============================================= Other =============================================

        public ViewInfo GetView()
        {
            ViewInfo view = new ViewInfo();
            view.pos = this.viewCamera.transform.position;
            view.rot = this.viewCamera.transform.rotation;
            view.json = this.visIntegrator.GetVisState();

            return view;
        }

        public Vector2Int GetViewingAreaSize()
        {
            int w = (int)(this.initScreenWidth * this.viewCamera.rect.width);
            int h = (int)(this.initScreenHeight * this.viewCamera.rect.height);

            return new Vector2Int(w, h);
        }

        public void ClearVis()
        {
            this.visIntegrator.Clear();
        }

    }
}