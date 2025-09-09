using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIGameplay : UIBase
{
    public Gameplay_AvatarFrame frameMe;
    public Gameplay_AvatarFrame frameEnemy;
    public TMP_Text txtGold;
    public TMP_Text txtIDRoom;
    public override void Show(System.Action onHideDone)
    {
        base.Show(onHideDone);
    }
    public void InitData(DataRoom dataRoom)
    {
        txtIDRoom.text = "ID: " + dataRoom.idRoom;
        txtGold.text = dataRoom.goldRate.ToString();
        frameMe.SetData(dataRoom.dataMe);
        frameEnemy.SetData(dataRoom.dataMember2);
    }

}
