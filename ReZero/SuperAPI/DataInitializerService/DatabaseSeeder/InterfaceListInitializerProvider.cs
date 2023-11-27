﻿using System;
using System.Collections.Generic;
using System.Text; 

namespace ReZero.SuperAPI
{
    internal partial class InterfaceListInitializerProvider
    {
        /// <summary>
        /// 数据库管理
        /// </summary> 
        [TextCN("数据库管理")]
        [TextEN("Database management")]
        public const long Id1 = 1;
        /// <summary>
        /// 内部接口
        /// </summary> 
        [TextCN("接口列表")]
        [TextEN("Internal interface list")]
        public const long Id2 = 2;
        /// <summary>
        /// 接口分类
        /// </summary> 
        [TextCN("接口分类")]
        [TextEN("Interface classification")]
        public const long Id3 = 3;

        /// <summary>
        /// 接口详情
        /// </summary>
        [TextCN("接口详情")]
        [TextEN("Interface Detail")]
        public const long Id4 = 4;



        /// <summary>
        /// 动态接口[测试01]
        /// </summary>
        [TextCN("测试动态接口01")]
        [TextEN("Test API 01")]
        public const long TestId = 5;

        private static ZeroInterfaceList GetNewItem(Action<ZeroInterfaceList> action)
        {
            var result = new ZeroInterfaceList()
            {
                IsInitialized = true,
                DataModel = new DataModel()
            };
            action(result);
            return result;
        }

        private static string GetUrl(ZeroInterfaceList zeroInterface, string actionName)
        {
            return $"/{NamingConventionsConst.ApiReZeroRoute}/{zeroInterface.InterfaceCategoryId}/{actionName}";
        }
    }
}