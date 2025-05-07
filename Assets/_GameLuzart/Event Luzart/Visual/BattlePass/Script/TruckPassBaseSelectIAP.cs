using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckPassBaseSelectIAP : MonoBehaviour
{
    public BaseSelect bs;
    private void OnEnable()
    {
        UIManager.AddActionRefreshUI(OnRefreshUI);
        OnRefreshUI();
    }
    private void OnDisable()
    {
        UIManager.RemoveActionRefreshUI(OnRefreshUI);
    }
    private void OnRefreshUI()
    {
        bool isIAP = EventManager.Instance.battlePassManager.dataEvent.isBuyIAP;
        bs.Select(isIAP);
    }
}
