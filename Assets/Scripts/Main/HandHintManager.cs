using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandHintManager : MonoBehaviour
{
    [SerializeField]
    private Transform handPivot;

    [SerializeField]
    private Animator handAnimator;

    [SerializeField]
    private List<HandHintDefine> defines;

    private Dictionary<string, HandHintDefine> defineMap;
    private HandHintDefine currentHandHint;

    private void Start()
    {
        defineMap = new Dictionary<string, HandHintDefine>();
        foreach (HandHintDefine define in defines)
        {
            defineMap[define.TutorialID] = define;
        }
    }

    public void Show(string tutorialID, Transform pivot)
    {
        handPivot.position = pivot.position;
        handPivot.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(pivot.forward, Vector3.up), Vector3.up);

        if (currentHandHint != null)
        {
            handAnimator.SetBool(currentHandHint.AnimationKey, false);
        }

        if(defineMap.TryGetValue(tutorialID, out currentHandHint))
        {
            handAnimator.SetBool(currentHandHint.AnimationKey, true);
        }
    }

    public void Hide()
    {
        if (currentHandHint != null)
        {
            handAnimator.SetBool(currentHandHint.AnimationKey, false);
            currentHandHint = null;
        }
    }
}

[Serializable]
public class HandHintDefine
{
    public string TutorialID;
    public string AnimationKey;
}
