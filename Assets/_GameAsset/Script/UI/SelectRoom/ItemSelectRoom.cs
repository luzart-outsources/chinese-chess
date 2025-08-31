using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSelectRoom : MonoBehaviour
{
    public TMP_Text txtName, txtGold;
    public BaseSelect selectIm, selectPeople;
    public ConfigDataRoom data;
    public void InitData(ConfigDataRoom _data)
    {
        this.data = _data;
        txtName.text = data.name;
        txtGold.text = data.gold.ToString();
        selectIm.Select(data.index % 2 == 0);
        selectPeople.Select(data.isFilled);
    }
}


