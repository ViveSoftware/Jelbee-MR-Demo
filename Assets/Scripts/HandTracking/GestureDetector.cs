using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using com.HTC.Common;

namespace com.HTC.Gesture
{
    public class GestureDetector : MonoBehaviour
    {
        [SerializeField]
        private bool detectOnStart = false;

        [SerializeField]
        private int curStep;

        [SerializeField]
        private IGestureDetectMethod[] detectedMethods;

        public UnityEvent StartRecognizingHandler;

        public UnityEvent RecognizingHandler;                   //整套手勢辨識中

        public UnityEventFloat RecognizingStepHandler;          //現在步驟的辨識進度

        public UnityEventInt StartWaitingNextStepHandler;       //開始等待偵測到下一個step

        public UnityEvent WaitingNextStepHandler;               //上一個步驟完成，等待下一個步驟的Update

        public UnityEventInt StartRecognizeStepHandler;         //開始一個步驟的偵測

        public UnityEvent CompleteStepHandler;                  //現在偵測的步驟完成

        public UnityEvent StopRecognizingHandler;

        public UnityEvent CancelHandler;

        public UnityEvent CompleteHandler;

        public UnityEvent ReleaseHandler;

        private float curDetectTime;                            //每個步驟的已辨識時間

        private float curDetectLostTime;                        //每個步驟內辨識遺失的目前等待時間
       
        private float curWaitNextStepTime;                      //等待下一個步驟的時間

        private Action curStateUpdateHandler = delegate { };

        private IGestureDetectMethod curDetectMethod
        {
            get
            {
                return detectedMethods[curStep];
            }
        }

        private IGestureDetectMethod nextDetectMethod
        {
            get
            {
                return detectedMethods[curStep+1];
            }
        }

        private bool isDetecting = false;

        private void Start()
        {
            if (detectOnStart)
            {
                StartDetect();
            }
        }

        public void StartDetect()
        {
            if(!isDetecting)
            {
                isDetecting = true;
                curStep = 0;
                curStateUpdateHandler = waitStart;
            }
            else
            {
                Debug.LogWarning("Gesture Detector is already detecting.");
            }
        }

        private void waitStart()
        {
            if (curDetectMethod.IsDetected())
            {
                curDetectTime = curDetectMethod.Duration;
                curDetectLostTime = curDetectMethod.DetectingToleration;
                StartRecognizingHandler.Invoke();
                StartRecognizeStepHandler.Invoke(curStep);
                curStateUpdateHandler = recognizingStep;
            }
        }

        private void recognizingStep()
        {
            curDetectTime -= Time.deltaTime;

            if (curDetectTime <= 0)
            {
                CompleteStepHandler.Invoke();
                if (curStep == detectedMethods.Length - 1)
                {
                    curStateUpdateHandler = waitStart;
                    complete();
                }
                else
                {
                    curWaitNextStepTime = nextDetectMethod.StartToleration;
                    curStateUpdateHandler = waitNextStep;
                    StartWaitingNextStepHandler.Invoke(curStep + 1);
                }
            }
            else
            {
                if (curDetectMethod.IsDetected())
                {
                    curDetectLostTime = curDetectMethod.DetectingToleration;
                    RecognizingStepHandler.Invoke(1f - curDetectTime / curDetectMethod.Duration);
                }
                else
                {
                    curDetectLostTime -= Time.deltaTime;
                    if (curDetectLostTime <= 0)
                    {
                        curStateUpdateHandler = waitStart;
                        cancel();
                    }
                }
            }
        }

        private void waitNextStep()
        {
            if (nextDetectMethod.IsDetected())
            {
                curStep = curStep + 1;
                curDetectTime = curDetectMethod.Duration;
                curStateUpdateHandler = recognizingStep;
                StartRecognizeStepHandler.Invoke(curStep);
            }
            else
            {
                WaitingNextStepHandler.Invoke();

                if (curDetectMethod.IsDetected())
                {
                    curDetectLostTime = curDetectMethod.DetectingToleration;
                    curWaitNextStepTime = nextDetectMethod.StartToleration;
                }
                else
                {
                    curDetectLostTime -= Time.deltaTime;
                    if (curDetectLostTime > 0) return;

                    curWaitNextStepTime -= Time.deltaTime;
                    if (curWaitNextStepTime > 0) return;

                    curStateUpdateHandler = waitStart;
                    cancel();
                }
            }
        }

        public void StopDetect()
        {
            if (isDetecting)
            {
                isDetecting = false;
                if (curStateUpdateHandler != waitStart)
                {
                    cancel();
                }
                curStateUpdateHandler = delegate { };
            } 
        }

        private void Update()
        {
            curDetectMethod.UpdateTime(Time.deltaTime);
            curStateUpdateHandler.Invoke();
        }

        private void complete()
        {
            curStep = 0;
            CompleteHandler.Invoke();
            StopRecognizingHandler.Invoke();  
        }

        private void cancel()
        {
            curStep = 0;
            CancelHandler.Invoke();
            StopRecognizingHandler.Invoke();  
        }
    }
}
