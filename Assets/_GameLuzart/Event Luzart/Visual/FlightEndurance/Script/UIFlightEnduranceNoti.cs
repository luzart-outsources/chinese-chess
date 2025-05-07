namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIFlightEnduranceNoti : UIBase
    {
        public BaseSelect baseSelect;
        public Button btnLetGo;
        public TMP_Text txtTime, txtTimeWait;
        private FlightEnduranceManager flightEnduranceManager
        {
            get
            {
                return EventManager.Instance.flightEnduranceManager;
            }
        }
    
    
        public void ClickLetGo()
        {
            flightEnduranceManager.StartEvent();
            UIManager.Instance.ShowUI(UIName.FlightEndurance);
            flightEnduranceManager.dataFlightEndurance.isShowNoti = true;
            EventManager.Instance.SaveDataEvent();
            Hide();
        }
        protected override void Setup()
        {
            base.Setup();
            GameUtil.ButtonOnClick(btnLetGo, ClickLetGo, true);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
    
        }
        public override void RefreshUI()
        {
            base.RefreshUI();
            int index = 0;
            if (flightEnduranceManager.IsLoss)
            {
                index = 1;
            }
            else if (flightEnduranceManager.IsWin)
            {
                index = 2;
            }
            baseSelect.Select(index);
            if (flightEnduranceManager.IsLoss || flightEnduranceManager.IsWin)
            {
                Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
                if (flightEnduranceManager.IsLoss)
                {
                    long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeLoss;
                    txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
                }
                else if (flightEnduranceManager.IsWin)
                {
                    long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeWin;
                    txtTimeWait.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
                }
            }
            else
            {
                Observer.Instance.RemoveObserver(ObserverKey.TimeActionPerSecond, OnPerSecond);
            }
        }
        private void OnPerSecond(object data)
        {
            if (flightEnduranceManager.IsLoss)
            {
                long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeLoss;
                txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
            }
            else if (flightEnduranceManager.IsWin)
            {
                long timeContain = flightEnduranceManager.dataFlightEndurance.countTimeWin;
                txtTimeWait.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
            }
            else
            {
                RefreshUI();
            }
        }
    }
}
