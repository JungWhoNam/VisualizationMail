using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail.Viewer.Realtime
{
    public class ViewerRenderOnOff : MonoBehaviour, IViewer
    {
        public static float Threshold = 0.00025f; // 0.025f

        public ViewerExploratoryVis viewerExploratoryVis;

        private bool isExplorationMode = true;

        public void OpenMessage(Message message)
        {
            // nothing to do
        }

        public void SetState(Page page)
        {
            if (this.isExplorationMode || page == null)
            {
                return;
            }

            SetVisibility(true);
        }

        public void SetState(Transition t)
        {
            if (this.isExplorationMode || t == null)
            {
                return;
            }

            // on the page, turn the rendering objects on.
            // otherwise, turn them off.
            bool state = true;

            if (t.amt <= ViewerRenderOnOff.Threshold)
            {
                state = false;
            }
            else if (t.amt >= (1f - ViewerRenderOnOff.Threshold))
            {
                state = false;
            }

            SetVisibility(state);
        }

        public void SetStoryMode(bool storyMode)
        {
            if (!storyMode)
            {
                this.SetVisibility(true);
            }

            this.isExplorationMode = !storyMode;
        }

        private void SetVisibility(bool v)
        {
            if (this.viewerExploratoryVis == null || this.viewerExploratoryVis.visIntegrator == null)
            {
                return;
            }

            this.viewerExploratoryVis.visIntegrator.SetVisibility(v);
        }

    }
}