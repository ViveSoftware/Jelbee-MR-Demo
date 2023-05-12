using DG.Tweening;
using UnityEngine;
using com.HTC.Common;
using static MRFlowManager;

public class UniversePortalBall : PortalBallBehaviour
{
    [SerializeField] private GameObject solarObject;
    [SerializeField] private ParticleSystem fogParticle;
    [SerializeField] private Transform jelbeePivot;

    [SerializeField] private string groupKey = "Space";
    [SerializeField] private string destroyCmdKey = "Planet";
    [SerializeField] private string pivotKey = "Desk";

    private bool universePortalStart = false;
    private float time = 0;
    private float planetGameTime = 0;

    protected override void OnStartPortal()
    {
        Transform pivot = PivotManager.Instance.GetPivot(pivotKey);
        if (pivot != null)
        {
            fogParticle.transform.SetPositionAndRotation(pivot.position, Quaternion.identity);
            jelbeePivot.transform.SetPositionAndRotation(pivot.position + jelbeePivot.transform.localPosition, Quaternion.identity);
            solarObject.transform.SetPositionAndRotation(transform.position + transform.forward * 0.05f, Quaternion.identity);
        }

        RobotAssistantManager.robotAssistantManagerInstance.SetRobotPosition(jelbeePivot.transform.position);

        fogParticle.Play(true);
        solarObject.transform.DOScale(Vector3.one, 3).OnComplete(GrabberManager.Instance.TurnOn);
        solarObject.transform.DOMove(pivot.position + Vector3.up * 0.25f, 3).SetEase(Ease.InOutCubic);

        //GrabberManager.Instance.TurnOn();
        GrabberManager.Instance.EnableGroup(groupKey);

        time = 0;
        planetGameTime = 30;
        universePortalStart = true;
    }

    private void Update()
    {
        if (universePortalStart) 
        {
            if (time <= planetGameTime && planetGameTime > 0)
                time += Time.deltaTime;
            else if (time > planetGameTime && planetGameTime > 0)
            {
                universePortalStart = false;
                time = 0;
                MRFlowManager.Instance.GameState = State.end;
                StopExperience();
            }
        }
    }

    public void StopExperience()
    {
        fogParticle.Stop(true);
        ClosePortalRing();
        solarObject.transform.DOScale(Vector3.zero, 3).OnComplete(() => {
            DestroyPortal();
            MRFlowManager.Instance.GameState = State.end;
        });
        GrabberManager.Instance.TurnOff();
    }

    public int completeInSolarCount = 0;
    public void SetInParent(int a, GameObject obj)
    {
        solarObject.transform.GetChild(a - 1).GetComponent<SolorRotate>().enabled = true;
        obj.transform.SetParent(solarObject.transform.GetChild(a - 1));
        obj.transform.DOLocalMove(Vector3.zero, 3);
        obj.GetComponent<PlanetsHandler>().enterSolarSystem();
        completeInSolarCount++;
        if (completeInSolarCount < 8)
        {
            time = 0;
            planetGameTime = 30;
        }
    }
    public void SetOutParent(int a, GameObject obj)
    {
        obj.transform.DOKill();
        if (obj.transform.parent == solarObject.transform.GetChild(a - 1)) completeInSolarCount--;
    }
}
