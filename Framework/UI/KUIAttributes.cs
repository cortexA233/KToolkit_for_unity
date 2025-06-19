using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KToolkit
{

    /// <summary>
    /// 对于任意KUIBase，使用这个特性，即可自动进行KUIManager的UI类型注册
    /// </summary>
    public class KUI_Info : Attribute
    {
        public string prefabPath;
        public string name;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path">预制体相对于Resources/UIPrefabs的目录</param>
        /// <param name="uiName">UI的名字，一般是类名</param>
        public KUI_Info(string path, string uiName)
        {
            if (!path.StartsWith("UI_prefabs/"))
            {
                prefabPath = "UI_prefabs/" + path;
            }
            else
            {
                prefabPath = path;
            }

            name = uiName;
        }
    }

    /// <summary>
    /// 对于任意KUIBase，使用这个特性，即可自动进行KUIManager的UI_Cell类型注册
    /// 和前者的区别在于自动注册的映射MAP不一样，把Cell单独分出来处理了
    /// </summary>
    public class KUI_Cell_Info : Attribute
    {
        public string prefabPath;
        public string cellName;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="path">预制体相对于Resources/UIPrefabs的目录</param>
        /// <param name="uiCellName">UI cell的名字，一般是类名</param>
        public KUI_Cell_Info(string path, string uiCellName)
        {
            if (!path.StartsWith("UI_prefabs/"))
            {
                prefabPath = "UI_prefabs/" + path;
            }
            else
            {
                prefabPath = path;
            }

            cellName = uiCellName;
        }
    }
}
