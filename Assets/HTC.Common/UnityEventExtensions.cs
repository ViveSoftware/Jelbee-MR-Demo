using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.HTC.Common
{
    [Serializable] public class UnityEventFloat : UnityEvent<float> { }
    [Serializable] public class UnityEventInt : UnityEvent<int> { }
    [Serializable] public class UnityEventVector3 : UnityEvent<Vector3> { }
    [Serializable] public class UnityEventColiider : UnityEvent<Collider> { }
}