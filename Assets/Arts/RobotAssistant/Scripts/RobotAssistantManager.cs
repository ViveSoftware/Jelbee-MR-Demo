using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wave.Native;

public class RobotAssistantManager : MonoBehaviour
{
    private static RobotAssistantManager Instance;
    public static RobotAssistantManager robotAssistantManagerInstance { get { return Instance; } private set { Instance = value; } }

    [SerializeField] private RobotAssistantSpeechBubble robotAssistantSpeechBubbleInstance;
    public RobotAssistantSpeechBubble robotAssistantSpeechBubble { get { return robotAssistantSpeechBubbleInstance; } private set { robotAssistantSpeechBubbleInstance = value; } }

    public Animator robotAssistantAnimator;
    [Header("Teleportation")]
    [SerializeField] private Material teleportationMaterial;
    [SerializeField] private int teleportationStrength = 2;

    public delegate void OnRobotAnimationCompleteDelegate();
    public event OnRobotAnimationCompleteDelegate OnRobotAnimationCompleteCallback_Teleport;

    private string animation_TeleportOut = "Move.TeleportOut";
    private string animation_TeleportIn = "Move.TeleportIn";
    private string animation_FloatAirLoop = "Base Layer.FloatAirLoop";
    private string animation_PowerUp = "Base Layer.PowerUp";
    private string animation_PowerOff = "Base Layer.PowerOff";
    private string animationTrigger_TeleportOut = "TeleportOut";
    private string animationTrigger_TeleportIn = "TeleportIn";
    private string animationTrigger_Move = "tMove";
    private string animationBool_isMove = "isMove";
    private string animationBool_isPowerOn = "isPowerOn";
    private string animtionFloat_rotAngle = "rotAngle";
    private string animtionFloat_moveSpeed = "moveSpeed";
    private int animarionLayer_Base = 0, animationLayer_Move = 1;

    private int animation_TeleportOutHash, animation_TeleportInHash;
    private bool isSpeechBubbleIsActiveBeforeAction = false;
    private bool isRobotPoweredOn = false;

    private readonly string LOG_TAG = "RobotAssistantManager";

    public MeshRenderer RobotRender;
    private void Awake()
    {
        robotAssistantManagerInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        ForcePowerOn();
        animation_TeleportOutHash = Animator.StringToHash(animation_TeleportOut);
        animation_TeleportInHash = Animator.StringToHash(animation_TeleportIn);
    }

	private void Update()
	{
        if (isIdle)
        {
            LookAtPoint(Camera.main.transform.position, rotSpeed);
        }

        if (isLeisure)
        {
            PlayLeisureAnimation();
        }

        if (knockingTime > 0) knockingTime -= Time.deltaTime;

        if (isFacialTimer > 0 && isFacial == true)
        {
            isFacialTimer -= Time.deltaTime;
        }
        else if (isFacialTimer <= 0 && isFacial == true)
        {
            isFacial = false;
            OnChangeDefaultFacial();
        }
    }

    #region Power state

