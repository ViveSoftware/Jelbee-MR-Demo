using UnityEngine;
using Wave.Essence.Hand.StaticGesture;
using Wave.Essence.Hand;
using System.Collections;
using com.HTC.Common;

public class BubbleLauncher : Singleton<BubbleLauncher>
{
    [SerializeField] private Collider[] handColliders;
    [SerializeField] private Collider[] handColliders_Left;
    [SerializeField] private Transform[] jalbeeEscapePoint;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private Transform launchPoint_Left;
    [SerializeField] private Transform praticleFOVLimit;
    [SerializeField] private ParticleSystem bubbleParticle;
    [SerializeField] private ParticleSystem bubbleParticle_Left;
    [SerializeField] private float offset = 0f;
    [SerializeField] private bool ignoreHit;

    private Vector3 jointPos = new Vector3();
    private Vector3 targetDir = new Vector3();
    private Vector3 newDir = new Vector3();
    private Transform cam;
    private Transform jelbee;
    private Transform desk;
    private Transform lastEscapePoint;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.EmissionModule emissionModule_Left;
    private bool stopEscape = false;
    private float stopTimer = 0;

    protected override void AwakeSingleton()
    {
        bubbleParticle.Stop();
        emissionModule = bubbleParticle.emission;
        emissionModule_Left = bubbleParticle_Left.emission;
        gameObject.SetActive(false);
        MRTutorialManager.Instance.OnTutorialStartHandler += OnTutorialStart;
        MRTutorialManager.Instance.OnTutorialEndHandler += OnTutorialEnd;
    }

    private void OnEnable()
    {
        cam = Camera.main.transform;
        desk = PivotManager.Instance.GetPivot("Desk");
        jalbeeEscapePoint[0].parent.transform.position = desk.position;
    }

    private void OnTutorialStart()
    {
        stopEscape = true;
        StopAllCoroutines();
    }

    private void OnTutorialEnd()
    {
        stopEscape = false;
        ignoreHit = false;
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                OnBubbleTouched();
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                StopAllCoroutines();
            }
        }

        praticleFOVLimit.eulerAngles = cam.eulerAngles;
        praticleFOVLimit.position = cam.position;

        if (HandManager.Instance != null)
        {
            LaunchBubble(true);
            LaunchBubble(false);
        }
    }

    private bool LaunchBubble(bool isLeftHand)
    {
#if UNITY_EDITOR
        if (MRFlowManager.Instance.GameState == MRFlowManager.State.portal) return false;
#endif
        bool hadLaunch = false;
        string currentGesture = WXRGestureHand.GetSingleHandGesture(isLeftHand);
        if (currentGesture.Equals("LaunchBubble") || currentGesture.Equals("OK"))
        {
#if !UNITY_EDITOR
            PortalSceneManager.Instance.PauseDrawindPortal();
#endif
            SwitchColliders(false, isLeftHand);
            HandManager.Instance.GetJointPosition(HandManager.HandJoint.Thumb_Joint2, ref jointPos, isLeftHand);
            targetDir = (jointPos - cam.position).normalized;

            if (isLeftHand)
            {
#if !UNITY_EDITOR
                newDir = Vector3.RotateTowards(launchPoint_Left.forward, targetDir, 1, 0.0f);
                launchPoint_Left.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Left.position = jointPos + targetDir * offset;
#else
                newDir = Camera.main.transform.forward;
                launchPoint_Left.rotation = Quaternion.LookRotation(newDir);
                launchPoint_Left.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f));
#endif
                emissionModule_Left.enabled = true;
            }
            else
            {
#if !UNITY_EDITOR
                newDir = Vector3.RotateTowards(launchPoint.forward, targetDir, 1, 0.0f);
                launchPoint.rotation = Quaternion.LookRotation(newDir);
                launchPoint.position = jointPos + targetDir * offset;
#else
                newDir = Camera.main.transform.forward;
                launchPoint.rotation = Quaternion.LookRotation(newDir);
                launchPoint.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f));
#endif
                emissionModule.enabled = true;
            }

            if (!bubbleParticle.isPlaying)
            {
                bubbleParticle.Play();
            }

            hadLaunch = true;

            return hadLaunch;
        }
#if UNITY_ANDROID
        PortalSceneManager.Instance.RestartDrawindPortal();
#endif

        if (isLeftHand)
        {
            emissionModule_Left.enabled = false;
            SwitchColliders(true, true);
        }
        else
        {
            emissionModule.enabled = false;
            SwitchColliders(true, false);
        }
        return hadLaunch;
    }

    public void SwitchColliders(bool value, bool isLeftHand)
    {
        if (isLeftHand)
        {
            foreach (Collider collider in handColliders_Left)
            {
                collider.enabled = value;
            }
        }
        else
        {
            foreach (Collider collider in handColliders)
            {
                collider.enabled = value;
            }
        }
    }

    public void OnBubbleTouched()
    {
        if (stopEscape || !gameObject.activeSelf) return;
        StartCoroutine(JalbeeEscape());
    }

    private IEnumerator JalbeeEscape()
    {
        if (!ignoreHit)
        {
            RobotAssistantManager robotAssistantManager = null;
            if (RobotAssistantManager.robotAssistantManagerInstance != null)
            {
                ignoreHit = true;
                robotAssistantManager = RobotAssistantManager.robotAssistantManagerInstance;
                robotAssistantManager.moveSpeed = 2;

                jelbee = robotAssistantManager.transform;

                Transform targetPoint = jalbeeEscapePoint[Random.Range(0, jalbeeEscapePoint.Length - 1)];

                if (lastEscapePoint != null)
                {
                    while (targetPoint == lastEscapePoint)
                    {
                        targetPoint = jalbeeEscapePoint[Random.Range(0, jalbeeEscapePoint.Length - 1)];
                    }
                }
                lastEscapePoint = targetPoint;

                Vector3 targetPosition = targetPoint.position;

                robotAssistantManager.TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Happy);

                yield return new WaitForSeconds(1f);
                robotAssistantManager.ForceStopReaction();
                robotAssistantManager.SetRobotPosition(targetPosition);

                yield return new WaitForSeconds(2f);
                robotAssistantManager.ForceStopReaction();
                robotAssistantManager = RobotAssistantManager.robotAssistantManagerInstance;
                robotAssistantManager.moveSpeed = 1;
                robotAssistantManager.TriggerLeisure();
                ignoreHit = false;
            }
        }
        yield return null;
    }
}