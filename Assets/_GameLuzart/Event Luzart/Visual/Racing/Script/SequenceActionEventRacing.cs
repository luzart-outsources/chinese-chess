using Luzart;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceActionEventRacing : SequenceActionEvent
{
    private EEventName eEvent = EEventName.Racing;
    private Action onDoneCallback = null;
    private RacingManager racingManager
    {
        get
        {
            return EventManager.Instance.racingManager;
        }
    }

    public ObjectCurveMover objMove;

    public override void Init(Action callback)
    {
        onDoneCallback = callback;
        if (IsCheckEvent())
        {
            GameUtil.StepToStep(new Action<Action>[]
            {
                OnCheckUINoti,
                OnCheckToFly,
                OnDoneCall,
            });
        }
        else
        {
            callback?.Invoke();
        }
    }
    private void OnCheckUINoti(Action onDone)
    {
        if(racingManager.dataEvent.timeStartEvent == -1)
        {
            var uiNoti = UIManager.Instance.ShowUI<UIRacingNoti>(UIName.RacingNoti);
            uiNoti.InitializeTutorial(() =>
            {
                UIManager.Instance.ShowUI(UIName.Racing, onDone);
            },
            () =>
            {
                onDone?.Invoke();
            });
        }
        else
        {
            onDone?.Invoke();
        }
    }
    private void OnCheckToFly(Action onDone)
    {
        int length = racingManager.dataEvent.cacheLevel;
        if (length == 0)
        {
            onDone?.Invoke();
        }
        else
        {
            objMove.StartMove(length, OnMoveDone, OnMoveComplete);
        }


        void OnMoveComplete()
        {
            onDone?.Invoke();
        }
    }

    private void OnMoveDone()
    {

    }

    private void OnDoneCall(Action onDone)
    {
        racingManager.dataEvent.cacheLevel = 0;
        racingManager.SaveData();
        onDoneCallback?.Invoke();
    }
    public override void PreInit()
    {

    }
    private void OnCheckRacing()
    {

    }

    private bool IsCheckEvent(int level = -1)
    {
        if (level == -1)
        {
            level = DataWrapperGame.CurrentLevel;
        }
        return EventManager.Instance.IsHasEvent(eEvent) && EventManager.Instance.IsUnlockLevel(eEvent, level);
    }

}
