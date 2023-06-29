using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMail.Viewer.Social
{
    public class BrushPallette : MonoBehaviour
    {
        public List<Button> buttons;
        public Slider slider;

        public void SetInteractable(bool interactable)
        {
            foreach (Button button in this.buttons)
            {
                button.interactable = interactable;
            }
            this.slider.interactable = interactable;
        }

    }
}