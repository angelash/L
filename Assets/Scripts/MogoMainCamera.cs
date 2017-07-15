using UnityEngine;
using System.Collections;
using Mogo.Util;

public class MogoMainCamera : MonoBehaviour
{
    public Transform target;
    //距离目标
    public float m_distance = 5.0f;
    //角度（0为水平,90俯视）
    public float RotationX = 3;
    public float RotationY = 360;
    public float FixedRotSensitivity = 3;
    public float RotSensitivity = 3;
    public float TouchtSensitivity = 0.5f;
    /// <summary>
    /// 两点触摸的时候，记录第一次触摸是按摇杆还是旋转屏幕
    /// </summary>
    private bool m_isRotationFirst;

    public Camera Main_Camera;
    private GameObject mainCamera;
    public Transform GoLockSource;
    private GameObject vr;
    private Transform vrLeft;
    private Transform vrRight;

    public static float minWidth = 0f;
    public static float minHeight = 0f;
    public static float maxWidth;
    public static float maxHeight;

    private bool isVr;

    void Start()
    {
        mainCamera = transform.FindChild("Main_Camera").gameObject;
        Main_Camera = transform.FindChild("Main_Camera").GetComponent<Camera>();
        GoLockSource = transform.FindChild("GoLockSource");
        vr = transform.FindChild("VR").gameObject;
        vrLeft = vr.transform.FindChild("L");
        vrRight = vr.transform.FindChild("R");
        BillboardManager.I.RelatedCamera = Main_Camera;
        maxWidth = (float)(Main_Camera.pixelWidth);
        maxHeight = (float)(Main_Camera.pixelHeight);

        if (Application.platform == RuntimePlatform.Android)
        {
            FixedRotSensitivity = (5 * Screen.width) / 1960;
            if (FixedRotSensitivity >= 5)
                FixedRotSensitivity = 5;
            LoggerHelper.Debug("m_fixedRotSensitivity: " + FixedRotSensitivity);
            RotSensitivity = FixedRotSensitivity;
        }

        AddFogFoWar();
    }

    void Update()
    {

    }

    void LateUpdate()
    {
    }

    public void MoveRotation(Vector2 dragDelta)
    {
    }

    private void MoveRotationY(float value)
    {
        //if (value > 20 || value < -20)
        //    return;
        RotationY += value;
    }

    private void MoveRotationX(float value)
    {
        //if (value > 20 || value < -20)
        //    return;
        RotationX -= value;
        CheckApplyGravity();
    }

    void R3Horizontal()
    {
        float value = (float)(Input.GetAxis("R3Horizontal") * RotSensitivity);
        RotationY += value;
    }

    void R3Vertical()
    {
        float value = (float)(Input.GetAxis("R3Vertical") * RotSensitivity);
        RotationX -= value;
        CheckApplyGravity();
    }

    void CheckApplyGravity()
    {
        //if (MogoWorld.thePlayer.IsApplyGravity())
        //{
        if (RotationX > 90f)
            RotationX = 90f;
        if (RotationX < -60f)
            RotationX = -60f;
        //}
    }

    public void LockSight()
    {
        if (this == null) return;
        if (transform == null) return;
        if (target == null) return;
        transform.position = target.position - Vector3.forward * m_distance;
        transform.LookAt(target);
        transform.RotateAround(target.position, new Vector3(1, 0, 0), RotationX);
        transform.RotateAround(target.position, new Vector3(0, 1, 0), RotationY);
    }

    public void LockLookatTarget(Transform targetPlayer)
    {
        transform.LookAt(targetPlayer);
        float xan = transform.eulerAngles.x;
        transform.position = target.position - Vector3.forward * m_distance;

        //怪跟人
        Vector3 v1 = target.position - targetPlayer.position;
        Vector2 vec1 = new Vector2(v1.x, v1.z).normalized;

        //人跟摄像机
        Vector3 v2 = transform.position - target.position;
        Vector2 vec2 = new Vector2(v2.x, v2.z).normalized;

        //算出夹角
        float angle = Vector2.Angle(vec1, vec2);

        //判断方向并转向
        if (vec1.x > vec2.x) angle = -angle;
        RotationY = angle;
        if (xan > 90)
            xan = xan - 360;
        RotationX = xan;
    }

    public void SwitchVR()
    {
        isVr = !isVr;
        mainCamera.SetActive(!isVr);
        vr.SetActive(isVr);
        MogoWorld.m_uiManager.MogoMainUIPanel.SafeSetActive(!isVr);
        Application.targetFrameRate = isVr ? 60 : 30;
        m_distance = isVr ? 0.01f : 5;
        //MogoWorld.thePlayer.MeshModel.SetActive(!isVr);
        RotSensitivity = isVr ? 1 : FixedRotSensitivity;
    }

    public void SetVRWidth(bool isPlus)
    {
        vrLeft.localPosition = new Vector3(vrLeft.localPosition.x - (isPlus ? 0.1f : -0.1f), vrLeft.localPosition.y, vrLeft.localPosition.z);
        vrRight.localPosition = new Vector3(vrRight.localPosition.x + (isPlus ? 0.1f : -0.1f), vrRight.localPosition.y, vrRight.localPosition.z);
        LoggerHelper.Debug(vrLeft.localPosition + " " + vrRight.localPosition);
    }

    public void SetVRRotation(bool isPlus)
    {
        vrLeft.localEulerAngles = new Vector3(vrLeft.localEulerAngles.x, vrLeft.localEulerAngles.y + (isPlus ? 0.1f : -0.1f), vrLeft.localEulerAngles.z);
        vrRight.localEulerAngles = new Vector3(vrRight.localEulerAngles.x, vrRight.localEulerAngles.y - (isPlus ? 0.1f : -0.1f), vrRight.localEulerAngles.z);
        LoggerHelper.Debug(vrLeft.localEulerAngles + " " + vrRight.localEulerAngles);
    }

    private void AddFogFoWar()
    {
        return;
        FogOfWar3D _fog = mainCamera.AddComponent<FogOfWar3D>();
        _fog.m_viewer = GameObject.Find("Avatar(Clone)").transform;
        _fog.m_topLeft = GameObject.Find("S03_railing02_0").transform;
        _fog.m_topRight = GameObject.Find("S03_railing02_1").transform;
        _fog.m_bottomRight = GameObject.Find("S03_railing02_2").transform;
        _fog.m_bottomLeft = GameObject.Find("S03_railing02_3").transform;
        _fog.m_fogCoverLayer = (int)Mogo.Util.LayerMask.Default;
        _fog.m_edgeSmoothValue = 0.0f;
        _fog.m_fogDensity = 0.75f;
    }
}
