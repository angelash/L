/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：LanguageData
// 创建者：Steven Yang
// 修改者列表：
// 创建日期：2013-2-7
// 模块描述：本地化语言信息数据模块
//----------------------------------------------------------------*/

using System;
using Mogo.Util;

namespace Mogo.GameData
{
    public partial class LanguageData : GameData<LanguageData>

    {
        public static readonly string fileName = "xml/ChineseData";

        public LanguageData()
        {
            content = string.Empty;
        }

        // Monster 数据
        public string content { get; set; }

        public static string MONEY
        {
            get { return dataMap.Get(20002).content; }
        }

        public static string EXP
        {
            get { return dataMap.Get(20003).content; }
        }

        public static string DIAMOND
        {
            get { return dataMap.Get(20004).content; }
        }

        public string Format(params object[] args)
        {
            if (args != null)
                return string.Format(content, args);
            return content;
        }

        public static string GetContent(int id)
        {
            if (dataMap.ContainsKey(id))
            {
                return dataMap.Get(id).content;
            }
            LoggerHelper.Error(String.Format("Language key {0:0} is not exist ", id));
            return "***";
        }

        public static string GetContent(int id, params object[] args)
        {
            if (dataMap.ContainsKey(id))
            {
                return dataMap[id].Format(args);
            }
            LoggerHelper.Error(String.Format("Language key {0:0} is not exist ", id));
            return "***";
        }

        public static string GetPVPLevelName(int PVPLevel)
        {
            return dataMap.Get(3000 + PVPLevel).content;
        }
    }
}