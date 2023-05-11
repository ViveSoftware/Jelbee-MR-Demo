using System.Collections;
using UnityEngine;
using DG.Tweening;
using com.HTC.Common;

public class WelcomeSequence : Singleton<WelcomeSequence>
{
    public GameObject WallHole;
    public Transform IntoRoomPivot;
    public Transform IntoWallPivot;

    private Vector3 holeOrginScale;
    private bool isIntoWall = false;

    protected override void AwakeSingleton()
    {
        holeOrginScale = WallHole.transform.localScale;
        WallHole.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (RobotAssistantManager.robotAssistantManagerInstance.RobotRender.isVisible && !isIntoWall)
        {
            RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(IntoRoomPivot.position, IntoWall);
            isIntoWall = true;
        }
    }

    private void IntoWall()
    {
        StartCoroutine(IntoWallCoroutine());
    }

    private IEnumerator IntoWallCoroutine()
    {
        RobotAssistantManager.robotAssistantManagerInstance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Happy);
        yield return new WaitForSeconds(2f);
        RobotAssistantManager.robotAssistantManagerInstance.ForceStopReaction();
        yield return new WaitForSeconds(1f);
        WallHole.transform.DOScale(holeOrginScale,0.45f);
        yield return new WaitForSeconds(0.5f);        
        RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(IntoWallPivot.position, MRFlowManager.Instance.StartWhackAMole);
        yield return new WaitForSeconds(0.5f);
    }    
}