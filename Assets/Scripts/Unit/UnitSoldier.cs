using Mogo.Game;
using Mogo.GameData;
using Mogo.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSoldier : UnitBase
{
    public SoldierData BaseSoldierData;
    public UISprite m_imgSoldier;
    public UISprite m_imgSoldierColor;

    private UILabel m_labelName;
    private UILabel m_labelNum;

    public EntityParent BelongTo;
    public bool IsMoveAnimation = true;

    //运动路径
    private MapUtil.RoutePlanResult m_listMovePath;

    private int m_iCurMoveStep = 0;
    private float m_fMoveTime = 1000;
    private object[] TipsContent;

    private static float soldierOrgScale = 10.0f;
    private static float soldierAddtiveScale = 10.0f;
    /// <summary>
    /// 超过当前数值时，icon缩放到最大
    /// </summary>
    private static int maxSoldierNumScale = 100;

    public UnitStar BelongToStar;//所属星球（在协议初始化士兵设置）

    public int SelfProduceEnergy = 0;//记录星球上该兵种自产兵总能量

    public bool IsShowSoldierIntrusionStar = false;

    protected override string prefabName
    {
        get { return "SoldierUI.prefab"; }
    }

    public MapUtil.RoutePlanResult MovePath
    {
        set { m_listMovePath = value; m_iCurMoveStep = 0; MoveToNextStar(); }
    }

    protected override void InitUnitData()
    {
        UnitGO.transform.parent = UnitParent;
        if (IsMoveAnimation)
        {
            var tweenPos = Move(PositionX, PositionY);
            tweenPos.from = new Vector3(BelongToStar.PositionX, BelongToStar.PositionY, 0);
        }
        else
        {
            UnitGO.transform.localPosition = new Vector3(PositionX, PositionY, 0);
        }
        var _scale = CountScale();
        UnitGO.transform.localScale = new Vector3(_scale, _scale, 1);

        if (m_imgSoldier == null)
        {
            m_imgSoldier = UnitGO.transform.FindChild("ItemImg").GetComponent<UISprite>();
            m_imgSoldierColor = UnitGO.transform.FindChild("ItemColor").GetComponent<UISprite>();
            m_imgSoldierColor.gameObject.SetActive(false);
            m_labelName = UnitGO.transform.FindChild("NameText").GetComponent<UILabel>();
            m_labelNum = UnitGO.transform.FindChild("NumText").GetComponent<UILabel>();
        }

        m_imgSoldier.spriteName = BaseSoldierData.icon;

        m_labelName.text = LanguageData.GetContent(BaseSoldierData.name);
        m_labelNum.text = GetSoldierNum().ToString();

        /*if (CallBack != null)
        {
            CallBack(BelongToStar);
        }*/
        if (IsShowSoldierIntrusionStar)
        {
            ShowSoldierIntrusionStar();
        }
    }

    protected override void ClearUnitData()
    {
        BelongTo = null;
        SelfProduceEnergy = 0;
        IsShowSoldierIntrusionStar = false;
    }

    public TweenPosition Move(float posX, float posY, float duration = 0.5f)
    {
        PositionX = posX;
        PositionY = posY;
        return Move(new Vector3(posX, posY, 0), duration);
    }

    private TweenPosition Move(Vector3 targetPos, float duration = 0.5f)
    {
        if (Vector3.Distance(UnitGO.transform.localPosition, targetPos) > 500)
            duration = 0;
        return TweenPosition.Begin(UnitGO, duration, targetPos);
    }

    public void AddSoldier(int _Energy, int _isSelfProduce)
    {
        CurEnergy += _Energy;
        if (_isSelfProduce == 1)//是自产的兵
            SelfProduceEnergy += _Energy;
        if (m_labelNum != null)
            m_labelNum.text = GetSoldierNum().ToString();

        ChangeNumberScale();
    }

    public void RemoveSoldier(int _Energy, bool _isRemoveSelfProduce = true)
    {
        CurEnergy -= _Energy;
        if (_isRemoveSelfProduce)
        {
            SelfProduceEnergy -= _Energy;
        }
        if (SelfProduceEnergy < 0) SelfProduceEnergy = 0;
        if (CurEnergy <= 0)
            return;
        if (m_labelNum != null)
        {
            m_labelNum.text = GetSoldierNum().ToString();
        }

        ChangeNumberScale();
    }

    public int GetSoldierNum()
    {
        return (int)(Math.Ceiling((float)CurEnergy / BaseSoldierData.energy));
    }

    /// <summary>
    /// 获取集合兵数量
    /// </summary>
    /// <returns></returns>
    public int GetGatherSoldierNum()
    {
        return (int)(Math.Ceiling((float)SelfProduceEnergy / BaseSoldierData.energy));
    }

    public int GetLeftEnergy()
    {
        return CurEnergy % BaseSoldierData.energy;
    }

    //private int AttackStarNum = 2;
    //private int AttackStarEnergy = 0;
    public int GetAttackStarEnergy()
    {
        /*AttackStarNum ++;
        if (AttackStarNum * MogoWorld.AttackStarFrequeny < 1)
        {
            return AttackStarEnergy;
        }
        AttackStarNum = 0;
        AttackStarEnergy = (int)(GetLeftEnergy() * MogoWorld.AttackStarFrequeny);
        if (AttackStarEnergy <= 0)
        {
            AttackStarEnergy = (int)(BaseSoldierData.energy * MogoWorld.AttackStarFrequeny);
        }

        return AttackStarEnergy;*/
        var AttackStarEnergy = GetLeftEnergy();
        if (AttackStarEnergy <= 0)
        {
            AttackStarEnergy = (int)(BaseSoldierData.energy);
        }
        return AttackStarEnergy;
    }

    public void MoveToNextStar()
    {
        if ((m_iCurMoveStep + 1) < m_listMovePath.PassedStars.Count)
        {
            UnitStar _startStar = m_listMovePath.PassedStars[m_iCurMoveStep];
            UnitStar _endStar = m_listMovePath.PassedStars[m_iCurMoveStep + 1];
            float distace = Vector2.Distance(new Vector2(_startStar.PositionX, _startStar.PositionY), new Vector2(_endStar.PositionX, _endStar.PositionY));
            m_fMoveTime = distace / BaseSoldierData.move;
            Move(_endStar.PositionX, _endStar.PositionY, m_fMoveTime - 0.01f);
            if (BelongTo.ID != MogoWorld.thePlayer.ID)
            {
                ShowUnit(_endStar.IsExplore);
            }
        }
        else
        {
            if (BelongTo.ID == MogoWorld.thePlayer.ID)
            {
                MogoWorld.thePlayer.RpcCall("AddSoldier", BelongTo.ID, m_listMovePath.PassedStars[m_iCurMoveStep].UnitId, BaseSoldierData.id, CurEnergy, 0);
            }
            m_fMoveTime = 1000;
            m_listMovePath = null;
            ClearUnit();
            MogoWorld.m_dataMapManager.RemoveArrivedSoldier(this);
        }
    }

    public void CulMoveTime(float _dtTime)
    {
        if (m_listMovePath == null)
        {
            return;
        }
        m_fMoveTime -= _dtTime;
        if (m_fMoveTime > 0)
        {
            return;
        }
        m_iCurMoveStep++;
        MoveToNextStar();
    }

    protected override void SetGroupData()
    {
        m_imgSoldierColor.gameObject.SetActive(true);
        m_imgSoldierColor.spriteName = MyGroup.soldier_color;
    }


    public float CountScale()
    {
        var soldierNum = GetSoldierNum();
        if (soldierNum > maxSoldierNumScale)
            return soldierOrgScale + soldierAddtiveScale;
        else
            return soldierOrgScale + ((float)GetSoldierNum() / maxSoldierNumScale) * soldierAddtiveScale;
    }

    public void ChangeNumberScale()
    {
        if (UnitGO == null)
        {
            return;
        }
        var _scale = CountScale();
        //float _scale = (soldierMaxScale * (GetSoldierNum() / 100) * 0.1f);
        //if (_scale < soldierMinScale)
        //{
        //    _scale = soldierMinScale;
        //}
        //if (_scale >= soldierMaxScale)
        //{
        //    _scale = soldierMaxScale;
        //}
        TweenScale.Begin(UnitGO, 0.2f, new Vector3(_scale, _scale, 1));
    }

    public void OnAttack(Vector3 targetPos)
    {
        BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), targetPos, BaseSoldierData.id);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnHit()
    {
        //BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), new Vector3(PositionX, PositionY, 0), 4);

        var tweenPosition = TweenPosition.Begin(UnitGO, 0.02f, new Vector3(PositionX - 4, PositionY, 0));
        m_imgSoldier.gameObject.SetActive(false);
        m_imgSoldier.ShowAsWhiteBlack(true);
        m_imgSoldier.gameObject.SetActive(true);
        tweenPosition.onFinished = (go) =>
        {
            TweenPosition.Begin(UnitGO, 0.02f, new Vector3(PositionX, PositionY, 0));
            m_imgSoldier.gameObject.SetActive(false);
            m_imgSoldier.ShowAsWhiteBlack(false);
            m_imgSoldier.gameObject.SetActive(true);
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

    /// <summary>
    /// 入侵
    /// </summary>
    public void OnIntrusion(int _Energy)
    {
        SoundManager.PlaySound("b002_Seedling_Dive0.ogg");
        //BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), new Vector3(BelongToStar.PositionX, BelongToStar.PositionY, 0), 3);
        MogoWorld.m_dataMapManager.ShowSoldierIntrusionStar(BelongTo.ID, BelongToStar.UnitId, BaseSoldierData.id, _Energy, PositionX, PositionY);
        //BillboardManager.I.Attack(new Vector3(PositionX, PositionY, 0), new Vector3(BelongToStar.PositionX, BelongToStar.PositionY, 0), BaseSoldierData.id);
    }


    private void ShowSoldierIntrusionStar()
    {
        var tweenPos = TweenPosition.Begin(UnitGO, 0.5f, new Vector3(BelongToStar.PositionX, BelongToStar.PositionY, 1.0f));
        var tweenScale = TweenScale.Begin(UnitGO, 0.5f, new Vector3(0.3f, 0.3f, 1));

        tweenScale.onFinished = (go) => { ClearUnit(); };
    }
}