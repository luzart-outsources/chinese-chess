namespace Luzart
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UITicketTallyNoti : UIBase
    {
        public Button btnLetsGo;
        public TextMeshProUGUI txtTime;
        private void Awake()
        {
            Observer.Instance.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnDestroy()
        {
            if (Observer.Instance != null)
                Observer.Instance.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = EventManager.Instance.ticketTallyManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }

        // Start is called before the first frame update
        void Start()
        {
            GameUtil.ButtonOnClick(btnLetsGo, ClickLetGo, true);
        }
        public void ClickLetGo()
        {
            Hide();
        }
    }

}
