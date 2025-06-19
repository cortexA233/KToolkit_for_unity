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

        private List<Tuple<float, UnityAction<object[]>, object[]>> delayTimerList =
            new List<Tuple<float, UnityAction<object[]>, object[]>>();

        public void Update()
        {
            for (int i = delayTimerList.Count - 1; i >= 0; i--)
            {
                if (delayTimerList[i].Item1 <= 0)
                {
                    delayTimerList[i].Item2(delayTimerList[i].Item3);
                    delayTimerList.RemoveAt(i);
                }
            }
        }

        public void AddDelayTimerFunc(float delayTime, UnityAction<object[]> func, params object[] args)
        {
            delayTimerList.Add(new Tuple<float, UnityAction<object[]>, object[]>(delayTime, func, args));
        }
    }
}
