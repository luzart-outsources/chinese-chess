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
    public int typeChess = 0;
    private int typeChestFlash = 0;
    private int valueRate = 0;
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
    public void SetDefaultChess()
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
    public void OnDropDownChanged(int index)
    {
        string value = dropDown.options[index].text;
        valueRate = int.Parse(value);
    }
    public void OnClickCreateRoom()
    {
        GlobalServices.Instance.RequestCreateRoom((EChessType)typeChess, valueRate, typeChestFlash == 0 ? true : false);
    }
}
