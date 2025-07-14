using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class UISplash : UIBase
{
    public ProgressBarUI progressBarUI;
    private bool isHasDataGameLocal = false;
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        progressBarUI.SetSlider(0, 1, 3f, InitLoading);
        isHasDataGameLocal = DataManager.Instance.IsHasDataGame();
    }
    private void InitLoading()
    {
        if (isHasDataGameLocal)
        {

        }
        else
        {
            UIManager.Instance.ShowUI(UIName.Login);
            Hide();
        }
    }
    private void InitStartGame()
    {
        UIManager.Instance.ShowUI(UIName.MainMenu);
        Hide();
    }
}
