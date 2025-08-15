using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldObserver : MonoBehaviour
{
    public TMP_Text txtGold;
    private void Start()
    {
        Observer.Instance.AddObserver(ObserverKey.RefreshDataMeByServer, RefreshDataServer);
        RefreshDataServer(null);
    }
    private void OnDestroy()
    {
        Observer.Instance.RemoveObserver(ObserverKey.RefreshDataMeByServer, RefreshDataServer);
    }
    private void RefreshDataServer(object data)
    {
        txtGold.text = DataManager.Instance.DataUser.gold.ToString();
    }
}
