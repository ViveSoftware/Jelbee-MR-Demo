using com.HTC.Common;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class WhackAMole : Singleton<WhackAMole>
{
    public GameObject[] Hole;
    private Vector3[] holeOrginScale;

    public Transform[] HolePivot;
    private float height;

    [SerializeField] private float welcomeToReadyTime = 0.5f;
    [SerializeField] private float everyRoundTime = 2.0f;

    private float timer;
    private int knockTimes = 0;
    public WhackAMoleGameState GameState = WhackAMoleGameState.Init;

    public AudioSource SFX;
    public AudioClip Knock;

    [SerializeField] private GameObject[] occlusionObjs;
    private Action<bool> setOcclusionObjActivated = delegate { };

    private bool changing = false;

    public enum WhackAMoleGameState
    {
        Init,
        Ready,
        Start,
        End
    }

    protected override void AwakeSingleton()
    {
        GameState = WhackAMoleGameState.Init;
        holeOrginScale = new Vector3[Hole.Length];

        for (int a = 0; a < Hole.Length; a++)
        {
            holeOrginScale[a] = Hole[a].transform.localScale;
            Hole[a].transform.localScale = Vector3.zero;
        }

        foreach (GameObject obj in occlusionObjs)
        {
            setOcclusionObjActivated += obj.SetActive;
        }
        setOcclusionObjActivated(false);
    }

    private void Start()
    {
        setOcclusionObjActivated(true);
        //Start to Ready
        for (int a = 0; a < Hole.Length; a++)
            Hole[a].transform.DOScale(holeOrginScale[a], 0.5f).SetDelay(a * 0.05f + 0.2f).OnComplete(SetRobot);
        height = HolePivot[0].position.y;
        RobotAssistantManager.robotAssistantManagerInstance.transform.DOMoveY(height + 0.5f, 0.5f).SetDelay(welcomeToReadyTime).SetEase(Ease.OutBack).OnComplete(ReadyGame);
    }

    private void SetRobot()
    {
        RobotAssistantManager.robotAssistantManagerInstance.transform.position = HolePivot[0].position + HolePivot[0].up * -0.5f;
    }

    private void Update()
    {
        if (GameState == WhackAMoleGameState.Start && readyToBeKnocked == true)
        {
            if (timer < everyRoundTime)
            {
                timer += Time.deltaTime;
            }
            else if (changing == false && timer >= everyRoundTime)
            {
                ChangeHole();
            }
        }

#if UNITY_EDITOR
        if (MRFlowManager.Instance.GameState == MRFlowManager.State.wackamole && Input.GetKeyDown(KeyCode.Space))
        {
            Knocking(Vector3.zero);
        }
#endif
    }

    bool readyToBeKnocked = false;
    Vector3 knockingHandPos;
    public void Knocking(Vector3 pos)
    {
        knockingHandPos = pos;
        if (!readyToBeKnocked)
        {
            if (changing == false && RobotAssistantManager.robotAssistantManagerInstance.Knocking(knockingHandPos))
            {
                SFX.PlayOneShot(Knock);
            }
            return;
        }
        else
        {
            if (GameState == WhackAMoleGameState.Ready && RobotAssistantManager.robotAssistantManagerInstance.Knocking(knockingHandPos))
            {
                StartGame();
                knockTimes += 1;
                return;
            }
            else if (GameState == WhackAMoleGameState.Start && RobotAssistantManager.robotAssistantManagerInstance.Knocking(knockingHandPos))
            {
                knockTimes += 1;
                StartCoroutine(AfterKnockedCoroutine());
            }
        }
    }

    private void ReadyGame()
    {
        Debug.Log("[Whack-A-Mole] RadyGame");
        readyToBeKnocked = true;
        GameState = WhackAMoleGameState.Ready;
        MRTutorialManager.Instance.ShowTutorial("Tutorial1", false);
    }

    private void StartGame()
    {
        GameState = WhackAMoleGameState.Start;
        AudioCenter.Instance.Stop("Tutorial1");
        MRTutorialManager.Instance.ForceStopTutorial();
        StartCoroutine(AfterKnockedCoroutine());
    }

    private void ChangeHole()
    {
        changing = true;
        RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        RobotAssistantManager.robotAssistantManagerInstance.transform.DOComplete();
        StartCoroutine(ChangeHoleCoroutine());
    }

    public void CloseHole()
    {
        for (int a = 0; a < Hole.Length; a++)
            Hole[a].transform.DOScale(Vector3.zero, 0.5f);
    }

    private IEnumerator ChangeHoleCoroutine()
    {
        RobotAssistantManager.robotAssistantManagerInstance.transform.DOMoveY(height - 0.5f, 0.65f).SetEase(Ease.InOutBack).SetDelay(0.1f);
        yield return new WaitForSeconds(0.75f);
        RobotAssistantManager.robotAssistantManagerInstance.transform.position = HolePivot[UnityEngine.Random.RandomRange(0, 3)].position + Vector3.down * 0.5f;
        yield return new WaitForSeconds(0.25f);
        RobotAssistantManager.robotAssistantManagerInstance.transform.DOMoveY(height + 0.5f, 0.5f).SetEase(Ease.OutBack);
        yield return new WaitForSeconds(0.25f);
        timer = 0;
        changing = false;
        readyToBeKnocked = true;
    }

    private IEnumerator AfterKnockedCoroutine()
    {
        SFX.PlayOneShot(Knock);
        readyToBeKnocked = false;

        yield return new WaitForSeconds(1.0f);
        RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        yield return new WaitForSeconds(0.5f);
        if (knockTimes < 3) ChangeHole();
        else MRFlowManager.Instance.EndWhackAMole();
    }
}