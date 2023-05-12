using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DestroyCommandMediator 
{
    private static Dictionary<string, Action> handlerMap = new Dictionary<string, Action>();

    public static void RegisterHandler(string key, Action handler)
    { 
        if(!handlerMap.ContainsKey(key))
        {
            handlerMap[key] = delegate { };
        }

        handlerMap[key] += handler;
    }

    public static void UnregisterHandler(string key, Action handler)
    {
        if(handlerMap.ContainsKey(key))
        {
            handlerMap[key] -= handler;
        }
    }

     public static void Invoke(string key)
    {
        if(handlerMap.ContainsKey(key))
        {
            handlerMap[key].Invoke();
        }
    }
}
