using com.HTC.Gesture;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using com.HTC.WVRLoader;
using UnityEngine.Events;
using com.HTC.Common;
using HTC.UnityPlugin.Utility;

public class PortalSceneManager : Singleton<PortalSceneManager>
{
    [SerializeField] private List<GameObject> portalPrefab = new List<GameObject>();
    [SerializeField] private List<PortalType> showPortalOrder = new List<PortalType>();
    [SerializeField] private GameObject portalParticle;

    private bool isPortalOpen = false;
    private int currentPortalOrder = 0;
    private PortalBallBehaviour currentPortalBallBehaviour = null;
    private ViveRoleProperty rightHandRole = ViveRoleProperty.New(TrackedHandRole.RightHand);
    private ViveRoleProperty leftHandRole = ViveRoleProperty.New(TrackedHandRole.LeftHand);
    private bool gameStart = false;
    private bool drawingPortal_rightHand = false;
    private bool drawingPortal_leftHand = false;
    private Coroutine leftHandDrawingCoroutine;
    private Coroutine rightHandDrawingCoroutine;
    private GameObject leftHandPortalParticle;
    private GameObject rightHandPortalParticle;
    private bool tryCreatingPortal = false;

    // Start is called before the first frame update
    void Start()
    {
        leftHandPortalParticle = Instantiate(portalParticle);
        leftHandPortalParticle.SetActive(false);
        rightHandPortalParticle = Instantiate(portalParticle);
        rightHandPortalParticle.SetActive(false);
    }

    private void OnEnable()
    {
        gameStart = true;
    }

    private void OnDisable()
    {
        PauseDrawindPortal();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStart)
        {
            if (GestureRecognizeMethod.CheckHandOpen(rightHandRole) && !drawingPortal_rightHand)
            {
                drawingPortal_rightHand = true;
                rightHandDrawingCoroutine = StartCoroutine(OnStartDrawingPortal(rightHandRole));
            }

            if (GestureRecognizeMethod.CheckHandOpen(leftHandRole) && !drawingPortal_leftHand)
            {
                drawingPortal_leftHand = true;
                leftHandDrawingCoroutine = StartCoroutine(OnStartDrawingPortal(leftHandRole));
            }
        }
    }

    IEnumerator OnStartDrawingPortal(ViveRoleProperty handRole)
    {
        Debug.Log("[PortalSceneManager][OnStartDrawingPortal]");
        float checkGestureTimer = 0;
        float drawingTimer = 0;
        Vector3 preFramePos;
        bool drawingPortal = true;
        List<Vector3> drawingPath = new List<Vector3>();
        GameObject handParticle;
        if (handRole == ViveRoleProperty.New(TrackedHandRole.RightHand)) handParticle = rightHandPortalParticle;
        else handParticle = leftHandPortalParticle;
#if UNITY_EDITOR
        preFramePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f);
        preFramePos = Camera.main.ScreenToWorldPoint(preFramePos);
#else
        VivePose.TryGetHandJointPose(handRole, HandJointName.Palm, out JointPose firstJointPose);
        preFramePos = firstJointPose.pose.pos;
#endif
        while (drawingPortal)
        {
            if (!GestureRecognizeMethod.CheckHandOpen(handRole))
            {
                checkGestureTimer += Time.deltaTime;
                if (checkGestureTimer > 0.5)
                {
                    drawingPath.Clear();
                    drawingPortal = false;
                    if (handRole == ViveRoleProperty.New(TrackedHandRole.RightHand)) drawingPortal_rightHand = false;
                    else if (handRole == ViveRoleProperty.New(TrackedHandRole.LeftHand)) drawingPortal_leftHand = false;
                    break;
                }
            }
            else
            {
                checkGestureTimer = 0;
            }

            Vector3 handPos;
            Vector3 particalPos;
#if UNITY_EDITOR
            handPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f);
            handPos = Camera.main.ScreenToWorldPoint(handPos);
            particalPos = handPos;
#else
            VivePose.TryGetHandJointPose(handRole, HandJointName.Palm, out JointPose jointPose);
            handPos = jointPose.pose.pos;
            particalPos = jointPose.pose.pos + -jointPose.pose.up * 0.02f;
