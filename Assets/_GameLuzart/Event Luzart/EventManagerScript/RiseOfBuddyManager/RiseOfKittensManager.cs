namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class RiseOfKittensManager : BaseEventManager
    {
        public override EEventName eEvent => EEventName.RiseOfKittens;

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
                        dataEvent.levelEnd = DataWrapperGame.CurrentLevel + 1;
                    }
                    // Chuyển đổi Unix time sang DateTime (UTC)
                    DateTimeOffset currentDate = DateTimeOffset.FromUnixTimeSeconds(TimeUtils.GetLongTimeStartToday);
                    DateTimeOffset fixedDate = DateTimeOffset.FromUnixTimeSeconds(dataEvent.timeStartEvent);

                    // Tính số ngày chênh lệch
                    int dayDifference = (int)(currentDate - fixedDate).TotalDays;

                    int nearestDaysAgo = (dayDifference / 17) * 17;
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
        public DataRiseOfKittens dataEvent;
        private const string PATH_RISE_OF_KITTENS = "rise_of_kittens";
        private const long timeEventDuration = 60 * 60 * 24 * 2;
        public readonly int levelSpace = 15;
        private bool isLoad = false;
        private void LoadDataPath()
        {
            dataEvent = SaveLoadUtil.LoadDataPrefs<DataRiseOfKittens>(PATH_RISE_OF_KITTENS);
            if (dataEvent == null)
            {
                dataEvent = new DataRiseOfKittens();
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
            DB_Event dbEvent = EventManager.Instance.GetEvent(eEvent);
            if (string.IsNullOrEmpty(db_EventLocal.idEvent) || !db_EventLocal.idEvent.Equals(dbEvent.idEvent))
            {
                long timeStart = dataEvent.timeStartEvent;
                long timeEnd = dataEvent.timeEndEvent;

                dataEvent = new DataRiseOfKittens();
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
            SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_RISE_OF_KITTENS);
        }
    
        public bool IsHasDataDontClaim(int level)
        {
            var data = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
            int length = data.Count;
            int levelCurrent = dataEvent.IndexByTotalUse(dataEvent.totalUse);
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
        public int GetLevelReturnDontClaim
        {
            get
            {
                var data = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
                int length = data.Count;
                int levelCurrent = dataEvent.LevelCurrent;
                for (int i = 0; i < length; i++)
                {
                    int index = i;
                    if (index < levelCurrent)
                    {
                        if (!data[index].isClaimed)
                        {
                            return index;
                        }
                    }
                }
                return -1;
            }
        }
        public bool IsMaxReceive()
        {
            var gift = dataEvent.dbEvent.GetRewardType(ETypeResource.None).gifts;
            if (gift == null || gift.Count == 0)
            {
                return true;
            }
            int levelCurrent = dataEvent.LevelCurrent;
            if (levelCurrent >= gift.Count)
            {
                return true;
            }
            return false;
        }
        public void UseItem(int level)
        {
            var gifts = dataEvent.dbEvent.GetRewardType(ETypeResource.None);
            var giftClone = gifts.gifts[level];
            giftClone.isClaimed = true;
            gifts.gifts[level] = giftClone;
            SaveData();
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
{
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,level.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),

});
#endif
            PushCompleteEvent(level);
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

        public override void OnCompleteLevelData(int level)
        {
            base.OnCompleteLevelData(level);
            if (!EventManager.Instance.IsHasEvent(EEventName.RiseOfKittens) || !EventManager.Instance.IsUnlockLevel(EEventName.RiseOfKittens))
            {
                return;
            }
            dataEvent.totalUse++;
            SaveData();
        }
        public override void OnLoseLevelData(int level)
        {
            base.OnLoseLevelData(level);
            if(!EventManager.Instance.IsHasEvent(EEventName.RiseOfKittens) || !EventManager.Instance.IsUnlockLevel(EEventName.RiseOfKittens))
            {
                return;
            }
            dataEvent.totalUse = 0;
            //int contain = dataRiseOfKittens.ContainItem;
            //dataRiseOfKittens.totalUse -= contain;
            SaveData();
        }

        public override void OnCompleteLevelToUnlock(int level)
        {
            base.OnCompleteLevelToUnlock(level);
            if (EventManager.Instance.IsLevelUnlockExactly(eEvent, level + 1))
            {
                UnlockDataInLevel(level);
            }
            else if (EventManager.Instance.IsUnlockLevel(eEvent, level + 1) && dataEvent.timeStartEvent == -1)
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
    public class DataRiseOfKittens
    {
        public DB_Event dbEvent;
        public int totalUse;
        public long timeStartEvent = -1;
        public long timeEndEvent = -1;
        public int levelEnd = -1;
        public int totalUseCacheUI = 0;
        public int totalUseCacheHome = 0;
        public bool isEndEvent = false;

        public int IndexByTotalUse(int totalUse)
        {
            int total = totalUse;
            var data = dbEvent.GetRewardType(ETypeResource.None).gifts;
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
        public int ContainByTotalUse(int totalUse)
        {
            int total = totalUse;
            var data = dbEvent.GetRewardType(ETypeResource.None).gifts;
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
            int levelCurrent = IndexByTotalUse(totalUse);
            return RequireByIndex(levelCurrent);
        }
        public int RequireByIndex(int index)
        {
            var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
            index = Mathf.Clamp(index, 0, dataAll.Count - 1);
            return dataAll[index].require;
        }
        public int TotalRequireByIndex(int level)
        {
            int total = 0;
            for (int i = 0; i <= level; i++)
            {
                total += RequireByIndex(i);
            }
            return total;   
        }
    
        public int Require
        {
            get
            {
                return RequireByTotalUse(totalUse);
            }
        }
    
        public int ContainItem
        {
            get
            {
                return ContainByTotalUse(totalUse);
            }
        }
        public int LevelCurrent
        {
            get
            {
                return IndexByTotalUse(totalUse);
            }
        }
        public int MaxLevel
        {
            get
            {
                var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
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
        public int TotalRequireMaxLevel
        {
            get
            {
                return TotalRequireByIndex(MaxLevel - 1);
            }
        }
        public bool IsLevelRequire(int totalUse, out int index)
        {
            index = IndexByTotalUse(totalUse - 1);
            int totalUseFind = TotalRequireByIndex(index);
            return totalUse == totalUseFind;
        }
    }
}
