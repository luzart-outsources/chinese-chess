using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luzart;

public class UIRacingWin : UIBase
{
    public BaseSelect bsMedal;
    public ListResUI listResUI;
    public void Initialize(int index)
    {
        bsMedal?.Select(index);
        var racingManager = EventManager.Instance.racingManager;
        var dbGiftEvent = racingManager.dataEvent.dbEvent.GetRewardType(ETypeResource.None);
        if (index < dbGiftEvent.gifts.Count)
        {
            var reward = dbGiftEvent.gifts[index].groupGift.dataResources;
            listResUI.InitResUI(reward.ToArray());
        }
    }
}
