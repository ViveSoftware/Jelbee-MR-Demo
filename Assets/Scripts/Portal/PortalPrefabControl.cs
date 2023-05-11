using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPrefabControl : MonoBehaviour
{
    [SerializeField] private GameObject fullPortal;
    [SerializeField] private GameObject halfPortal_1;
    [SerializeField] private GameObject halfPortal_2;

    private void Awake()
    {
        fullPortal.SetActive(false);
        halfPortal_1.SetActive(false);
        halfPortal_2.SetActive(false);
    }

    public void FullPortalOn(Vector3 portalScale, Vector3 fullPortalPos, Vector3 fullPortalNormal) 
    {
        fullPortal.SetActive(true);
        halfPortal_1.SetActive(false);
        halfPortal_2.SetActive(false);

        fullPortal.transform.position = fullPortalPos;
        fullPortal.transform.forward = fullPortalNormal;

        fullPortal.transform.localScale = Vector3.zero;
        fullPortal.transform.DOScale(portalScale, 1f).SetEase(Ease.OutBack);

        fullPortal.transform.Find("portal/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeLeft", 2);
        fullPortal.transform.Find("portal/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeRight", 2);
    }
    public void HalfPortalOn(Vector3 portalScale, Vector3 halfPortal_1_Pos, Vector3 halfPortal_1_Normal, Vector3 halfPortal_2_Pos, Vector3 halfPortal_2_Normal, float[] cutLeft, float[] cutRight)
    {
        fullPortal.SetActive(false);
        halfPortal_1.SetActive(true);
        halfPortal_2.SetActive(true);

        halfPortal_1.transform.position = halfPortal_1_Pos;
        halfPortal_1.transform.forward = halfPortal_1_Normal;

        halfPortal_2.transform.position = halfPortal_2_Pos;
        halfPortal_2.transform.forward = halfPortal_2_Normal;

        halfPortal_1.transform.localScale = Vector3.zero;
        halfPortal_1.transform.DOScale(portalScale, 1f).SetEase(Ease.OutBack);

        halfPortal_2.transform.localScale = Vector3.zero;
        halfPortal_2.transform.DOScale(portalScale, 1f).SetEase(Ease.OutBack);

        halfPortal_1.transform.Find("portal h1/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeLeft", cutLeft[0]);
        halfPortal_1.transform.Find("portal h1/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeRight", cutRight[0]);
        halfPortal_2.transform.Find("portal h2/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeLeft", cutLeft[1]);
        halfPortal_2.transform.Find("portal h2/PortalMesh").GetComponent<MeshRenderer>().material.SetFloat("_VisibleRangeRight", cutRight[1]);
    }

    public void FullPortalOff() 
    {
        StartCoroutine(FullPortalCloseing());
    }

    IEnumerator FullPortalCloseing() 
    {
        fullPortal.transform.DOScale(0, 1.5f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(1.5f);
        fullPortal.SetActive(false);
    }

    public void HalfPortalOff()
    {
        StartCoroutine(HalfPortalCloseing());
    }

    IEnumerator HalfPortalCloseing()
    {
        halfPortal_1.transform.DOScale(0, 1.5f).SetEase(Ease.InBack);
        halfPortal_2.transform.DOScale(0, 1.5f).SetEase(Ease.InBack);
        yield return new WaitForSeconds(1.5f);
        halfPortal_1.SetActive(false);
        halfPortal_2.SetActive(false);
    }
}
