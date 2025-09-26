using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gameplay_AvatarFrame : MonoBehaviour
{
    public Image imAvatar;
    public TMP_Text txtName;
    public TMP_Text txtCountDown;
    public ProgressBarUI progressBar;

    public void SetData(DataPlayerInRoom data)
    {
        bool isHasData = data != null;
        this.gameObject.SetActive(isHasData);
        if (!isHasData)
        {
            return;
        }
        txtName.text = data.name;
        int idAvt = int.Parse(data.avatar);
        imAvatar.sprite = ResourcesManager.Instance.avatarResourcesSO.GetSpriteAvatar(idAvt);   
    }
    public void ResetCountDown(long timeRemain)
    {
        progressBar.SetSlider(0, 0, 0);
        string str = GameUtil.LongTimeSecondToUnixTime(timeRemain, true, "", "", "", "");
        SetText(str);
    }
    public void StartCountCountDown(int time, int timeTotal)
    {
        progressBar.SetSlider(1, 0, time, null, (valuePercent) =>
        {
            float invertValuePercent = 1 - valuePercent;
            int value =Mathf.RoundToInt(invertValuePercent * time);
            int timeTotalRemain = timeTotal - value;
            string str = GameUtil.LongTimeSecondToUnixTime(timeTotalRemain,true,"","","","");
            SetText(str);
        });
    }
    private void SetText(string str)
    {
        if (txtCountDown != null)
        {
            txtCountDown.text = str;
        }
    }


}
