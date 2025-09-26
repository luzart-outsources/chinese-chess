using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegisterPopup : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField phoneInputField;
    public TMP_InputField passwordInputField;

    void Start()
    {
        // Thiết lập password field
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();
    }

    public void OnClickeRegister()
    {
        UIManager.Instance.ShowUI(UIName.NotiFake);
        return;
        string username = usernameInputField.text;
        string phone = phoneInputField.text;
        string password = passwordInputField.text;

        Debug.Log($"Register: {username}, {phone}, {password}");

        GlobalServices.Instance.Register(username, phone, password);
    }
}
