using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableItemPool : IGrabbableItemPool
{
    public IGrabbableItem[] Prefabs;
    public bool IsEnableDuplicate = true;

    private List<IGrabbableItem> grabbableItems;

    public int PoolCount { get { return grabbableItems.Count; } }

    public override void Init()
    {
        grabbableItems = new List<IGrabbableItem>();
        foreach (IGrabbableItem prefab in Prefabs)
        {
            grabbableItems.Add(prefab);
        }
    }

    public override IGrabbableItem GetItem(Transform parent)
    {
        if (grabbableItems.Count > 0)
        {
            int index = Random.Range(0, grabbableItems.Count);
            IGrabbableItem prefab = grabbableItems[index];

            if (!IsEnableDuplicate)
            {
                grabbableItems.RemoveAt(index);
            }

            IGrabbableItem item = Instantiate(prefab, parent);
            return item;
        }
        return null;
    }
}