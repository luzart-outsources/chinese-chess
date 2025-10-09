using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISettings : UIBase
{
    public SoundSettings soundSettings;
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        soundSettings.Show();
    }
    public void OnClickButtonLogOut()
    {
        UIManager.Instance.HideAll();
        UIManager.Instance.ShowUI<UILogin>(UIName.Login);
    }
}
