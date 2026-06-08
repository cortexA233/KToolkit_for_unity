using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif


namespace KToolkit
{
    public class KFrameworkManager : KSingleton<KFrameworkManager>
    {
        private const string PoolTransformParentObjectName = "pool_transform_parent";

        private GameObject frameworkManagerObject;
    
        protected override void Awake()
        {
            base.Awake();
        }

        public virtual void InitKFramework()
        {
            KDebugLogger.Cortex_DebugLog("InitKFramework");
            KUIManager.instance.Init();
            CreateAndKeepPoolTransform();
        }

        void CreateAndKeepPoolTransform()
        {
            var poolTransformParent = GameObject.Find(PoolTransformParentObjectName);
            if (poolTransformParent == null)
            {
                poolTransformParent = new GameObject(PoolTransformParentObjectName);
            }

            DontDestroyOnLoad(poolTransformParent);
        }
        
        private void Update()
        {
            KUIManager.instance.Update();
            KTimerManager.instance.Update();
            KTickManager.instance.Update();
        }
    }

}
