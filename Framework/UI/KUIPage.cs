using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KToolkit
{
    // todo KUIPage施工中
    public abstract class KUIPage : KUIBase
    {
    
        public void Activate()
        {
            gameObject.SetActive(true);
        }
    
        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
        
    }
}
