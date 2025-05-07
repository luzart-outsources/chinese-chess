namespace Luzart
{
    using System;
    using System.Collections;
    using TMPro;
    using UnityEngine;
    
    public class ButtonEventCup : ButtonEvent
    {
        public CupManager cupManager
        {
            get
            {
                return EventManager.Instance.cupManager;
            }
        }
    
        public TMP_Text txtTime;
        public TMP_Text txtIndex;
        public BaseSelect bsLevelCup;
        public override void InitEvent(Action action)
        {
            this.actionClick = action;
            if (IsUnlockLevel)
            {
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                return;
            }
            InitButton();
        }
        protected override void InitButton()
        {
            bool isHasEventLocal = cupManager.IsHasEventLocal();
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, TimePerSecond);
            if (!isHasEventLocal && !IsActiveEvent)
            {
                gameObject.SetActive(false);
                return;
            }
            bsLevelCup.Select(isHasEventLocal);
            cupManager.UpdateBotByTimeOnline();
            int indexMe = cupManager.GetIndexMe();
            txtIndex.text = $"{indexMe + 1}";
            bool isEventFinish = cupManager.IsEventFinish();
            if (isEventFinish)
            {
    
                txtTime.text = "FINISH";
                return;
            }
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, TimePerSecond);
            TimePerSecond(null);
        }
        private void TimePerSecond(object data)
        {
            if (!IsActiveEvent)
            {
                return;
            }
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeEndEvent = EventManager.Instance.GetEvent(eEventName).timeEvent.TimeEndActionUnixTime;
            long deltaTime = timeEndEvent - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(deltaTime);
        }
        public void SetItem(int index)
        {
            txtIndex.text = (index + 1).ToString();
        }
        public void RefreshItem()
        {
            int indexMe = cupManager.GetIndexMe();
            SetItem(indexMe);
        }
    }
}
