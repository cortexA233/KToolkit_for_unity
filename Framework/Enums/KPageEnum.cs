using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace KToolkit
{
    // UIManager的自动注册函数写在这里
    public partial class KUIManager
    {

        public static Dictionary<Type, KUI_Info> UI_INFO_MAP { get; private set; } = new Dictionary<Type, KUI_Info>();
        public static Dictionary<Type, KUI_Cell_Info> KUI_CELL_INFO_MAP { get; private set; } =
            new Dictionary<Type, KUI_Cell_Info>();
        
        // 新建页面可以通过KUI_Info宏进行自动注册
        private void AutoInitPageDict()
        {
            var UIPagesType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(KUIBase)))
                .Where(type => type.GetCustomAttribute<KUI_Info>() != null);
            foreach (var uiType in UIPagesType)
            {
                // 简单判重，优先以手动注册为准
                if (!UI_INFO_MAP.ContainsKey(uiType))
                {
                    // KUI_CELL_INFO_MAP.Add(uiType, uiType.GetCustomAttribute<KUI_Cell_Info>());
                    UI_INFO_MAP.Add(uiType,
                        new KUI_Info(uiType.GetCustomAttribute<KUI_Info>().prefabPath,
                            uiType.GetCustomAttribute<KUI_Info>().name));
                } 
            }
        }
        
        // 新建页面可以通过扫描所有KUI_Info进行自动注册
        private void AutoInitCellDict()
        {
            var UICellsType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(KUICell)))
                .Where(type => type.GetCustomAttribute<KUI_Cell_Info>() != null);
            foreach (var cellType in UICellsType)
            {
                if (!KUI_CELL_INFO_MAP.ContainsKey(cellType))
                {
                    
                    // KUI_CELL_INFO_MAP.Add(cellType, cellType.GetCustomAttribute<KUI_Cell_Info>());
                    KUI_CELL_INFO_MAP.Add(cellType,
                        new KUI_Cell_Info(cellType.GetCustomAttribute<KUI_Cell_Info>().prefabPath,
                            cellType.GetCustomAttribute<KUI_Cell_Info>().cellName));
                }
            }
            
        }
    }

}