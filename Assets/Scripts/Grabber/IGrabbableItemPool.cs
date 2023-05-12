using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGrabbableItemPool : MonoBehaviour
{
    public string PoolID;

    public abstract void Init();
    public abstract IGrabbableItem GetItem(Transform parent);
}