#endif
            float distance = Vector3.Distance(handPos, preFramePos);
            if (distance > 1)
            {
                drawingPath.Clear();
                drawingPortal = false;
                if (handRole == ViveRoleProperty.New(TrackedHandRole.RightHand)) drawingPortal_rightHand = false;
                else if (handRole == ViveRoleProperty.New(TrackedHandRole.LeftHand)) drawingPortal_leftHand = false;
                break;
            }
            else
            {
                preFramePos = handPos;
            }

            handParticle.transform.position = particalPos;
#if UNITY_EDITOR
            handParticle.SetActive(true);
#else
            if (jointPose.isValid == true) handParticle.SetActive(true);
            else handParticle.SetActive(false);
#endif

            if (drawingTimer > 0.01f)
            {
                drawingTimer = 0;
                if (drawingPath.Count > 1)
                {
                    distance = Vector3.Distance(handPos, drawingPath[drawingPath.Count - 1]);
                    if (distance < 0.05)
                    {
                        continue;
                    }
                }

                if (drawingPath.Count > 100)
                {
                    drawingPath.RemoveAt(0);
                }

                drawingPath.Add(handPos);

                Debug.Log("[PortalSceneManager][OnStartDrawingPortal] Drawing path count" + drawingPath.Count.ToString());

                if (drawingPath.Count > 10 && isCircle(drawingPath, out List<Vector3> circlePath))
                {
                    OneThirdCirclePoint(circlePath, out int p0, out int p1, out int p2);
                    if (isGoodCircle(circlePath[p0], circlePath[p1], circlePath[p2]))
                    {
                        DrawPathToPortal(circlePath[p0], circlePath[p1], circlePath[p2], out Vector3 normal, out Vector3 centroid, out float radius);
                        drawingPath.Clear();
                        if (radius < 0.1 || radius > 2)
                        {
                            Debug.Log($"[PortalSceneManager][OnStartDrawingPortal] Circle {radius} too small or too big, pass creation.");
                        }
                        else
                        {
                            if (!isPortalOpen && !tryCreatingPortal)
                            {
                                tryCreatingPortal = true;
                                CreatPortalBall((int)showPortalOrder[currentPortalOrder], centroid, normal, newRange(0.1f, 2, 1f, 4f, radius));
                                drawingPortal = false;
                                handParticle.SetActive(false);
                                if (handRole == ViveRoleProperty.New(TrackedHandRole.RightHand)) drawingPortal_rightHand = false;
                                else if (handRole == ViveRoleProperty.New(TrackedHandRole.LeftHand)) drawingPortal_leftHand = false;
                                Debug.Log($"[PortalSceneManager][OnStartDrawingPortal] Circle({radius}) Creat.");
                                break;
                            }
                            else
                            {
                                Debug.Log("[PortalSceneManager][OnStartDrawingPortal] Portal exist, pass creation.");
                            }
                        }
                    }
                    else
                    {
                        drawingPath.Clear();
                    }
                }
            }
            else
            {
                drawingTimer += Time.deltaTime;
            }

            yield return null;
        }
        handParticle.SetActive(false);
    }

    float newRange(float oldMin, float oldMax, float newMin, float newMax, float targetNum)
    {
        if (oldMin > oldMax || newMin > newMax || targetNum < oldMin || targetNum > oldMax)
        {
            return 0;
        }
        return (targetNum - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
    }

    bool isCircle(List<Vector3> path, out List<Vector3> circlePath)
    {
        int minimumPointAmount = 6;
        for (int i = 0; i < path.Count; i++)
        {
            for (int j = 0; j < path.Count; j++)
            {
                if (Mathf.Abs(i - j) > minimumPointAmount)
                {
                    if (Vector3.Distance(path[i], path[j]) < 0.2f)
                    {
                        List<Vector3> newPath = new List<Vector3>();
                        for (int k = i; k < j + 1; k++)
                        {
                            newPath.Add(path[k]);
                        }
                        circlePath = newPath;
                        return true;
                    }
                }
                else
                {
                    continue;
                }
            }
        }
        circlePath = path;
        return false;
    }

    bool isGoodCircle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a = Vector3.Distance(p1, p2);
        float b = Vector3.Distance(p2, p3);
        float c = Vector3.Distance(p3, p1);
        List<float> sideLength = new List<float>() { a, b, c };
        sideLength.Sort();
        if (sideLength[2] * sideLength[2] > sideLength[1] * sideLength[1] + sideLength[0] * sideLength[0])
        {
            Debug.Log("[PortalSceneManager][isGoodCircle] false");
            return false;
        }
        Debug.Log("[PortalSceneManager][isGoodCircle] true");
        return true;
    }

    private void OneThirdCirclePoint(List<Vector3> path, out int p0, out int p1, out int p2)
    {
        float distance = 0;
        List<float> pointDistance = new List<float>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            distance += Vector3.Distance(path[i], path[i + 1]);
            pointDistance.Add(distance);
        }
        float distanceOneThird = distance / 3;

        int[] pointsCount = new int[3];

        for (int j = 1; j <= 3; j++)
        {
            for (int i = 0; i < pointDistance.Count - 1; i++)
            {
                if (distanceOneThird * j - pointDistance[i] >= 0 && distanceOneThird * j - pointDistance[i + 1] <= 0)
                {
                    if (Mathf.Abs(distanceOneThird * j - pointDistance[i]) < Mathf.Abs(distanceOneThird * j - pointDistance[i + 1]))
                    {
                        pointsCount[j - 1] = i + 1;
                    }
                    else
                    {
                        pointsCount[j - 1] = i + 2;
                    }
                    break;
                }
                else if (i == pointDistance.Count - 2 && j == 3)
                {
                    pointsCount[j - 1] = i + 2;
                }
            }
        }
        p0 = pointsCount[0];
        p1 = pointsCount[1];
        p2 = pointsCount[2];
    }

    private void PortalIsClose()
    {
        isPortalOpen = false;
        currentPortalBallBehaviour = null;
    }

    private void PortalIsOpen()
    {
        isPortalOpen = true;
        tryCreatingPortal = false;
    }

    public void PauseDrawindPortal() 
    {
        gameStart = false;
        Debug.Log("[PortalSceneManager][PauseDrawindPortal]");
        if (drawingPortal_rightHand) 
        {
            if(rightHandDrawingCoroutine != null) StopCoroutine(rightHandDrawingCoroutine);
            drawingPortal_rightHand = false;
        }
        
        if (drawingPortal_leftHand) 
        {
            if (leftHandDrawingCoroutine != null) StopCoroutine(leftHandDrawingCoroutine);
            drawingPortal_leftHand = false;
        }
        if (leftHandPortalParticle != null) leftHandPortalParticle.SetActive(false);
        if (rightHandPortalParticle != null) rightHandPortalParticle.SetActive(false);
    }

    public int GetCurrentPortalOrder() 
    {
        return currentPortalOrder;
    }

    public PortalBallBehaviour GetCurrentPortalBallBehaviour()
    {
        return currentPortalBallBehaviour;
    }

    public void RestartDrawindPortal()
    {
        gameStart = true;
    }

    public void SetNextPortal()
    {
        if (currentPortalOrder < showPortalOrder.Count - 1)
        {
            currentPortalOrder++;
            Debug.Log("[PortalSceneManager][SetNextPortal] To Portal Order " + currentPortalOrder);
        }
        else
        {
            PauseDrawindPortal();
            this.enabled = false;
            Debug.Log("[PortalSceneManager][SetNextPortal] End Portal Show");
        }
    }

    private void DrawPathToPortal(Vector3 p1, Vector3 p2, Vector3 p3, out Vector3 normal, out Vector3 centroid, out float radius)
    {
        Vector3 v1 = p2 - p1;
        Vector3 v2 = p3 - p1;
        normal = Vector3.Cross(v1, v2);
        float xs = p1.x + p2.x + p3.x;
        float ys = p1.y + p2.y + p3.y;
        float zs = p1.z + p2.z + p3.z;
        centroid = new Vector3(xs / 3, ys / 3, zs / 3);
        radius = (Vector3.Distance(centroid, p1) + Vector3.Distance(centroid, p2) + Vector3.Distance(centroid, p3)) / 3;

        Vector3 hmdPose;
#if UNITY_EDITOR
        hmdPose = Camera.main.transform.forward;
#else
        hmdPose = VivePose.GetPose(DeviceRole.Hmd).forward;
#endif
        float angleFromNormalToHMD = Vector3.Angle(normal, hmdPose);
        if (angleFromNormalToHMD > 90) normal *= -1;
    }

    private void CreatPortalBall(int portalType, Vector3 spawnPoint, Vector3 flyDirection, float radius)
    {
        Vector3 destination = Vector3.zero;
        Vector3 rotationUp = Vector3.up;
        float disappearAfter = 10;
        float tableHight = PivotManager.Instance.GetPivot("Desk").position.y; ;

        List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(spawnPoint, flyDirection, 100));
        hits.Sort((x, y) => x.distance.CompareTo(y.distance));
        if (hits.Count == 0)
        {
            Debug.Log("[PortalSceneManager][CreatPortalBall] Didn't hit anything.");
            tryCreatingPortal = false;
            return;
        }

        PlaneController planeController = null;
        bool hitCorner = false;
        Vector3 extensionSecondPortalCenter = Vector3.zero;
        Vector3 extensionSecondPortalNormal = Vector3.zero;
        Vector3 cornerPortalNormal = Vector3.zero;
        Vector3 cornerPortalCenter = Vector3.zero;
        float[] cutLeft = new float[2];
        float[] cutRight = new float[2];

        for (int i = 0; i < hits.Count; i++)
        {
            if (hits[i].collider.gameObject.TryGetComponent<PlaneController>(out planeController))
            {
                if (planeController.ShapeType == ShapeTypeEnum.wall)
                {
                    switch (portalType)
                    {
                        case 0:
                            disappearAfter = 10;
                            destination = hits[i].point;
                            break;
                        case 1:
                            disappearAfter = 0;
                            destination = hits[i].point;
                            break;
                        default:
                            disappearAfter = 10;
                            destination = hits[i].point;
                            break;
                    }

                    rotationUp = hits[i].normal;
                    break;
                }
                else
                {
                    if (planeController.ShapeType != ShapeTypeEnum.table) 
                    {
                        Debug.Log("[PortalSceneManager][CreatPortalBall] Hit wrong plane.");
                        tryCreatingPortal = false;
                        return;
                    }
                }
            }

            if(i == hits.Count - 1)
            {
                tryCreatingPortal = false;
                return;
            }
        }

        float radiusLength = radius * 0.3f;

        for (int i = 0; i < 4; i++)
        {
            int iPlus;
            if (i < 3) iPlus = i + 1;
            else iPlus = 0;

            Vector3 toSide = CommonFormula.PointFromPointToLine(planeController.Data.Points[i], planeController.Data.Points[iPlus], destination);
            if (Vector3.Distance(toSide, destination) < radiusLength)
            {
                if (planeController.Data.Points[i].y == planeController.Data.Points[iPlus].y)
                {
                    Debug.Log("[PortalSceneManager][CreatPortalBall] Hit floor or ceiling.");
                    if (portalType != 3) 
                    {
                        tryCreatingPortal = false;
                        return;
                    }
                }
                else
                {
                    Vector3 secondWallCenter = Vector3.zero;
                    bool getSecondWall = false;
                    for (int j = 0; j < SceneComponentManager.Instance.Planes.Count; j++)
                    {
                        if (SceneComponentManager.Instance.Planes[j].Type == "wall")
                        {
                            if (planeController.Data == SceneComponentManager.Instance.Planes[j])
                            {
                                continue;
                            }
                            for (int k = 0; k < SceneComponentManager.Instance.Planes[j].Points.Length; k++)
                            {
                                int kPlus;
                                if (k < 3) kPlus = k + 1;
                                else kPlus = 0;
                                if ((SceneComponentManager.Instance.Planes[j].Points[k] == planeController.Data.Points[i] && SceneComponentManager.Instance.Planes[j].Points[kPlus] == planeController.Data.Points[iPlus]) ||
                                    (SceneComponentManager.Instance.Planes[j].Points[k] == planeController.Data.Points[iPlus] && SceneComponentManager.Instance.Planes[j].Points[kPlus] == planeController.Data.Points[i]))
                                {
                                    secondWallCenter = SceneComponentManager.Instance.Planes[j].Center;
                                    extensionSecondPortalNormal = SceneComponentManager.Instance.Planes[j].Normal;
                                    getSecondWall = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!getSecondWall)
                    {
                        Debug.LogError("[PortalSceneManager][CreatPortalBall] No match second Wall!");
                        tryCreatingPortal = false;
                        return;
                    }

                    secondWallCenter = new Vector3(secondWallCenter.x, destination.y, secondWallCenter.z);
                    Vector3 extensionVector = CommonFormula.UnitVector(secondWallCenter, toSide);
                    extensionSecondPortalCenter = toSide + Vector3.Distance(toSide, destination) * extensionVector;

                    Vector3 firstPortalBoundary = destination + radiusLength * CommonFormula.UnitVector(toSide, destination);
                    Vector3 secondPortalBoundary = extensionSecondPortalCenter + radiusLength * CommonFormula.UnitVector(toSide, secondWallCenter);

                    cornerPortalNormal = CommonFormula.UnitVector(CommonFormula.PointFromPointToLine(firstPortalBoundary, secondPortalBoundary, toSide), toSide);
                    cornerPortalCenter = (firstPortalBoundary + secondPortalBoundary) / 2;

                    Vector3 mn = Vector3.Cross(toSide - spawnPoint, Vector3.up).normalized;
                    Vector3 fn = firstPortalBoundary -CommonFormula.PointFromPointToLine(new Vector3(spawnPoint.x, firstPortalBoundary.y, spawnPoint.z),
                        new Vector3(toSide.x, firstPortalBoundary.y, toSide.z), firstPortalBoundary);

                    if (Vector3.Angle(mn, fn) == 0)
                    {
                        cutLeft[0] = radiusLength;
                        cutRight[0] = Vector3.Distance(toSide, firstPortalBoundary) - radiusLength;

                        cutLeft[1] = -1 * (radiusLength - Vector3.Distance(toSide, secondPortalBoundary));
                        cutRight[1] = radiusLength;
                    }
                    else
                    {
                        cutRight[0] = radiusLength;
                        cutLeft[0] = Vector3.Distance(toSide, firstPortalBoundary) - radiusLength;

                        cutRight[1] = -1 * (radiusLength - Vector3.Distance(toSide, secondPortalBoundary));
                        cutLeft[1] = radiusLength;
                    }

                    hitCorner = true;

                    break;
                }
            }
        }

        GameObject portal = Instantiate(portalPrefab[portalType], spawnPoint, Quaternion.identity);
        PortalBallBehaviour portalBehaviour = portal.GetComponent<PortalBallBehaviour>();
        float duration = Vector3.Distance(spawnPoint, destination) / 7.5f;
        portal.transform.DOMove(destination, duration).SetEase(Ease.Linear);
        PortalIsOpen();

        if (!hitCorner)
        {
            portalBehaviour.Initialized(duration, radius, destination, rotationUp, PortalIsClose, disappearAfter);
        }
        else
        {
            if (Mathf.Abs(destination.y - extensionSecondPortalCenter.y) > 0.001f)
            {
                extensionSecondPortalCenter = new Vector3(extensionSecondPortalCenter.x, destination.y, extensionSecondPortalCenter.z);
            }
            portalBehaviour.Initialized(duration, radius, destination, rotationUp, PortalIsClose, disappearAfter, hitCorner,
                extensionSecondPortalCenter, extensionSecondPortalNormal, cornerPortalNormal, cornerPortalCenter, cutLeft, cutRight);
        }
        currentPortalBallBehaviour = portalBehaviour;
        Debug.Log("[PortalSceneManager][CreatPortalBall] Creat finish.");
    }
}

public enum PortalType 
{
    Basic = 0,
    Universe = 1
}