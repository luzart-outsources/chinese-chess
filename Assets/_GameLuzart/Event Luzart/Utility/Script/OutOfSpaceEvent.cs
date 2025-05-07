using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfSpaceEvent : MonoBehaviour
{
    public List<IconEvent> listIconEvent = new List<IconEvent>();
    public GameObject obHeart;

    private RectTransform _rectTransform = null;
    public RectTransform rectTransform
    {
        get
        {
            if(_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }   
            return _rectTransform;
        }
    }

    public bool CheckAndInitialize()
    {
        int countListIcon = listIconEvent.Count;
        for (int i = 0; i < countListIcon; i++)
        {
            int index = i;
            listIconEvent[index].gameObject.SetActive(false);
        }
        obHeart.SetActive(false);
        List<EEventName> list = EventManager.Instance.GetEEventCurrent();
        if (list.Contains(EEventName.FlightEndurance))
        {
            var flightEndurance = EventManager.Instance.flightEnduranceManager;
            if (!flightEndurance.dataFlightEndurance.isShowStart || (flightEndurance.dataFlightEndurance.isWin || flightEndurance.IsLoss))
            {
                list.Remove(EEventName.FlightEndurance);
            }
        }
        bool isCanShow = false;
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
                        amount = EventManager.Instance.battlePassManager.GetKey(Difficulty.Normal);
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
                        amount = EventManager.Instance.riseOfKittensManager.dataEvent.totalUse;
                        break;
                    }
            }

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

            if (amount == 0)
            {
                iconEvent.gameObject.SetActive(false);
                continue;
            }

            iconEvent.Initialize(eEvent, amount);
            isCanShow = true;
        }
        bool isShowHeart = CheckAndInitHeart();
        return isCanShow || isShowHeart;
    }
    private bool CheckAndInitHeart()
    {
        bool isNoneHeart = HeartManager.Instance.EStateHeart != EStateHeart.Infinite;
        obHeart?.SetActive(isNoneHeart);
        return isNoneHeart;
    }
}
