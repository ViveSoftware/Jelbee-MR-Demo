using UnityEngine;
using DG.Tweening;

public class PlanetsHandler : MonoBehaviour
{
    public Collider collider;
    float knockingTime = 0;
    bool inSolarSyeten = false;

    void Awake()
    {
        collider = this.GetComponent<Collider>();
        collider.enabled = false;        
    }    

    public void enterSolarSystem()
    {
        knockingTime = 3;
        inSolarSyeten = true;
    }

    private void Update()
    {
        if (knockingTime > 0) knockingTime -= Time.deltaTime;
        else
        {
            if(inSolarSyeten)  collider.enabled = true;
        }
    }
    public void Knocking(Vector3 pos)
    {
        if (knockingTime > 0) return;
        else
        {
            knockingTime = 1f;
            Vector3 oriPos = this.transform.position;
            Vector3 dir = (oriPos - pos).normalized;
            this.transform.DOMove(oriPos + dir * 0.1f, 0.15f).SetEase(Ease.Linear);
            this.transform.DOLocalMove(Vector3.zero, 0.5f).SetDelay(0.15f);
        }
    }
}
