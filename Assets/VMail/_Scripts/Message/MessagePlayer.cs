using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VMail
{
    public class MessagePlayer : MonoBehaviour
    {
        public enum PlaybackMode { SliderFree, SliderPage, Forward, ForwardTil, Backward, BackwardTil, None };

        [SerializeField]
        private ManagerDesktop manager;

        [Space(10)]
        [SerializeField]
        private float playbackSpeed = 0.4f;
        [SerializeField]
        [Range(0f, 2f)]
        private float pagePauseTime = 1f;

        [Space(10)]
        [SerializeField]
        private Button forwardBtn;
        [SerializeField]
        private Button forwardTilBtn;

        [Space(10)]
        [SerializeField]
        private Button backwardBtn;
        [SerializeField]
        private Button backwardTilBtn;

        [Space(10)]
        [SerializeField]
        private Slider slider;

        private float prevSldierValue;
        private PlaybackMode currMode = PlaybackMode.None;
        private float pagePauseTimer;

        [Space(10)]
        [SerializeField]
        private Page pg0;
        [SerializeField]
        private Page pg1;
        Transition expoToStory;
        private float phaseTime = 1.5f;
        private float amtScale = 1.5f;
        private bool isSwitchingToStory = false;
        private float switchingProgress = 0f;

        public ViewerModeTracker tracker;


        private void Awake()
        {
            this.prevSldierValue = this.slider.value;

            this.expoToStory = new Transition();
            this.expoToStory.from = pg0;
            this.expoToStory.from.pageInfo = new PageInfo();
            this.expoToStory.from.pageInfo.viewInfo = new ViewInfo();
            this.expoToStory.to = pg1;
            this.expoToStory.to.pageInfo = new PageInfo();
            this.expoToStory.to.pageInfo.viewInfo = new ViewInfo();
        }


        public void PausePlaying()
        {
            this.currMode = PlaybackMode.None;

            this.UpdateButtonTexts();
        }

        private void SaveCurrentDataView()
        {
            Page page = this.expoToStory.from;
            page.pageInfo.viewInfo = this.manager.viewer.GetView();
            page.img = this.manager.viewer.GetImage(false);
            page.imgToNext = this.manager.viewer.GetImage(this.expoToStory.to.pageInfo.viewInfo);

            //Debug.Log("Saved the current data view... " + page.pageInfo.viewInfo.rot);
        }

        public void SaveStoryDataView()
        {
            if (this.expoToStory == null) return;

            Page page = this.expoToStory.to;
            page.pageInfo.viewInfo = this.manager.viewer.GetView();
            page.img = this.manager.viewer.GetImage(false);

            //Debug.Log("Saved the story data view... " + page.pageInfo.viewInfo.rot);
        }

        // process the button click 
        public void ProcessButtonClick(Button clicked)
        {
            if (this.tracker.isExploreMode && this.currMode == PlaybackMode.None &&
                (clicked == this.forwardBtn || clicked == this.forwardTilBtn || clicked == this.backwardBtn || clicked == this.backwardTilBtn))
            {
                ViewInfo storyView = this.expoToStory.to.pageInfo.viewInfo;
                ViewInfo currView = this.manager.viewer.GetView();

                bool isCamDiff = this.manager.viewer.visStateComparator.IsCameraLocationDiff(storyView.pos, currView.pos, storyView.rot, currView.rot);
                List<KeyValuePair<string, string>> diffs = this.manager.viewer.visStateComparator.GetVisDiffs(storyView.json, currView.json);

                if (isCamDiff || diffs == null || diffs.Count > 0)
                {
                    this.isSwitchingToStory = true;
                    this.switchingProgress = 0;
                    this.SaveCurrentDataView();

                    this.manager.viewer.SetCurrentMode(Viewer.Realtime.ViewerExploratoryVis.VisMode.Story);
                }
                else
                {
                    this.isSwitchingToStory = false;
                    this.switchingProgress = 0;
                }
            }
            else
            {
                this.isSwitchingToStory = false;
            }

            if (clicked == this.forwardBtn)
            {
                this.pagePauseTimer = 0f;
                this.currMode = this.currMode == PlaybackMode.Forward ? PlaybackMode.None : PlaybackMode.Forward;
            }
            else if (clicked == this.forwardTilBtn)
            {
                if (this.currMode == PlaybackMode.None)
                {
                    float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
                    if (prog >= 0.9999f)
                    {
                        // move to the next transition, e.g. 0.9999 -> 1.0
                        this.SetTransitionValue((int)this.GetTransitionValue() + 1f);
                    }
                }

                this.currMode = this.currMode == PlaybackMode.ForwardTil ? PlaybackMode.None : PlaybackMode.ForwardTil;
            }
            else if (clicked == this.backwardBtn)
            {
                this.pagePauseTimer = 0f;
                this.currMode = this.currMode == PlaybackMode.Backward ? PlaybackMode.None : PlaybackMode.Backward;
            }
            else if (clicked == this.backwardTilBtn)
            {
                if (this.currMode == PlaybackMode.None)
                {
                    float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
                    if (prog <= 0)
                    {
                        // move to the previous transition, e.g. 1.0 -> 0.9999
                        this.SetTransitionValue(((int)this.GetTransitionValue()) - 1f + 0.9999f);
                    }
                }

                this.currMode = this.currMode == PlaybackMode.BackwardTil ? PlaybackMode.None : PlaybackMode.BackwardTil;
            }

            this.UpdateButtonTexts();
        }

        // process the slider click 
        public void ProcessSliderClick()
        {
            this.currMode = PlaybackMode.SliderPage;

            this.UpdateButtonTexts();
        }

        public void OnSliderValueChanged()
        {
            if (this.currMode == PlaybackMode.SliderFree || this.currMode == PlaybackMode.SliderPage)
            {
                if (this.currMode == PlaybackMode.SliderPage)
                {
                    int idxPrev = (int)this.prevSldierValue;
                    int idxCurr = (int)this.GetTransitionValue();

                    if (!Input.GetMouseButtonDown(0) && idxPrev < idxCurr) // trying to move to the next page
                    {
                        //Debug.Log("slider next " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = (int)this.prevSldierValue + 0.9999f;
                        this.slider.value = (int)this.prevSldierValue + 0.9999f;
                    }
                    else if (!Input.GetMouseButtonDown(0) && idxPrev > idxCurr) // tyring to move to the previous page
                    {
                        //Debug.Log("slider prev " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = (int)this.prevSldierValue;
                        this.slider.value = (int)this.prevSldierValue;
                    }
                    else
                    {
                        //Debug.Log("slider curr " + idxPrev + " " + idxCurr);
                        this.prevSldierValue = this.slider.value;
                        this.manager.UpdateStateFromCurrentTransition();
                    }
                }
                else
                {
                    this.prevSldierValue = this.slider.value;
                    this.manager.UpdateStateFromCurrentTransition();
                }
            }
            else if (this.currMode == PlaybackMode.None)
            {
                this.prevSldierValue = this.slider.value;
                this.manager.UpdateStateFromCurrentTransition();
            }
            else
            {
                this.manager.UpdateStateFromCurrentTransition();
            }
        }

        // update the button texts depending on the playback mode
        private void UpdateButtonTexts()
        {
            this.forwardBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.Forward ? "||" : ">";
            this.forwardTilBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.ForwardTil ? "||" : ">|";

            this.backwardBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.Backward ? "||" : "<";
            this.backwardTilBtn.GetComponentInChildren<Text>().text = this.currMode == PlaybackMode.BackwardTil ? "||" : "|<";
        }

        public void Clear()
        {
            this.SetTransitionSize(0f, 0f);
        }

        public void InvokeSliderOnValueChanged()
        {
            this.slider.onValueChanged.Invoke(this.slider.value);
        }


        public void GoToPreviousPage()
        {
            this.PausePlaying();

            float prog = this.GetTransitionValue() - (int)this.GetTransitionValue();
            if (prog <= 0f)
            {
                this.SetTransitionValue((int)(this.GetTransitionValue() - 1));
            }
            else
            {
                this.SetTransitionValue((int)this.GetTransitionValue());
            }
        }

        public void GoToNextPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(((int)this.GetTransitionValue()) + 1);
        }

        public void GoToLastPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(this.slider.maxValue);
        }

        public void GoToFirstPage()
        {
            this.PausePlaying();

            this.SetTransitionValue(0);
        }


        public void SetTransitionSize(float min, float max)
        {
            if (max < min)
            {
                //Debug.LogWarning("invalid range... " + min + "~" + max);
                return;
            }

            this.slider.minValue = min;
            this.slider.maxValue = max;
        }

        public void SetTransitionValue(float val)
        {
            this.prevSldierValue = val;
            this.slider.value = val;
        }

        public void SetTransitionValueAsRatio(float ratio)
        {
            this.SetTransitionValue((this.slider.maxValue - this.slider.minValue) * ratio + this.slider.minValue);
        }

        public void SetInteractable(bool interactable)
        {
            this.slider.interactable = interactable;
        }


        public float GetTransitionValue()
        {
            return this.slider.value;
        }

        public float GetMinValue()
        {
            return this.slider.minValue;
        }

        public float GetMaxValue()
        {
            return this.slider.maxValue;
        }




        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.SaveStoryDataView();
            }

            if (this.isSwitchingToStory)
            {
                this.switchingProgress += Time.deltaTime;

                // interapolate in-between
                float amt = Mathf.Clamp(this.switchingProgress, 0f, this.phaseTime);
                amt = (amt / this.phaseTime) * this.amtScale;
                amt = Mathf.Clamp(amt, 0f, 0.999f);
                this.expoToStory.amt = amt;
                this.manager.viewer.SetState(this.expoToStory);

                if (this.switchingProgress > this.phaseTime)
                {
                    this.isSwitchingToStory = false;
                    this.switchingProgress = 0;
                    PlaybackMode mode = this.currMode;
                    this.manager.SetStoryMode(true);
                    // since the set story mode pauses the animation, resumes the animation.
                    this.currMode = mode;
                    this.UpdateButtonTexts();
                }
            }
            // slider
            else if (this.currMode == PlaybackMode.SliderFree || this.currMode == PlaybackMode.SliderPage)
            {
                this.currMode = Input.GetKey(KeyCode.LeftShift) ? PlaybackMode.SliderFree : PlaybackMode.SliderPage;

                this.UpdateButtonTexts();
            }
            // animation...
            else if (this.currMode == PlaybackMode.Backward || this.currMode == PlaybackMode.BackwardTil ||
                this.currMode == PlaybackMode.Forward || this.currMode == PlaybackMode.ForwardTil)
            {
                // determine the new slider value
                float speed = (this.currMode == PlaybackMode.Backward || this.currMode == PlaybackMode.BackwardTil) ? this.playbackSpeed * -1f : this.playbackSpeed;
                float val = this.GetTransitionValue() + (Time.deltaTime * speed);

                // handles boundary cases
                if (val < this.slider.minValue)
                {
                    val = this.slider.minValue;
                    this.PausePlaying();
                }
                else if (val > this.slider.maxValue)
                {
                    val = this.slider.maxValue;
                    this.PausePlaying();
                }
                // pause if reached the next page
                else
                {
                    int idxPrev = (int)this.prevSldierValue;
                    int idxCurr = (int)val;

                    // reached the next page
                    if (this.currMode == PlaybackMode.ForwardTil && idxPrev < idxCurr)
                    {
                        val = idxPrev + 0.9999f;
                        this.PausePlaying();
                    }
                    // reached the previous page
                    else if (this.currMode == PlaybackMode.BackwardTil && idxPrev > idxCurr)
                    {
                        val = idxPrev;
                        this.PausePlaying();
                    }
                    // reached the next page
                    if (this.currMode == PlaybackMode.Forward && idxPrev < idxCurr)
                    {
                        this.pagePauseTimer += Time.deltaTime;
                        if (this.pagePauseTimer > this.pagePauseTime) // move to the next page
                        {
                            // move to the next transition, e.g. 0.9999 -> 1.0
                            this.SetTransitionValue((int)this.GetTransitionValue() + 1f);
                            this.pagePauseTimer = 0f;
                        }
                        else // stay put and increase the page timer
                        {
                            val = idxPrev + 0.9999f;
                        }
                    }
                    // reached the previous page
                    else if (this.currMode == PlaybackMode.Backward && idxPrev > idxCurr)
                    {
                        this.pagePauseTimer += Time.deltaTime;
                        if (this.pagePauseTimer > this.pagePauseTime) // move to the previous page
                        {
                            // move to the previous transition, e.g. 1.0 -> 0.9999
                            this.SetTransitionValue(((int)this.GetTransitionValue()) - 1f + 0.9999f);
                            this.pagePauseTimer = 0f;
                        }
                        else // stay put and increase the page timer
                        {
                            val = idxPrev;
                        }
                    }
                }

                // update the transition value
                this.prevSldierValue = this.slider.value;
                this.slider.value = val;
            }
            else
            {
                this.prevSldierValue = this.slider.value;
            }
        }

    }
}