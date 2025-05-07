namespace Luzart
{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    
    public class ToolJsonEvent : MonoBehaviour
    {
        public string json;
        public DB_EventJsonFirebase dbEventJsonFirebase;
    #if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
    #endif
        public void ToJsonEvent()
        {
            json = JsonUtility.ToJson(dbEventJsonFirebase);
            EditorUtility.SetDirty(this);
        }
    }
    #endif
}
