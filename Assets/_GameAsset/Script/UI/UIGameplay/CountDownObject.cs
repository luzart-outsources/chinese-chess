using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CountDownObject : MonoBehaviour
{
    public TMP_Text txtCountDown;
    private Tween countDownTween;
    public void StartCountDown(float start, float end, float duration, Action<int> _onUpdate, Action _onComplete)
    {
        countDownTween = DOVirtual.Int((int)start, (int)end, duration, (value) =>
        {
            _onUpdate?.Invoke(value);
            txtCountDown.text = Mathf.RoundToInt(value).ToString();
        }).OnComplete(() =>
        {
            _onComplete?.Invoke();
        })
        .SetTarget(this);
        ;
    }
    private void OnDisable()
    {
        countDownTween?.Kill();
    }
}
