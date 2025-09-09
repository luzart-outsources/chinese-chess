using Assets._GameAsset.Script.Session;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICreateRoom : UIBase
{
    public BaseSelect bsTypeChess;
    public BaseSelect bsTypeChestFlash;

    public TMP_Dropdown dropDown;
    public Button btnCreateRoom;
    private int[] valuesRate;
    private int typeChess = 0;
    private int typeChestFlash = 0;

    protected override void Setup()
    {
        base.Setup();
        GameUtil.ButtonOnClick(btnCreateRoom, OnClickCreateRoom);
    }
    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        valuesRate = DataManager.Instance.GetValuesRate();
        List<string> listOption = new List<string>();
        dropDown.ClearOptions();
        for (int i = 0; i < valuesRate.Length; i++)
        {
            listOption.Add(valuesRate[i].ToString());
        }
        dropDown.AddOptions(listOption);
        UpdateInteractableButtonCreateRoom();
        SetDefaultChess();
    }
    private void SetDefaultChess()
    {
        bsTypeChess.Select(typeChess);
        OnClickChangeTypeChess(typeChess);
        bsTypeChestFlash.Select(typeChestFlash);
        OnClickChangeTypeChestFlash(typeChestFlash);
    }
    public void OnClickChangeTypeChess(int index)
    {
        typeChess = index;
        UpdateInteractableButtonCreateRoom();
    }
    public void OnClickChangeTypeChestFlash(int index)
    {
        typeChestFlash = index;
        UpdateInteractableButtonCreateRoom();
    }
    public void UpdateInteractableButtonCreateRoom()
    {
        bool isCreateRoom = typeChess != -1 && typeChestFlash != -1;
        btnCreateRoom.interactable = isCreateRoom;
    }

    public void OnClickCreateRoom()
    {
        GlobalServices.Instance.RequestCreateRoom((EChessType)typeChess, 10000 ,typeChestFlash == 0? true : false);
    }
}
