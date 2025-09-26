using Assets._GameAsset.Script.Session;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIChatGlobal : UIBase
{
    public TMP_InputField txtInputField;

    public TMP_Text txtContent;

    public Transform parentSpawn;
    public string strMe;

    public void OnReceiveString(string str)
    {
        Instantiate(txtContent, parentSpawn).text = str;
    }
    public void OnPostMesseage()
    {
        string strChat = $"Tôi : {strMe}";
        GlobalServices.Instance.RequestChatWorld(strMe);
        OnReceiveString(strChat);
    }
    public void OnValidateInputField(string str)
    {
        this.strMe = str;
    }
}
