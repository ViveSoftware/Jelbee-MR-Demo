using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEventMediator
{
    public static Action<GameObject> OnFishFoodAppearHandler = delegate { };
}
