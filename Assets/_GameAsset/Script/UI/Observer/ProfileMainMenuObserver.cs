using Assets._GameAsset.Script.Session;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileMainMenuObserver : MonoBehaviour
{
    public Image imAvt;
    public TMP_Text txtName;
    private void Start()
    {
        Observer.Instance.AddObserver(ObserverKey.RefreshDataMeByServer, RefreshDataServer);
        RefreshDataServer();
    }
    private void OnDestroy()
    {
        Observer.Instance.RemoveObserver(ObserverKey.RefreshDataMeByServer, RefreshDataServer);
    }
    private void RefreshDataServer(object data = null)
    {
        txtName.text = DataManager.Instance.DataUser.name;
        UpdateAvt();
    }
    private void UpdateAvt()
    {
        string avt = DataManager.Instance.DataUser.avt;
        int intAvt = 0;
        bool canTryPase = int.TryParse(avt, out intAvt);
        imAvt.sprite = ResourcesManager.Instance.avatarResourcesSO.GetSpriteAvatar(intAvt);
    }
    public void OnClickProfile()
    {
        GlobalServices.Instance.RequestGetInfoUser(DataManager.Instance.DataUser.name);
    }
}
