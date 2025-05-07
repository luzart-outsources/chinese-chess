using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRacingNoti : UIBase
{
    private RacingManager racingManager
    {
        get
        {
            return EventManager.Instance.racingManager;
        }
    }
    private Action onDoneStartEvent;
    private Action onDoneClickClose;
    private bool isTutorial = false;
    public void InitializeTutorial(Action actionStartEvent, Action actionClickClose)
    {
        this.onDoneClickClose = actionClickClose;
        this.onDoneStartEvent = actionStartEvent;
        isTutorial = true;
    }
    public void OnClickStartEvent()
    {
        racingManager.StartEvent();
        racingManager.StartInitBot();
        Hide();
        if (isTutorial)
        {
            onDoneStartEvent?.Invoke();
        }
        else
        {
            UIManager.Instance.ShowUI(UIName.Racing);
        }
    }
    public override void OnClickClose()
    {
        base.OnClickClose();
        if (isTutorial)
        {
            onDoneClickClose?.Invoke();
        }
    }
}
