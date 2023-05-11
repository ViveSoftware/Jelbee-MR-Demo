using com.HTC.WVRLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
public class PortalBallBehaviour : MonoBehaviour
{
    public PortalPrefabControl portalRing;
    public GameObject firingRay;

    protected PortalStatus currentPortalStatus = PortalStatus.Awake;
    protected UnityAction informManagerClosePortal;
    protected bool portalOnCorner = false;
    protected Vector3 cornerPortalMainNormal;
    protected Vector3 cornerPortalMainCenter;
    protected float portalRadius;

    private bool portalInitialized = false;

    protected enum PortalStatus 
    {
        Awake,
        PortalFlying,
        RingOpening,
        Activate,
        RingClosing,
        RingClose,
        Destroying
    }

    private void Awake()
    {
        portalRing.gameObject.SetActive(false);
        firingRay.SetActive(true);
    }

    public void Initialized(float delay, float radius, Vector3 firstHitPos, Vector3 firstHitNormal, UnityAction onClosePortal = null, float disappearAfterSecond = 0
        , bool hitTwoWalls = false, Vector3 secondHitPos = new Vector3(), Vector3 secondHitNormal = new Vector3(), Vector3 cornerPortalNormal = new Vector3()
        , Vector3 cornerPortalCenter = new Vector3(), float[] cutLeft = null, float[] cutRight = null)
    {
        if (portalInitialized)
            return;
        portalInitialized = true;
        currentPortalStatus = PortalStatus.PortalFlying;

        portalRadius = radius;
        firingRay.transform.localScale = new Vector3(portalRadius, portalRadius, portalRadius);
        if (disappearAfterSecond != 0)
            StartCoroutine(DestroyingAfterCountdown(disappearAfterSecond));

        if (onClosePortal != null)
            informManagerClosePortal = onClosePortal;

        portalOnCorner = hitTwoWalls;

        if (!portalOnCorner)
        {
            StartCoroutine(SpwanPortalOnWall(delay, firstHitPos, firstHitNormal));
            cornerPortalMainNormal = firstHitNormal;
            cornerPortalMainCenter = firstHitPos;
        }
        else
        {
            StartCoroutine(SpwanPortalOnCorner(delay, firstHitPos, firstHitNormal, secondHitPos, secondHitNormal, cutLeft, cutRight));
            cornerPortalMainNormal = -cornerPortalNormal;
            cornerPortalMainCenter = cornerPortalCenter;
        }
    }

    IEnumerator SpwanPortalOnWall(float delay, Vector3 firstHitPos, Vector3 firstHitNormal)
    {
        yield return new WaitForSeconds(delay);
        portalRing.gameObject.SetActive(true);
        firingRay.SetActive(false);

        portalRing.FullPortalOn(new Vector3(portalRadius, portalRadius, portalRadius), firstHitPos, firstHitNormal);
        currentPortalStatus = PortalStatus.RingOpening;
        yield return new WaitForSeconds(1);

        OnStartPortal();
        currentPortalStatus = PortalStatus.Activate;
        EnvironmentManager.Instance.OnPortalOpen();
        Debug.Log("[PortalBallBehaviour][SpwanPortalOnWall] Portal Open Success");
    }

    IEnumerator SpwanPortalOnCorner(float delay, Vector3 firstHitPos, Vector3 firstHitNormal, Vector3 secondHitPos, Vector3 secondHitNormal, float[] cutLeft, float[] cutRight)
    {
        yield return new WaitForSeconds(delay);
        portalRing.gameObject.SetActive(true);
        firingRay.SetActive(false);

        portalRing.HalfPortalOn(new Vector3(portalRadius, portalRadius, portalRadius), firstHitPos, firstHitNormal, secondHitPos, secondHitNormal, cutLeft, cutRight);
        currentPortalStatus = PortalStatus.RingOpening;

        yield return new WaitForSeconds(1);
        OnStartPortal();
        currentPortalStatus = PortalStatus.Activate;
        EnvironmentManager.Instance.OnPortalOpen();
        Debug.Log("[PortalBallBehaviour][SpwanPortalOnWall] Portal Open Success");
    }

    protected virtual void OnStartPortal()
    {

    }

    protected virtual void OnDestroyPortal()
    {

    }

    protected virtual void OnClosePortalRing()
    {

    }

    protected IEnumerator DisappearingAfterCountdown(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClosePortalRing();
    }

    protected IEnumerator DestroyingAfterCountdown(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroyPortal();
    }

    protected void DestroyPortal()
    {
        StartCoroutine(DestroyingPortal());
    }

    IEnumerator DestroyingPortal() 
    {
        if (currentPortalStatus == PortalStatus.Activate)
        {
            ClosePortalRing();
            Invoke("DestroyPortal", 1.5f);
            yield break;
        }

        if (currentPortalStatus == PortalStatus.RingClosing)
        {
            yield return new WaitUntil(() => currentPortalStatus == PortalStatus.RingClose);
        }

        if (currentPortalStatus != PortalStatus.RingClose)
        {
            Debug.LogError($"[PortalBallBehaviour][DestroyPortal] wrong status {currentPortalStatus}, can't destroy portal");
            yield break;
        }

        currentPortalStatus = PortalStatus.Destroying;

        PortalSceneManager.Instance.SetNextPortal();
        OnDestroyPortal();
        if (informManagerClosePortal != null)
            informManagerClosePortal();
        Debug.Log("[PortalBallBehaviour][DestroyPortal]");
        Destroy(this.gameObject);
    }

    protected void ClosePortalRing()
    {
        StartCoroutine(ClosingPortalRing());
    }

    IEnumerator ClosingPortalRing() 
    {
        if (currentPortalStatus != PortalStatus.Activate)
        {
            Debug.LogError($"[PortalBallBehaviour][ClosingPortalRing] wrong status {currentPortalStatus}, can't close portal");
            yield break;
        }

        currentPortalStatus = PortalStatus.RingClosing;
        OnClosePortalRing();
        if (!portalOnCorner)
        {
            portalRing.FullPortalOff();
        }
        else
        {
            portalRing.HalfPortalOff();
        }
        yield return new WaitForSeconds(1.5f);
        currentPortalStatus = PortalStatus.RingClose;
    }

    protected void RestartPortalRing(Vector3 pos, Vector3 forward)
    {
        StartCoroutine(RestartingPortalRing(pos, forward));
    }

    IEnumerator RestartingPortalRing(Vector3 pos, Vector3 forward) 
    {
        if (currentPortalStatus != PortalStatus.RingClose)
        {
            Debug.LogError($"[PortalBallBehaviour][RestartingPortalRing] wrong status {currentPortalStatus}, can't restart portal ring");
            yield break;
        }

        currentPortalStatus = PortalStatus.RingOpening;
        portalOnCorner = false;
        portalRing.FullPortalOn(Vector3.one*0.68f, pos, forward);

        yield return new WaitForSeconds(1f);
        currentPortalStatus = PortalStatus.Activate;
        Debug.Log("[PortalBallBehaviour][RestartPortalRing]");
    }
}
