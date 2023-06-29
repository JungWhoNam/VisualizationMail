using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VMail.Viewer.Social
{
    // Helper methods used to set drawing settings
    public class DrawSettings : MonoBehaviour
    {
        public Color penColor = Color.red;     // Change these to change the default drawing settings                       
        public int penWidth = 3;                // PEN WIDTH (actually, it's a radius, in pixels)
        [Space]
        public Color resetColour = new Color(0f, 0f, 0f, 0f);  // By default, reset the canvas to be transparent
        public LayerMask drawingLayer;

        // Changing pen settings is easy as changing the static properties Pen_Colour and Pen_Width
        public void SetMarkerColour(Color new_color)
        {
            this.penColor = new_color;
        }
        // new_width is radius in pixels
        public void SetMarkerWidth(int new_width)
        {
            this.penWidth = new_width;
        }
        public void SetMarkerWidth(float new_width)
        {
            SetMarkerWidth((int)new_width);
        }

        // Call these these to change the pen settings
        public void SetMarkerRed()
        {
            SetMarkerColour(new Color(240 / 255f, 38 / 255f, 47 / 255f));
        }
        public void SetMarkerGreen()
        {
            SetMarkerColour(new Color(239 / 255f, 155 / 255f, 27 / 255f));
        }
        public void SetMarkerBlue()
        {
            SetMarkerColour(new Color(79 / 255f, 137 / 255f, 167 / 255f));
        }
        public void SetEraser()
        {
            SetMarkerColour(this.resetColour);
        }

    }
}