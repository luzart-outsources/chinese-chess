using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPopUp : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField passwordInputField;

    void Start()
    {
        // Thiết lập password field
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
        passwordInputField.ForceLabelUpdate();

    }

    public void OnClickLogin()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;
        GlobalServices.Instance.login(username, password);
        Debug.Log($"Login: {username}, {password}");

        // TODO: Thực hiện xử lý đăng nhập
    }

    public void OnClickForgotPassword()
    {
        Debug.Log("Forgot Password Clicked");

        // TODO: Chuyển tới màn hình quên mật khẩu
    }
}
