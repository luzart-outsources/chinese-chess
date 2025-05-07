using DG.Tweening;
using Luzart;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCarRacingUI : MonoBehaviour
{
    public int indexRace;
    public RectTransform rectRace;
    private int totalRace = 5;
    public BaseSelect bsMedal;

    private RectTransform _rectTransform = null;
    private RectTransform rectTransform
    {
        get
        {
            if(_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }
    private void Start()
    {
        totalRace = EventManager.Instance.racingManager.MaxStep;
    }

    private float PosY(int step, int totalRace)
    {
        return rectRace.rect.height / totalRace * step;
    }
    public Vector2 PosVisual(int step, int totalRace)
    {
        Vector2 pos = rectTransform.anchoredPosition;
        return new Vector2(pos.x, PosY(step, totalRace));
    }
    public void SetVisual(int step)
    {
        rectTransform.anchoredPosition = PosVisual(step, totalRace);
    }
    public Tween SetVisualMove(int preStep, int curStep, float timeMove)
    {
        Vector2 prePos = PosVisual(preStep, totalRace);
        Vector2 curPos = PosVisual(curStep, totalRace);
        Sequence sq = DOTween.Sequence();
        sq.AppendCallback(()=> rectTransform.anchoredPosition = prePos);
        sq.Append(rectTransform.DOAnchorPos(curPos, timeMove));
        return sq;
    }
    public void SetMedal(int index)
    {
        bsMedal?.Select(index);
    }
}
