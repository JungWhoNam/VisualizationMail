using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

namespace VMail.Utils
{
    public class VideoPreviewGenerator : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        public List<Preview> previews { get; private set; }

        private long currFrame = -1;
        public bool isDone { get; private set; }

        public UnityEvent onGenerated;

        private void Start()
        {
            this.previews = new List<Preview>();
        }

        private void Initialize()
        {
            this.videoPlayer = null;
            this.previews.Clear();
            this.currFrame = -1;
            this.isDone = false;
        }

        private void Update()
        {
            if (!this.isDone && this.videoPlayer != null && this.videoPlayer.isPrepared && this.previews.Count > 0)
            {
                if (this.currFrame == -1)
                {
                    bool allDone = true;
                    foreach (Preview preview in this.previews)
                    {
                        if (preview.image == null)
                        {
                            Debug.Log("generating the preview image for the frame: " + preview.frameIdx);
                            this.currFrame = preview.frameIdx;
                            this.videoPlayer.frame = preview.frameIdx;
                            allDone = false;
                            break;
                        }
                    }

                    if (allDone)
                    {
                        Debug.Log("Job Done for " + this.previews.Count + " frames.");
                        this.isDone = true;
                        this.videoPlayer.seekCompleted -= this.OnSeekCompleted;
                        this.onGenerated.Invoke();
                    }
                }
            }
        }

        public void Generate(VideoPlayer videoPlayer, List<long> frameIndice)
        {
            Debug.Log("generating " + frameIndice.Count + " previews. Frame Count: " + videoPlayer.frameCount);
            // initialize the process
            this.Initialize();
            this.videoPlayer = videoPlayer;
            for (int i = 0; i < frameIndice.Count; i++)
            {
                this.previews.Add(new Preview(frameIndice[i], null));
            }
            this.videoPlayer.seekCompleted += this.OnSeekCompleted;
            this.videoPlayer.Pause();
        }

        private void OnSeekCompleted(VideoPlayer source)
        {
            Debug.Log("OnSeekCompleted: " + source.frame);
            long frameIdx = source.frame;
            if (frameIdx >= 0 && this.currFrame == frameIdx)
            {
                Preview preview = this.GetPreview(frameIdx);
                RenderTexture rt = this.videoPlayer.texture as RenderTexture;
                preview.image = Tools.GetRTPixels(rt);
                this.currFrame = -1;
                Debug.Log("seek done: added a preview image at the frame: " + frameIdx + " / " + source.frameCount);
            }
        }

        private Preview GetPreview(long frameIdx)
        {
            foreach (Preview preview in this.previews)
            {
                if (preview.frameIdx == frameIdx)
                {
                    return preview;
                }
            }
            return null;
        }

        public class Preview
        {
            public long frameIdx;
            public Texture2D image;

            public Preview(long frameIdx, Texture2D image)
            {
                this.frameIdx = frameIdx;
                this.image = image;
            }
        }

    }
}