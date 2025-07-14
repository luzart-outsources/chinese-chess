using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UINotiAButton : UIBase
{
    public TMP_Text txtTitle, txtDescription, txtButton;
    public Action actionClick;
    public void InitPopup(Action onDone = null, string strTitle = "Thông báo", string strDescription ="Chạm Ok để bắt đầu", string strButton = "Ok")
    {
        this.actionClick = onDone;
        txtTitle.text = strTitle;
        txtDescription.text = strDescription;
        txtButton.text = strButton;
    }
    //public override void OnClickClose()
    //{
    //    base.OnClickClose();
    //    actionClick?.Invoke();
    //}
}
