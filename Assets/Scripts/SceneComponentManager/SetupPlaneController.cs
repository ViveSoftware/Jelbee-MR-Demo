using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SetupPlaneController : PlaneController, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private MeshRenderer meshRenderer;
    private Material mat;

    [SerializeField] private Color SelectedColor;

    private Color nonSelectedColor;
    private Color selectedColor;

    private const string COLOR_KEY = "_Color";

    public Action<PlaneController> OnClickedHandler;

    void Awake()
    {
        mat = meshRenderer.material;
        nonSelectedColor = mat.GetColor(COLOR_KEY);
        selectedColor = nonSelectedColor * SelectedColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mat.SetColor(COLOR_KEY, selectedColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mat.SetColor(COLOR_KEY, nonSelectedColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickedHandler?.Invoke(this);
    }
}
