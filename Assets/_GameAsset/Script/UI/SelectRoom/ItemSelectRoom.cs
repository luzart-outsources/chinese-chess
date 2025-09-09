using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ItemSelectRoom : MonoBehaviour
{
    public TMP_Text txtName, txtGold;
    public BaseSelect selectIm, selectPeople;
    public ConfigDataRoom data;
    public Action<ConfigDataRoom> actionConfigDataRoom;
    public void InitData(ConfigDataRoom _data, Action<ConfigDataRoom> _actionConfigDataRoom)
    {
        this.data = _data;
        txtName.text = data.name;
        txtGold.text = data.gold.ToString();
        selectIm.Select(data.index % 2 == 0);
        selectPeople.Select(data.isFilled);
        this.actionConfigDataRoom = _actionConfigDataRoom;
    }
    public void OnclickRoom()
    {
        this.actionConfigDataRoom?.Invoke(this.data);
    }
}


