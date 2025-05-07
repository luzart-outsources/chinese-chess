namespace Luzart
{
    using System;

    public class UITutorialStepTicketTally : UITutorial
    {
        public ButtonEventTicketTally btnTicket;
        public void InitTutorial(Action onDone)
        {
            if (btnTicket == null)
            {
                btnTicket = FindAnyObjectByType<ButtonEventTicketTally>();
            }

            if(btnTicket == null)
            {
                return;
            }
            ShowScreenTutorial(0, btnTicket.gameObject, OnClickTicketTally);

            void OnClickTicketTally()
            {
                Hide();
                EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.TicketTally,true);
                EventManager.Instance.SaveAllData();
            }
        }
    }

}