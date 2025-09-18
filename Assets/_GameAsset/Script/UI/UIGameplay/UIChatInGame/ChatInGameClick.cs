using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInGameClick : MonoBehaviour
{
    public TMP_Text txtContent;
    public string content;

    public Action<string> actionClick;

    void Start()
    {
        SetText();
    }

    public void Init(Action<string> actionClick)
    {
        this.actionClick = actionClick;
    }

    public void OnClick()
    {
        txtContent.text = content;
    }

    [Button]
    public void SetText()
    {
        txtContent.text = content;
    }

}
