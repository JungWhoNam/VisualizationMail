using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

namespace VMail.Utils
{
    public class CustomVideoCreator : MonoBehaviour
    {
        public VideoPreviewGenerator previewGenerator;
        public MyCustomVideoEvent onVideoCreated;

        private VideoPlayer videoPlayer;
        private float frameRate;

        public void Create(string videoFilePath, float newFrameRate)
        {
            if (this.videoPlayer != null)
            {
                Destroy(this.videoPlayer);
            }
            this.frameRate = newFrameRate;
            this.videoPlayer = this.gameObject.AddComponent<VideoPlayer>();
            this.videoPlayer.url = videoFilePath;
            this.videoPlayer.prepareCompleted += EndPreparation;
            this.videoPlayer.Play();
        }

        private void EndPreparation(VideoPlayer vp)
        {
            Debug.Log("end preparation");

            // update the display area size.
            this.videoPlayer.Play();

            // get frame indice
            List<long> interestedFrames = new List<long>();
            float duration = this.videoPlayer.frameCount / this.videoPlayer.frameRate;
            int frameCount = (int)(duration * this.frameRate);
            for (int i = 0; i < frameCount; i++)
            {
                float time = i / ((float)frameCount - 1) * duration;
                // long idx = (long)Mathf.Round(time * (this.videoPlayer.frameCount - 1) / duration);
                long idx = (long) (time * (this.videoPlayer.frameCount - 0.001f) / duration);
                interestedFrames.Add(idx);
            }

            //
            this.previewGenerator.Generate(this.videoPlayer, interestedFrames);
        }

        public void OnPreviewGenerated()
        {
            // create a custom video
            CustomVideo video = new CustomVideo();
            video.url = this.videoPlayer.url;
            video.frameRate = this.frameRate;

            float duration = this.videoPlayer.frameCount / this.videoPlayer.frameRate;
            video.times = new List<float>();
            video.images = new List<Texture2D>();
            foreach (VideoPreviewGenerator.Preview preview in this.previewGenerator.previews)
            {
                float t = ((float)preview.frameIdx / ((float)this.videoPlayer.frameCount - 1)) * duration;
                video.times.Add(t);
                video.images.Add(preview.image);
            }

            Destroy(this.videoPlayer);
            this.frameRate = 0;
            this.onVideoCreated.Invoke(video);
        }

    }

    public class CustomVideo
    {
        public string url;
        public float frameRate;

        public List<float> times;
        public List<Texture2D> images;
    }

    [System.Serializable]
    public class MyCustomVideoEvent : UnityEvent<CustomVideo> { }

}