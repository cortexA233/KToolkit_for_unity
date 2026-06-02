using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using Object = UnityEngine.Object;


namespace KToolkit
{
    public partial class KUIManager : KSingletonNoMono<KUIManager>
    {
        private List<KUIBase> uiList = new List<KUIBase>();

        private const string CanvasObjectName = "KCanvas";
        private const string EventSystemObjectName = "EventSystem";
        public List<KUIBase> DebugGetUIList()
        {
            return uiList;
        }

        public KUIManager()
        {
            KeepCanvas();
            KeepEventSystem();
            AutoInitPageDict();
            AutoInitCellDict();
        }

        #region Initialize
        void KeepCanvas()
        {
            var kCanvas = GameObject.Find(CanvasObjectName);
            if (kCanvas == null)
            {
                kCanvas = Object.Instantiate(new GameObject());
                kCanvas.name = CanvasObjectName;
                kCanvas.AddComponent<Canvas>();
                kCanvas.AddComponent<CanvasScaler>();
                kCanvas.AddComponent<GraphicRaycaster>();
                kCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                // todo 配置化
                // kCanvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                // kCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
                // kCanvas.GetComponent<Canvas>().planeDistance = Camera.main.nearClipPlane + 0.5f;
                kCanvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                kCanvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            }
            Object.DontDestroyOnLoad(kCanvas);
        }
        
        void KeepEventSystem()
        {
            var eventSystemObject = Object.FindAnyObjectByType<EventSystem>().gameObject;
            if (eventSystemObject == null)
            {
                eventSystemObject = new GameObject(EventSystemObjectName);
            }

            if (eventSystemObject.GetComponent<EventSystem>() == null)
            {
                eventSystemObject.AddComponent<EventSystem>();
            }

            if (eventSystemObject.GetComponent<BaseInputModule>() == null)
            {
#if ENABLE_INPUT_SYSTEM
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
                eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
            }
            Object.DontDestroyOnLoad(eventSystemObject);
        }
        #endregion
        
        public T CreateUI<T>(params object[] args) where T : KUIBase, new()
        {
            var newUI = new T();
            var uiInfo = UI_INFO_MAP[typeof(T)];
            var prefab = Resources.Load<GameObject>(uiInfo.prefabPath);
            if (prefab == null)
            {
                Debug.LogError("KUIManager failed to load UI prefab: " + uiInfo.prefabPath);
                return null;
            }

            if (uiInfo.renderMode == KUIRenderMode.World)
            {
                newUI.gameObject = GameObject.Instantiate(prefab);
                ValidateWorldCanvas(newUI.gameObject, uiInfo.name);
            }
            else
            {
                newUI.gameObject = GameObject.Instantiate(prefab, GetCanvas().transform);
            }

            newUI.transform = newUI.gameObject.transform;
            newUI.InitParams(args);
            uiList.Add(newUI);
            newUI.OnStart();
            // if (newUI is UIPage)
            // {
            //     if (pageStack.Count > 0)
            //     {
            //         pageStack[^1].Deactivate();
            //     }
            //     pageStack.Add((UIPage)(object)newUI);
            //     pageStack[^1].Activate();
            // }
            // KDebugLogger.UI_DebugLog("UI 创建: ", UI_INFO_MAP[typeof(T)].name);
            return newUI;
        }

        private void ValidateWorldCanvas(GameObject uiGameObject, string uiName)
        {
            var canvas = uiGameObject.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
            {
                Debug.LogError("World UI prefab is missing a Canvas: " + uiName);
                return;
            }

            if (canvas.renderMode != RenderMode.WorldSpace)
            {
                Debug.LogError("World UI prefab Canvas must use RenderMode.WorldSpace: " + uiName);
            }
            canvas.worldCamera = Camera.main;
        }

        public void DestroyUI(KUIBase ui)
        {
            uiList.Remove(ui);
            // if (ui is UIPage)
            // {
            //     pageStack.Remove((UIPage)ui);
            //     if (pageStack.Count > 0)
            //     {
            //         pageStack[^1].Activate();
            //     }
            // }
            // KDebugLogger.UI_DebugLog("UI 销毁: ", ui);
            ui.OnDestroy();
            Object.Destroy(ui.gameObject);
            ui.DestroySelf();
        }

        public void HideUI(KUIBase ui)
        {
            ui.gameObject.SetActive(false);
            // if (ui is UIPage)
            // {
            //     pageStack.Remove((UIPage)ui);
            //     if (pageStack.Count > 0)
            //     {
            //         pageStack[^1].Activate();
            //     }
            // }
            // KDebugLogger.UI_DebugLog("UI 隐藏: ", ui);
        }

        public void HideAllUIWithType<T>() where T : KUIBase
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (uiList[i].GetType() == typeof(T))
                {
                    HideUI(uiList[i]);
                }
            }
        }

        public void ShowUI(KUIBase ui)
        {
            ui.gameObject.SetActive(true);
            // if (ui is UIPage)
            // {
            //     pageStack.Remove((UIPage)ui);
            //     if (pageStack.Count > 0)
            //     {
            //         pageStack[^1].Activate();
            //     }
            // }
            // KDebugLogger.UI_DebugLog("UI 重新显示: ", ui);
        }

        public void ShowAllUIWithType<T>() where T : KUIBase
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (uiList[i].GetType() == typeof(T))
                {
                    ShowUI(uiList[i]);
                }
            }
        }
        
        public void DestroyFirstUIWithType<T>() where T : KUIBase
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (uiList[i].GetType() == typeof(T))
                {
                    DestroyUI(uiList[i]);
                    break;
                }
            }
        }
        
        public KUIBase GetFirstUIWithType<T>() where T : KUIBase
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (uiList[i].GetType() == typeof(T))
                {
                    return uiList[i];
                }
            }
            return null;
        }

        public void DestroyAllUI()
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                DestroyUI(uiList[i]);
            }
        }

        public void DestroyAllUIWithType<T>() where T : KUIBase
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (i > uiList.Count - 1)
                {
                    continue;
                }
                if (uiList[i] is not null && uiList[i].GetType() == typeof(T))
                {
                    DestroyUI(uiList[i]);
                }
            }
        }

        public Camera GetUICamera()
        {
            return GameObject.Find("UICamera").GetComponent<Camera>();
        }

        public Canvas GetCanvas()
        {
            return GameObject.Find("KCanvas").GetComponent<Canvas>();
        }

        public void Update()
        {
            for (int i = uiList.Count - 1; i >= 0; i--)
            {
                if (uiList[i] != null && !uiList[i].isDestroyed)
                {
                    uiList[i].Update();
                }
            }
        }
    }
}
