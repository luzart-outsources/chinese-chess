using Assets._GameAsset.Script.Session;
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
    public GameObject obReady;
    private DataPlayerInRoom dataPlayerInRoom;

    private void OnEnable()
    {
        SetActiveReady(false);
    }
    public void SetActiveReady(bool isReady)
    {
        obReady?.SetActive(isReady);
    }
    public void SetData(DataPlayerInRoom data)
    {
        this.dataPlayerInRoom = data;
        bool isHasData = data != null;
        this.gameObject.SetActive(isHasData);
        if (!isHasData)
        {
            return;
        }
        txtName.text = data.name;
        imAvatar.sprite = ResourcesManager.Instance.GetAvatar(data.avatar);
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
    public void OnClickCheckProfile()
    {
        GlobalServices.Instance.RequestGetInfoUser(dataPlayerInRoom.name);
    }


}
