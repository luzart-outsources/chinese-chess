using Assets._GameAsset.Script.Session;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGameplay : UIBase
{
    public Gameplay_AvatarFrame frameTop;
    public Gameplay_AvatarFrame frameBot;

    public BubbleChatInGame bubbleTop;
    public BubbleChatInGame bubbleBot;

    public TMP_Text txtGold;
    public TMP_Text txtIDRoom;
    public BaseSelect selectStartMaster;

    public CountDownObject obReady;
    public WaitingText watingTxt;
    private DataRoom dataRoom;

    private void OnEnable()
    {
        Observer.Instance.AddObserver(ObserverKey.OnReceiveChatInGame, OnReceiveChat);
    }
    private void OnDisable()
    {
        Observer.Instance.RemoveObserver(ObserverKey.OnReceiveChatInGame, OnReceiveChat);
    }
    public void StartGame()
    {
        HideWait();
        UIManager.Instance.ShowUI(UIName.StartGame);
        frameBot.SetActiveReady(false);
        frameTop.SetActiveReady(false);
    }
    public void OnShowString(string str)
    {
        HideWait();
        watingTxt.InitText(str);
    }
    public void HideWait()
    {
        obReady.gameObject.SetActive(false);
        watingTxt?.gameObject?.SetActive(false);
    }
    public override void Show(System.Action onHideDone)
    {
        base.Show(onHideDone);
        obReady.gameObject.SetActive(false);
    }
    public void InitData(DataRoom dataRoom)
    {
        this.dataRoom = dataRoom;
        txtIDRoom.text = "ID Phòng: " + dataRoom.idRoom;
        txtGold.text = dataRoom.goldRate.ToString();
        frameBot.SetData(dataRoom.dataMe);
        frameTop.SetData(dataRoom.dataMember2);
        HideWait();

    }
    public void StartReady(float time)
    {
        HideWait();
        obReady.gameObject.SetActive(true);
        selectStartMaster.Select(dataRoom.isMaster);
        StartCountDown(time);
    }
    public void OnClickReady()
    {
        GlobalServices.Instance.RequestReady(true);
        obReady.gameObject.SetActive(false);
    }
    public void AvatarReady(bool isBot, bool isReady)
    {
        var frame_avt = isBot ? frameBot : frameTop;
        frame_avt.SetActiveReady(isReady);
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
    public void StartCountDown(float time)
    {
        obReady.StartCountDown(time, 0, time, null, () =>
        {
            obReady.gameObject.SetActive(false);
            // Countdown complete
        });
    }
    public void CountdownPlayer(bool isMe ,int count, int totalTime, int totalTimeOponent)
    {
        var itemRoll = isMe ? frameTop : frameBot;
        var itemReset = isMe ? frameBot : frameTop;
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
        bubbleTop.ShowBubble(chatData);
    }

}
