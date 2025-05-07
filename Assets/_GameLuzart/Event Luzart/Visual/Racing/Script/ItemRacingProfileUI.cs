using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemRacingProfileUI : MonoBehaviour
{
    public TMP_Text txtName;
    public void SetVisual(string strName)
    {
        txtName.text = strName;
    }
}
