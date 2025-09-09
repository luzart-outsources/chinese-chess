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

    }
    public void OpenRoom(DataRoom dataRoom)
    {
        UIManager.Instance.HideAll();
    }

}
