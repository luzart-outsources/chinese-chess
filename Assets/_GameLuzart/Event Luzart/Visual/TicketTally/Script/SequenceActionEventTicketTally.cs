namespace Luzart
{
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class SequenceActionEventTicketTally : SequenceActionEvent
    {
        [SerializeField]
        private ButtonEventTicketTally _btnTicketTally;
        public ButtonEventTicketTally btnTicketTally
        {
            get
            {
                if (_btnTicketTally == null)
                {
                    _btnTicketTally = FindAnyObjectByType<ButtonEventTicketTally>();
                }
                return _btnTicketTally;
            }
        }

        public ObjectCurveMover objectCurveMover;
        public RectTransform rtTicketTally;
        public TMP_Text txtValueTicketTally;
        private TicketTallyManager ticketTallyManager
        {
            get
            {
                return EventManager.Instance.ticketTallyManager;
            }
        }

        public override void PreInit()
        {
            int countKey = ticketTallyManager.dataEvent.cacheKey;
            int totalCurrent = ticketTallyManager.dataEvent.totalUse;
            int preTotalCurrent = totalCurrent - countKey;
            bool isHasEvent = EventManager.Instance.IsHasEvent(EEventName.TicketTally);
            bool isUnlockEvent = EventManager.Instance.IsUnlockLevel(EEventName.TicketTally, DataWrapperGame.CurrentLevel);
            bool isTutorial = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.TicketTally);
            if (isHasEvent)
            {
                if(isUnlockEvent && !isTutorial)
                {
                    btnTicketTally.SetVisual(0);
                }
                else
                {
                    btnTicketTally.SetVisual(preTotalCurrent);
                }
            }

        }

        public float delay;
        private bool isDelay = false;
    
        public override void Init(Action callback)
        {
            GameUtil.StepToStep(new Action<Action>[]
            {
                ShowPopUpTicketIfHas,
                OnShowTutorialTicketTally,
                MoveTicketTally,
                CallOnDone,
            });

            async void  CallOnDone(Action onDone)
            {
                if(isDelay) await UniTask.WaitForSeconds(delay);
                callback?.Invoke();
            }
        }


        private void ShowPopUpTicketIfHas(Action onDone)
        {
            onDone?.Invoke();
            return;
            if(ticketTallyManager.dataEvent.dbEvent.eventStatus == EEventStatus.Running)
            {
                onDone?.Invoke();
                return;
            }
            if (!ticketTallyManager.IsHasDataDontClaim())
            {
                onDone?.Invoke();
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
            DataWrapperGame.ReceiveReward(ValueFirebase.TicketTally, listDataRes.ToArray());
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, btnTicketTally.resUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.TicketTally, OnCallReceiveRewardEventTicket, isAnim: true, posSpawn: pos, listDataRes.ToArray());

            void OnCallReceiveRewardEventTicket()
            {
                onDone?.Invoke();
            }
        }

        private void OnShowTutorialTicketTally(Action onDone)
        {
            bool isHasEvent = EventManager.Instance.IsHasEvent(EEventName.TicketTally);
            bool isUnlockEvent = EventManager.Instance.IsUnlockLevel(EEventName.TicketTally, DataWrapperGame.CurrentLevel);
            bool isTutorial = EventManager.Instance.dataEvent.GetTutorialComplete(EEventName.TicketTally);
            if (isHasEvent && isUnlockEvent && !isTutorial)
            {
                btnTicketTally.SetVisual(0);
                var uiNoti = Luzart.UIManager.Instance.ShowUI<UITicketTallyNoti>(UIName.TicketTallyNoti, () =>
                {
                    var ui = Luzart.UIManager.Instance.ShowUI<UITicketTally>(UIName.TicketTally, onDone);
                    ui.InitTutorial();
                    EventManager.Instance.dataEvent.SetTutorialComplete(EEventName.TicketTally, true);
                    EventManager.Instance.SaveAllData();
                });
            }
            else
            {
                onDone?.Invoke();
            }
        }
        private void MoveTicketTally(Action onDone)
        {
            AnimationAndCheckTicket(onDone);
        }
    
        public void AnimationAndCheckTicket(Action onDone)
        {
            if (IsHasAnimTicketDaily)
            {
                isDelay = true;
                AnimationTicketTally(onDone);
            }
            else
            {
                onDone?.Invoke();
            }
        }
        private int countKey = 0;
        public void AnimationTicketTally(Action onDone = null)
        {
            countKey = ticketTallyManager.dataEvent.cacheKey;
            // SAVE DATA ANIM
            ticketTallyManager.dataEvent.cacheKey = 0;
            ticketTallyManager.SaveData();

            int totalCurrent = ticketTallyManager.dataEvent.totalUse;
            int preTotalCurrent = totalCurrent - countKey;
            btnTicketTally.SetVisual(preTotalCurrent);
            txtValueTicketTally.text = $"x{countKey}";
            TicketRewardRunning(onDone);
        }

        private void TicketRewardRunning(Action onDone)
        {
            int totalCurrent = ticketTallyManager.dataEvent.totalUse;
            int preTotalCurrent = totalCurrent - countKey;
            
            int preLevel = ticketTallyManager.dataEvent.LevelByTotalUse(preTotalCurrent);
            int currentExp = ticketTallyManager.dataEvent.ContainByTotalUse(preTotalCurrent);
            int nextExpLevelUp = ticketTallyManager.dataEvent.RequireByLevel(preLevel);
            
            int nextExp = currentExp + countKey;

            if (nextExp >= nextExpLevelUp)
            {
                int expRunning = nextExpLevelUp - currentExp;
                RunningExpProgressBar(expRunning, currentExp, nextExpLevelUp, 0.1f,  
                    () => ClaimReward(preLevel, () => OnClaimReward(preLevel, expRunning, onDone)));
            }
            else
            {
                RunningExpProgressBar(countKey, currentExp, nextExpLevelUp, 0.1f, onDone);
            }
        }

        private void OnClaimReward(int levelCurrent, int expRunning, Action onDone)
        {
            countKey -= expRunning;
            var data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts;
            bool hasReward = levelCurrent < data.Count;
            if (countKey == 0 || !hasReward)
            {
                onDone?.Invoke();
            }
            else
            {
                TicketRewardRunning(onDone);
            }

            if (hasReward)
            {
                btnTicketTally.progressBarUI.SetSlider(0,0,0,null);
                btnTicketTally.SetResUILevel(levelCurrent + 1);
                int require = ticketTallyManager.dataEvent.RequireByLevel(levelCurrent+1);
                btnTicketTally.txtTicketCurrent.text = $"{0}/{require}";
            }
            else
            {
                btnTicketTally.progressBarUI.SetSlider(1,1,0,null);
                btnTicketTally.SetResUILevel(levelCurrent);
                btnTicketTally.txtTicketCurrent.text = $"All Golden Ticket rewards claimed!";
            }
        }

        private void RunningExpProgressBar(int totalTicket, int currentExp, int nextExpLevelUp, float duration, Action onRunningFinished = null)
        {
            objectCurveMover.StartMove(totalTicket, () =>
            {
                btnTicketTally.SetSliderNoTotal(currentExp, currentExp + 1, nextExpLevelUp, duration);
                currentExp++;
            }, onRunningFinished);   
        }

        private async void ClaimReward(int levelCurrent, Action onClaim)
        {
            var data = ticketTallyManager.dataEvent.DB_GiftEvent.gifts;
            if (levelCurrent >= data.Count)
            {
                onClaim?.Invoke();
                return;
            }
            
            await UniTask.WaitForSeconds(0.25f);
            
            DataResource dataResource = data[levelCurrent].groupGift.dataResources[0];
            
            EventManager.Instance.ticketTallyManager.UseItem(levelCurrent);
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, btnTicketTally.resUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.TicketTally, onClaim, isAnim: true, posSpawn: pos, dataResource);
        }

        public bool IsHasAnimTicketDaily
        {
            get
            {
                if (EventManager.Instance.IsHasEvent(EEventName.TicketTally) && EventManager.Instance.IsUnlockLevel(EEventName.TicketTally, DataWrapperGame.CurrentLevel))
                {
                    TicketTallyManager ticketTallyManager = EventManager.Instance.ticketTallyManager;
                    if (ticketTallyManager != null)
                    {
                        if (ticketTallyManager.IsCacheShowVisual /*&& ticketTallyManager.valueKey > 0*/)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
        private void OnShowRewardTicket(Action onDone)
        {
            int totalCurrent = ticketTallyManager.dataEvent.totalUse;
            int preTotalCurrent = totalCurrent - countKey;
            OnShowRewardTicket(preTotalCurrent, totalCurrent, onDone);
        }
        private void OnShowRewardTicket(int preTotal, int total, Action onDone)
        {
            var dataTicket = EventManager.Instance.ticketTallyManager.dataEvent;
    
            int preIndex = dataTicket.LevelByTotalUse(preTotal);
            int curIndex = dataTicket.LevelByTotalUse(total);
            if (curIndex <= preIndex)
            {
                onDone?.Invoke();
                return;
            }
            List<DataResource> listDataRes = new List<DataResource>();
            for (int i = preIndex; i < curIndex; i++)
            {
                var listRes = dataTicket.GetDBGiftEvent(i).groupGift.dataResources;
                listDataRes.AddRange(listRes);
                EventManager.Instance.ticketTallyManager.UseItem(i);
            }
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, btnTicketTally.resUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.TicketTally, onDone, isAnim: true, posSpawn: pos,  listDataRes.ToArray());
        }
    }
}
