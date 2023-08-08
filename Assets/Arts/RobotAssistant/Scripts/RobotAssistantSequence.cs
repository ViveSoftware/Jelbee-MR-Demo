using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using com.HTC.Common;

public class RobotAssistantSequence : MonoBehaviour
{
    [SerializeField] private RobotAssistantManager robotAssistantManagerInstance = null;
    [SerializeField] private Animator theEndAnimator;
    [SerializeField] private Vector3 theEndOffset;
    private bool starting = false;
    private bool ending = false;
    private Vector3 readyIntoPos;

    private void Awake()
    {
        Transform window = PivotManager.Instance.GetPivot("Window_1");
        readyIntoPos = window.position + window.forward * 0.5f + window.up * -0.35f;
        robotAssistantManagerInstance.moveSpeed = 1.5f;
        robotAssistantManagerInstance.transform.position = readyIntoPos + window.up * -0.75f;
        robotAssistantManagerInstance.transform.LookAt(window);
        robotAssistantManagerInstance.transform.eulerAngles = new Vector3(0, robotAssistantManagerInstance.transform.eulerAngles.y, 0);
    }

    private void Update()
    {
        if (MRFlowManager.Instance.GameState == MRFlowManager.State.welcome && !starting) 
        {
            StartFlow();
        }
            
        if (MRFlowManager.Instance.GameState == MRFlowManager.State.end && ending == false)
        {
            EndFlow();
            ending = true;
        }
    }

    public void StartFlow()
    {
        starting = true;
        StartCoroutine(PowerOnSequenceCoroutine());
    }

    public void EndFlow()
    {
        StartCoroutine(EndingCoroutine());
    }

    private IEnumerator PowerOnSequenceCoroutine()
    {
        EnvironmentManager.Instance.ChangeEnvironment(EnvType.Universe);
        robotAssistantManagerInstance.transform.DOMove(readyIntoPos, 1.5f).SetEase(Ease.OutBack).OnComplete(MRFlowManager.Instance.WelcomeState);
        yield return null;
    }

    private IEnumerator EndingCoroutine()
    {
        MRFlowManager.Instance.EndingPortal.DOScale(Vector3.one * 1.25f, 1.5f).SetDelay(2).SetEase(Ease.InBack);
        MRFlowManager.Instance.WindowObj.transform.DOScale(Vector3.zero, 1.5f).SetDelay(2).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(2f);
        robotAssistantManagerInstance.moveSpeed = 2;
        robotAssistantManagerInstance.OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.NormalBlinkOnce);
        yield return new WaitForSeconds(robotAssistantManagerInstance.SetRobotPosition(MRFlowManager.Instance.EndingPortal.position + MRFlowManager.Instance.EndingPortal.forward * 0.4f + Vector3.down * 0.16f) * 0.85f);

        robotAssistantManagerInstance.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Happy);

        yield return new WaitForSeconds(3.5f);

        robotAssistantManagerInstance.ForceStopReaction();
        while (!robotAssistantManagerInstance.robotAssistantAnimator.GetCurrentAnimatorStateInfo(0).IsName("FloatAirLoop"))
        {
            yield return null;
        }
        robotAssistantManagerInstance.TriggerLeisure();

        robotAssistantManagerInstance.transform.DOMove(MRFlowManager.Instance.EndingPortal.position + MRFlowManager.Instance.EndingPortal.forward * -0.4f + Vector3.down * 0.16f, 3f);

        MRFlowManager.Instance.EndingPortal.DOScale(0, 1.5f).SetEase(Ease.InBack).SetDelay(2);

        yield return new WaitForSeconds(4f);

        Transform desk = PivotManager.Instance.GetPivot("Desk");

        theEndAnimator.transform.position = desk.position;
        theEndAnimator.transform.localPosition += theEndOffset;
        theEndAnimator.transform.rotation = desk.rotation;
        theEndAnimator.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);
        theEndAnimator.gameObject.SetActive(false);
        Destroy(MRFlowManager.Instance.EndingPortal.gameObject);
        EventMediator.RestartGame();
    }
}