using Luzart;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "DB_BotRacingSO", menuName = "SO/DB_BotRacingSO")]
public class DB_BotRacingSO : ScriptableObject
{
    public DBBotRacing[] dbBotRacings;
    private Dictionary<int, DBBotRacing> dictDBBotRacing = new Dictionary<int, DBBotRacing>();
    public int MaxLevel
    {
        get
        {
            if (dbBotRacings == null)
            {
                return 0;
            }
            return dbBotRacings.Max(a => a.timePassLevel.Count);
        }
    }

    public DBBotRacing GetDBBotRacing(int id)
    {
        if (dictDBBotRacing.Count == 0)
        {
            dictDBBotRacing.Clear();
            dictDBBotRacing.AddListToDictionary(dbBotRacings, e => new KeyValuePair<int, DBBotRacing>(e.id,e));
        }
        return dictDBBotRacing[id];
    }

    public bool IsTimeToCheck(long time)
    {
        ReCheckHashset();
        int timeInt = (int)time;
        return timeHashset.Contains(timeInt);
    }
    private HashSet<int> timeHashset = new HashSet<int>();
    private void ReCheckHashset()
    {
        if(timeHashset!=null && timeHashset.Count > 0)
        {
            return;
        }
        foreach (var botRacing in dbBotRacings)
        {
            foreach (var time in botRacing.timePassLevel)
            {
                timeHashset.Add(time);
            }
        }
    }
    
}

[System.Serializable]
public class DBBotRacing
{
    public int id;
    public List<int> timePassLevel = new List<int>();
}
