using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : UIBase
{
    public Button btnClickLogin;
    public Button btnClickRegister;
    public LoginPopUp loginPopUp;
    public RegisterPopup registerPopup;

    private void Start()
    {
        GameUtil.ButtonOnClick(btnClickLogin, ClickLogin);
        GameUtil.ButtonOnClick(btnClickRegister, ClickRegister);

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
    public void ConnectToSession()
    {
        SessionMe.Instance.Connect();
    }
}
