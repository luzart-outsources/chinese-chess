using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScreenMainMenu : MonoBehaviour
{
    public abstract EMainMenu eMainMenu
    {
        get;
    }
    public virtual void Setup()
    {

    }
    public virtual void Show()
    {

    }
    public virtual void RefreshUI()
    {

    }
    public virtual void Hide()
    {
        actionHide?.Invoke();
    }
    public Action actionHide;
}
public enum EMainMenu
{
    Home = 0,
    Leaderboard = 1,
    Room =2,
}