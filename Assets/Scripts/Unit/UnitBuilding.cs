using Mogo.GameData;
using Mogo.Util;
using UnityEngine;

public class UnitBuilding : UnitBase
{
    public BuildingData BaseBuildingData;
    public UISprite m_imgBuilding;
    public UISprite m_imgBuildingColor;
    public int InitPercentage;
    public UnitStar BelongToStar;//所属星球（在协议初始化建筑设置）
    public uint AttackPlayerId;//攻击者id（在协议初始化建筑设置）

    private float ProcductionTime = 10000.0f;
    private float BuildingTime = 0f;
    private int ProductionEnergy = 0;

    public float ScaleAtStar = 1f;//相对于星球的缩放比例
    public bool IsBuilding = false;//是否正在建设

    public int Key;

    protected override string prefabName
    {
        get { return "BuildingUI.prefab"; }
    }

    protected override void InitUnitData()
    {
        UnitGO.transform.parent = UnitParent;
        UnitGO.transform.localPosition = new Vector3(PositionX, PositionY, -1);
        UnitGO.transform.localScale = new Vector3(1, 1, 1);

        if (m_imgBuilding == null)
        {
            m_imgBuilding = UnitGO.transform.FindChild("ItemImg").GetComponent<UISprite>();
            m_imgBuildingColor = UnitGO.transform.FindChild("ItemColor").GetComponent<UISprite>();
            m_imgBuildingColor.gameObject.SetActive(false);
        }
        if (BaseBuildingData.soldier_id != 0)//炮塔不产兵
            ProductionEnergy = SoldierData.dataMap.Get(BaseBuildingData.soldier_id).energy;
        InitBuildingImg();
    }

    /// <summary>
    ///
    /// </summary>
    private void InitBuildingImg()
    {
        m_imgBuilding.spriteName = BaseBuildingData.icon;
        m_imgBuilding.MakePixelPerfect();
        m_imgBuilding.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        m_imgBuildingColor.transform.localScale = new Vector3(1, 1, 1);
        var buildingTime = BaseBuildingData.building_time * (1 - InitPercentage / 100);
        CurEnergy = BaseBuildingData.energy * (1 - InitPercentage / 100);

        if (BelongToStar.BelongTo == null)
        {
            ProcductionTime = BaseBuildingData.production_time;
        }
        else
        {
            ProcductionTime = buildingTime + BaseBuildingData.production_time;
        }
        m_imgBuilding.transform.localPosition = Vector3.zero;
        if (buildingTime > 0)//需要时间去建设
            IsBuilding = true;
        var tween = TweenScale.Begin(UnitGO, buildingTime, new Vector3(BelongToStar.BaseData.radius * ScaleAtStar, BelongToStar.BaseData.radius * ScaleAtStar, 1));
        tween.onFinished = OnBuildingFinished;
    }

    private void OnBuildingFinished(UITweener tween)
    {
        //LoggerHelper.Debug("建筑完成---id:" + BaseBuildingData.id);
        IsBuilding = false;
        if (BelongToStar.BelongTo == null)//发送星球占领协议
        {
            LoggerHelper.Debug("发送星球占领协议UpdateStarBelongTo---" + "AttackPlayerId:" + AttackPlayerId + "  BelongToStar.UnitId:" + BelongToStar.UnitId);
            MogoWorld.thePlayer.RpcCall("UpdateStarBelongTo", AttackPlayerId, BelongToStar.UnitId);
        }
        if (BelongToStar.BelongTo != null && BelongToStar.BelongTo.ID == MogoWorld.thePlayer.ID && BelongToStar.UnitId == MogoWorld.m_dataMapManager.CurStarID)
        {
            LoggerHelper.Debug("my building finish");
            SoundManager.PlaySound("tree grow 1.ogg");
            EventDispatcher.TriggerEvent(Events.StarUIEvent.AddBuildingEvent);//发送我方当前星球添加建筑的消息
        }
    }

    public void BuildingProcess(float _dtTime, int _starId)
    {
        CulBuildingEnergy(_dtTime);
        if (BaseBuildingData.soldier_id <= 0)
        {
            return;
        }
        ProcductionTime -= _dtTime;
        //LoggerHelper.Debug("ProcductionTime:" + ProcductionTime);
        if (ProcductionTime < 0)
        {
            ProcductionTime += BaseBuildingData.production_time;
            MogoWorld.thePlayer.RpcCall("AddSoldier", MogoWorld.thePlayer.ID, _starId, BaseBuildingData.soldier_id, ProductionEnergy, 1);
        }
    }

    private void CulBuildingEnergy(float _dtTime)
    {
        if (CurEnergy < BaseBuildingData.energy)
        {
            BuildingTime += _dtTime;
            if (BuildingTime > MogoWorld.BuildingReversionFrequency)
            {
                BuildingTime -= MogoWorld.BuildingReversionFrequency;
                CurEnergy += MogoWorld.BuildingReversionEnergy;
            }
        }
    }

    protected override void SetGroupData()
    {
        m_imgBuildingColor.gameObject.SetActive(true);
        m_imgBuildingColor.spriteName = MyGroup.building_color;
    }

    public void ReduceBuildingEnergy(int _Energy)
    {
        CurEnergy -= _Energy;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetPos"></param>
    public void OnAttack(Vector3 targetPos)
    {
        BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), targetPos, BaseBuildingData.id);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnHit()
    {
        var tweenPosition = TweenPosition.Begin(UnitGO, 0.02f, new Vector3(PositionX - 4, PositionY, 0));
        m_imgBuilding.gameObject.SetActive(false);
        m_imgBuilding.ShowAsWhiteBlack(true);
        m_imgBuilding.gameObject.SetActive(true);
        tweenPosition.onFinished = (go) =>
        {
            TweenPosition.Begin(UnitGO, 0.02f, new Vector3(PositionX, PositionY, 0));
            m_imgBuilding.gameObject.SetActive(false);
            m_imgBuilding.ShowAsWhiteBlack(false);
            m_imgBuilding.gameObject.SetActive(true);
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnDead()
    {
        SoundManager.PlaySound("Explode_1.ogg");
        BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), new Vector3(PositionX, PositionY, 0), 8, 1);
    }
}