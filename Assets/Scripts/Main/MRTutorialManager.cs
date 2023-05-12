using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using com.HTC.Common;
using System;

public class MRTutorialManager : Singleton<MRTutorialManager>
{
    [Serializable]
    public class TutorialDefine
    {
        public string Tutorial;
        public string HandHintPivot;
        public string VoicePlayer;
        public string Clip;
        public float Duration;
    }

    [SerializeField] private string robotPivotKey = "Desk";
    [SerializeField] private Vector3 pivotOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] private RobotAssistantEnums.FacialAnimationIndex[] faces;
    [SerializeField] private HandHintManager handHintMgr;
    [SerializeField] private TutorialDefine[] tutorialDefines;

    private string currentTutorial;
    private Dictionary<string, TutorialDefine> tutorialDefMap = new Dictionary<string, TutorialDefine>();

    public Action OnTutorialStartHandler;
    public Action OnTutorialEndHandler;
    public bool IsTutorialPlaying { get; private set; }

    private Action onTutorialCompletedCallback = delegate { };
    private Vector3 robotPivot;
    
    void Start()
    {
        tutorialDefMap = new Dictionary<string, TutorialDefine>();
        foreach(TutorialDefine def in tutorialDefines)
        {
            tutorialDefMap[def.Tutorial] = def; 
        }

        Transform pivot = PivotManager.Instance.GetPivot(robotPivotKey);
        robotPivot = pivot.position + pivotOffset;
    }

    public void ShowTutorial(string tutorialName, bool flyToPivot, Action onTutorialCompleted = null)
    {
        if (IsTutorialPlaying)
        {
            CancelInvoke();
            InvokeTutorialCompleted();
        }

        currentTutorial = tutorialName;
        IsTutorialPlaying = true;

        Debug.Log($"show tutorial: {tutorialName}");

        if (onTutorialCompleted != null)
        {
            onTutorialCompletedCallback += onTutorialCompleted;
        }

        if (flyToPivot)
        {
            MoveRobotToPivot();
        }
        else
        {
            ShowCurrentTutorial();
        }

        OnTutorialStartHandler?.Invoke();
    }

    private void MoveRobotToPivot()
    {
        RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(robotPivot, ShowCurrentTutorial);
    }

    private void ShowCurrentTutorial()
    { 
        if(!string.IsNullOrEmpty(currentTutorial))
        {
            TutorialDefine tutorial = null;
            if(tutorialDefMap.TryGetValue(currentTutorial, out tutorial))
            {
                RobotAssistantManager.robotAssistantManagerInstance.TriggerLeisure();
                //RobotAssistantManager.robotAssistantManagerInstance.OnChangeFacial(faces[UnityEngine.Random.Range(0, faces.Length)]);
                Transform handHintPivot = BoxPivotManager.Instance.Get(tutorial.HandHintPivot);
                handHintMgr.Show(tutorial.Tutorial, handHintPivot);
                Invoke("InvokeTutorialCompleted", tutorial.Duration);
            }
            else
            {
                Debug.LogError($"Tutorial: [{currentTutorial}] is not exist!");
            }
        }
        else
        {
            Debug.LogError($"Tutorial ID cannot be empty!");
        }

    }

    private void InvokeTutorialCompleted()
    {
        RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        IsTutorialPlaying = false;
        onTutorialCompletedCallback.Invoke();
        OnTutorialEndHandler?.Invoke();
        currentTutorial = null;
        handHintMgr.Hide();
    }

    public void ForceStopTutorial()
    {
        if(IsTutorialPlaying)
        {
            CancelInvoke();
            InvokeTutorialCompleted();
        }
    }
}
