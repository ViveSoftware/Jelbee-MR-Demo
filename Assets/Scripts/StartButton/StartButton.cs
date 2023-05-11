using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using UnityEngine.Playables;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using com.HTC.Common;
using static PivotManager;

public class StartButton : Singleton<StartButton>
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
{
    [SerializeField] private GameObject fullButton;
    [SerializeField] private PlayableDirector dragTimeline;
    [SerializeField] private Transform downPivot;
    [SerializeField] private Transform upPivot;
    [SerializeField] private AudioSource clickAudio;
    private bool isHover = false;
    private bool buttonPressed = false;
    private bool gameStart = false;
    private List<Transform> rolePos = new List<Transform>();
    private float buttonDistance = 0;

    private void Start()
    {
        Transform deskPivot = PivotManager.Instance.GetPivot("Desk");
        fullButton.transform.position = deskPivot.position;
        fullButton.transform.DOScale(new Vector3(1f,1f,1f),1f).SetEase(Ease.OutBack);
        buttonDistance = Vector3.Distance(upPivot.position, downPivot.position);
    }

    private void OnEnable()
    {        
        dragTimeline.stopped += OnPlayableDirectorStopped;
    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (!buttonPressed && eventData.TryGetEventCaster(out ViveColliderEventCaster caster))
        {
            if (!isHover)
            {
                isHover = true;
                dragTimeline.time = 0;
                dragTimeline.Evaluate();
                rolePos.Clear();
                rolePos.Add(caster.transform);
            }
            else
            {
                if (!rolePos.Contains(caster.transform))
                {
                    rolePos.Add(caster.transform);
                }
            }
            
        }

    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        if (!buttonPressed) 
        {
            if (eventData.TryGetEventCaster(out ViveColliderEventCaster caster) && rolePos.Contains(caster.transform))
            {
                rolePos.Remove(caster.transform);
                if (rolePos.Count == 0)
                {
                    isHover = false;
                }
            }

            if (dragTimeline.time < dragTimeline.duration * 0.5f * 0.3f + 0.5f)
            {
                StartCoroutine(ButtonPressed());
            }
            else
            {
                dragTimeline.time = 0;
                dragTimeline.Evaluate();
            }
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space) && !buttonPressed)
        {
            StartCoroutine(ButtonPressed());
        }
#endif
        if (isHover && !buttonPressed)
        {
            float value = Vector3.Distance(CommonFormula.PointFromPointToLine(upPivot.position, downPivot.position, rolePos[0].position), downPivot.position);
            for (int i = 1; i < rolePos.Count; i++)
            {
                float newValue = Vector3.Distance(CommonFormula.PointFromPointToLine(upPivot.position, downPivot.position, rolePos[i].position), downPivot.position);
                if (value > newValue)
                {
                    value = newValue;
                }
            }

            float dis = Mathf.InverseLerp(0, buttonDistance, value);
            if (dis >= 0 && dis <= 1)
            {
                if (dis < 0.3f)
                {
                    StartCoroutine(ButtonPressed());
                }
                else
                {
                    dragTimeline.time = dragTimeline.duration * dis + 0.5f;
                    dragTimeline.Evaluate();
                }
            }
        }
    }

    IEnumerator ButtonPressed() 
    {
        buttonPressed = true;
        dragTimeline.Play();
        isHover = false;
        clickAudio.Play();
        yield return new WaitForSeconds(0.5f);
        fullButton.transform.DOScale(Vector3.zero, 0.5f);
    }

    void OnPlayableDirectorStopped(PlayableDirector director)
    {
        if (dragTimeline == director)
        {
            gameStart = true;
        }
    }

    private void OnDisable()
    {
        dragTimeline.stopped -= OnPlayableDirectorStopped;
    }

    public bool GameStart()
    {
        return gameStart;
    }

    public void Restart()
    {
        gameStart = false;
        dragTimeline.time = 0;
        dragTimeline.Evaluate();
        rolePos.Clear();
    }
}
