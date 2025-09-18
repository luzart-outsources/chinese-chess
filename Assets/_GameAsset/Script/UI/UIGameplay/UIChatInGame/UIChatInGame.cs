using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIChatInGame : UIBase
{
    public TMP_InputField txtInputField;
    public List<ChatInGameClick> listChatClick;
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
        Debug.Log("OnClickChat: " + content);
    }
    public void OnChangeValueTMPInputField(string value)
    {
        Debug.Log("OnChangeValueTMPInputField: " + value);
    }
    public void OnClickSend()
    {
        Debug.Log("OnClickSend: ");
    }
}
