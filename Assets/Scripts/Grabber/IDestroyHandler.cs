using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IDestroyHandler : MonoBehaviour
{
    public abstract void RunDestroy(bool isInstant = false);
    public abstract void StopDestroy();
}
