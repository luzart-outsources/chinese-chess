using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public Action ActionOnLoadDoneLevel = null;
    public Action<bool> ActionOnEndGame = null;
    void Start()
    {
        var pf = Resources.Load<UIBase>("UINotiAButton");
        if (pf == null)
        {
            Debug.LogError("❌ Prefab null!");
        }
        else
        {
            Debug.Log("✅ Prefab loaded");
        }
    }
}
