using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSeeChess : MonoBehaviour
{
    [Header("Player 1")]
    public Image icon1;
    public TMP_Text txtName1;
    [Header("Player 2")]
    public Image icon2;
    public TMP_Text txtName2;
    public TMP_Text txtViewer;
    public TMP_Text txtGold;
    public Action<DataRoom> actionClick;
    [SerializeField]
    private DataRoom dataRoom;

    public void InitData(DataRoom dataRoom, Action<DataRoom> actionClick)
    {
        this.dataRoom = dataRoom;
        icon1.sprite = ResourcesManager.Instance.GetAvatar(dataRoom.dataMe.avatar);
        icon2.sprite = ResourcesManager.Instance.GetAvatar(dataRoom.dataMember2.avatar);
        txtName1.text = dataRoom.dataMe.name;
        txtName2.text = dataRoom.dataMember2.name;
        txtViewer.text = GameUtil.FormatNumber(dataRoom.viewer);
        txtGold.text = GameUtil.FormatNumber(dataRoom.goldRate);
        this.actionClick = actionClick;
    }
    public void OnClickItem()
    {
        this.actionClick?.Invoke(this.dataRoom);
    }
    
}
