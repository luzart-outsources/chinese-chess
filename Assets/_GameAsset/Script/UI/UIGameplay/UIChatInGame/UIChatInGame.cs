using Assets._GameAsset.Script.Session;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIChatInGame : UIBase
{
    public TMP_InputField txtInputField;
    public List<ChatInGameClick> listChatClick;
    private string strMe;
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        foreach (var item in listChatClick)
        {
            item.Init(OnClickChat);
        }
    }
    public void OnClickChat(string content)
    {
        GlobalServices.Instance.RequestChatInGame(0, content);
        CallChatInGame(content);
    }
    public void OnChangeValueTMPInputField(string value)
    {
        strMe = value;

        Debug.Log("OnChangeValueTMPInputField: " + value);
    }
    public void OnClickSend()
    {
        GlobalServices.Instance.RequestChatInGame(0, strMe);
        CallChatInGame(strMe);
    }
    private void CallChatInGame(string str)
    {
        var ui = UIManager.Instance.GetUiActive<UIGameplay>(UIName.Gameplay);
        ui.bubbleMe.ShowBubble(str);
    }
}
