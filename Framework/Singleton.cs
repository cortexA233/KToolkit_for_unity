using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
        
    public class KSingleton<T> : KObserver where T: KSingleton<T>, new()
    {
        private static T _instance;

        // public static T instance;
        public static T instance
        {
            get
            {
                // 确保只创建一个实例
                if (_instance == null)
                {
                    // 查找现有的 GameManager 实例
                    _instance = FindObjectOfType<T>();

                    // 如果没有找到，则创建一个新的
                    if (_instance == null)
                    {
                        string objectName = typeof(T).Name;
                        GameObject instanceObject = new GameObject(objectName);
                        _instance = instanceObject.AddComponent<T>();
                        DontDestroyOnLoad(instanceObject); // 确保跨场景不会销毁
                    }
                }

                return _instance;
            }
        }
            
        protected virtual void Awake()
        {
            _instance = (T)this;
        }
    }


    public class KSingletonNoMono<T> : KObserverNoMono where T : KSingletonNoMono<T>, new()
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                return _instance;
            }
        }

        public virtual void Init()
        {
            
        }
    }

}

