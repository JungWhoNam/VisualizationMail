using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VMail.Viewer.Social
{
    public class AnnotationSpatial : MonoBehaviour
    {
        public Page page { get; private set; }

        private Material mat;
        private Texture2D annTex;

        // public MeshRenderer snapshotView;

        [SerializeField]
        private DrawSettings drawingSettings;


        [Space(10)]
        [SerializeField]
        private EventSystem es;
        [SerializeField]
        private GraphicRaycaster gr;

        [SerializeField]
        private Color initColor = Color.blue;
        [SerializeField]
        private Color selectedColor = Color.magenta;
        [SerializeField]
        private GameObject[] frames;
        [SerializeField]
        private MeshRenderer bgImage;


        Vector2 previous_drag_position;
        Color[] clean_colours_array;
        Color transparent;
        Color32[] cur_colors;
        bool mouse_was_previously_held_down = false;
        bool no_drawing_on_current_drag = false;

        bool doneCreatingCleanColor = false;

        private void Awake()
        {
            this.GetComponent<MeshRenderer>().material = Instantiate(this.GetComponent<MeshRenderer>().material);
            if (this.bgImage != null)
            {
                this.bgImage.GetComponent<MeshRenderer>().material = Instantiate(this.bgImage.GetComponent<MeshRenderer>().material);
            }
        }

        public void SetPage(Page page)
        {
            this.page = page;
            this.annTex = page.annTex;

            this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", this.annTex);
            if (this.bgImage != null)
            {
                this.bgImage.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", page.img);
            }

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

        public void SetSelected(bool selected)
        {
            foreach (GameObject frame in this.frames)
            {
                frame.GetComponent<MeshRenderer>().material.color = selected ? this.selectedColor : this.initColor;
            }
        }

        public void SetInteractable(bool interactable)
        {
            this.GetComponent<MeshCollider>().enabled = interactable;
        }

        public void Clear()
        {
            if (this.annTex != null)
            {
                this.annTex.SetPixels(clean_colours_array);
                this.annTex.Apply();
                this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", this.annTex);
            }
        }

        public void SetBackgroundImageVisibility(bool v)
        {
            if (this.bgImage != null)
            {
                this.bgImage.gameObject.SetActive(v);
            }
        }

        void Update()
        {
            // Is the user holding down the left mouse button?
            bool mouse_held_down = Input.GetMouseButton(0);
            if (Input.GetMouseButton(0) && !no_drawing_on_current_drag && this.GetComponent<MeshCollider>().enabled)
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

                    if (results.Count <= 0)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 1000f, drawingSettings.drawingLayer))
                        {
                            // we're over the texture we're drawing on! Change them pixel colours
                            Vector2 pixelCoord = hit.textureCoord;
                            pixelCoord.x *= this.annTex.width;
                            pixelCoord.y *= this.annTex.height;
                            ChangeColourAtPoint(pixelCoord);

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
        public void ChangeColourAtPoint(Vector2 pixelCoord)
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
            this.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", this.annTex);
        }
    }
}