    private void ForcePowerOn()
    {
        robotAssistantAnimator.SetBool(animationBool_isPowerOn, true); // PowerIdle -> PowerUp
        OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.Normal);
        isIdle = true;
        isLeisure = true;
        isRobotPoweredOn = true;
    }
    public IEnumerator PowerOnRobot()
	{
        robotAssistantAnimator.SetBool(animationBool_isPowerOn, true); // PowerIdle -> PowerUp

        while (!robotAssistantAnimator.GetCurrentAnimatorStateInfo(animarionLayer_Base).IsName(animation_PowerUp))
        {
            yield return null;
        }
        OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.PowerOn);

        float currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animarionLayer_Base).normalizedTime;
        while ((currentNormalizedTime) < 0.6f)
        {
            currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animarionLayer_Base).normalizedTime;
            yield return null;
        }

        OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.Normal);

        isIdle = true;
        isLeisure = true;
        isRobotPoweredOn = true;
    }

    public IEnumerator PowerOffRobot()
    {
        isRobotPoweredOn = false;
        isIdle = false;
        isLeisure = false;

        robotAssistantAnimator.SetBool(animationBool_isPowerOn, false);

        OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.PowerOff);

        yield return null;
    }

    #endregion

    #region Robot teleport
    public IEnumerator RobotStartTeleport(Vector3 destinationCoordinates)
	{
        if (!isRobotPoweredOn) yield break;

        ForceStopReaction();
        ResetIdleTimer();

        isIdle = false;
        isLeisure = false;

        isSpeechBubbleIsActiveBeforeAction = robotAssistantSpeechBubble.IsActive;

        if (isSpeechBubbleIsActiveBeforeAction)
		{
            robotAssistantSpeechBubble.TextBoardShowup(false);
        }

        //Calculate Teleportation Direction
        Vector3 direction = TeleportationDirection(destinationCoordinates, this.transform.position) * teleportationStrength;
        //Set Teleport Out direction
        teleportationMaterial.SetVector("_Direction", direction);
        //Play Teleport Out Animation and wait for completion
        robotAssistantAnimator.SetLayerWeight(animationLayer_Move, 1f);
        robotAssistantAnimator.SetTrigger(animationTrigger_TeleportOut);
        //robotAssistantAnimator.Play(animation_TeleportOutHash);

        while (!robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).IsName(animation_TeleportOut))
        {
            yield return null;
        }

        float currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).normalizedTime;
		while ((currentNormalizedTime) < 1f)
		{
			currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).normalizedTime;
			yield return null;
		}
        Log.d(LOG_TAG, gameObject.name + " RobotStartTeleport: " + animation_TeleportOut + " is complete.", true);

        transform.position = destinationCoordinates;
        yield return StartCoroutine(RobotEndTeleport(-direction));
    }

	private IEnumerator RobotEndTeleport(Vector3 TeleportDirection)
    {
        //Set Teleport In direction
        teleportationMaterial.SetVector("_Direction", TeleportDirection);
        //Play Teleport In Animation and wait for completion
        robotAssistantAnimator.SetTrigger(animationTrigger_TeleportIn);

        while (!robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).IsName(animation_TeleportIn))
        {
            yield return null;
        }

        float currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).normalizedTime;
        while ((currentNormalizedTime) < 1f)
        {
            currentNormalizedTime = robotAssistantAnimator.GetCurrentAnimatorStateInfo(animationLayer_Move).normalizedTime;
            yield return null;
        }
        Log.d(LOG_TAG, gameObject.name + " RobotEndTeleport: " + animation_TeleportIn + " is complete.", true);
        robotAssistantAnimator.SetLayerWeight(animationLayer_Move, 0f);

        isIdle = true;
        isLeisure = true;
        OnChangeDefaultFacial();

        if (OnRobotAnimationCompleteCallback_Teleport != null)
		{
            Log.d(LOG_TAG, gameObject.name + " RobotEndTeleport: OnRobotAnimationCompleteCallback_Teleport.Invoke", true);
            OnRobotAnimationCompleteCallback_Teleport.Invoke();
        }

        if (isSpeechBubbleIsActiveBeforeAction)
        {
            robotAssistantSpeechBubble.TextBoardShowup(true);
        }
    }

    private Vector3 TeleportationDirection(Vector3 destinationCoordinates, Vector3 currentCoordinates)
	{
        Vector3 directionVector = destinationCoordinates - currentCoordinates;
        directionVector = new Vector3(-directionVector.x, directionVector.y, -directionVector.z); //negative x and z due to shader
        return directionVector.normalized;
	}
	#endregion

    #region Robot idle
    private bool isIdle = false;
    private bool isLeisure = false;
    private bool isSleep = false;
    private float idleTimer = 0f, leisureTimer = 0;
    private float leisureTimeThres = 6f;
    private float sleepTimeThres = 60f;
    public void PlayLeisureAnimation()
    {
		if (defaultFacial != RobotAssistantEnums.FacialAnimationIndex.Normal)
		{
			return;
		}

		if (idleTimer > leisureTimeThres && leisureTimer <= (sleepTimeThres - leisureTimeThres))
        {
            TriggerLeisure();
        }

		if (leisureTimer > sleepTimeThres)
		{
			isSleep = true;
			TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Sleep); //Sleep
            RobotAssistantLoSCaster.RobotAssistantLoS_EnterCallback += TriggerWakeUp;
        }

		idleTimer += Time.deltaTime;
        
        if (RobotAssistantLoSCaster.isAlreadyInLoS)
		{
            leisureTimer = 0; //Reset timer if robot is being looked at
        }
        else
		{
            leisureTimer += Time.deltaTime;
        }      
    }

    private void ResetIdleTimer()
    {
        idleTimer = 0;
        leisureTimer = 0;
        transform.DOKill();
    }

    public bool LookAtPoint(Vector3 point, float rotSpeed)
    {
        Vector3 dir = point - transform.position;
        dir = new Vector3(dir.x, 0, dir.z);
        Quaternion dirRot = Quaternion.LookRotation(dir, Vector3.up);
        float angle = Quaternion.Angle(transform.rotation, dirRot);

        transform.rotation = Quaternion.Slerp(transform.rotation, dirRot, rotSpeed);

        return (rotSpeed / angle > 1);
    }
    #endregion

    #region Robot move
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float rotSpeed = 0.08f;
    public float rotAnimLength = 2f;
    private Vector3 targetPos;
    private TweenCallback moveComplete = null;
    public float SetRobotPosition(Vector3 pos, TweenCallback callback = null)
    {
        if (!isRobotPoweredOn) return 0f;
        
        transform.parent = null;
        ForceStopReaction();
        ResetIdleTimer();        

        float dist = Vector3.Distance(transform.position, pos);
        float duration = dist / moveSpeed + rotAnimLength;

        if (dist == 0)
        {
            callback?.Invoke();
            return 0.0f;
        }

        targetPos = pos;
        moveComplete = callback;

        RobotRotate();

        isSpeechBubbleIsActiveBeforeAction = robotAssistantSpeechBubble.IsActive;

        if (isSpeechBubbleIsActiveBeforeAction)
        {
            robotAssistantSpeechBubble.TextBoardShowup(false);
        }

        StartCoroutine(MoveLayerWeightCoroutine(1, 1));

        return duration;
    }

    private void RobotRotate()
    {
        Vector3 dir = (new Vector3(targetPos.x, 0, targetPos.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;

        float angle = Vector3.Angle(transform.forward, dir);
        angle = Vector3.Dot(dir, transform.right) > 0 ? angle : -angle;

        robotAssistantAnimator.SetTrigger(animationTrigger_Move);
        robotAssistantAnimator.SetBool(animationBool_isMove, true);
        robotAssistantAnimator.SetFloat(animtionFloat_rotAngle, angle);
        isIdle = false;
        isLeisure = false;

        transform.DORotate(Vector3.up * angle, 0.5f, RotateMode.WorldAxisAdd).SetDelay(0.2f).OnComplete(RobotMove);
    }

    private void RobotMove()
    {
        float dist = Vector3.Distance(transform.position, targetPos);
        float duration = dist / moveSpeed;

        transform.DOMove(targetPos, duration).SetEase(Ease.Linear).OnComplete(RobotBackDefault);

        //start move sound;                
        GetComponent<AudioSource>().Play();
    }

    private void RobotBackDefault()
    {
        robotAssistantAnimator.SetBool("isMove", false);

        Vector3 dir = (new Vector3(Camera.main.transform.position.x, 0, Camera.main.transform.position.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        float angle = Vector3.Angle(transform.forward, dir);
        angle = Vector3.Dot(dir, transform.right) > 0 ? angle : -angle;

        transform.DORotate(Vector3.up * angle, 0.5f, RotateMode.WorldAxisAdd).SetDelay(0.2f).OnComplete(RobotMoveComplete);

        //stop move sound;
        GetComponent<AudioSource>().Stop();
    }

    private void RobotMoveComplete()
    {
        isIdle = true;
        isLeisure = true;

        StartCoroutine(MoveLayerWeightCoroutine(0, 0.5f));

        if (isSpeechBubbleIsActiveBeforeAction)
        {
            robotAssistantSpeechBubble.TextBoardShowup(true);
        }

        robotAssistantAnimator.Play("FloatAirLoop", 0);

        if (moveComplete != null)
        {
            moveComplete.Invoke();
        }
    }

    private IEnumerator MoveLayerWeightCoroutine(float weight, float duration)
    {
        float from = robotAssistantAnimator.GetLayerWeight(animationLayer_Move);
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            robotAssistantAnimator.SetLayerWeight(animationLayer_Move, Mathf.Lerp(from, weight, timer / duration));

            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region Robot facial
    [Header("Facial")]
    [SerializeField] private TextureAnimator texAnim = null;
    //[SerializeField] private GuideRobotEffect effect = null;

    public RobotAssistantEnums.FacialAnimationIndex defaultFacial = RobotAssistantEnums.FacialAnimationIndex.Normal;

    private IEnumerator FacialControllerIEnumerator = null;

    private bool isFacial = false;
    private float isFacialTimer = 0;

    public void OnChangeDefaultFacial()
    {        
        OnChangeFacial(defaultFacial);
    }

    public void OnChangeDefaultFacial(RobotAssistantEnums.FacialAnimationIndex facial)
    {
        defaultFacial = facial;
        OnChangeFacial(defaultFacial);
    }

    public void OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex facial)
    {
        isFacial = true;
        isFacialTimer = 3;
        if (FacialControllerIEnumerator != null)
        {
            StopCoroutine(FacialControllerIEnumerator);
        }

        switch (facial)
        {
            case RobotAssistantEnums.FacialAnimationIndex.Normal:
                texAnim.Play(facial.ToString());
                FacialControllerIEnumerator = WaitBlinkCoroutine();
                StartCoroutine(FacialControllerIEnumerator);
                break;
            default:
                texAnim.Play(facial.ToString());
                StartCoroutine(WaitBackToNormal());
                break;
        }
    }

    public virtual void OverridePlaySpeed(float PlayOnceSpeed = 0.05f, float LoopSpeed = 0.05f)
    {
        texAnim.OverridePlaySpeed(PlayOnceSpeed, LoopSpeed);
    }

    private IEnumerator WaitBackToNormal()
    {
        while (texAnim.IsPlaying)
        {         
            yield return null;
        }        
        //OnChangeFacial(defaultFacial);
    }

    private IEnumerator WaitBackToNormal(float delay)
    {
        yield return new WaitForSeconds(delay);

        OnChangeFacial(defaultFacial);
    }

    private float blinkInterval = 4f;
    private IEnumerator WaitBlinkCoroutine()
    {
        yield return new WaitForSeconds(blinkInterval);
        OnChangeFacial(RobotAssistantEnums.FacialAnimationIndex.NormalBlinkOnce);
    }
#endregion

#region Robot reaction
    public void TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex pose)
    {
        robotAssistantAnimator.SetTrigger("tReaction");
        robotAssistantAnimator.SetBool("isIdle", false);
        robotAssistantAnimator.SetInteger("inxReaction", (int)pose);

        isLeisure = false;

        OnChangeFacial(RobotAssistantEnums.InquireFacial(pose));
    }

    public void TriggerLeisure()
    {
        robotAssistantAnimator.SetTrigger("tLeisure");

        int random = Random.Range(0, (int)RobotAssistantEnums.IdleAnimationIndex.size);

        if (random == 3) { return; } // Rotate dance has rotation info.

        robotAssistantAnimator.SetInteger("inxLeisure", random);

        idleTimer = 0;
        leisureTimeThres = Random.Range(5f, 7f);

        OnChangeFacial(RobotAssistantEnums.InquireFacial((RobotAssistantEnums.IdleAnimationIndex)random));
    }

    public void TriggerWakeUp()
	{
        RobotAssistantLoSCaster.RobotAssistantLoS_EnterCallback -= TriggerWakeUp;

        ForceStopReaction();
        ResetIdleTimer();

        isSleep = false;
        isIdle = true;
        isLeisure = true;
    }

    public void ForceStopReaction()
    {
        robotAssistantAnimator.SetBool("isIdle", true);
        if (isSleep)
        {
            isSleep = false;
        }

        isIdle = true;
        isLeisure = true;

        //OnChangeDefaultFacial();
        ResetIdleTimer();

    }

    float knockingTime = 0;
    public bool Knocking(Vector3 pos)
    {
        if (knockingTime > 0)
            return false;
        else
        {
            knockingTime = 0.3f;
            Vector3 oriPos = RobotAssistantManager.robotAssistantManagerInstance.transform.position;
            Vector3 dir = (oriPos - pos).normalized;
            TriggerReaction(RobotAssistantEnums.ReactionAnimationIndex.Angry);
            this.transform.DOMove(oriPos + dir * 0.2f, 0.1f).SetEase(Ease.OutQuart);
            this.transform.DOMove(oriPos, 0.15f).SetDelay(0.1f);
            return true;
        }
    }
    #endregion
}

#region Animation State enum definition
public static class RobotAssistantEnums
{
    public enum ReactionAnimationIndex
    {
        Angry = 0,
        Happy = 1,
        Sleep = 2,
        GoAhead = 3,
        size
    }

    public enum IdleAnimationIndex
    {
        RotateDance = 0,
        size
    }

    public enum FacialAnimationIndex
    {
        Normal = 0,
        PowerOn,
        PowerOff,
        NormalBlinkOnce,
        NormalBlinkTwice,
        Angry,
        AngryGlitch,
        Happy,
        HappyGlitch,
        none,
        size
    }

    public static FacialAnimationIndex InquireFacial(ReactionAnimationIndex pose)
    {
        FacialAnimationIndex facial = FacialAnimationIndex.none;
        switch (pose)
        {
            case ReactionAnimationIndex.Angry:
                facial = FacialAnimationIndex.Angry;
                break;
            case ReactionAnimationIndex.Happy:
                facial = FacialAnimationIndex.Happy;
                break;
            case ReactionAnimationIndex.Sleep://Sleep
                facial = FacialAnimationIndex.PowerOff;
                break;
            case ReactionAnimationIndex.GoAhead:
            default:
                facial = FacialAnimationIndex.Normal;
                break;
        }
        return facial;
    }

    public static FacialAnimationIndex InquireFacial(IdleAnimationIndex pose)
    {
        FacialAnimationIndex facial = FacialAnimationIndex.none;
        switch (pose)
        {
            case IdleAnimationIndex.RotateDance:
                facial = FacialAnimationIndex.NormalBlinkOnce;
                break;
            default:
                facial = FacialAnimationIndex.Normal;
                break;
        }
        return facial;
    }
}
#endregion