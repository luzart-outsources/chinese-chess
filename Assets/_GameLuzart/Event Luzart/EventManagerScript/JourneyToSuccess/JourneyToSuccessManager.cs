namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class JourneyToSuccessManager : BaseEventManager
    {
        private const string PATH_JTS = "journey_to_success";
        public DataJourneyToSuccess dataEvent;
        public DB_JourneyResources[] dbIAP;
        public override EEventName eEvent => EEventName.JourneyToSuccess;

        public override TimeEvent GetTimeEvent
        {
            get
            {
                TimeEvent timeEvent = new TimeEvent();
                timeEvent.timeStart = TuesdayThursdaySaturday();
                timeEvent.timeEndPreview = 0;
                timeEvent.timeStartReview = MondayWednesdayFriday() - TuesdayThursdaySaturday();
                timeEvent.timeEnd = 0;
                return timeEvent;
            }
        }

        public long TuesdayThursdaySaturday()
        {
            DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.GetLongTimeCurrent);
            int day = (int)now.DayOfWeek;

            if (day == (int)DayOfWeek.Monday) // Thứ Hai (1) → lấy Thứ 7 tuần trước
            {
                return TimeUtils.GetDateTimeDayOfCustomWeeks(DayOfWeek.Saturday, -1).ToUnixTimeSeconds();
            }
            else if (day >= (int)DayOfWeek.Saturday) // Thứ 7 (6)
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Saturday).ToUnixTimeSeconds();
            }
            else if (day >= (int)DayOfWeek.Thursday) // Thứ 5 (4) đến Thứ 6 (5)
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Thursday).ToUnixTimeSeconds();
            }
            else if (day >= (int)DayOfWeek.Tuesday) // Thứ 3 (2) đến Thứ 4 (3)
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Tuesday).ToUnixTimeSeconds();
            }
            else
            {
                // Chủ nhật (0) → vẫn tính là tuần hiện tại
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Saturday).ToUnixTimeSeconds();
            }
        }

        public long MondayWednesdayFriday()
        {
            DateTimeOffset now = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.GetLongTimeCurrent);
            int day = (int)now.DayOfWeek;
            if (day == (int)DayOfWeek.Monday) // Thứ Hai (1) → lấy Thứ 3 tuần nay
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Tuesday).ToUnixTimeSeconds();
            }
            else if (day == (int)DayOfWeek.Saturday || day == (int)DayOfWeek.Sunday)
            {
                // Nếu hôm nay là t7, chu nhat, lấy thứ 3 tuan sau
                return TimeUtils.GetDateTimeDayOfCustomWeeks(DayOfWeek.Tuesday, 1).ToUnixTimeSeconds();
            }
            else if (day >= (int)DayOfWeek.Thursday) // Thứ 5,6 -> Lấy thứ 7 tuần này
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Saturday).ToUnixTimeSeconds();
            }
            else if (day >= (int)DayOfWeek.Tuesday) // Thứ 3,4 -> Lấy thứ 5 tuần này
            {
                return TimeUtils.GetDateTimeDayOfCurrentWeek(DayOfWeek.Thursday).ToUnixTimeSeconds();
            }
            else
            {
                // Nếu hôm nay là t7, chu nhat, lấy thứ 3 tuan sau
                return TimeUtils.GetDateTimeDayOfCustomWeeks(DayOfWeek.Tuesday, 1).ToUnixTimeSeconds();
            }
        }

        public override void LoadData()
        {
            DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.JourneyToSuccess);
            if (dbEvent == null || dbEvent.eventStatus == EEventStatus.Finish)
            {
                return;
            }
            dataEvent = SaveLoadUtil.LoadDataPrefs<DataJourneyToSuccess>(PATH_JTS);
            if (dataEvent == null)
            {
                dataEvent = new DataJourneyToSuccess();
                dataEvent.dbEvent = new DB_Event();

            }
            DB_Event db_EventLocal = dataEvent.dbEvent;
            if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
            {
                dataEvent = new DataJourneyToSuccess();
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
        }

        private void PushFirebaseShow()
        {
            if (dataEvent == null)
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

        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_JTS);
        }
        public bool IsHasLevelIAP(int level)
        {
            var db = GetDBJourneyResources(level);
            return db.eTypeResources == ETypeResources.IAP;
        }
        public DB_JourneyResources GetDBJourneyResources(int index)
        {
            return dbIAP.FirstOrDefault(item => item.index == index);
        }
        public bool IsHasClaimedFull()
        {
            return dataEvent.level >= dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts.Count;
        }
        public bool IsHasDataFreeDontReceive()
        {
            var gift = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0)
            {
                return false;
            }
            int levelCurrent = dataEvent.level;
            bool isIAP = IsHasLevelIAP(levelCurrent);
            if (isIAP)
            {
                return false;
            }
            if (levelCurrent >= gift.Count)
            {
                return false;
            }
            if (gift[levelCurrent].isClaimed)
            {
                return false;
            }
            return true;
        }
        public bool IsMaxReceive()
        {
            var gift = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0)
            {
                return true;
            }
            int levelCurrent = dataEvent.level;
            if (levelCurrent >= gift.Count)
            {
                return true;
            }
            return false;
        }
        public long TimeContain
        {
            get
            {
                long timeCurrent = TimeUtils.GetLongTimeCurrent;
                long timeContain = dataEvent.dbEvent.timeEvent.TimeEndUnixTime - timeCurrent;
                return timeContain;
            }
        }
        public void ClaimItem(int index)
        {
            var gift = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0 || index >= gift.Count)
            {
                return;
            }
            var giftClone = gift[index];
            giftClone.isClaimed = true;
            gift[index] = giftClone;
            string strType = IsHasLevelIAP(index) ? "iap" : "free";
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,index.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.Type,strType),
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString())

});
#endif
            PushCompleteEvent(index);
            SaveData();
        }
        private void PushCompleteEvent(int index)
        {
            if (dataEvent == null || dataEvent.dbEvent == null || dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts == null)
            {
                return;
            }
            if(index >= dataEvent.MaxLevel - 1)
            {
#if FIREBASE_ENABLE
                FirebaseEvent.LogEvent(KeyFirebase.Event_Completed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,index.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString())
});
#endif
            }
        }
    }
    
    [System.Serializable]
    public class DataJourneyToSuccess
    {
        public DB_Event dbEvent;
        private List<DataCountADS> listCountADS = new List<DataCountADS>();
        public int MaxLevel
        {
            get
            {
                var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
                return dataAll.Count;

            }
        }
        public int level
        {
            get
            {
                var gift = dbEvent.GetRewardType(ETypeResource.None).gifts;
                int length = gift.Count;
                for (int i = 0; i < length; i++)
                {
                    if (!gift[i].isClaimed)
                    {
                        return i;
                    }
                }
                return length;
    
            }
        }
        public void AddDataCountADS(int _index, int amount = 1)
        {
            var data = listCountADS.FirstOrDefault(item => item.index == _index);
            if (data != null)
            {
                data.total += amount;
            }
            else
            {
                listCountADS.Add(new DataCountADS { index = _index, total = amount });
            }

        }
        public int GetCountADS(int index)
        {
            var data = listCountADS.FirstOrDefault(item => item.index == index);
            if(data != null)
            {
                return data.total;
            }
            else
            {
                return 0;
            }
        }
    }
    
    [System.Serializable]
    public struct DB_JourneyResources
    {
        public int index;
        public ETypeResources eTypeResources;
        // IAP
        public DB_Pack dbPack;
        //public IAPProductStats stats;
        // Ads
        public int countAds;
        
    }
    public enum ETypeResources
    {
        None = 0,
        ADS = 1,
        IAP = 2
    }
    [System.Serializable]
    public class DataCountADS
    {
        public int index;
        public int total = 3;
    }
}
