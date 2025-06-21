using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KToolkit;


namespace KToolkit
{
    public abstract class KUIBase : KObserverNoMono
    {
        // 和MonoBehavior的gameObject属性类似，在OnStart时初始化
        public GameObject gameObject;
        // 和MonoBehavior的transform属性类似，在OnStart时初始化
        public Transform transform;
    
        protected List<KUICell> childCellPool = new List<KUICell>();
        
        public virtual void InitParams(params object[] args) {}
    
        public virtual void SetVisible(bool state)
        {
            gameObject.SetActive(state);
        }
    
        protected T CreateUICell<T>(Transform parent=null, params object[] args) where T : KUICell, new()
        {
            // T newCell = KUIManager.instance.CreateUI<T>(args);
            T newCell = new T();
            
            // todo 这里应该改成把cell和base区分开的
            // var newCell = new T();
            Transform transformParent = parent ?? transform;
            newCell.gameObject =
                GameObject.Instantiate(Resources.Load<GameObject>(KUIManager.KUI_CELL_INFO_MAP[typeof(T)].prefabPath),
                    transformParent);
            newCell.transform = newCell.gameObject.transform;
            newCell.OnCreate(args);
            
            // if (parent)
            // {
            //     newCell.transform.SetParent(parent);
            // }
            // else
            // {
            //     newCell.transform.SetParent(this.transform);
            // }
            childCellPool.Add(newCell);
            return newCell;
        }
    
        public override void DestroySelf()
        {
            if (isDestroyed)
            {
                return;
            }
    
            for (int i = 0; i < childCellPool.Count; ++i)
            {
                childCellPool[i].DestroySelf();
            }
            base.DestroySelf();
            KUIManager.instance.DestroyUI(this);
        }
        
        public virtual void OnStart() {}
        public virtual void OnDestroy() {}

        public virtual void Update()
        {
            foreach (var cell in childCellPool)
            {
                cell.Update();
            }
        }
    }

}
