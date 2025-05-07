namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public abstract class BaseEventManager : MonoBehaviour
    {
        public abstract EEventName eEvent { get; }
        public abstract TimeEvent GetTimeEvent { get;}
    
        public abstract void LoadData();
    #if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
    #endif
        public abstract void SaveData();
    
        public virtual void OnStartGame(int level) { }
    
        public virtual void OnCompleteLevelData(int level) { }
        public virtual void OnCompleteLevelVisual(int level) { }
    
        public virtual void OnLoseLevelData(int level) { }
        public virtual void OnLoseLevelVisual(int level) { }

        public virtual void OnCompleteLevelToUnlock(int level) { }
    
    }
}
