namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class TicketTallyManager : BaseEventManager
    {
        public DataTicketTally dataEvent;
        private const string PATH_TICKETTALLY = "ticket_tally";
        public override EEventName eEvent => EEventName.TicketTally;

        public readonly int levelSpace = 15;

        [Sirenix.OdinInspector.ShowInInspector]
        public bool IsCacheShowVisual
        {
            get
            {
                if (dataEvent != null)
                {
                    return dataEvent.cacheKey > 0;
                }
                return false;
            }
        }
        [Sirenix.OdinInspector.ShowInInspector]
        public int valueKey { get; set; } = 0;
        private const long timeEventDuration = 60 * 60 * 24 * 3;
        private bool isLoad = false;
        public override TimeEvent GetTimeEvent 
        {
            get
            {
                if (!isLoad)
                {
                    LoadDataPath();
                    isLoad = true;
                }
                TimeEvent timeEvent = new TimeEvent();
                // Đã pass qua level có event thì lấy thời gian event
                if (dataEvent != null && dataEvent.timeStartEvent != -1)
                {
                    long timeCurrent = TimeUtils.GetLongTimeCurrent;
                    if ((timeCurrent > dataEvent.timeEndEvent || dataEvent.isEndEvent) && (DataWrapperGame.CurrentLevel + 1 >= dataEvent.levelEnd + levelSpace))
                    {
                        dataEvent.timeStartEvent = TimeUtils.GetLongTimeStartToday;
                        dataEvent.timeEndEvent = dataEvent.timeStartEvent + timeEventDuration;
                        dataEvent.levelEnd = DataWrapperGame.CurrentLevel+1;
                    }
                    // Chuyển đổi Unix time sang DateTime (UTC)
                    DateTimeOffset currentDate = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.GetLongTimeStartToday);
                    DateTimeOffset fixedDate = DateTimeOffset.FromUnixTimeSeconds(dataEvent.timeStartEvent);

                    // Tính số ngày chênh lệch
                    int dayDifference = (int)(currentDate - fixedDate).TotalDays;

                    int nearestDaysAgo = (dayDifference / 4) * 4;
                    DateTimeOffset resultDate = fixedDate.AddDays(nearestDaysAgo);
                    timeEvent.timeStart = TimeUtils.GetLongTimeStartDay(resultDate.ToUnixTimeSeconds());
                    SaveData();
                }
                else
                {
                    // Chưa pass đến level để được event thì cứ lấy ngày hôm nay để có Event đã
                    timeEvent.timeStart = TimeUtils.GetLongTimeStartToday;
                }
                timeEvent.timeEndPreview = 0;
                timeEvent.timeStartReview = timeEventDuration;
                timeEvent.timeEnd = 0;
                return timeEvent;
            }
        }
        private void LoadDataPath()
        {
            dataEvent = SaveLoadUtil.LoadDataPrefs<DataTicketTally>(PATH_TICKETTALLY);
            if (dataEvent == null)
            {
                dataEvent = new DataTicketTally();
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
            DB_Event dbEvent = EventManager.Instance.GetEvent(EEventName.TicketTally);
            if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
            {
                long timeStart = dataEvent.timeStartEvent;
                long timeEnd = dataEvent.timeEndEvent;

                dataEvent = new DataTicketTally();
                dataEvent.dbEvent = dbEvent;
                dataEvent.totalUse = 0;
                dataEvent.timeStartEvent = timeStart;
                dataEvent.timeEndEvent = timeEnd;
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
        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_TICKETTALLY);
        }
        public override void OnStartGame(int level)
        {
            base.OnStartGame(level);
            valueKey = 0;
        }
        public override void OnCompleteLevelVisual(int level)
        {
            base.OnCompleteLevelVisual(level);
            if (!EventManager.Instance.IsUnlockLevel(EEventName.TicketTally, level))
            {
                return;
            }
            if (dataEvent.isEndEvent)
            {
                SaveData();
                return;
            }
            dataEvent.cacheKey += valueKey;
            dataEvent.levelEnd = level;
            dataEvent.totalUse += valueKey;
            SaveData();
        }
        public override void OnLoseLevelData(int level)
        {
            base.OnLoseLevelData(level);
            valueKey = 0;
        }
        public override void OnCompleteLevelToUnlock(int level)
        {
            base.OnCompleteLevelToUnlock(level);
            if (EventManager.Instance.IsLevelUnlockExactly(EEventName.TicketTally, level + 1))
            {
                UnlockDataInLevel(level);
            }
            else if (EventManager.Instance.IsUnlockLevel(EEventName.TicketTally, level + 1) && dataEvent.timeStartEvent == -1)
            {
                UnlockDataInLevel(level);
            }
        }
        private void UnlockDataInLevel(int level)
        {
            dataEvent.timeStartEvent = TimeUtils.GetLongTimeStartToday;
            dataEvent.timeEndEvent = dataEvent.timeStartEvent + timeEventDuration;
            dataEvent.levelEnd = level;
            dataEvent.dbEvent.timeEvent = GetTimeEvent;
            SaveData();
            EventManager.Instance.SaveAllData();
            EventManager.Instance.Initialize();
        }
        public bool IsHasDataDontClaim()
        {
            var data = dataEvent.DB_GiftEvent.gifts;
            int length = data.Count;
            int levelCurrent = dataEvent.LevelCurrent;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                if (index < levelCurrent)
                {
                    if (!data[index].isClaimed)
                    {
                        return true;
                    }
                }
    
            }
            return false;
        }
        public void UseItem(int level)
        {
            var gifts = dataEvent.DB_GiftEvent;
            var giftClone = gifts.gifts[level];
            giftClone.isClaimed = true;
            gifts.gifts[level] = giftClone;
            if(level == gifts.gifts.Count - 1)
            {
                dataEvent.isEndEvent = true;
            }
            PushFirebaseClaimed(level);
            PushCompleteEvent(level);
            SaveData();
        }
        private void PushFirebaseClaimed(int level)
        {
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,level.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),

});
#endif
        }

        private void PushCompleteEvent(int index)
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
                new Firebase.Analytics.Parameter(TypeFirebase.EventDuration, dataEvent.dbEvent.timeEvent.TimeContainEvent.ToString())
});
#endif
            }
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

        public void OnReceiveData(object data)
        {
            if (!EventManager.Instance.IsHasEvent(eEvent))
            {
                return;
            }
            /*
            var itemColorStats = data.ItemColorStats;
            int index  = colorStats.IndexOf(item=> item.Id == itemColorStats.Id);
            if(index == -1 || index != (int)ColorBlock)
            {
                return;
            }   
            */
            valueKey ++;
        }
      
    }



    [System.Serializable]
    public class DataTicketTally
    {
        public DB_Event dbEvent;
        public int totalUse;
        public long timeStartEvent = -1;
        public long timeEndEvent = -1;
        public bool isEndEvent = false;
        public int levelEnd = -1;
        public int cacheKey = 0;
    
        public int MaxReward()
        {
            return DB_GiftEvent.gifts.Count; 
        }
    
        public int LevelByTotalUse(int totalUse)
        {
            int total = totalUse;
            var data = DB_GiftEvent.gifts;
            int length = data.Count;
            for (int i = 0; i < length; i++)
            {
                total = total - data[i].require;
                if (total < 0)
                {
                    return i;
                }
            }
            return length;
        }
        public DB_GiftEvent DB_GiftEvent
        {
            get
            {
                return dbEvent.allTypeResources[0];
            }
        }
        public GiftEvent GetDBGiftEvent(int level)
        {
            var data = DB_GiftEvent.gifts;
            return data[level];
        }
        public int ContainByTotalUse(int totalUse)
        {
            int total = totalUse;
            var data = DB_GiftEvent.gifts;
            int length = data.Count;
            for (int i = 0; i < length; i++)
            {
                int totalBackup = total;
                total = total - data[i].require;
    
                if (total < 0)
                {
                    total = totalBackup;
                    break;
                }
            }
            return total;
        }
        public int RequireByTotalUse(int totalUse)
        {
            int levelCurrent = LevelByTotalUse(totalUse);
            return RequireByLevel(levelCurrent);
        }
        public int RequireByLevel(int level)
        {
            var dataAll = DB_GiftEvent.gifts;
            level = Mathf.Clamp(level, 0, dataAll.Count-1);
            return dataAll[level].require;
        }
        public int TotalUseByLevel(int level)
        {
            var data = DB_GiftEvent.gifts;
            int length = data.Count;
            int total = 0;
            for (int i = 0; i < length; i++)
            {
                total = total + data[i].require;
                if (i >= level)
                {
                    return total;
                }
            }
            return total;
        }
        public int Require
        {
            get
            {
                int _require = RequireByTotalUse(totalUse);
                return _require;
            }
        }
        public int ContainItem
        {
            get
            {
                int _containItem = ContainByTotalUse(totalUse);
                return _containItem;
            }
        }
        public int LevelCurrent
        {
            get
            {
                int _levelCurrent = LevelByTotalUse(totalUse);
                return _levelCurrent;
            }
        }
        public int MaxLevel
        {
            get
            {
                var dataAll = DB_GiftEvent.gifts;
                return dataAll.Count;
    
            }
        }
        public bool IsMaxLevel
        {
            get
            {
                return LevelCurrent == MaxLevel;
            }
        }
    }
}
