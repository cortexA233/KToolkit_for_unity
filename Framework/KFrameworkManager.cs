using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace KToolkit
{
    public class KFrameworkManager : KSingleton<KFrameworkManager>
    {
        private GameObject frameworkManagerObject;
    
        protected override void Awake()
        {
            base.Awake();
        }

        public virtual void InitKFramework()
        {
            // KTickManager.instance
            var canvasObject = GetOrCreateKCanvas();
            var eventSystemObject = GetOrCreateEventSystem();

            DontDestroyOnLoad(canvasObject);
            DontDestroyOnLoad(eventSystemObject);
            // DontDestroyOnLoad(GameObject.Find("pool_transform_parent"));
            KUIManager.instance.Init();
        }

        private static GameObject GetOrCreateKCanvas()
        {
            var canvasObject = GameObject.Find("KCanvas");
            if (canvasObject != null)
            {
                return canvasObject;
            }

            canvasObject = new GameObject("KCanvas");
            canvasObject.AddComponent<Canvas>();
            var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

            return canvasObject;
        }

        private static GameObject GetOrCreateEventSystem()
        {
            var eventSystemObject = GameObject.Find("EventSystem");
            if (eventSystemObject != null)
            {
                return eventSystemObject;
            }

            eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
            return eventSystemObject;
        }

        private void Update()
        {
            KUIManager.instance.Update();
            KTimerManager.instance.Update();
            KTickManager.instance.Update();
        }
    }

}
