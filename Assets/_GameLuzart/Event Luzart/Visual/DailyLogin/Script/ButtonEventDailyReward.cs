namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class ButtonEventDailyReward : ButtonEvent
    {
        public GameObject obNoti;
        private DailyLoginManager dailyRewardManager
        {
            get
            {
                return EventManager.Instance.dailyLoginManager;
            }
        }
        public void CheckNoti()
        {
            bool isNoti = dailyRewardManager.IsHasDataFreeDontReceive();
            GameUtil.SetActiveCheckNull(obNoti, isNoti);
        }
        protected override void InitButton()
        {
            base.InitButton();
            CheckNoti();
        }
        public override void InitEvent(Action action)
        {
            base.InitEvent(ClickDailyReward);
        }
        private void ClickDailyReward()
        {
            Debug.LogError("Click DailyReward");
        }
    }
}

