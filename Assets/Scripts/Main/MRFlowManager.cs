using com.HTC.Common;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRFlowManager : Singleton<MRFlowManager>
{
    public State GameState;
    public enum State
    {
        init,
        welcome,
        wackamole,
        bubble,
        portal,
        end
    }

    private GameObject windowObj;
    public GameObject WindowObj { get { return windowObj; } }
    public Transform EndingPortal;

    private Transform desk;
    private Transform wall;
    private Transform window;
    private float time = 0;

    [SerializeField] private float bubbleGameTime = 20;

    protected override void AwakeSingleton()
    {
        GameState = State.init;
    }

    private void Start()
    {
        desk = PivotManager.Instance.GetPivot("Desk");
        wall = PivotManager.Instance.GetPivot("Wall_1");
        window = PivotManager.Instance.GetPivot("Window_1");

        windowObj = GameObject.Find("Window(Clone)");
        windowObj.transform.localScale = Vector3.one;

        //sync all rotation
        transform.rotation = wall.rotation;

        //welcome
        WelcomeSequence.Instance.IntoRoomPivot.position = desk.position + Vector3.up * 0.6f;
        Vector3 fwd = transform.TransformDirection(wall.forward);
        Ray ray = new Ray(WelcomeSequence.Instance.IntoRoomPivot.position, fwd * 10);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10))
        {
            if (hit.transform.GetComponent<PlaneController>().ShapeType == com.HTC.WVRLoader.ShapeTypeEnum.wall)
                WelcomeSequence.Instance.WallHole.transform.position = hit.point + Vector3.up * 0.15f;
        }

        //whack-a-mole
        WhackAMole.Instance.transform.GetChild(0).position = desk.position;
        WhackAMole.Instance.transform.GetChild(0).rotation = desk.rotation;

        //ending portal
        EndingPortal.position = wall.position;
        EndingPortal.eulerAngles = new Vector3(wall.eulerAngles.x, wall.eulerAngles.y + 180f, wall.eulerAngles.z);
    }

    private void Update()
    {
        if(GameState == State.init && StartButton.Instance.GameStart())
        {
            GameState = State.welcome;
            RobotAssistantManager.robotAssistantManagerInstance.gameObject.SetActive(true);

            StartButton.Instance.Restart();
            StartButton.Instance.enabled = false;
        }
        else if (GameState == State.bubble) {
            if (time <= bubbleGameTime)
                time += Time.deltaTime;
            else
            {
                time = 0;
                StartPortal();
            }
        }
    }

    public void WelcomeState()
    {
        GameState = State.welcome;
        Debug.Log("Start [WelcomeSequence]");
        WelcomeSequence.Instance.enabled = true;        
    }

    public void StartWhackAMole()
    {
        GameState = State.wackamole;
        Debug.Log("Start [Whack-A-Mole]");
        WelcomeSequence.Instance.WallHole.transform.DOScale(Vector3.zero, 0.5f);
        WelcomeSequence.Instance.enabled = false;
        WhackAMole.Instance.enabled = true;
    }

    public void EndWhackAMole()
    {
        Debug.Log("End [Whack-A-Mole]");
        WhackAMole.Instance.enabled = false;
        WhackAMole.Instance.GameState = WhackAMole.WhackAMoleGameState.End;
        RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(WelcomeSequence.Instance.IntoRoomPivot.position, StartBubble);
    }

    private void StartBubble()
    {
        GameState = State.bubble;
        Debug.Log("Start [Bubble]");
        
        WhackAMole.Instance.CloseHole();
        MRTutorialManager.Instance.ShowTutorial("Tutorial2", true);
        StartBubbleInteract();
    }

    private void StartBubbleInteract()
    {
        BubbleLauncher.Instance.gameObject.SetActive(true);
        BubbleLauncher.Instance.enabled = true;
    }

    private void StartPortal()
    {
        GameState = State.portal;
        Debug.Log("Start [Portal]");
        StartPortalInteract();
        MRTutorialManager.Instance.ShowTutorial("Tutorial3", true);
    }

    private void StartPortalInteract()
    {
        PortalSceneManager.Instance.enabled = true;
    }
}