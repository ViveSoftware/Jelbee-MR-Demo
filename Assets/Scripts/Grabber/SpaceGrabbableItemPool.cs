using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceGrabbableItemPool : IGrabbableItemPool
{
    [Header("Planet Setting")]

    [SerializeField] private Planet planetPrefab;
    [SerializeField] private Vector2 planetSizeRange;
    [SerializeField] private Material[] planeMats;

    [SerializeField] private float ringPosibility = 0.2f;
    
    [SerializeField] private Material[] ringMats;
    [SerializeField] private MeshRenderer[] ringPrefabs;

    public override void Init()
    {
       
    }

    public override IGrabbableItem GetItem(Transform parent)
    {
        Planet newPlanet = Instantiate(planetPrefab, parent);
        newPlanet.PlanetObject.material = planeMats[Random.Range(0, planeMats.Length)];
        newPlanet.transform.localScale = Vector3.one * Mathf.Lerp(planetSizeRange.x, planetSizeRange.y, Random.Range(0f, 1f));
        
        bool isCreateRing = Random.Range(0.0f, 1.0f) <= ringPosibility;
        if(isCreateRing)
        {
            MeshRenderer ring = Instantiate(ringPrefabs[Random.Range(0, ringPrefabs.Length)]);
            ring.material = ringMats[Random.Range(0, ringMats.Length)];
            ring.transform.SetParent(newPlanet.transform, false);
            ring.transform.localScale = Vector3.one;
            ring.transform.forward = Random.insideUnitSphere;
            ring.gameObject.SetActive(true);
        }

        newPlanet.gameObject.SetActive(true);

        return newPlanet;
    }
}
