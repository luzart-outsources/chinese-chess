namespace Luzart
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    public class BattlePassManager : BaseEventManager
    {
        private const string PATH_BATTLEPASS = "battle_pass";
        public DataBattlePass dataEvent;
        public DifficultyKey[] diffKey = new DifficultyKey[3];
        private Dictionary<Difficulty, int> _dictDiffKey = null;
        public Dictionary<Difficulty, int> dictDiffKey
        {
            get
            {
                if(_dictDiffKey == null)
                {
                    _dictDiffKey = new Dictionary<Difficulty, int>();
                    int length = diffKey.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var item = diffKey[i];
                        _dictDiffKey.Add(item.difficulty, item.amountKey);
                    }
                }
                return _dictDiffKey;
            }
        }

        private const long timeEventDuration = 60 * 60 * 24 * 14;
        private bool isLoad = false;

        public override EEventName eEvent => EEventName.BattlePass;
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
                    if ((timeCurrent > dataEvent.timeEndEvent))
                    {
                        dataEvent.timeStartEvent = TimeUtils.GetLongTimeStartToday;
                        dataEvent.timeEndEvent = dataEvent.timeStartEvent + timeEventDuration;
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
            dataEvent = SaveLoadUtil.LoadDataPrefs<DataBattlePass>(PATH_BATTLEPASS);
            if (dataEvent == null)
            {
                dataEvent = new DataBattlePass();
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

                dataEvent = new DataBattlePass();
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
                        if(i < db_EventLocal.allTypeResources.Length && j < db_EventLocal.allTypeResources[i].gifts.Count)
                        {
                            gift.isClaimed = db_EventLocal.allTypeResources[i].gifts[j].isClaimed;
                        }
                        db.gifts.Add(gift);
                    }
                    list.Add(db);
                }
                db_EventLocal.allTypeResources = list.ToArray();


            }

        }
        public override void SaveData()
        {
            SaveLoadUtil.SaveDataPrefs(dataEvent, PATH_BATTLEPASS);
        }
        public override void OnCompleteLevelData(int level)
        {
            base.OnCompleteLevelData(level);
            if (!EventManager.Instance.IsHasEvent(EEventName.BattlePass) || !EventManager.Instance.IsUnlockLevel(EEventName.BattlePass, DataWrapperGame.CurrentLevel))
            {
                return;
            }
            Difficulty diff = Difficulty.Normal;
            int key = dictDiffKey[diff];
            dataEvent.totalUse += key;
            dataEvent.cacheKeyHome += key;
            dataEvent.cacheKeyUI += key;
            SaveData();
        }
        public int GetKey(Difficulty diff)
        {
            int key = dictDiffKey[diff];
            return key;
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
            dataEvent.dbEvent.timeEvent = GetTimeEvent;
            SaveData();
            EventManager.Instance.SaveAllData();
            EventManager.Instance.Initialize();
        }
        public void UseItemNormal(int level)
        {
            var gifts = dataEvent.dbEvent.GetRewardType(ETypeResource.None);
            var giftClone = gifts.gifts[level] ;
            giftClone.isClaimed = true;
            gifts.gifts[level] = giftClone;
            PushFirebaseClaimed(level, ETypeResource.None);
            SaveData() ;
        }
        public void UseItemVIP(int level)
        {
            var gifts = dataEvent.dbEvent.GetRewardType(ETypeResource.VIP);
            var giftClone = gifts.gifts[level];
            giftClone.isClaimed = true;
            gifts.gifts[level] = giftClone;
            PushFirebaseClaimed(level,ETypeResource.VIP);
            SaveData();
    
        }
        private void PushFirebaseClaimed(int level, ETypeResource type)
        {
            string strType = "free";
            if (type == ETypeResource.VIP)
            {
                strType = "pay";
            }
#if FIREBASE_ENABLE
            FirebaseEvent.LogEvent(KeyFirebase.Event_Claimed, new Firebase.Analytics.Parameter[]
            {
                new Firebase.Analytics.Parameter(TypeFirebase.EventName,eEvent.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelID,UserLevel.GetCurrentLevel().ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.LevelReward,level.ToString()),
                new Firebase.Analytics.Parameter(TypeFirebase.EventType, strType),
            });
#endif
            PushCompleteEvent(level, strType);
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
                new Firebase.Analytics.Parameter(TypeFirebase.EventType, strType),
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

        public bool IsHasDataDontClaim()
        {
            bool isNormal = IsHasDataDontClaim(ETypeResource.None);
            bool isVIP = false;
            if (dataEvent.isBuyIAP)
            {
                isVIP = IsHasDataDontClaim(ETypeResource.VIP);
            }
            return (isNormal || isVIP);
        }
        private bool IsHasDataDontClaim(ETypeResource eType)
        {
            return CountDataDontClaim(eType) > 0;
        }
        public int CountDataDontClaim()
        {
            return CountDataDontClaim(ETypeResource.None, dataEvent.totalUse) ;
        }
        public int CounDataDontClaim(int total)
        {
            int value = CountDataDontClaim(ETypeResource.None, total);
            int valueVIP = 0;
            if (dataEvent.isBuyIAP)
            {
                valueVIP = CountDataDontClaim(ETypeResource.VIP, total);
            }
    
            return value + valueVIP;
        }
        public int CountDataDontClaim(ETypeResource eType)
        {
            return CountDataDontClaim(eType, dataEvent.totalUse);
        }
        public int CountDataDontClaim(ETypeResource eType, int total)
        {
            int value = 0;
            var data = dataEvent.dbEvent.GetRewardType(eType).gifts;
            int length = data.Count;
            int levelCurrent = dataEvent.LevelByTotalUse(total);
            for (int i = 0; i < length; i++)
            {
                int index = i;
                if (index < levelCurrent)
                {
                    if (!data[index].isClaimed)
                    {
                        value++;
                    }
                }
    
            }
            return value;
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
    
        [System.Serializable]
        public struct DifficultyKey
        {
            public Difficulty difficulty;
            public int amountKey;
        }
    }
    [System.Serializable]
    public class DataBattlePass
    {
        public DB_Event dbEvent;
        public bool isBuyIAP;
        public int totalUse;
        public int cacheKeyHome;
        public int cacheKeyUI;

        public long timeStartEvent = -1;
        public long timeEndEvent = -1;

    
        public int LevelByTotalUse(int totalUse)
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
            int levelCurrent = LevelByTotalUse(totalUse);
            return RequireByLevel(levelCurrent);
        }
        public int RequireByLevel(int level)
        {
            var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
            level = Mathf.Clamp(level, 0, dataAll.Count - 1);
            return dataAll[level].require;
        }
        public int MaxLevel
        {
            get
            {
                var dataAll = dbEvent.GetRewardType(ETypeResource.None).gifts;
                return dataAll.Count;
    
            }
        }
        public void CalculateProgress(int totalUse, out int levelCurrent, out int contain, out int totalRequire, out float percent)
        {
            levelCurrent = LevelByTotalUse(totalUse);
            contain = ContainByTotalUse(totalUse);
            totalRequire = RequireByLevel(levelCurrent);
            percent = (float)contain/ totalRequire;
        }


    }
}
