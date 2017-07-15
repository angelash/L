using Mogo.Game;
using Mogo.GameData;
using Mogo.UI;
using Mogo.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUIMgr : UILogic
{
    //private Transform m_cameraTransform;
    private Transform m_StarMap;

    //private Transform m_BuildingMap;
    //private Transform m_SoldierMap;
    private Transform m_BattleUIPanel;

    private Transform m_BattleBG;
    private BoxCollider m_BattleMapCollider;

    //private MogoColoredEraser m_ColoredEraser;
    //private MogoUIEraserTexture m_EraserTexture;

    public CameraController MyCameraController;

    private float m_orgMapWidth;
    private float m_orgMapHeight;
    private float m_lastBGScale = 1;//保存背景实际缩放大小
    private float m_lastCamScale = 1;//记录上次缩放时摄像机的缩放大小
    private float m_BGScaleDelta;//根据摄像机缩放差值按比例取值的背景缩放差值

    public override UIProperties Properties
    {
        get
        {
            return UIProperties.DontShowLoadingTip;
        }
    }

    protected override string[] Resources
    {
        get
        {
            return new string[] { "BattleUI.prefab" };
        }
    }

    protected override void OnResourceLoaded()
    {
        //var parent = MogoWorld.m_uiManager.MogoMainUIPanel.FindChild("BattleUI/Center");
        var parent = MogoWorld.m_sceneManager.MogoLogicPanel;
        SyncCreateUIInstanceWithRootTransform(Resources[0], parent);
        m_myTransform.localPosition = new Vector3(0, 0, 0);
        m_myTransform.localScale = new Vector3(1.0f, 1.0f, 1f);

        //m_cameraTransform = FindTransform("EufloriaBattleUICamera");
        m_StarMap = FindTransform("StarMap");
        // m_BuildingMap = FindTransform("BuildingMap");
        // m_SoldierMap = FindTransform("SoldierMap");

        MogoWorld.m_dataMapManager.BuildLayer = FindTransform("BuildingMap");
        MogoWorld.m_dataMapManager.SoldierLyaer = FindTransform("SoldierMap");
        BillboardManager.I.FloatTextTrans = FindTransform("FloatText");

        m_BattleUIPanel = FindTransform("EufloriaBattleUIPanel");
        m_BattleBG = FindTransform("Bg");
        m_BattleMapCollider = m_BattleUIPanel.GetComponent<BoxCollider>();
        //UIDragObject _dragOb = m_BattleUIPanel.gameObject.AddComponent<UIDragObject>();
        MyCameraController = m_BattleUIPanel.gameObject.AddComponent<CameraController>();
        //_dragOb.target = m_BattleUIPanel;

        UIEventListener.Get(FindTransform("EufloriaBattleUIPanel").gameObject).onClick = OnCloseStarInfoUI;//关闭战斗UI面板

        //m_ColoredEraser = FindTransform("MapMask").gameObject.AddComponent<MogoColoredEraser>();
        // m_ColoredEraser.image = FindTransform("MapMask").GetComponent<UITexture>();
        //m_ColoredEraser.OnEraser(10, 10, 100);
        //m_EraserTexture = FindTransform("StarMapBG").gameObject.AddComponent<MogoUIEraserTexture>();

        //FindTransform("StarMapBG").gameObject.SetActive(false);

        AddListeners();
    }

    //public void EraserTexture(float posX, float posY, int Scale)
    //{
    //    m_EraserTexture.brushScale = Scale;
    //    m_EraserTexture.OnMouseMove(new Vector2(posX, posY));
    //}

    private void OnCloseStarInfoUI(GameObject go)
    {
        StarInfoController.getInstance().CloseUI();
    }

    protected override void OnShow(object[] param, System.Action callback)
    {
        base.OnShow(param, callback);
        if (param.Length > 0)
        {
            var mapId = (int)param[0];
            InitStarMap(mapId);
        }
        else
        {
            InitStarMap(1);
        }
    }

    protected override void OnRelease()
    {
        RemoveListeners();
        base.OnRelease();
    }

    private void AddListeners()
    {
        //EventDispatcher.AddEventListener<float,float>(Events.ControllerInputEvent.OneTouchMoved, OnMoveMap);
    }

    private void RemoveListeners()
    {
        //EventDispatcher.RemoveEventListener<float, float>(Events.ControllerInputEvent.OneTouchMoved, OnMoveMap);
    }

    /// <summary>
    /// 根据地图 ID 初始化 地图
    /// </summary>
    /// <param name="_mapType"></param>
    public void InitStarMap(int _mapType)
    {
        MogoWorld.m_dataMapManager.ClearDataInMap();

        MapData _mapdata = MapData.dataMap.Get(_mapType);
        List<int> _size = _mapdata.size;
        m_orgMapWidth = _size[0] * 1.2f;
        m_orgMapHeight = _size[1] * 1.2f;
        m_BattleBG.localScale = new Vector3(m_orgMapWidth, m_orgMapHeight, 1);
        m_BattleBG.localPosition = new Vector3(100, -100, 0);
        m_BattleMapCollider.size = new Vector3(m_orgMapWidth, m_orgMapWidth, 1);

        List<int> _starID = _mapdata.starId;
        List<int> _starX = _mapdata.positionX;
        List<int> _starY = _mapdata.positionY;

        MogoWorld.m_dataMapManager.InitStarDataInMap(_starID, _starX, _starY, m_StarMap);

        MoveCamera();//移动镜头到自己星球总部
        MyCameraController.onDrag += MoveBG;
        MyCameraController.onScale += ScaleBG;
    }

    private void ScaleBG(GameObject go, float scale)
    {
        if (m_BGScaleDelta == 0)
            m_BGScaleDelta = Mathf.Abs((m_lastBGScale - scale) * 0.2f);//获取缩放比例

        m_lastBGScale += m_lastCamScale - scale < 0 ? m_BGScaleDelta : -m_BGScaleDelta;//根据摄像机是缩小或者放大来决定加减
        m_lastCamScale = scale;
        TweenScale.Begin(m_BattleBG.gameObject, 0.2f, new Vector3(m_orgMapWidth * m_lastBGScale, m_orgMapHeight * m_lastBGScale, 1));
    }

    private void MoveBG(GameObject go, Vector3 offset, Vector3 curr)
    {
        m_BattleBG.localPosition += offset * 0.2f;
    }

    /// <summary>
    /// 移动镜头到自己星球总部
    /// </summary>
    private void MoveCamera()
    {
        UnitStar headStar = MogoWorld.m_dataMapManager.GetUnitStarById(MogoWorld.thePlayer.HeadQuarterId);
        m_BattleUIPanel.localPosition = new Vector3(-headStar.PositionX, -headStar.PositionY, 0);
        //GameObject camera = m_myGameObject.transform.parent.parent.FindChild("Camera").gameObject;
        //Vector3 pos = new Vector3(headStar.PositionX, headStar.PositionY, 0);
        //camera.transform.localPosition = pos;
        //m_myGameObject.transform.FindChild("Bg").localPosition = pos;
    }
}