using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//using SFB;

namespace VMail.Utils
{
    public class Tools
    {
        // https://docs.unity3d.com/ScriptReference/EventSystems.IDragHandler.html
        static public T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null) return null;
            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;
            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }
            return comp;
        }

        // https://docs.unity3d.com/ScriptReference/RenderTexture-active.html
        static public Texture2D GetRTPixels(RenderTexture rt)
        {
            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
            tex.Apply();

            // Restorie previously active render texture
            RenderTexture.active = currentActiveRT;
            return tex;
        }

        static public Texture2D GetRTPixels2(RenderTexture rt)
        {
            /* RenderTexture.active = rt;                      //Set my RenderTexture active so DrawTexture will draw to it.
             GL.PushMatrix();                                //Saves both projection and modelview matrices to the matrix stack.
             GL.LoadPixelMatrix(0, 512, 512, 0);            //Setup a matrix for pixel-correct rendering.
                                                            //Draw my stampTexture on my RenderTexture positioned by posX and posY.
             Graphics.DrawTexture(new Rect(posX - stampTexture.width / 2, (rt.height - posY) - stampTexture.height / 2, stampTexture.width, stampTexture.height), stampTexture);
             GL.PopMatrix();                                //Restores both projection and modelview matrices off the top of the matrix stack.
             RenderTexture.active = null;                    //De-activate my RenderTexture.*/

            // Remember currently active render texture
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one
            RenderTexture.active = rt;

            //Saves both projection and modelview matrices to the matrix stack.
            GL.PushMatrix();
            //Setup a matrix for pixel-correct rendering.
            GL.LoadPixelMatrix(0, rt.width, rt.height, 0);

            // Create a new Texture2D and read the RenderTexture image into it
            Texture2D tex = new Texture2D(rt.width, rt.height);
            int posX = 0;
            int posY = 0;
            //Graphics.DrawTexture(new Rect(posX - tex.width / 2, (rt.height - posY) - tex.height / 2, tex.width, tex.height), tex);
            Graphics.DrawTexture(new Rect(0, 0, tex.width, tex.height), tex);
            tex.Apply();

            //Restores both projection and modelview matrices off the top of the matrix stack.
            GL.PopMatrix();

            // Restorie previously active render texture
            RenderTexture.active = currentActiveRT;
            return tex;
        }
        
        static public Texture2D LoadTexture2D(string fPath)
        {
            byte[] imageData = File.ReadAllBytes(fPath);
            Texture2D thumbnail = new Texture2D(2, 2);
            thumbnail.LoadImage(imageData);
            return thumbnail;
        }

        static public void SaveTexture2D(string fPath, Texture2D tex)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fPath, bytes);
        }

        static public Texture2D Crop(Texture2D input, int x, int y, int w, int h)
        {
            Color[] colors = input.GetPixels(x, y, w, h);
            Texture2D tex = new Texture2D(w, h);
            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        static public Texture2D Resize(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / (float)targetWidth);
            float incY = (1.0f / (float)targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        static public bool SaveJsonFile(string fPath, string str)
        {
            if (!Path.GetExtension(fPath).Equals(".json"))
            {
                Debug.LogWarning("error on saving a json file");
                return false;
            }

            StreamWriter writer = new StreamWriter(fPath, false);
            writer.WriteLine(str);
            writer.Close();
            return true;
        }

        static public string ReadJsonFile(string fPath)
        {
            // fPath = "C://Users/Jung/AppData/LocalLow/Aviz/EverydaySciVIS/VisMessages/Three/msg.json";
            if (!File.Exists(fPath))
            {
                Debug.LogWarning("not existed..." + fPath);
                return null;
            }

            if (!Path.GetExtension(fPath).Equals(".json"))
            {
                Debug.LogWarning("error on reading a json file");
                return null;
            }

            StreamReader reader = new StreamReader(fPath);
            string str = reader.ReadToEnd();
            reader.Close();
            return str;
        }

        /*static public string OpenFilePanel(string dirPath)
        {
            var extensions = new[] { new ExtensionFilter("JSON Files", "json") };
            string[] selected = StandaloneFileBrowser.OpenFilePanel("Open a File", dirPath, extensions, false);

            if (selected == null || selected.Length <= 0)
            {
                return null;
            }
            else
            {
                return selected[0];
            }
        }*/

        static public Texture2D CreateAnnotationTexture(Vector2Int size)
        {
            Texture2D tex = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false, true);
            Color Reset_Colour = new Color(0f, 0f, 0f, 0f);  // By default, reset the canvas to be transparent
            //Color Reset_Colour = new Color(1f, 1f, 1f, 0.2f);  // By default, reset the canvas to be transparent
            Color[] clean_colours_array = new Color[size.x * size.y];
            for (int x = 0; x < clean_colours_array.Length; x++)
            {
                clean_colours_array[x] = Reset_Colour;
            }
            tex.SetPixels(clean_colours_array);
            tex.Apply();

            return tex;
        }

        /*static public string OpenDirPanel(string dirPath)
        {
            string[] selected = StandaloneFileBrowser.OpenFolderPanel("Select a Directory", dirPath, false);

            if (selected == null || selected.Length <= 0)
            {
                return null;
            }
            else
            {
                return selected[0];
            }
        }*/

        /*static public string SaveFilePanel(string dirPath)
        {
            var extensions = new[] { new ExtensionFilter("JSON Files", "json") };
            string selected = StandaloneFileBrowser.SaveFilePanel("Save File", dirPath, "VisMsg", extensions);

            if (string.IsNullOrEmpty(selected))
            {
                return null;
            }
            else
            {
                return selected;
            }
        }*/

        // function to find the number closest to n and divisible by m
        // https://www.geeksforgeeks.org/find-number-closest-n-divisible-m/
        static int GetClosestNumber(int n, int m)
        {
            // find the quotient 
            int q = n / m;
            // 1st possible closest number 
            int n1 = m * q;
            // 2nd possible closest number 
            int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));
            // if true, then n1 is the required closest number 
            if (Mathf.Abs(n - n1) < Mathf.Abs(n - n2))
            {
                return n1;
            }
            else // else n2 is the required closest number
            {
                return n2;
            }
        }

        // https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true
        // the directory needs to be emptied before being deleted.
        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

    }
}