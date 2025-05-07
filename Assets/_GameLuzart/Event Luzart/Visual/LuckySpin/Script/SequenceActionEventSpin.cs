using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luzart;
using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class SequenceActionEventSpin : SequenceActionEvent
{
    public ObjectCurveMover objectCurveMover;
    public ButtonEventLuckySpin btnEventLuckySpin;
    public Transform transformBtnPlay;
    [Header("LuckySpin")]
    public RectTransform parentSpawn;
    public RectTransform rtLuckySpinPf;
    private List<RectTransform> listRectTransforms = new List<RectTransform>();
    private LuckySpinManager luckySpinManager => EventManager.Instance.luckySpinManager;

    public float delay;
    private bool isDelay = false;
    
    public override void Init(Action callback)
    {
        GameUtil.StepToStep(new Action<Action>[]
        {
            SequenceJumpToIcon,
            CallOnDone
        });
        async void CallOnDone(Action onDone)
        {
            if(isDelay) await UniTask.WaitForSeconds(delay);
            callback?.Invoke();
        }
    }
    private int levelBreak;
    private int levelAnim = 0;
    private void SequenceJumpToIcon(Action onDone)
    {
        if (IsCheckEvent())
        {
            levelBreak = luckySpinManager.dataEvent.levelBreakVisual;
            luckySpinManager.dataEvent.levelBreakVisual = DataWrapperGame.CurrentLevel;
            luckySpinManager.SaveData();
            levelAnim = levelBreak;

            int levelCur = DataWrapperGame.CurrentLevel;
            int cache = levelCur - levelBreak;
            if (cache == 0)
            {
                onDone?.Invoke();
                return;
            }
            isDelay = true;
            objectCurveMover.StartMove(cache, OnMoveComplete, onDone);
        }
        else
        {
            onDone?.Invoke();
        }
       
    }

    private void OnMoveComplete()
    {
        btnEventLuckySpin.PreClaimIfHas(levelAnim, levelAnim + 1, 0.1f);
        levelAnim++;
    }

    public override void PreInit()
    {
        if(IsCheckEvent())
        {
            int levelBreak = luckySpinManager.dataEvent.levelBreakVisual;
            int levelCur = DataWrapperGame.CurrentLevel + 1;
            int cache = levelCur - levelBreak;
            if (cache == 0)
            {
                return;
            }
            btnEventLuckySpin.SetVisualInAnim(levelBreak);
        }

    }
    private bool IsCheckEvent(int level = -1)
    {
        if(level == -1)
        {
            level = DataWrapperGame.CurrentLevel + 1 ;
        }
        return EventManager.Instance.IsHasEvent(EEventName.LuckySpin) && EventManager.Instance.IsUnlockLevel(EEventName.LuckySpin, DataWrapperGame.CurrentLevel - 1);
    }
}
