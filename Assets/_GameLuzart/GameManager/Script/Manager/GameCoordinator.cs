using System;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    public Action ActionOnLoadDoneLevel = null;
    public Action<bool> ActionOnEndGame = null;
    public bool IsInRoom = false;
    void Start()
    {

    }
    public void OpenRoom(DataRoom dataRoom)
    {
        if (IsInRoom)
        {
            return;
        }
        IsInRoom = false;
    }

}
