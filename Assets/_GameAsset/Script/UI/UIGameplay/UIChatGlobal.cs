using Assets._GameAsset.Script.Session;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChatGlobal : UIBase
{

    public static List<string> listChatGlobal = new List<string>();

    public TMP_InputField txtInputField;

    public TMP_Text txtContent;

    public Transform parentSpawn;
    public string strMe;
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        foreach (var item in listChatGlobal)
        {
            OnReceiveString(item);
        }
    }
    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.OnReceiveChatServer, ReceiveString);
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnReceiveChatServer, ReceiveString);
    }
    private void ReceiveString(object data)
    {
        if(data == null) { return; }
        DataMessageWorld dataMessageWorld = data as DataMessageWorld;
        string strChat = $"{dataMessageWorld.name}: {dataMessageWorld.chat}";
        OnReceiveString(strChat);

    }
    public void OnReceiveString(string str)
    {
        Instantiate(txtContent, parentSpawn).text = str;
    }
    public void OnPostMesseage()
    {
        string strChat = $"Tôi : {strMe}";
        GlobalServices.Instance.RequestChatWorld(strMe);
        txtInputField.text = "";
    }
    public void OnValidateInputField(string str)
    {
        this.strMe = str;
    }
}
