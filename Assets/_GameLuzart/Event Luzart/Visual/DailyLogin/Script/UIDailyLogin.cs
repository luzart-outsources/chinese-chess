namespace Luzart
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class UIDailyLogin : UIBase
    {
        [Space, Header("UI")]
        private DB_Event dBEvent;
        public ItemDailyLogin[] itemDailyLogins;
        public BaseSelect bsButton;

        public TMP_Text txtTime;

        private void Awake()
        {
            Observer.Instance?.AddObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.TimeActionPerSecond, OnTimePerSecond);
        }

        public override void Show(Action onHideDone)
        {
            base.Show(onHideDone);
            RefreshUI();
        }

        private DailyLoginManager dailyLoginManager
        {
            get
            {
                return EventManager.Instance.dailyLoginManager;
            }
        }

        private void OnTimePerSecond(object data)
        {
            long timeCurrent = TimeUtils.GetLongTimeCurrent;
            long timeContain = TimeUtils.GetLongTimeStartTomorrow - timeCurrent;
            string strTimeCurrent = GameUtil.LongTimeSecondToUnixTime(timeContain, true);
            txtTime.text = $"{strTimeCurrent}";
        }
        public void OnClickButton()
        {
            OnDone();
        }
        public void OnClickButtonAdsX2()
        {
            AdsWrapperManager.ShowReward(KeyAds.AdsReceivedDailyReward, OnDoneAds, UIManager.Instance.ShowToastInternet);
        }
        private void InitList()
        {
            var allGift = dBEvent.GetRewardType(ETypeResource.None).gifts;
            if (allGift == null)
            {
                return;
            }
            int length = allGift.Count;
            for (int i = 0; i < length; i++)
            {
                var gift = allGift[i];
                DBDailyLogin newDB = new DBDailyLogin();
                newDB.index = i;
                newDB.dataRes = gift.groupGift;
                itemDailyLogins[i].Initialize(newDB, ActionClick);
            }
            ActionClick(itemDailyLogins[dailyLoginManager.Today]);
        }
        private ItemDailyLogin cacheItemDailyLogin;
        private void ActionClick(ItemDailyLogin itemDailyReward)
        {
            if (cacheItemDailyLogin != null)
            {
                cacheItemDailyLogin.Select(false);
            }
            this.cacheItemDailyLogin = itemDailyReward;
            this.cacheItemDailyLogin.Select(true);
            OnRefreshButton();
        }
        private void OnRefreshButton()
        {
            int index = this.cacheItemDailyLogin.dbDailyLogin.index;
            if(index != dailyLoginManager.Today)
            {
                bsButton.Select(3);
                return;
            }
            bool isClaim = dailyLoginManager.dataEvent.IsClaimDayNormal(index);
            bool isClaimX2 = dailyLoginManager.dataEvent.IsClaimDayX2(index);
            if (!isClaim)
            {
                bsButton.Select(0);
            }
            else if (!isClaimX2)
            {
                bsButton.Select(1);
            }
            else
            {
                bsButton.Select(2);
            }

        }


        private void OnDone()
        {
            var listData = cacheItemDailyLogin.dbDailyLogin.dataRes.dataResources;
            dailyLoginManager.ClaimReward(cacheItemDailyLogin.dbDailyLogin.index);
            ReceiveReward(RefreshUIOnDone, listData.ToArray());
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.DailyLoginReceived, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,cacheItemDailyLogin.dbDailyLogin.index),
                new Firebase.Analytics.Parameter(TypeFirebase.Type,"normal"),
});
#endif
        }
        private void OnDoneAds()
        {
            var listData = cacheItemDailyLogin.dbDailyLogin.dataRes.dataResources;
            //int length = listData.Count;
            //DataResource[] arrayDataResources = new DataResource[length];
            //for (int i = 0; i < length; i++)
            //{
            //    int index = i;
            //    var item = listData[index].Clone();
            //    item.amount = item.amount * 2;
            //    arrayDataResources[index] = item;
            //}
            dailyLoginManager.ClaimReward(cacheItemDailyLogin.dbDailyLogin.index, true);
            ReceiveReward(RefreshUIOnDone, listData.ToArray());
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.DailyLoginReceived, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,cacheItemDailyLogin.dbDailyLogin.index),
                new Firebase.Analytics.Parameter(TypeFirebase.Type,"ads"),
});
#endif
        }
        private void ReceiveReward(Action onDone ,params DataResource[] dataRes)
        {
            Vector3 pos = RectTransformUtility.WorldToScreenPoint(null, cacheItemDailyLogin.listResUI.transform.position);
            DataWrapperGame.ReceiveRewardShowPopUpAnim(ValueFirebase.ResDailyReward, onDone, isAnim: true, posSpawn: pos, dataRes);
        }
        private void RefreshUIOnDone()
        {
            UIManager.Instance.RefreshUI();
            RefreshUI();
        }
        public override void RefreshUI()
        {
            dBEvent = dailyLoginManager.dataEvent.dbEvent;
            if (dBEvent == null || dBEvent.eventStatus == EEventStatus.Finish)
            {
                Hide();
                return;
            }
            InitList();
        }
    }

}