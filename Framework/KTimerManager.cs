using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// 将其放到GameManger的Update中每帧更新
namespace KToolkit
{
    public class KTimerManager : KSingletonNoMono<KTimerManager>
    {
        // private double currentTime = 0f;

        private List<DelayFuncInfo> delayTimerList = new List<DelayFuncInfo>();

        public void Update()
        {
            for (int i = delayTimerList.Count - 1; i >= 0; i--)
            {
                delayTimerList[i].delayTime -= Time.deltaTime;
                if (delayTimerList[i].delayTime <= 0)
                {
                    delayTimerList[i].func(delayTimerList[i].args);
                    delayTimerList.RemoveAt(i);
                }
            }
        }

        public void AddDelayTimerFunc(float delayTime, UnityAction<object[]> func, params object[] args)
        {
            DelayFuncInfo newInfo = new DelayFuncInfo();
            newInfo.delayTime = delayTime;
            newInfo.func = func;
            newInfo.args = args;
            delayTimerList.Add(newInfo);
        }
    }

    class DelayFuncInfo
    {
        public float delayTime;
        public UnityAction<object[]> func;
        public object[] args;
    }
}
