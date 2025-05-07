namespace Luzart
{
    using DG.Tweening.Core.Easing;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIAreYouSureGiveUp : UIBase
    {
        public Button btnGiveUp;

        public List<IconEvent> listIconEvent = new List<IconEvent>();

        protected override void Setup()
        {
            isAnimBtnClose = true;
            base.Setup();
            GameUtil.ButtonOnClick(btnGiveUp, ClickGiveUp, true);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            List<EEventName> list = EventManager.Instance.GetEEventCurrent();
            if (list.Contains(EEventName.FlightEndurance))
            {
                var flightEndurance = EventManager.Instance.flightEnduranceManager;
                if (!flightEndurance.dataFlightEndurance.isShowStart || (flightEndurance.dataFlightEndurance.isWin || flightEndurance.IsLoss))
                {
                    list.Remove(EEventName.FlightEndurance);
                }
            }
            int length = list.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                EEventName eEvent = list[index];
                int amount = 0;
                switch (eEvent)
                {
                    case EEventName.BattlePass:
                        {
                            break;
                        }
                    case EEventName.TicketTally:
                        {
                            amount = EventManager.Instance.ticketTallyManager.valueKey;
                            break;
                        }
                    case EEventName.FlightEndurance:
                        {
                            amount = EventManager.Instance.flightEnduranceManager.playerMissing;
                            break;
                        }
                    case EEventName.RiseOfKittens:
                        {
                            amount = EventManager.Instance.riseOfKittensManager.dataEvent.ContainItem;
                            break;
                        }
                }

                if (amount == 0)
                {
                    continue;
                }

                int countListIcon = listIconEvent.Count;
                IconEvent iconEvent = null;
                for (int j = 0; j < countListIcon; j++)
                {
                    if (listIconEvent[j].eventName == eEvent)
                    {
                        iconEvent = listIconEvent[j];
                        break;
                    }
                }
                if (iconEvent == null)
                {
                    continue;
                }


                iconEvent.Initialize(eEvent, amount);
            }
        }
        private void ClickGiveUp()
        {
            onExcute?.Invoke();
            Hide();
        }
        public override void OnClickClose()
        {
            onHide?.Invoke();
            base.OnClickClose();
        }
        private Action onExcute;
        private Action onHide;
        public void InitActionOnExcuteAndActionOnHide(Action onExcute, Action onHide)
        {
            this.onExcute = onExcute;
            this.onHide = onHide;
        }


    }

}