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
        string username = usernameInputField.text;
        string phone = phoneInputField.text;
        string password = passwordInputField.text;

        Debug.Log($"Register: {username}, {phone}, {password}");

        // TODO: Thực hiện xử lý đăng ký
    }
}
