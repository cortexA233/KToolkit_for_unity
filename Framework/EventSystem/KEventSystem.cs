using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace KToolkit
{
    public static class KEventManager
    {
        private static Dictionary<KEventName, List<KObserver>> observers = new Dictionary<KEventName, List<KObserver>>();
        private static Dictionary<KEventName, List<KObserverNoMono>> observersNoMono = new Dictionary<KEventName, List<KObserverNoMono>>();

        public static int DebugGetKObserverCount()
        {
            return observers.Count + observersNoMono.Count;
        }
        
        public static void DeleteKObserver(KObserverNoMono observer)
        {
            foreach (var item in observersNoMono)
            {
                item.Value.Remove(observer);
            }
        }

        public static void AddListener(KObserver observer, KEventName eventName)
        {
            if (!observers.ContainsKey(eventName))
            {
                observers[eventName] = new List<KObserver>();
            }
            if (!observers[eventName].Contains(observer))
            {
                observers[eventName].Add(observer);
            }
        }
        public static void AddListener(KObserverNoMono observer, KEventName eventName)
        {
            if (!observersNoMono.ContainsKey(eventName))
            {
                observersNoMono[eventName] = new List<KObserverNoMono>();
            }
            if (!observersNoMono[eventName].Contains(observer))
            {
                observersNoMono[eventName].Add(observer);
            }
        }
        
        public static void SendNotification(KEventName eventName, params object[] args)
        {
            if (!observers.ContainsKey(eventName))
            {
                observers[eventName] = new List<KObserver>();
            }
            if (!observersNoMono.ContainsKey(eventName))
            {
                observersNoMono[eventName] = new List<KObserverNoMono>();
            }

            for (int i = observers[eventName].Count - 1; i >= 0; --i)
            {
                if (observers[eventName][i] is null || observers[eventName][i].IsDestroyed())
                {
                    observers[eventName].Remove(observers[eventName][i]);
                    continue;
                }
                observers[eventName][i].__CallEventMap(eventName, args);
            }

            for (int i = observersNoMono[eventName].Count - 1; i >= 0; --i)
            {
                if (observersNoMono[eventName][i] == null || observersNoMono[eventName][i].isDestroyed)
                {
                    observersNoMono[eventName].Remove(observersNoMono[eventName][i]);
                    continue;
                }
                observersNoMono[eventName][i].__CallEventMap(eventName, args);
            }
        }
    }

}
