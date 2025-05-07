namespace Luzart
{

    using DG.Tweening;
    using System;
    using TMPro;
    using UnityEngine;

    public class ButtonEventTicketTally : ButtonEvent
    {
        public Transform transformTicket;
        public TextMeshProUGUI txtTime;
        public TextMeshProUGUI txtTicketCurrent;
        public ResUI resUI;
        public ProgressBarUI progressBarUI;
        public GameObject obNoti;
        public GameObject obFX;
        public BaseSelect bsUnlock;
        private TicketTallyManager ticketTallyManager
        {
            get
            {
                return EventManager.Instance.ticketTallyManager;
            }
        }
        protected override void Start()
        {
            base.Start();
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnTimePerSecond(object data)
        {
            if (!IsActiveEvent)
            {
                return;
            }
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = EventManager.Instance.ticketTallyManager.dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
            txtTime.text = GameUtil.LongTimeSecondToUnixTime(timeContain);
        }
        protected override void InitButton()
        {
            base.InitButton();

            SetVisual(ticketTallyManager.dataEvent.totalUse);
            CheckNoti();
            OnTimePerSecond(null);

        }
        private void CheckNoti()
        {
            bool isNoti = ticketTallyManager.IsHasDataDontClaim();
            GameUtil.SetActiveCheckNull(obNoti, isNoti);
        }
        public void SetVisual(int totalUse)
        {
            int levelCurrent = ticketTallyManager.dataEvent.LevelByTotalUse(totalUse);
            int contain = ticketTallyManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequire = ticketTallyManager.dataEvent.RequireByTotalUse(totalUse);
            float percent = (float)((float)contain / (float)totalRequire);

            if (levelCurrent >= ticketTallyManager.dataEvent.DB_GiftEvent.gifts.Count)
            {
                txtTicketCurrent.text = $"All Golden Ticket rewards claimed!";

            }
            else
            {
                txtTicketCurrent.text = $"{contain}/{totalRequire}";
            }
            
            progressBarUI.SetSlider(percent, percent, 0f, null);
            SetResUI(totalUse);
        }
        private void SetResUI(int totalUse)
        {
            int levelCurrent = ticketTallyManager.dataEvent.LevelByTotalUse(totalUse);
            SetResUILevel(levelCurrent);
        }

        public void SetResUILevel(int levelCurrent)
        {
            levelCurrent = Mathf.Clamp(levelCurrent, 0, ticketTallyManager.dataEvent.MaxReward() - 1);
            var data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts;
            DataResource dataResource = new DataResource();
            if (data[levelCurrent].groupGift.IsHasChest)
            {
                dataResource = new DataResource(data[levelCurrent].groupGift.typeChest, 1);
            }
            else
            {
                dataResource = data[levelCurrent].groupGift.dataResources[0];
            }
            resUI.InitData(dataResource);
        }

        public void SetSliderNoTotal(int currentExp, int nextExp, int totalExp, float duration)
        {
            float percent = (float)((float)currentExp / (float)totalExp);
            float percentNext = (float)((float)nextExp / (float)totalExp);

            progressBarUI.SetSliderTween(percent, percentNext, duration, null);
            txtTicketCurrent.text = $"{nextExp}/{totalExp}";
        }

        public Tween SetSlider(int preTotalUse, int totalUse,float timeMove = 0.5f, Action onDone = null)
        {
            int levelPre = ticketTallyManager.dataEvent.LevelByTotalUse(preTotalUse);
            int containPre = ticketTallyManager.dataEvent.ContainByTotalUse(preTotalUse);
            int totalRequirePre = ticketTallyManager.dataEvent.RequireByTotalUse(preTotalUse);
            float percentPre = (float)((float)containPre / (float)totalRequirePre);


            int levelCurrent = ticketTallyManager.dataEvent.LevelByTotalUse(totalUse);
            int containCurrent = ticketTallyManager.dataEvent.ContainByTotalUse(totalUse);
            int totalRequireCurrent = ticketTallyManager.dataEvent.RequireByTotalUse(totalUse);
            float percentCurrent = (float)((float)containCurrent / (float)totalRequireCurrent);

            Sequence sq = DOTween.Sequence();
            txtTicketCurrent.text = $"{containPre}/{totalRequirePre}";
            if (levelCurrent > levelPre)
            {
                float timePre = (1 - percentPre) * timeMove;
                float timeEnd = (percentCurrent) * timeMove;
                sq.AppendCallback(() =>
                {
                    SetResUI(preTotalUse);
                });
                sq.Append(progressBarUI.SetSliderTween(percentPre, 1, timePre, null));
                sq.Join(DOVirtual.Int(containPre, totalRequirePre, timePre, (x) => txtTicketCurrent.text = $"{x}/{totalRequirePre}"));
                for (int i = levelPre + 1; i < levelCurrent; i++)
                {
                    int total = ticketTallyManager.dataEvent.TotalUseByLevel(i);
                    sq.Append(SetSliderFull(total, timeMove));
                }
                sq.AppendCallback(() =>
                {
                    SetResUI(totalUse);
                });
                sq.Append(progressBarUI.SetSliderTween(0, percentCurrent, timeEnd, null));
                sq.Join(DOVirtual.Int(0, containCurrent, timeEnd, (x) => txtTicketCurrent.text = $"{x}/{totalRequireCurrent}"));
            }
            else
            {
                sq.AppendCallback(() =>
                {
                    SetResUI(totalUse);
                });
                sq.Append(progressBarUI.SetSliderTween(percentPre, percentCurrent, timeMove, null));
                sq.Join(DOVirtual.Int(containPre, containCurrent, timeMove, (x) => txtTicketCurrent.text = $"{x}/{totalRequireCurrent}"));
            }
            return sq;
        }

        private Tween SetSliderFull(int totalUse, float timeFloat)
        {
            totalUse = Mathf.Clamp(totalUse - 1, 0, 1000000);
            int totalRequire = ticketTallyManager.dataEvent.RequireByTotalUse(totalUse);
            Sequence sq = DOTween.Sequence();
            sq.Append(resUI.transform.DOScale(0.3f, 0.1f));

            sq.AppendCallback(() =>
            {
                SetResUI(totalUse);
            });

            sq.Append(resUI.transform.DOScale(1f, 0.1f));
            sq.Append(progressBarUI.SetSliderTween(0, 1, timeFloat, null));
            sq.Join(DOVirtual.Int(0, totalRequire, timeFloat, (x) => txtTicketCurrent.text = $"{x}/{totalRequire}"));
            return sq;
        }
        public TMP_Text txtUnlock;
        public override void InitEvent(Action action)
        {
            this.actionClick = ClickTicketTally;
            if (gameObject == null)
            {
                return;
            }
            bsUnlock?.Select(IsUnlockLevel);
            if (!IsUnlockLevel)
            {
                if (txtUnlock != null)
                {
                    txtUnlock.text = $"UNLOCK AT LEVEL {EventManager.Instance.GetLevelUnlock(EEventName.TicketTally)+1}";
                }
            }
            if (IsActiveEvent)
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
        private void ClickTicketTally()
        {
            if (!IsUnlockLevel)
            {
                UIManager.Instance.ShowToast($"Reach to level {EventManager.Instance.GetLevelUnlock(EEventName.TicketTally)+1} unlock !");
                return;
            }
            var ui = UIManager.Instance.ShowUI<UITicketTally>(UIName.TicketTally);
            ui.ShowPopUpTicketIfHas();
        }
    }

}