using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luzart;
using System;
using DG.Tweening;

public class SequenceActionWinstreak : SequenceActionEvent
{
    public ButtonEventRiseOfKittens buttonEventRiseOfKittens;
    public ObjectCurveMover objectCurveMover;
    private RiseOfKittensManager riseOfKittensManager
    {
        get
        {
            return EventManager.Instance.riseOfKittensManager;
        }
    }
    private Action onDone;
    public float timeDelay = 0.3f;
    public override void Init(Action callback)
    {
        onDone = callback;
        bool isCheck = IsCheckEvent();
        if (isCheck)
        {
            GameUtil.StepToStep(new Action<Action>[]
            {
                OnTutorial,
                ActionFly,
            });
        }
        else
        {
            callback?.Invoke();
        }

    }
    private void OnTutorial(Action onDone)
    {
        bool isHasEvent = EventManager.Instance.IsHasEvent(EEventName.RiseOfKittens);
        bool isUnlockEvent = EventManager.Instance.IsUnlockLevel(EEventName.RiseOfKittens, DataWrapperGame.CurrentLevel + 1);
        bool isTutorial = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.RiseOfKittens);
        if (isHasEvent&& isUnlockEvent && !isTutorial)
        {
            UIManager.Instance.ShowUI(UIName.RiseOfKittens, () =>
            {
                EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.RiseOfKittens, true);
                onDone?.Invoke();
            });
        }
        else
        {
            onDone?.Invoke();
        }
    }
    private void ActionFly(Action onDone)
    {
        levelBreak = riseOfKittensManager.dataEvent.totalUseCacheHome;
        int levelCurrent = riseOfKittensManager.dataEvent.totalUse;
        int length = levelCurrent - levelBreak;
        if(length > 0)
        {
            objectCurveMover.StartMove(length, OnMoveComplete, () => GameUtil.Instance.WaitAndDo(timeDelay, OnDoneCallBack));
        }
        else
        {
            OnDoneCallBack();
        }
        onDone?.Invoke();
    }
    private void OnDoneCallBack()
    {
        riseOfKittensManager.dataEvent.totalUseCacheHome = riseOfKittensManager.dataEvent.totalUse;
        riseOfKittensManager.SaveData();
        buttonEventRiseOfKittens.SetVisual(riseOfKittensManager.dataEvent.totalUse);
        onDone?.Invoke();
    }
    private int levelBreak;
    private void OnMoveComplete()
    {
        levelBreak++;
        buttonEventRiseOfKittens.SetVisual(levelBreak);
    }

    public override void PreInit()
    {
        if (IsCheckEvent())
        {
            int levelBreak = riseOfKittensManager.dataEvent.totalUseCacheHome;
            buttonEventRiseOfKittens.SetVisual(levelBreak);
        }
    }

    private bool IsCheckEvent(int level = -1)
    {
        if (level == -1)
        {
            level = DataWrapperGame.CurrentLevel;
        }
        return EventManager.Instance.IsHasEvent(EEventName.RiseOfKittens) && EventManager.Instance.IsUnlockLevel(EEventName.RiseOfKittens, level);
    }
}
