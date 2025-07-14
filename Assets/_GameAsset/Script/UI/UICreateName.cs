using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICreateName : UIBase
{
    private string strTextValid = "Tên người dùng không hợp lệ";
    public TMP_InputField inputField;
    public TMP_Text txtInValid;
    private void Awake()
    {
        Observer.Instance.AddObserver(ObserverKey.OnReceiveCreateName, OnReceiveCreateName);
    }
    private void OnDestroy()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnReceiveCreateName, OnReceiveCreateName);
    }
    public void OnClickOk()
    {
        string nameStr = inputField.text;
        if (string.IsNullOrEmpty(nameStr))
        {
            txtInValid.text = strTextValid;
            return;
        }
        GlobalServices.Instance.PostNameUser(nameStr);
    }
    private void OnReceiveCreateName(object data)
    {
        if(data == null)
        {
            return;
        }
        DataCreateName dataCreateName = (DataCreateName)data;
        if(dataCreateName.indexStatus == 1)
        {
            Hide();
            return;
        }
        txtInValid.text = dataCreateName.str;
    }
}
[System.Serializable]
public struct DataCreateName
{
    public int indexStatus;
    public string str;
}

