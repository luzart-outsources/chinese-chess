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
    public Image imIcon;
    public string content;

    public Action<string> actionClick;

    void Start()
    {
        SetText();
        SetIcon();
    }

    public void Init(Action<string> actionClick)
    {
        this.actionClick = actionClick;
    }

    public void OnClick()
    {
        actionClick?.Invoke(content);
    }

    [Button]
    public void SetText()
    {
        if(txtContent!= null)
            txtContent.text = content;
    }

    [Button]
    public void SetIcon()
    {
        if(imIcon != null)
        {
            imIcon.sprite = ResourcesManager.Instance.GetIconByString(content);
        }
    }

}
