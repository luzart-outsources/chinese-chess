using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ButtonEventRacing : ButtonEvent
{
    public TMP_Text txtTime;
    private RacingManager racingManager
    {
        get
        {
            return EventManager.Instance.racingManager;
        }
    }
    public GameObject obEventEnd;
    protected override void Start()
    {
        base.Start();
        if (IsActiveEvent)
        {
            Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
    }
    private void OnTimePerSecond(object data)
    {
        long timeContain = 0;
        long timeCurrent = TimeUtils.GetLongTimeCurrent;
        if (!racingManager.IsEventProgress)
        {
            timeContain = racingManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
        }
        else
        {
            timeContain = racingManager.dataEvent.timeEndEvent - timeCurrent;
        }
        txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
    }
    private void OnClickRacing()
    {
        if (racingManager.dataEvent.timeStartEvent == -1)
        {
            UIManager.Instance.ShowUI(UIName.RacingNoti);
        }
        else
        {
            UIManager.Instance.ShowUI(UIName.Racing);
        }
    }
    public override void InitEvent(Action action)
    {
        base.InitEvent(OnClickRacing);
    }
    protected override void InitButton()
    {
        base.InitButton();
        if(racingManager.dataEvent.isEventComplete && racingManager.dataEvent.isRewardClaimed)
        {
            gameObject.SetActive(false);
        }
        CheckNoti();

    }
    public void CheckNoti()
    {
        obEventEnd?.SetActive(racingManager.dataEvent.isEventComplete);
    }
}
