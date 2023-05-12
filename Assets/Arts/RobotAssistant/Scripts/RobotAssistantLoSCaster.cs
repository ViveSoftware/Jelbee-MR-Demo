using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAssistantLoSCaster : MonoBehaviour
{
    public delegate void RobotAssistantLoS_Enter();
    public static event RobotAssistantLoS_Enter RobotAssistantLoS_EnterCallback;

    public delegate void RobotAssistantLoS_Exit();
    public static event RobotAssistantLoS_Exit RobotAssistantLoS_ExitCallback;

    public GameObject DebugObj;

    private int robotLayerMask = 1 << 9;
    private static bool m_isAlreadyInLoS = false;
    public static bool isAlreadyInLoS
	{
        get { return m_isAlreadyInLoS; }
        private set { m_isAlreadyInLoS = value; }
	}

    // Start is called before the first frame update
    void Start()
    {
        DebugObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.3f, transform.forward, out hit, 10f, robotLayerMask))
        {
            if (!isAlreadyInLoS) //New hit
			{
                //DebugObj.SetActive(true);

                isAlreadyInLoS = true;
                if (RobotAssistantLoS_EnterCallback != null)
				{
                    RobotAssistantLoS_EnterCallback.Invoke();
                }
            }
            else //Persistent hit
			{
                DebugObj.transform.position = hit.point;
            }
        }
        else
		{
            if (isAlreadyInLoS)
			{
                //DebugObj.SetActive(false);

                isAlreadyInLoS = false;
                if (RobotAssistantLoS_ExitCallback != null)
                {
                    RobotAssistantLoS_ExitCallback.Invoke();
                }
            }
		}
    }
}
