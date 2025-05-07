namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using TMPro;
    using Unity.VisualScripting;
    using UnityEngine;
    using UnityEngine.UI;

    public class UITicketTally : UIBase
    {
        private TicketTallyManager ticketTallyManager
        {
            get
            {
                return EventManager.Instance.ticketTallyManager;
            }
        }
        public TMP_Text txtTime;
        public ProgressBarUI progressBarUI;
        public TMP_Text txtAmount;
        public ResUI resUI;

        //
        public ScrollRect scrollView;
        public Transform parentSpawn;
        public ItemTicketTallyUI itemTicketTallyPf;
        private List<ItemTicketTallyUI> listItemTicket = new List<ItemTicketTallyUI>();
        protected override void Setup()
        {
            isAnimBtnClose = true;
            base.Setup();
        }

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
            long timeContain = ticketTallyManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
            OnTimePerSecond(null);
        }
        public void InitTutorial()
        {
            int levelCurrent = 0;
            int contain = 0;
            GiftEvent data = new GiftEvent();
            if (levelCurrent >= ticketTallyManager.dataEvent.MaxReward())
            {
                int maxLevel = ticketTallyManager.dataEvent.DB_GiftEvent.gifts.Count - 1;
                data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts[maxLevel];
            }
            else
            {
                data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts[levelCurrent];
            }

            int totalRequire = data.require;
            float percent = (float)((float)contain / (float)totalRequire);
            txtAmount.text = $"{contain}/{totalRequire}";
            resUI.InitData(data.groupGift.dataResources[0]);
            progressBarUI.SetSlider(percent, percent, 0, null);
            InitListItem(0);
        }
        public override void RefreshUI()
        {
            base.RefreshUI();
            int levelCurrent = ticketTallyManager.dataEvent.LevelCurrent;
            int contain = ticketTallyManager.dataEvent.ContainItem;
            GiftEvent data = new GiftEvent();
            if (levelCurrent >= ticketTallyManager.dataEvent.MaxReward())
            {
                int maxLevel = ticketTallyManager.dataEvent.DB_GiftEvent.gifts.Count - 1;
                data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts[maxLevel];
            }
            else
            {
                data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts[levelCurrent];
            }

            int totalRequire = data.require;
            float percent = (float)((float)contain / (float)totalRequire);
            txtAmount.text = $"{contain}/{totalRequire}";
            resUI.InitData(data.groupGift.dataResources[0]);
            progressBarUI.SetSlider(percent, percent, 0, null);
            InitListItem();

        }
        private void InitListItem(int levelCurrent = -1)
        {
            if(levelCurrent == -1)
            {
                levelCurrent = ticketTallyManager.dataEvent.LevelCurrent;
            }
            var data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts;
            int length = data.Count;
            MasterHelper.InitListObj(length, itemTicketTallyPf, listItemTicket, parentSpawn, (item, index) =>
            {
                item.gameObject.SetActive(true);
                var dataEach = data[index];
                EStateClaim eState = EStateClaim.WillClaim;
                if (dataEach.isClaimed)
                {
                    eState = EStateClaim.Claimed;
                }
                else if (levelCurrent == index)
                {
                    eState = EStateClaim.CanClaim;
                }
                item.Initialize(index, dataEach, eState, Click);
            });
            if (!ticketTallyManager.dataEvent.IsMaxLevel)
            {
                itemTicketCur = listItemTicket[levelCurrent];
            }
            else
            {
                int max = listItemTicket.Count - 1;
                itemTicketCur = listItemTicket[max];
            }

            GameUtil.Instance.WaitAndDo(0.1f, MoveToTarget);

        }
        private ItemTicketTallyUI itemTicketCur = null;
        private void MoveToTarget()
        {
            scrollView.FocusOnRectTransform(itemTicketCur.GetComponent<RectTransform>(), true);
        }
        private void Click(ItemTicketTallyUI item)
        {
            item.boxInforMess.gameObject.SetActive(true);
        }
        public void ShowPopUpTicketIfHas()
        {
            if (!ticketTallyManager.IsHasDataDontClaim())
            {
                return;
            }
            int levelCurrent = ticketTallyManager.dataEvent.LevelCurrent;
            List<DataResource> listDataRes = new List<DataResource>();
            for (int i = 0; i < levelCurrent; i++)
            {
                var data = ticketTallyManager.dataEvent.GetDBGiftEvent(i);
                if (!data.isClaimed)
                {
                    var listRes = data.groupGift.dataResources;
                    listDataRes.AddRange(listRes);
                    ticketTallyManager.UseItem(i);
                }
            }
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, resUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.TicketTally, UIManager.Instance.RefreshUI, isAnim: true, posSpawn: pos, listDataRes.ToArray());
        }
    }
}

