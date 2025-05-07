using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemBeMoveRise : MonoBehaviour
{
    private RectTransform _rectTransform = null;
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    public TMP_Text txt;
    public void Initialize(Vector3 pos, string str)
    {
        _rectTransform.anchoredPosition = pos;
        txt.text = str;
    }

}
