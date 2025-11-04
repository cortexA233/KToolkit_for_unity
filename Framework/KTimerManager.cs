using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// 将其放到GameManger的Update中每帧更新
namespace KToolkit
{
    public class KTimerManager : KSingletonNoMono<KTimerManager>
    {
        private readonly List<DelayFuncInfo> delayTimerList = new List<DelayFuncInfo>();
        private readonly Dictionary<Guid, DelayFuncInfo> timerLookup = new Dictionary<Guid, DelayFuncInfo>();
        private readonly Stack<DelayFuncInfo> infoPool = new Stack<DelayFuncInfo>();

        /// <summary>
        /// 默认使用 Unity 的 <see cref="Time.deltaTime"/> 与 <see cref="Time.unscaledDeltaTime"/> 更新定时器。
        /// </summary>
        public void Update()
        {
            InternalUpdate(null);
        }

        /// <summary>
        /// 使用外部自定义的 deltaTime 更新定时器。
        /// </summary>
        /// <param name="deltaTime">外部提供的时间增量。</param>
        public void Update(float deltaTime)
        {
            InternalUpdate(deltaTime);
        }

        private void InternalUpdate(float? customDeltaTime)
        {
            for (int i = delayTimerList.Count - 1; i >= 0; i--)
            {
                DelayFuncInfo info = delayTimerList[i];

                if (!info.IsActive)
                {
                    RecycleTimerAt(i);
                    continue;
                }

                if (info.IsPaused)
                {
                    continue;
                }

                float delta = customDeltaTime ?? (info.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

                info.RemainingTime -= delta;
                if (info.RemainingTime > 0f)
                {
                    continue;
                }

                try
                {
                    info.Func?.Invoke(info.Args);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[KTimerManager] Timer {info.Id} callback threw an exception: {ex}");
                }

                if (info.Loop)
                {
                    info.RemainingTime += Mathf.Max(info.Interval, 0f);
                }
                else
                {
                    RecycleTimerAt(i);
                }
            }
        }

        /// <summary>
        /// 添加一个延迟回调任务。
        /// </summary>
        /// <param name="delayTime">首次触发前的等待时间。</param>
        /// <param name="func">触发时执行的回调。</param>
        /// <param name="args">回调参数。</param>
        /// <returns>定时器的唯一标识（<see cref="Guid"/>），用于后续管理。</returns>
        public Guid AddDelayTimerFunc(float delayTime, UnityAction<object[]> func, params object[] args)
        {
            return AddDelayTimerFunc(delayTime, func, false, -1f, false, args);
        }

        /// <summary>
        /// 添加一个可选循环的延迟回调任务。
        /// </summary>
        /// <param name="delayTime">首次触发前的等待时间。</param>
        /// <param name="func">触发时执行的回调。</param>
        /// <param name="loop">是否循环执行。</param>
        /// <param name="repeatInterval">循环间隔，小于等于 0 时会使用 <paramref name="delayTime"/>。</param>
        /// <param name="useUnscaledTime">是否使用 <see cref="Time.unscaledDeltaTime"/> 计时。</param>
        /// <param name="args">回调参数。</param>
        /// <returns>定时器的唯一标识（<see cref="Guid"/>），用于后续管理。</returns>
        public Guid AddDelayTimerFunc(float delayTime, UnityAction<object[]> func, bool loop, float repeatInterval, bool useUnscaledTime, params object[] args)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            DelayFuncInfo info = infoPool.Count > 0 ? infoPool.Pop() : new DelayFuncInfo();
            info.Reset(Guid.NewGuid(), delayTime, loop, repeatInterval, useUnscaledTime, func, args);

            delayTimerList.Add(info);
            timerLookup[info.Id] = info;

            return info.Id;
        }

        /// <summary>
        /// 移除指定定时器。
        /// </summary>
        public bool RemoveTimer(Guid timerId)
        {
            if (!timerLookup.TryGetValue(timerId, out DelayFuncInfo info))
            {
                return false;
            }

            info.IsActive = false;
            timerLookup.Remove(timerId);
            return true;
        }

        /// <summary>
        /// 暂停指定定时器。
        /// </summary>
        public bool PauseTimer(Guid timerId)
        {
            if (!timerLookup.TryGetValue(timerId, out DelayFuncInfo info) || !info.IsActive)
            {
                return false;
            }

            info.IsPaused = true;
            return true;
        }

        /// <summary>
        /// 恢复指定定时器。
        /// </summary>
        public bool ResumeTimer(Guid timerId)
        {
            if (!timerLookup.TryGetValue(timerId, out DelayFuncInfo info) || !info.IsActive)
            {
                return false;
            }

            info.IsPaused = false;
            return true;
        }

        /// <summary>
        /// 获取指定定时器剩余时间。
        /// </summary>
        public bool TryGetRemainingTime(Guid timerId, out float remainingTime)
        {
            if (timerLookup.TryGetValue(timerId, out DelayFuncInfo info) && info.IsActive)
            {
                remainingTime = Mathf.Max(info.RemainingTime, 0f);
                return true;
            }

            remainingTime = 0f;
            return false;
        }

        /// <summary>
        /// 检查定时器是否存在且处于激活状态。
        /// </summary>
        public bool ContainsTimer(Guid timerId)
        {
            return timerLookup.TryGetValue(timerId, out DelayFuncInfo info) && info.IsActive;
        }

        /// <summary>
        /// 清除所有定时器。
        /// </summary>
        public void ClearAllTimers()
        {
            for (int i = delayTimerList.Count - 1; i >= 0; i--)
            {
                RecycleTimerAt(i);
            }

            timerLookup.Clear();
        }

        private void RecycleTimerAt(int index)
        {
            DelayFuncInfo info = delayTimerList[index];
            delayTimerList.RemoveAt(index);

            if (info.Id != Guid.Empty)
            {
                timerLookup.Remove(info.Id);
            }

            infoPool.Push(info.Reset());
        }
    }

    class DelayFuncInfo
    {
        public Guid Id { get; private set; }
        public float RemainingTime { get; set; }
        public float Interval { get; private set; }
        public bool Loop { get; private set; }
        public bool UseUnscaledTime { get; private set; }
        public bool IsActive { get; set; }
        public bool IsPaused { get; set; }
        public UnityAction<object[]> Func { get; private set; }
        public object[] Args { get; private set; }

        public DelayFuncInfo Reset(Guid id, float delayTime, bool loop, float repeatInterval, bool useUnscaledTime, UnityAction<object[]> func, object[] args)
        {
            Id = id;
            RemainingTime = delayTime;
            Loop = loop;
            Interval = repeatInterval > 0f ? repeatInterval : delayTime;
            UseUnscaledTime = useUnscaledTime;
            Func = func;
            Args = args;
            IsActive = true;
            IsPaused = false;
            return this;
        }

        public DelayFuncInfo Reset()
        {
            Id = Guid.Empty;
            RemainingTime = 0f;
            Interval = 0f;
            Loop = false;
            UseUnscaledTime = false;
            Func = null;
            Args = null;
            IsActive = false;
            IsPaused = false;
            return this;
        }
    }
}
