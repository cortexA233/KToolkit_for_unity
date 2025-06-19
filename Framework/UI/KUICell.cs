using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    public abstract class KUICell
    {
        // 和MonoBehavior的gameObject属性类似，在父页面CreateUICell时初始化
        public GameObject gameObject;

        // 和MonoBehavior的transform属性类似，在父页面CreateUICell时初始化
        public Transform transform;

        public void DestroySelf()
        {
            Object.Destroy(gameObject);
        }

        public abstract void OnCreate(params object[] args);
        public virtual void Update(){}

    }
}