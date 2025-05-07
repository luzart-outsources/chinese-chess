namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class ButtonEventJourneyToSuccess : ButtonEvent
    {
        private DB_Event db_Event;
        public TextMeshProUGUI txtTime;
        public GameObject obNoti;
        private JourneyToSuccessManager journeyToSuccessManager
        {
            get
            {
                return EventManager.Instance.journeyToSuccessManager;
            }
        }
        protected override void Start()
        {
            base.Start();
            if (IsActiveEvent)
            {
                Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
                Observer.Instance.AddObserver(ObserverKey.OnCheckNotiTreasure, OnCheckNotiTreasure);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
            Observer.Instance?.RemoveObserver(ObserverKey.OnCheckNotiTreasure, OnCheckNotiTreasure);
        }
        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = journeyToSuccessManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        public void CheckNoti()
        {
            bool isNoti = journeyToSuccessManager.IsHasDataFreeDontReceive();
            GameUtil.SetActiveCheckNull(obNoti, isNoti);
        }
        protected override void InitButton()
        {
            if (journeyToSuccessManager.IsMaxReceive())
            {
                gameObject.SetActive(false);
                return;
            }
            CheckNoti();
            OnTimePerSecond(null);
    
        }
        public override void InitEvent(Action action)
        {
            base.InitEvent(OnClickJourneyToSuccess);
        }
        public void OnClickJourneyToSuccess()
        {
            Debug.LogError("ClickJourneyToSuccess");
        }
        private void OnCheckNotiTreasure(object data)
        {
            InitRefreshUI();
        }

    }
}
