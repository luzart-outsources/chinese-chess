using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIProfile : UIBase
{
    public Image imAvt;
    public TMP_Text txtName;
    public TMP_Text txtID;
    public TMP_Text txtGold;

    [Header("Win lose")]
    public List<TxtWinLose> arrTxtWinLose;

    public GameObject obButton;
    public override void Show(System.Action onHideDone)
    {
        base.Show(onHideDone);
    }
    public void InitData(DataUser dataUser)
    {
        imAvt.sprite = ResourcesManager.Instance.GetAvatar(dataUser.avt);
        txtName.text = dataUser.name;
        txtID.text = dataUser.idPlayer.ToString();
        txtGold.text = GameUtil.FormatNumber((int)dataUser.gold);
        for (int i = 0; i < dataUser.dataHistoryGame.Count; i++)
        {
            var data = dataUser.dataHistoryGame[i];
            arrTxtWinLose[i].txtWin.text = data.win.ToString();
            arrTxtWinLose[i].txtLose.text = data.lose.ToString();
        }
        bool isShowObButton = dataUser.name != DataManager.Instance.DataUser.name;
        obButton.SetActive(isShowObButton);
    }
    public void OnClickAddFriend()
    {

    }
    public void OnClickMess()
    {

    }
}
[System.Serializable]
public class TxtWinLose
{
    public TMP_Text txtWin;
    public TMP_Text txtLose;
    public EChessType eChessType;
}