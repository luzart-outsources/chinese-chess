using Assets._GameAsset.Script.Session;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : UIBase
{
    public Gameplay_AvatarFrame frameMe;
    public Gameplay_AvatarFrame frameEnemy;

    public BubbleChatInGame bubbleMe;
    public BubbleChatInGame bubbleEnemy;

    public TMP_Text txtGold;
    public TMP_Text txtIDRoom;
    public BaseSelect selectStartMaster;

    public CountDownObject obReady;
    private DataRoom dataRoom;

    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.OnReceiveChatInGame, OnReceiveChat);
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnReceiveChatInGame, OnReceiveChat);
    }

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
    public void OnClickChat()
    {
        UIManager.Instance.ShowUI(UIName.Chat);
    }
    public void OnClickChatGlobal()
    {
        UIManager.Instance.ShowUI(UIName.ChatGlobal);
    }
    public void StartCountDown()
    {
        obReady.StartCountDown(10, 0, 10f, null, () =>
        {
            obReady.gameObject.SetActive(false);
            // Countdown complete
        });
    }
    public void CountdownPlayer(bool isMe ,int count, int totalTime, int totalTimeOponent)
    {
        var itemRoll = isMe ? frameMe : frameEnemy;
        var itemReset = isMe ? frameEnemy : frameMe;
        itemRoll.StartCountCountDown(count, totalTime);
        itemReset.ResetCountDown(totalTimeOponent);
    }
    private void OnReceiveChat(object data)
    {
        if(data == null)
        {
            return;
        }
        var chatData = data as string;
        bubbleEnemy.ShowBubble(chatData);
    }

}
