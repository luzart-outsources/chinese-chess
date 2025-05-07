namespace Luzart
{
    using System;
    using UnityEngine;

    public class SequenceActionEventBattlePass : SequenceActionEvent
    {
        public ButtonEventBattlePass buttonEventBattlePass;

        public ObjectCurveMover objectCurveMover;
        private BattlePassManager battlePassManager
        {
            get
            {
                return EventManager.Instance.battlePassManager;
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
            bool isHasEvent = EventManager.Instance.IsHasEvent(EEventName.BattlePass);
            bool isUnlockEvent = EventManager.Instance.IsUnlockLevel(EEventName.BattlePass, DataWrapperGame.CurrentLevel + 1);
            bool isTutorial = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.BattlePass);
            if (isHasEvent && isUnlockEvent && !isTutorial)
            {
                UIManager.Instance.ShowUI(UIName.BattlePass, onDone);
                EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.BattlePass, true);
            }
            else
            {
                onDone?.Invoke();
            }
        }
        private void ActionFly(Action onDone)
        {
            int totalKey = battlePassManager.dataEvent.totalUse;
            int length = battlePassManager.dataEvent.cacheKeyHome;
            preTotalUse = battlePassManager.dataEvent.totalUse - length;
            int levelCurrent = battlePassManager.dataEvent.totalUse;
            if (length > 0)
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
            battlePassManager.dataEvent.cacheKeyHome = 0;
            battlePassManager.SaveData();
            buttonEventBattlePass.SetVisual(battlePassManager.dataEvent.totalUse);
            onDone?.Invoke();
        }
        private int preTotalUse;
        private void OnMoveComplete()
        {
            int _preTotalUse = preTotalUse;
            int _totalUse = preTotalUse + 1;
            preTotalUse++;
            buttonEventBattlePass.SetSlider(_preTotalUse, _totalUse, 0.2f);
        }

        public override void PreInit()
        {
            if (IsCheckEvent())
            {
                int totalKey = battlePassManager.dataEvent.totalUse;
                int levelBreak = battlePassManager.dataEvent.totalUse - battlePassManager.dataEvent.cacheKeyHome;
                buttonEventBattlePass.SetVisual(levelBreak);
            }
        }

        private bool IsCheckEvent(int level = -1)
        {
            if (level == -1)
            {
                level = DataWrapperGame.CurrentLevel;
            }
            return EventManager.Instance.IsHasEvent(EEventName.BattlePass) && EventManager.Instance.IsUnlockLevel(EEventName.BattlePass, level);
        }
    }

}