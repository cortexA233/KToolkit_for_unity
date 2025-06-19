using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
            
            KUIManager.instance.Init();
        }

        private void Update()
        {
            KUIManager.instance.Update();
            KTimerManager.instance.Update();
        }
    }

}
