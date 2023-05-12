using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Gesture
{
    public static class GestureDetectorEventCenter
    {
        public class ActionHandler
        {
            public Dictionary<string, Action> Map = new Dictionary<string, Action>();

            public void RegisterActionMap(string key, Action value)
            {
                if (!Map.ContainsKey(key))
                {
                    Map[key] = delegate { };
                }
                Map[key] += value;
            }

            public void UnregisterActionMap(string key, Action value)
            {
                if (Map.ContainsKey(key))
                {
                    Map[key] -= value;
                }
            }

            public void Invoke(string key)
            {
                if (Map.ContainsKey(key))
                {
                    Map[key].Invoke();
                }
            }
        }

        public enum ActionTypeEnum
        {
            StartDetect,
            StopDetect,
            StartRecognizing,
            StopRecognizing,
            Recognizing,
            Complete,
            Cancel
        }

        private static Dictionary<ActionTypeEnum, ActionHandler> actionMap = new Dictionary<ActionTypeEnum, ActionHandler>()
        {
            {ActionTypeEnum.StartDetect,        new ActionHandler() },
            {ActionTypeEnum.StopDetect,         new ActionHandler() },
            {ActionTypeEnum.StartRecognizing,   new ActionHandler() },
            {ActionTypeEnum.StopRecognizing,    new ActionHandler() },
            {ActionTypeEnum.Recognizing,        new ActionHandler() },
            {ActionTypeEnum.Complete,           new ActionHandler() },
            {ActionTypeEnum.Cancel,             new ActionHandler() }
        };

        public static void RegisterHandler(ActionTypeEnum actionType, string detectorID, Action handler)
        {
            actionMap[actionType].RegisterActionMap(detectorID, handler);
        }

        public static void UnregisterHandler(ActionTypeEnum actionType, string detectorID, Action handler)
        {
            actionMap[actionType].UnregisterActionMap(detectorID, handler);
        }

        public static void InvokeHandler(ActionTypeEnum actionType, string detectorID)
        {
            actionMap[actionType].Invoke(detectorID);
        }
    }
}
