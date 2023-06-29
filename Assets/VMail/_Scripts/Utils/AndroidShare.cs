using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VMail.Utils.Android
{
    public class AndroidShare : MonoBehaviour
    {
        private bool isFocus = false;
        private bool isProcessing = false;
        
        void OnApplicationFocus(bool focus)
        {
            this.isFocus = focus;
        }

        public void ShareText(string shareSubject, string shareMessage)
        {

#if UNITY_ANDROID
            if (!isProcessing)
            {
                StartCoroutine(ShareTextInAnroid(shareSubject, shareMessage));
            }
#else
		Debug.Log("No sharing set up for this platform.");
#endif
        }

#if UNITY_ANDROID
        public IEnumerator ShareTextInAnroid(string shareSubject, string shareMessage)
        {
            this.isProcessing = true;

            if (!Application.isEditor)
            {
                //Create intent for action send
                AndroidJavaClass intentClass =
                    new AndroidJavaClass("android.content.Intent");
                AndroidJavaObject intentObject =
                    new AndroidJavaObject("android.content.Intent");
                intentObject.Call<AndroidJavaObject>
                    ("setAction", intentClass.GetStatic<string>("ACTION_SEND"));

                //put text and subject extra
                intentObject.Call<AndroidJavaObject>("setType", "text/plain");
                intentObject.Call<AndroidJavaObject>
                    ("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), shareSubject);
                intentObject.Call<AndroidJavaObject>
                    ("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareMessage);

                //call createChooser method of activity class
                AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity =
                    unity.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject chooser =
                    intentClass.CallStatic<AndroidJavaObject>
                    ("createChooser", intentObject, "Share your high score");
                currentActivity.Call("startActivity", chooser);
            }

            yield return new WaitUntil(() => this.isFocus);
            this.isProcessing = false;
        }
#endif

    }
}