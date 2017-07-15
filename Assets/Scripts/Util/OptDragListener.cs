using System.Collections;
using UnityEngine;

public class OptDragListener
{
    private Plane mPlane;
    private Vector3 mLastPos;

    public delegate void BoolDelegate(GameObject go, bool state);

    public delegate void VectorDelegate(GameObject go, Vector3 delta, Vector3 currentPos);

    public BoolDelegate onPress;
    public VectorDelegate onDrag;

    public static OptDragListener Get(GameObject gameObject)
    {
        var opt = new OptDragListener();
        var listener = UIEventListener.Get(gameObject);
        listener.onDrag = opt.OnThisDrag;
        listener.onPress = opt.OnThisPress;

        return opt;
    }

    private void OnThisPress(GameObject go, bool isPress)
    {
        // Remember the hit position
        mLastPos = UICamera.lastHit.point;

        // Create the plane to drag along
        mPlane = new Plane(UICamera.currentCamera.transform.rotation * Vector3.back, mLastPos);
        if (onPress != null) onPress(go, isPress);
    }

    private void OnThisDrag(GameObject go, Vector2 delta)
    {
        Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

        float dist = 0f;

        if (mPlane.Raycast(ray, out dist))
        {
            Vector3 currentPos = ray.GetPoint(dist);
            Vector3 offset = currentPos - mLastPos;
            mLastPos = currentPos;
            if (onDrag != null) onDrag(go, offset, currentPos);
        }
    }
}