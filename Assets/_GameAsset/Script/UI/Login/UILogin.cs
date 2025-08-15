using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : UIBase
{
    public Button btnClickLogin;
    public Button btnClickRegister;
    public Button btnUpClickLogin;
    public Button btnUpClickRegister;
    public LoginPopUp loginPopUp;
    public RegisterPopup registerPopup;
    public BaseSelect bsLogin;

    private void Start()
    {
        GameUtil.ButtonOnClick(btnClickLogin, ClickLogin);
        GameUtil.ButtonOnClick(btnClickRegister, ClickRegister);
        GameUtil.ButtonOnClick(btnUpClickLogin, ClickUpLogin);
        GameUtil.ButtonOnClick(btnUpClickRegister, ClickUpRegister);

    }
    private void ClickLogin()
    {
        ConnectToSession();
        loginPopUp.OnClickLogin();
    }
    private void ClickRegister()
    {
        ConnectToSession();
        registerPopup.OnClickeRegister();
    }
    private void ClickUpLogin()
    {
        bsLogin.Select(true);
    }
    private void ClickUpRegister()
    {
        bsLogin.Select(false);
    }
    public void ConnectToSession()
    {
        SessionMe.Instance.Connect();
    }
}
