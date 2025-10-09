using Assets._GameAsset.Script.Session;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TestChatP2P : MonoBehaviour
{
    public string userName;
    public string content;

    public void OnChangeUsername(string value)
    {
               userName = value;
    }
    public void OnChangeContent(string value)
    {
               content = value;
    }


    [Button("Send")]
    public void OnClickSend()
    {
        GlobalServices.Instance.RequestChatP2P(userName, content);
    }
}
