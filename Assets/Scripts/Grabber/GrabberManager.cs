using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HTC.UnityPlugin.Vive;
using com.HTC.Common;
using System;
using Random = UnityEngine.Random;
using com.HTC.Gesture;

public class GrabberManager : Singleton<GrabberManager>
{
    [Serializable]
    public class GrabbableItemDefine
    {
        public string Group;
        public IGestureDetectMethod[] Gestures;
        public string PoolID;
    }

    private class GrabbableItemDefineGroup
    {
        private Dictionary<string, List<GrabbableItemDefine>> grabbableDefineMap;
        private List<string> enabledGroups;
        private string prefabPoolID;

        public GrabbableItemDefineGroup(List<GrabbableItemDefine> defines)
        {
            grabbableDefineMap = new Dictionary<string, List<GrabbableItemDefine>>();
            foreach (GrabbableItemDefine define in defines)
            {
                if (!grabbableDefineMap.ContainsKey(define.Group))
                {
                    grabbableDefineMap[define.Group] = new List<GrabbableItemDefine>();
                }
                grabbableDefineMap[define.Group].Add(define);
            }
            enabledGroups = new List<string>();
        }

        public void EnableGroup(string groupName, bool isDisableOther)
        {
            if (isDisableOther)
            {
                enabledGroups.Clear();
            }

            if (!enabledGroups.Contains(groupName))
            {
                enabledGroups.Add(groupName);
            }
        }

        public void DisableGroup(string groupName)
        {
            enabledGroups.Remove(groupName);
        }

        public bool IsGestureDetected()
        {
            string groupName = "";
            List<GrabbableItemDefine> defines = null;
            for (int i = 0; i < enabledGroups.Count; ++i)
            {
                groupName = enabledGroups[i];
                if (grabbableDefineMap.TryGetValue(groupName, out defines))
                {
                    for (int j = 0; j < defines.Count; ++j)
                    {
                        for(int k = 0; k< defines[j].Gestures.Length; ++k)
                        {
                            if(defines[j].Gestures[k].IsDetected())
                            {
                                prefabPoolID = defines[j].PoolID;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public string GetPrefabPoolID()
        {
            return prefabPoolID;
        }
    }

    [SerializeField] private GrabberTracker leftGrabber;
    [SerializeField] private List<GrabbableItemDefine> leftGrabbableDefines;
    [Space]
    [SerializeField] private GrabberTracker rightGrabber;
    [SerializeField] private List<GrabbableItemDefine> rightGrabbableDefines;
    [Space]
    [SerializeField] private GameObject trackedHanderGrabber;
    [SerializeField] private IGrabbableItemPool[] grabbablePool;

    private GrabbableItemDefineGroup leftDefineGroup;
    private GrabbableItemDefineGroup rightDefineGroup;

    private Action<string, bool> enableGroupHandler;
    private Action<string> disableGroupHandler;

    private Dictionary<string, IGrabbableItemPool> grabbablePoolMap;

    private void Start()
    {
        Init();
        TurnOff();
    }

    private void Init()
    {
        leftDefineGroup = new GrabbableItemDefineGroup(leftGrabbableDefines);
        leftGrabber.IsGestrureDetected = leftDefineGroup.IsGestureDetected;
        enableGroupHandler += leftDefineGroup.EnableGroup;
        disableGroupHandler += leftDefineGroup.DisableGroup;

        rightDefineGroup = new GrabbableItemDefineGroup(rightGrabbableDefines);
        rightGrabber.IsGestrureDetected = rightDefineGroup.IsGestureDetected;
        enableGroupHandler += rightDefineGroup.EnableGroup;
        disableGroupHandler += rightDefineGroup.DisableGroup;

        grabbablePoolMap = new Dictionary<string, IGrabbableItemPool>();
        foreach (IGrabbableItemPool itemPool in grabbablePool)
        {
            grabbablePoolMap[itemPool.PoolID] = itemPool;
            itemPool.Init();
        }
    }

    public void EnableGroup(string groupName, bool disableOthers = true)
    {
        enableGroupHandler.Invoke(groupName, disableOthers);
    }

    public void DisableGroup(string groupName = null)
    {
        disableGroupHandler.Invoke(groupName);
    }

    [Button]
    public void TurnOn()
    {
        leftGrabber.enabled = true;
        rightGrabber.enabled = true;
        trackedHanderGrabber.gameObject.SetActive(true);
    }

    [Button]
    public void TurnOff()
    {
        leftGrabber.enabled = false;
        rightGrabber.enabled = false;
        trackedHanderGrabber.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    [SerializeField] private string testGroup;
    [Button]
    public void EnableTestGroup()
    {
        EnableGroup(testGroup, true);
    }
#endif

    public void OnGrabbingStart(GrabberTracker grabber)
    {
        string prefabPoolID = "";
        if (grabber == leftGrabber)
        {
            prefabPoolID = leftDefineGroup.GetPrefabPoolID();
        } 
        else
        {
            prefabPoolID = rightDefineGroup.GetPrefabPoolID();
        }

        IGrabbableItem newItem = grabbablePoolMap[prefabPoolID].GetItem(leftGrabber.Pivot);
        if(newItem != null)
        {
            grabber.SetGrabItem(newItem);
        }        
    }

    public void OnGrabbingEnd(GrabberTracker grabber)
    {
        grabber.SetGrabItem(null);
    }

    public void OnGrabbingFailed(GrabberTracker grabber)
    {
        grabber.SetGrabFailed();
    }

    public T GetGrabbingItem<T>() where T : IGrabbableItem
    {
        if (leftGrabber.GrabbingItem != null && leftGrabber.GrabbingItem is T) return leftGrabber.GrabbingItem as T;
        if (rightGrabber.GrabbingItem != null && rightGrabber.GrabbingItem is T) return rightGrabber.GrabbingItem as T;
        return null;
    }
}
