namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SequenceActionJourneyToSuccess : SequenceActionEvent
    {
        public ButtonEventJourneyToSuccess btnEvent;
        public override void Init(Action callback)
        {
            OnShowTutorialJourneyToSuccess(callback);
        }

        public override void PreInit()
        {

        }

        private void OnShowTutorialJourneyToSuccess(Action onDone)
        {
            bool isHasEvent = EventManager.Instance.IsHasEvent(EEventName.JourneyToSuccess);
            bool isUnlockEvent = EventManager.Instance.IsUnlockLevel(EEventName.JourneyToSuccess, DataWrapperGame.CurrentLevel);
            bool isTutorial = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.JourneyToSuccess);
            if (isHasEvent && isUnlockEvent && !isTutorial)
            {
                UIManager.Instance.HideUiActive(UIName.JourneyToSuccess);
                var ui = UIManager.Instance.ShowUI<UITutorialStepJourneyToSuccess>(UIName.TutorialStepJourneyToSuccess);
                ui.btnEvent = btnEvent;
                ui.InitTutorial(onDone);
            }
            else
            {
                onDone?.Invoke();
            }
        }
    }
}

