using com.HTC.Common;
using System.Collections;
using UnityEngine;
public enum EnvType
{
    none = 0,
    Universe = 1,
    Forest = 2,
}
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    public GameObject[] envs;
    public EnvType currentEnv = EnvType.none;

    //queue
    private GameObject currentEnvObj;
    private GameObject nextEnvObj;


    private void Awake()
    {
        Instance = this;
    }

    public void OnPortalOpen()
    {
        MRTutorialManager.Instance.ForceStopTutorial();
        switch (PortalSceneManager.Instance.GetCurrentPortalOrder())
        {
            case 0:
                ChangeEnvironment(EnvType.Forest);
                break;
            case 1:
                ChangeEnvironment(EnvType.Universe);
                break;
        }

        //switch grabber group
        switch (currentEnv)
        {
            case EnvType.Universe:
                MRTutorialManager.Instance.ShowTutorial("Tutorial4_Planet", false);
                BubbleLauncher.Instance.gameObject.SetActive(false);
                break;
        }
    }

    public void ChangeEnvironment(EnvType envType)
    {
        if (currentEnvObj == envs[(int)envType - 1]) return;
        currentEnv = envType;
        nextEnvObj = envs[(int)envType - 1];
        StartCoroutine(OnChangeEnvironmentCoroutine(envType));
    }

    private IEnumerator OnChangeEnvironmentCoroutine(EnvType envType)
    {
        if (currentEnvObj)
        {
            nextEnvObj.SetActive(true);
            float currentValue = 1;
            float nextValue = 0;
            while (currentValue >= 0)
            {
                currentValue -= Time.deltaTime;
                nextValue += Time.deltaTime;
                currentEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", currentValue);
                nextEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", nextValue);
                yield return null;
            }
            currentEnvObj.SetActive(false);
        }
        else
        {
            nextEnvObj.SetActive(true);
            float nextValue = 0;
            while (nextValue < 1)
            {
                nextValue += Time.deltaTime * 0.3f;
                nextEnvObj.GetComponent<MeshRenderer>().material.SetFloat("_ErosionValue", nextValue);
                yield return null;
            }
        }

        currentEnvObj = nextEnvObj;
        nextEnvObj = null;
        yield return null;
    }
}