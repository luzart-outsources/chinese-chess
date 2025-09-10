using Assets._GameAsset.Script.Session;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : UIBase
{
    public Gameplay_AvatarFrame frameMe;
    public Gameplay_AvatarFrame frameEnemy;
    public TMP_Text txtGold;
    public TMP_Text txtIDRoom;
    public BaseSelect selectStartMaster;

    public CountDownObject obReady;
    private DataRoom dataRoom;


    public override void Show(System.Action onHideDone)
    {
        base.Show(onHideDone);
        obReady.gameObject.SetActive(false);
    }
    public void InitData(DataRoom dataRoom)
    {
        this.dataRoom = dataRoom;
        txtIDRoom.text = "ID: " + dataRoom.idRoom;
        txtGold.text = dataRoom.goldRate.ToString();
        frameMe.SetData(dataRoom.dataMe);
        frameEnemy.SetData(dataRoom.dataMember2);
    }
    public void StartReady()
    {
        obReady.gameObject.SetActive(true);
        selectStartMaster.Select(dataRoom.isMaster);
        StartCountDown();
    }
    public void OnClickReady()
    {
        GlobalServices.Instance.RequestReady(true);
        obReady.gameObject.SetActive(false);
    }
    public void OnFailedReady()
    {
        GlobalServices.Instance.RequestReady(false);
    }

    public void OnClickLeaveRoom()
    {
        UIManager.Instance.ShowUI(UIName.LeaveRoom);
    }
    public void StartCountDown()
    {
        obReady.StartCountDown(10, 0, 10f, null, () =>
        {
            obReady.gameObject.SetActive(false);
            // Countdown complete
        });
    }

}
