using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PivotBtn : MonoBehaviour
{
    [SerializeField] private Text label;

    public Action<string> OnClickedHandler;

    public void Init(Action<string> clickHandler, string key)
    {
        label.text = key;
        OnClickedHandler = clickHandler;
    }

    public void OnClicked()
    {
        OnClickedHandler?.Invoke(label.text);
    }
}
