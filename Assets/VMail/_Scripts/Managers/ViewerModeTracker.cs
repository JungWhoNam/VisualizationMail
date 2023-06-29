using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace VMail
{
    public class ViewerModeTracker : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent onSwitchToExploreView;
        [SerializeField]
        private UnityEvent onSwitchToStoryView;

        public bool isExploreMode { get; private set; }

        private void Awake()
        {
            this.isExploreMode = false;
        }

        public void SetExploreView()
        {
            if (!this.isExploreMode)
            {
                this.onSwitchToExploreView.Invoke();
            }

            this.isExploreMode = true;
        }

        public void SetStoryboardView()
        {
            if (this.isExploreMode)
            {
                this.onSwitchToStoryView.Invoke();
            }

            this.isExploreMode = false;
        }
    }
}