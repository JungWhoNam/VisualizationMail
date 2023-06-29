using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VMail.Viewer.Baked
{
    public class AnnotationRawImage : MonoBehaviour
    {
        private Texture2D annTex;

        [SerializeField]
        private Social.DrawSettings drawingSettings;

        [Space(10)]
        [SerializeField]
        private EventSystem es;
        [SerializeField]
        private GraphicRaycaster gr;

        Vector2 previous_drag_position;
        Color[] clean_colours_array;
        Color transparent;
        Color32[] cur_colors;
        bool mouse_was_previously_held_down = false;
        bool no_drawing_on_current_drag = false;
        bool doneCreatingCleanColor = false;


        public void SetTexture(Texture2D tex)
        {
            this.annTex = tex;
            this.GetComponent<RawImage>().texture = this.annTex;
            this.GetComponent<AspectRatioFitter>().aspectRatio = (float)this.annTex.width / this.annTex.height;

            // Initialize clean pixels to use
            if (!this.doneCreatingCleanColor)
            {
                this.clean_colours_array = new Color[this.annTex.width * this.annTex.height];
                for (int x = 0; x < clean_colours_array.Length; x++)
                {
                    clean_colours_array[x] = this.drawingSettings.resetColour;
                }
                this.doneCreatingCleanColor = true;
            }
        }

        public void SetInteractable(bool interactable)
        {
            this.GetComponent<RawImage>().raycastTarget = interactable;
        }

        public void Clear()
        {
            if (this.annTex == null)
            {
                return;
            }

            this.annTex.SetPixels(clean_colours_array);
            this.annTex.Apply();
        }

        void Update()
        {
            // Is the user holding down the left mouse button?
            bool mouse_held_down = Input.GetMouseButton(0);
            if (Input.GetMouseButton(0) && !no_drawing_on_current_drag && this.annTex && this.GetComponent<RawImage>().raycastTarget)
            {
                // 1. check if the mouse is in the view port.
                // 2. check if the mouse collides with the drawing layer.
                Vector3 pt = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                if (pt.x >= 0f && pt.x <= 1f && pt.y >= 0f && pt.y <= 1f)
                {
                    // for checking if it hits UI components.
                    PointerEventData ped = new PointerEventData(this.es);
                    ped.position = Input.mousePosition;
                    List<RaycastResult> results = new List<RaycastResult>();
                    gr.Raycast(ped, results);

                    if (results.Count > 0 && results[0].gameObject.GetComponent<AnnotationRawImage>() != null
                        && results[0].gameObject.GetComponent<AnnotationRawImage>() == this)
                    {
                        Vector3 imgPos = this.transform.InverseTransformPoint(results[0].screenPosition);

                        int x = (int)((imgPos.x + GetComponent<RectTransform>().rect.width * 0.5f)
                            * (this.annTex.width / GetComponent<RectTransform>().rect.width));
                        int y = (int)((imgPos.y + GetComponent<RectTransform>().rect.height * 0.5f)
                            * (this.annTex.height / GetComponent<RectTransform>().rect.height));

                        ChangeColourAtPoint(new Vector2(x, y));

                        Utils.CloseWithoutSaveWarning.closeWithoutWarning = false;
                    }
                    else
                    {
                        // We're not over our destination texture
                        previous_drag_position = Vector2.zero;
                        if (!mouse_was_previously_held_down)
                        {
                            // This is a new drag where the user is left clicking off the canvas
                            // Ensure no drawing happens until a new drag is started
                            no_drawing_on_current_drag = true;
                        }
                    }
                }
            }
            // Mouse is released
            else if (!mouse_held_down)
            {
                previous_drag_position = Vector2.zero;
                no_drawing_on_current_drag = false;
            }
            mouse_was_previously_held_down = mouse_held_down;
        }

        // Pass in a point in WORLD coordinates
        // Changes the surrounding pixels of the world_point to the static pen_colour
        private void ChangeColourAtPoint(Vector2 pixelCoord)
        {
            cur_colors = this.annTex.GetPixels32();

            if (previous_drag_position == Vector2.zero)
            {
                // If this is the first time we've ever dragged on this image, simply colour the pixels at our mouse position
                MarkPixelsToColour(pixelCoord, drawingSettings.penWidth, drawingSettings.penColor);
            }
            else
            {
                // Colour in a line from where we were on the last update call
                ColourBetween(previous_drag_position, pixelCoord);
            }
            ApplyMarkedPixelChanges();

            //Debug.Log("Dimensions: " + pixelWidth + "," + pixelHeight + ". Units to pixels: " + unitsToPixels + ". Pixel pos: " + pixel_pos);
            previous_drag_position = pixelCoord;
        }


        // Set the colour of pixels in a straight line from start_point all the way to end_point, to ensure everything inbetween is coloured
        private void ColourBetween(Vector2 start_point, Vector2 end_point)
        {
            // Get the distance from start to finish
            float distance = Vector2.Distance(start_point, end_point);
            Vector2 direction = (start_point - end_point).normalized;

            Vector2 cur_position = start_point;

            // Calculate how many times we should interpolate between start_point and end_point based on the amount of time that has passed since the last update
            float lerp_steps = 1 / distance;

            for (float lerp = 0; lerp <= 1; lerp += lerp_steps)
            {
                cur_position = Vector2.Lerp(start_point, end_point, lerp);
                MarkPixelsToColour(cur_position, drawingSettings.penWidth, drawingSettings.penColor);
            }
        }

        private void MarkPixelsToColour(Vector2 center_pixel, int pen_thickness, Color color_of_pen)
        {
            // Figure out how many pixels we need to colour in each direction (x and y)
            int center_x = (int)center_pixel.x;
            int center_y = (int)center_pixel.y;
            int extra_radius = Mathf.Min(0, pen_thickness - 2);

            for (int x = center_x - pen_thickness; x <= center_x + pen_thickness; x++)
            {
                // Check if the X wraps around the image, so we don't draw pixels on the other side of the image
                if (x >= (int)this.annTex.width || x < 0)
                    continue;

                for (int y = center_y - pen_thickness; y <= center_y + pen_thickness; y++)
                {
                    MarkPixelToChange(x, y, color_of_pen);
                }
            }
        }

        private void MarkPixelToChange(int x, int y, Color color)
        {
            // Need to transform x and y coordinates to flat coordinates of array
            int array_pos = y * (int)this.annTex.width + x;

            // Check if this is a valid position
            if (array_pos > cur_colors.Length || array_pos < 0)
                return;

            cur_colors[array_pos] = color;
        }

        private void ApplyMarkedPixelChanges()
        {
            this.annTex.SetPixels32(cur_colors);
            this.annTex.Apply();
        }

    }
}