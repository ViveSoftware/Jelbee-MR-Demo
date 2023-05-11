using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Gesture
{
    [RequireComponent(typeof(GestureDetector))]
    public class GestureDetectorEventHook : MonoBehaviour
    {
        [SerializeField]
        private string detectorID;

        private GestureDetector detector;

        public void Awake()
        {
            detector = GetComponent<GestureDetector>();
        }

        private void OnEnable()
        {
            GestureDetectorEventCenter.RegisterHandler(GestureDetectorEventCenter.ActionTypeEnum.StartDetect, detectorID, startDetect);
            GestureDetectorEventCenter.RegisterHandler(GestureDetectorEventCenter.ActionTypeEnum.StopDetect, detectorID, stopDetect);
            detector.StartRecognizingHandler.AddListener(startRecognizing);
            detector.RecognizingHandler.AddListener(recognizing);
            detector.StopRecognizingHandler.AddListener(stopRecognizing);
            detector.CancelHandler.AddListener(cancel);
            detector.CompleteHandler.AddListener(complete);
        }

        private void OnDisable()
        {
            GestureDetectorEventCenter.UnregisterHandler(GestureDetectorEventCenter.ActionTypeEnum.StartDetect, detectorID, startDetect);
            GestureDetectorEventCenter.UnregisterHandler(GestureDetectorEventCenter.ActionTypeEnum.StopDetect, detectorID, stopDetect);
            detector.StartRecognizingHandler.RemoveListener(startRecognizing);
            detector.RecognizingHandler.RemoveListener(recognizing);
            detector.StopRecognizingHandler.RemoveListener(stopRecognizing);
            detector.CancelHandler.RemoveListener(cancel);
            detector.CompleteHandler.RemoveListener(complete);
        }

        private void startDetect() => detector.StartDetect();
        private void stopDetect() => detector.StopDetect();
        private void startRecognizing() => GestureDetectorEventCenter.InvokeHandler(GestureDetectorEventCenter.ActionTypeEnum.StartRecognizing, detectorID);
        private void recognizing() => GestureDetectorEventCenter.InvokeHandler(GestureDetectorEventCenter.ActionTypeEnum.Recognizing, detectorID);
        private void stopRecognizing() => GestureDetectorEventCenter.InvokeHandler(GestureDetectorEventCenter.ActionTypeEnum.StopRecognizing, detectorID);
        private void cancel() => GestureDetectorEventCenter.InvokeHandler(GestureDetectorEventCenter.ActionTypeEnum.Cancel, detectorID);
        private void complete() => GestureDetectorEventCenter.InvokeHandler(GestureDetectorEventCenter.ActionTypeEnum.Complete, detectorID);
    }
}
