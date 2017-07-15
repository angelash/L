using UnityEngine;
using System;
using System.Collections;

public class BtnPressedMgr : MonoBehaviour {

    public Action ActionPressed;
    public Action ActionUnPressed;
    public bool UseIsPressing = true;

    private bool isDragging = false;
    private Vector2 dragDelta;
    public Action<Vector2> dragHandler;

    void OnPress(bool isPressed)
    {
        if (UseIsPressing)
        {
            UIUtils.ButtonIsPressing = isPressed;
        }
        if (isPressed)
        {
            if (ActionPressed != null)
                ActionPressed();
        }
        else
        {
            if (ActionUnPressed != null)
                ActionUnPressed();
        }
    }

    void OnDrag(Vector2 delta)
    {
        dragDelta = delta;
        if (dragHandler != null)
        {
            dragHandler(delta);
        }
    }
}
