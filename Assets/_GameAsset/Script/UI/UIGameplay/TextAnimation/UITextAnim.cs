using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class UITextAnim : UIBase
{
    public float timeShow = 1f;

    public override void Show(Action onHideDone)
    {
        base.Show(onHideDone);
        DOVirtual.DelayedCall(timeShow, () =>
        {
            Hide();
        }).SetTarget(this);
    }
    private void OnDisable()
    {
        DOTween.Kill(this);
    }
}
