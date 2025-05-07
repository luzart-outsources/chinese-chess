namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class ButtonEventFlightEndurance : ButtonEvent
    {
        public TMP_Text txtTime;
        public TMP_Text txtWinStreak;
        public BaseSelect selectWinStreak;
        private FlightEnduranceManager flightEnduranceManager
        {
            get
            {
                return EventManager.Instance.flightEnduranceManager;
            }
        }
        private void OnEnable()
        {
            if (IsActiveEvent)
                Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnDisable()
        {
            if (Observer.Instance != null)
                Observer.Instance.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        public override void InitEvent(Action action)
        {
            base.InitEvent(action);
        }
        protected override void InitButton()
        {
            base.InitButton();
            int winstreak = flightEnduranceManager.dataFlightEndurance.winStreak;
            bool isWinStreak = winstreak > 0 && !flightEnduranceManager.IsWin && !flightEnduranceManager.IsLoss && flightEnduranceManager.dataFlightEndurance.isShowStart;
            selectWinStreak.Select(isWinStreak);
            if(isWinStreak)
            {
                txtWinStreak.text = $"{winstreak}";
            }
            OnTimePerSecond(null);
        }
        private void OnTimePerSecond(object data)
        {
            if (flightEnduranceManager.IsLoss)
            {
                long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeLoss;
                txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
            }
            else if (flightEnduranceManager.IsWin)
            {
                long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeWin;
                txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
            }
            else
            {
                long timeCurrent = TimeUtils.GetLongTimeCurrent;
                long timeContain = flightEnduranceManager.dataFlightEndurance.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
                txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
            }
    
        }
    }
}
