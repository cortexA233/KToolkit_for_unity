using System.Collections.Generic;
using UnityEngine;

namespace KToolkit
{
    public interface IKTickable
    {
        void OnTick(KTickContext context);
    }

    public struct KTickContext
    {
        public readonly float tickDeltaTime;
        public readonly long tickCount;
        public readonly float elapsedTime;

        public KTickContext(float tickDeltaTime, long tickCount, float elapsedTime)
        {
            this.tickDeltaTime = tickDeltaTime;
            this.tickCount = tickCount;
            this.elapsedTime = elapsedTime;
        }
    }

    /// <summary>
    /// Global tick system. Place exactly one in your scene.
    /// </summary>
    public class KTickManager : KSingleton<KTickManager>
    {
        [Header("Tick Settings")]
        [SerializeField] private float ticksPerSecond = 10f;
        [SerializeField] private bool useUnscaledTime = false;
        [SerializeField] private bool autoRun = true;

        public float TickInterval => tickInterval;
        public long TickCount => tickCount;
        public float ElapsedTime => elapsedTime;

        private float tickInterval;
        private float accumulator;
        private long tickCount;
        private float elapsedTime;

        private readonly List<IKTickable> listeners = new();
        private readonly List<IKTickable> pendingAdd = new();
        private readonly List<IKTickable> pendingRemove = new();

        protected override void Awake()
        {
            base.Awake(); // assigns Singleton<KTickManager>.instance
            RecalculateInterval();
        }

        private void Update()
        {
            if (!autoRun) return;

            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            ManualTick(dt);
        }

        public void SetTicksPerSecond(float newTicksPerSecond)
        {
            ticksPerSecond = Mathf.Max(0.01f, newTicksPerSecond);
            RecalculateInterval();
        }

        public void SetAutoRun(bool enabled)
        {
            autoRun = enabled;
        }

        public void ManualTick(float deltaTime)
        {
            if (tickInterval <= 0f) return;

            accumulator += deltaTime;

            while (accumulator >= tickInterval)
            {
                accumulator -= tickInterval;
                tickCount++;
                elapsedTime = tickCount * tickInterval;

                var ctx = new KTickContext(tickInterval, tickCount, elapsedTime);

                FlushPendingChanges();

                var snapshot = listeners.ToArray();
                for (int i = 0; i < snapshot.Length; i++)
                {
                    snapshot[i]?.OnTick(ctx);
                }
            }
        }

        public void Register(IKTickable tickable)
        {
            if (tickable == null) return;
            if (!pendingAdd.Contains(tickable) && !listeners.Contains(tickable))
            {
                pendingAdd.Add(tickable);
            }
        }

        public void Unregister(IKTickable tickable)
        {
            if (tickable == null) return;
            if (!pendingRemove.Contains(tickable))
            {
                pendingRemove.Add(tickable);
            }
        }

        private void FlushPendingChanges()
        {
            foreach (var t in pendingAdd)
            {
                if (t != null && !listeners.Contains(t))
                    listeners.Add(t);
            }
            pendingAdd.Clear();

            foreach (var t in pendingRemove)
            {
                if (t != null)
                    listeners.Remove(t);
            }
            pendingRemove.Clear();
        }

        private void RecalculateInterval()
        {
            tickInterval = 1f / Mathf.Max(0.01f, ticksPerSecond);
        }
    }
}
