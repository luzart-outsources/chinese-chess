namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class DailyLoginManager : BaseEventManager
    {
        private const string PATH_DAILY_LOGIN = "daily_login";
        public int Today;
        public override EEventName eEvent => EEventName.DailyLogin;

        public override TimeEvent GetTimeEvent
        {
            get
            {
                if (!isLoad)
                {
                    LoadDataPath();
                    isLoad = true;
                }
                if(dataEvent.timeStartEvent == -1)
                {
                    dataEvent.timeStartEvent = TimeUtils.GetLongTimeStartDay(EventManager.Instance.TimeFirstTimeStartGame);
                }
                else if (!dataEvent.isReceiveToday && dataEvent.GetDay == -1)
                {
                    dataEvent.timeStartEvent = TimeUtils.GetLongTimeStartToday;
                }
                TimeEvent timeEvent = new TimeEvent();
                timeEvent.timeStart = dataEvent.timeStartEvent;
                timeEvent.timeStartReview = int.MaxValue;
                return timeEvent;

            }
        }
        private bool isLoad = false;

        private void Start()
        {
            Observer.Instance?.AddObserver(ObserverKey.OnNewDay, OnNewDay);
        }
        private void OnDestroy()
        {
            Observer.Instance?.RemoveObserver(ObserverKey.OnNewDay, OnNewDay);
        }
        public DataDailyLogin dataEvent;
        private void OnNewDay(object data)
        {
            dataEvent.isReceiveToday = false;
            SaveData();
            Today = GetToday();
        }
        private void LoadDataPath()
        {
            dataEvent = SaveLoadUtil.LoadDataPrefs<DataDailyLogin>(PATH_DAILY_LOGIN);
            if (dataEvent == null)
            {
                dataEvent = new DataDailyLogin();
                dataEvent.dbEvent = new DB_Event();
            }
        }
        public override void LoadData()
        {
            LoadDataPath();

            if (!EventManager.Instance.IsHasEvent(eEvent))
            {
                return;
            }
            DB_Event db_EventLocal = dataEvent.dbEvent;
            DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.DailyLogin);
            if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
            {
                dataEvent = new DataDailyLogin();
                dataEvent.dbEvent = new DB_Event();
                dataEvent.dbEvent = dbEvent;
                PushFirebaseShow();
            }
            else
            {
                db_EventLocal.idEvent = dbEvent.idEvent;
                db_EventLocal.timeEvent = dbEvent.timeEvent;
                int length = dbEvent.allTypeResources.Length;
                List<DB_GiftEvent> list = new List<DB_GiftEvent>();
                for (int i = 0; i < length; i++)
                {
                    DB_GiftEvent db = new DB_GiftEvent();
                    db.type = dbEvent.allTypeResources[i].type;
                    int lengthGift = dbEvent.allTypeResources[i].gifts.Count;
                    db.gifts = new List<GiftEvent>();
                    for (int j = 0; j < lengthGift; j++)
                    {
                        GiftEvent gift = new GiftEvent();
                        gift.groupGift = dbEvent.allTypeResources[i].gifts[j].groupGift;
                        gift.require = dbEvent.allTypeResources[i].gifts[j].require;
                        gift.isClaimed = db_EventLocal.allTypeResources[i].gifts[j].isClaimed;
                        db.gifts.Add(gift);
                    }
                    list.Add(db);
                }
                db_EventLocal.allTypeResources = list.ToArray();

            }
            Today = GetToday();
        }

        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_DAILY_LOGIN);
        }
        public bool IsClaimDay(int day)
        {
            return dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[day].isClaimed;
        }
        public void ClaimReward(int day = -1, bool isX2 = false)
        {
            var gift = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[day];
            gift.isClaimed = true;
            dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts[day] = gift;
            dataEvent.isReceiveToday = true;
            if (isX2)
            {
                dataEvent.ClaimDayX2(day);
                PushClaimEvent(day,"ads");
                PushCompleteEvent(day, "ads");
            }
            else
            {
                dataEvent.ClaimDayNormal(day);
                PushClaimEvent(day, "free");
                PushCompleteEvent(day, "free");
            }
            SaveData();
        }

    
        public int GetToday()
        {
            return dataEvent.GetDay;
        }

        public bool IsHasDataFreeDontReceive()
        {
            Today = GetToday();
            return !IsClaimDay(Today);
        }
        private void PushCompleteEvent(int index, string strType)
        {
            if (dataEvent == null || dataEvent.dbEvent == null || dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts == null)
            {
                return;
            }
            if (index >= dataEvent.MaxLevel - 1)
            {
#if FIREBASE_ENABLE
                FirebaseEvent.LogEvent(KeyFirebase.Event_Completed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,index.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventType, strType),
});
#endif
            }
        }
        private void PushClaimEvent(int level, string strType)
        {
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,level.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventType, strType),
});
#endif
        }
        private void PushFirebaseShow()
        {
            if (dataEvent == null || dataEvent.timeStartEvent == -1)
            {
                return;
            }
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Show, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName, eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,"0"),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString())

});
#endif
        }
    }
    [System.Serializable]
    public class DataDailyLogin
    {
        public DB_Event dbEvent;
        public bool[] listIsClaim =  new bool[0];
        public bool[] listIsClaimX2 = new bool[0];
        public bool isReceiveToday = false;
        public long timeStartEvent = -1;

        public int MaxLevel
        {
            get
            {
                var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
                return dataAll.Count;

            }
        }

        public bool IsClaimedDay(int day)
        {
            var db_giftEvent = dbEvent.GetRewardType(ETypeResource.None);
            var allDB = db_giftEvent.gifts;
            int length = allDB.Count;
            if (day < length)
            {
                return allDB[day].isClaimed;
            }
            else
            {
                return false;
            }

        }
        public int GetDay
        {
            get
            {
                var db_giftEvent = dbEvent.GetRewardType(ETypeResource.None);
                var allDB = db_giftEvent.gifts;
                int length = allDB.Count;
                for (int i = 0; i < length; i++)
                {
                    if (!allDB[i].isClaimed)
                    {
                        if (isReceiveToday)
                        {
                            return i - 1;
                        }
                        else
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }
        }
        public bool IsClaimDayNormal(int day)
        {
            InitIsClaim();
            if(day == -1 || day > listIsClaim.Length)
            {
                return false;
            }
            return listIsClaim[day];
        }
        public bool IsClaimDayX2(int day)
        {
            InitIsClaim();
            if(day == -1 || day > listIsClaimX2.Length)
            {
                return false;
            }
            return listIsClaimX2[day];  
        }
        public void ClaimDayNormal(int day)
        {
            InitIsClaim();
            if (day == -1 || day > listIsClaim.Length)
            {
                return;
            }
            listIsClaim[day] = true;
        }

        public void ClaimDayX2(int day)
        {
            InitIsClaim();
            if (day == -1 || day > listIsClaimX2.Length)
            {
                return;
            }
            listIsClaimX2[day] = true;
        }

        private void InitIsClaim()
        {
            var db_giftEvent = dbEvent.GetRewardType(ETypeResource.None);
            var allDB = db_giftEvent.gifts;
            int length = allDB.Count;
            if(listIsClaim == null || listIsClaim.Length == 0)
            {
                listIsClaim = new bool[length];
            }
            if(listIsClaimX2 == null || listIsClaimX2.Length == 0)
            {
                listIsClaimX2 = new bool[length];
            }
        }
    }
    [System.Serializable]
    public class DataProcessReward
    {
        public bool[] isClaim = new bool[4];
    }
    [System.Serializable]
    public class DBProcessReward
    {
        public int day;
        public GroupDataResources groupDataResources;
    }
}
