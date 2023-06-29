using System;
using System.IO;

using UnityEngine;

namespace VMail.Utils.MessageFile
{
    public class ThumbnailFileGenerator : MonoBehaviour
    {
        public static readonly string ThumbnailDirName = "thumbnails";
        public static readonly string ImgNamePrefix = "img_";
        public static readonly string Img360NamePrefix = "img360_";
        public static readonly string ImgToNextNamePrefix = "nextImg_";
        public static readonly string AnnImgNamePrefix = "annImg_";


        public static void SaveThumbnailFiles(Message message, string msgDirPath, bool save360Images = true)
        {
            // create the thumbnail direction, if it does not exist.
            string dirPath = msgDirPath + "/" + ThumbnailDirName;
            if (Directory.Exists(dirPath))
            {
                Directory.Delete(dirPath, true);
            }
            Directory.CreateDirectory(dirPath);

            // save the thumbnail image per page
            for (int i = 0; i < message.pages.Count; i++)
            {
                Page page = message.pages[i];

                {
                    Texture2D tex = page.GetThumbnailImage(false);
                    if (tex == null)
                    {
                        page.pageInfo.img.fPath = string.Empty;
                    }
                    else
                    {
                        string relPath = ThumbnailDirName + "/" + ImgNamePrefix + i + ".png";
                        Tools.SaveTexture2D(msgDirPath + "/" + relPath, tex);
                        page.pageInfo.img.fPath = relPath;
                    }
                }
                if (save360Images)
                {
                    Texture2D tex = page.GetThumbnailImage(true);
                    if (tex == null)
                    {
                        page.pageInfo.img360.fPath = string.Empty;
                    }
                    else
                    {
                        string relPath = ThumbnailDirName + "/" + Img360NamePrefix + i + ".png";
                        Tools.SaveTexture2D(msgDirPath + "/" + relPath, tex);
                        page.pageInfo.img360.fPath = relPath;
                    }
                }
                {
                    Texture2D tex = page.annTex;
                    if (tex == null)
                    {
                        page.pageInfo.ann.imgFilePath = string.Empty;
                    }
                    else
                    {
                        string relPath = ThumbnailDirName + "/" + AnnImgNamePrefix + i + ".png";
                        Tools.SaveTexture2D(msgDirPath + "/" + relPath, tex);
                        page.pageInfo.ann.imgFilePath = relPath;
                    }
                }
                {
                    Texture2D tex = page.imgToNext;
                    if (tex == null)
                    {
                        page.pageInfo.imgToNext.fPath = string.Empty;
                    }
                    else
                    {
                        string relPath = ThumbnailDirName + "/" + ImgToNextNamePrefix + i + ".png";
                        Tools.SaveTexture2D(msgDirPath + "/" + relPath, tex);
                        page.pageInfo.imgToNext.fPath = relPath;
                    }
                }
            }
        }

        public static void SaveThumbnailFiles(MessageMobile message, string msgDirPath, bool save360Images = true)
        {
            // create the thumbnail direction, if it does not exist.
            string dirPath = msgDirPath + "/" + ThumbnailDirName;
            if (!Directory.Exists(dirPath))
            {
                Debug.LogWarning("the directory does not exists: " + dirPath);
                return;
            }

            // update the annotation image per page
            for (int i = 0; i < message.pages.Count; i++)
            {
                Page page = message.pages[i];

                string fPath = msgDirPath + "/" + page.pageInfo.ann.imgFilePath;

                if (File.Exists(fPath))
                {
                    Tools.SaveTexture2D(fPath, page.annTex);
                }
                else
                {
                    Debug.LogWarning("the files does not exists: " + fPath);
                }
            }
        }

    }
}