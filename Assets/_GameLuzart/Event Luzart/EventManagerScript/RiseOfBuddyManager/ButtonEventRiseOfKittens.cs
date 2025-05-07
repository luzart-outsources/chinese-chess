namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class ButtonEventRiseOfKittens : ButtonEvent
    {
        public TextMeshProUGUI txtTime;
        public TextMeshProUGUI txtIndex;
        public GameObject obNoti;
        private RiseOfKittensManager riseOfKittensManager
        {
            get
            {
                return EventManager.Instance.riseOfKittensManager;
            }
        }
        private void Awake()
        {
            if (IsActiveEvent)
                Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = riseOfKittensManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        private void CheckNoti(int level)
        {
            bool isNoti = riseOfKittensManager.IsHasDataDontClaim(level);
            GameUtil.SetActiveCheckNull(obNoti, isNoti);
        }
        protected override void InitButton()
        {
            base.InitButton();
            if (riseOfKittensManager.IsMaxReceive())
            {
                gameObject.SetActive(false);
                return;
            }
            OnTimePerSecond(null);
            SetVisual(riseOfKittensManager.dataEvent.totalUse);


        }
        public override void InitEvent(Action action)
        {
            base.InitEvent(ClickButton);
        }
        private void ClickButton()
        {
            UIManager.Instance.ShowUI(UIName.RiseOfKittens);
        }
        public void SetVisual(int level)
        {
            txtIndex.text = level.ToString();
            CheckNoti(level);
        }
    }
}
