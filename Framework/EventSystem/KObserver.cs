using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace KToolkit
{
    public abstract class KObserver : MonoBehaviour
    {
        private Dictionary<KEventName, UnityAction<object[]>> _eventMap = new Dictionary<KEventName, UnityAction<object[]>>();
    
        protected void AddEventListener(KEventName eventName, UnityAction<object[]> func)
        {
            KEventManager.AddListener(this, eventName);
            _eventMap[eventName] = func;
        }

        public void __CallEventMap(KEventName eventName, params object[] args)
        {
            _eventMap[eventName](args);
        }
    }


    public abstract class KObserverNoMono
    {
        private Dictionary<KEventName, UnityAction<object[]>> _eventMap = new Dictionary<KEventName, UnityAction<object[]>>();
        public bool isDestroyed { get; protected set; }
        protected void AddEventListener(KEventName eventName, UnityAction<object[]> func)
        {
            KEventManager.AddListener(this, eventName);
            _eventMap[eventName] = func;
        }

        public void __CallEventMap(KEventName eventName, params object[] args)
        {
            _eventMap[eventName](args);
        }

        public virtual void DestroySelf()
        {
            isDestroyed = true;
            KEventManager.DeleteKObserver(this);
        }
    }
}

