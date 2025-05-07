namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class UITutorialStepJourneyToSuccess : UITutorial
    {
        // Start is called before the first frame update
        public ButtonEventJourneyToSuccess btnEvent;
        public void InitTutorial(Action onDone)
        {
            if (btnEvent == null)
            {
                btnEvent = FindAnyObjectByType<ButtonEventJourneyToSuccess>();
            }
            if (btnEvent == null)
            {
                Hide();
                return;
            }
            ShowScreenTutorial(0, btnEvent.gameObject, OnClickJourneyToSuccess);

            void OnClickJourneyToSuccess()
            {
                Hide();
                UIManager.Instance.ShowUI(UIName.JourneyToSuccess, onDone);
                EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.JourneyToSuccess, true);
                EventManager.Instance.SaveAllData();
            }
        }
    }
}
