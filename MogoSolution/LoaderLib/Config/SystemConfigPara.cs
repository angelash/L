#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：SystemConfig
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.3.19
// 模块描述：系统参数配置。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;

namespace Mogo.Util
{
    /// <summary>
    /// 系统参数配置。
    /// </summary>
    public partial class SystemConfig
    {
        //true
#if !UNITY_IPHONE
        public const string PLATFORM_SSJJ = "4399";
        public const string PLATFORM_DJ = "91";
        public const string PLATFORM_PPTV = "pptv";
        public const string PLATFORM_360 = "360";
        public const string PLATFORM_DOWNJOY = "dangle";
        public const string PLATFORM_UC = "uc";
        public const string PLATFORM_MI = "mi";
        public const string PLATFORM_PPS = "pps";
        public const string PLATFORM_ANZHI = "anzhi";
        public const string PLATFORM_DK = "DK";
        public const string PLATFORM_CW = "eone";
        public const string PLATFORM_KK = "keke";
        public const string PLATFORM_3G = "3g";
        public const string PLATFORM_LENOVO = "lenovo";
        public const string PLATFORM_HUAWEI = "huawei";
        public const string PLATFORM_WANPU = "wanpu";
        public const string PLATFORM_BAIDU = "baiduqianbao";
        public const string PLATFORM_37WAN = "37wan";
        public const string PLATFORM_37WANBY = "37wan";
        public const string PLATFORM_KUGOU = "kugou";
        public const string PLATFORM_YOUMI = "youmi";
        public const string PLATFORM_VIVO = "vivo";
        public const string PLATFORM_JINSHAN = "jinshan";
        public const string PLATFORM_WANDOUJIA = "wdj";
        public const string PLATFORM_MENGCHENG = "mc";
        public const string PLATFORM_PAOJIAO = "paojiao";
        public const string PLATFORM_KENO = "keno";
        public const string PLATFORM_EGAME = "egame";
        public const string PLATFORM_MOGU = "mogu";
        public const string PLATFORM_CC = "cc";
        public const string PLATFORM_YYWAN = "yywan";
        public const string PLATFORM_SM = "smnet";
        public const string PLATFORM_APPCHINA = "appchina";
        public const string PLATFORM_ZEYU = "6637";
        /// <summary>
        /// key是平台缩写，value是平台包名及平台文件夹名
        /// </summary>
        public static Dictionary<string, string> PlatformDic = new Dictionary<string, string>()
        {
            {"com.ahzs.sy4399",PLATFORM_SSJJ},
            {"com.ahzs.qx.sy4399",PLATFORM_SSJJ},
            {"com.ahzscx.sy4399",PLATFORM_SSJJ},
            {"com.ahzs.dj",PLATFORM_DJ},
            {"com.ahzs.pptv",PLATFORM_PPTV},
            {"com.ahzs.downjoy",PLATFORM_DOWNJOY},
            {"com.ahzs.uc",PLATFORM_UC},
            {"com.ahzs.qihoo360",PLATFORM_360},
            {"com.ahzs.mi",PLATFORM_MI},
            {"com.ahzs.pps",PLATFORM_PPS},
            {"com.ahzs.anzhi",PLATFORM_ANZHI},
            {"com.ahzs.s91",PLATFORM_DJ},
            {"com.ahzs.DK",PLATFORM_DK},
            {"com.ahzs.cw",PLATFORM_CW},
            {"com.ahzs.cw2",PLATFORM_CW},
            {"com.ahzs.cw3",PLATFORM_CW},
            {"com.ahzs.cw4",PLATFORM_CW},
            {"com.ahzs.nearme.gamecenter",PLATFORM_KK},
            {"com.ahzs.jiubang3g",PLATFORM_3G},
            {"com.ahzs.lenovo",PLATFORM_LENOVO},
            {"com.ahzs.HUAWEI",PLATFORM_HUAWEI},
            {"com.ahzs.wanpu",PLATFORM_WANPU},
            {"com.ahzs.baidu",PLATFORM_BAIDU},
            {"com.ahzs.sqwan",PLATFORM_37WAN},
            {"com.ahzs.sqwanby",PLATFORM_37WANBY},
            {"com.ahzs.s37wan.fg",PLATFORM_37WAN},
            {"com.ahzs.kugou",PLATFORM_KUGOU},
            {"com.ahzs.youmi",PLATFORM_YOUMI},
            {"com.sy4399.ahzs.vivo",PLATFORM_VIVO},
            {"com.ahzs.jinshan",PLATFORM_JINSHAN},
            {"com.ahzs.wdj",PLATFORM_WANDOUJIA},
            {"com.ahzs.mc",PLATFORM_MENGCHENG},
            {"com.ahzs.paojiao",PLATFORM_PAOJIAO},
            {"com.ahzs.keno",PLATFORM_KENO},
            {"com.ahzs.egame",PLATFORM_EGAME},
            {"com.ahzs.mogu",PLATFORM_MOGU},
            {"com.ahzs.cc",PLATFORM_CC},
            {"com.ahzs.yayawan",PLATFORM_YYWAN},
            {"com.ahzs.sm",PLATFORM_SM},
            {"com.ahzs.sm2",PLATFORM_SM},
            {"com.ahzs.sm3",PLATFORM_SM},
            {"com.ahzs.appchina",PLATFORM_APPCHINA},
            {"com.ahzs.zeyu",PLATFORM_ZEYU },
            {"com.ahzs.zeyu2",PLATFORM_ZEYU}
        };
#endif
        //false
    }
}