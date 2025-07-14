using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BottomMainMenu : MonoBehaviour
{
    public float timeMove = 0.25f;
    public RectTransform rtLighting;
    public RectTransform[] icons;
    public Button[] btns;
    public Action[] actionClicks;

    private int indexCache = 1;

    private void Start()
    {
        int length = btns.Length;
        for (int i = 0; i < length; i++)
        {
            int index = i;
            GameUtil.ButtonOnClick(btns[index], ()=> Click(index));
        }
    }
    public void InitBottom(Action[] action)
    {
        actionClicks = new Action[action.Length];
        int length = action.Length;
        for (int i = 0; i < length; i++)
        {
            this.actionClicks[i] = action[i];
        }
    }
    private void Click(int index)
    {
        if(indexCache == index)
        {
            return;
        }
        actionClicks[index]?.Invoke();
        MoveLighting(index);
    }
    private Sequence sq;
    private void MoveLighting(int index)
    {
        Vector2 originRtLightingMin = rtLighting.anchorMin;
        Vector2 originRtLightingMax = rtLighting.anchorMax;
        Vector2 targetRtLightingMin = new Vector2((float)index/(float)3, 0);
        Vector2 targetRtLightingMax = new Vector2((float)(index+1) / (float)3, 1);
        Vector2 offsetMin = rtLighting.offsetMin;
        Vector2 offsetMax = rtLighting.offsetMax;

        sq?.Kill(true);
        sq = DOTween.Sequence();
        sq.Insert(0, DOVirtual.Vector3(originRtLightingMin, targetRtLightingMin, timeMove, (vt) =>
        {
            rtLighting.anchorMin = vt;
            rtLighting.offsetMin = new Vector2(0, offsetMin.y);
            rtLighting.offsetMax = new Vector2(0, offsetMax.y);
        }));
        sq.Insert(0, DOVirtual.Vector3(originRtLightingMax, targetRtLightingMax, timeMove, (vt) =>
        {
            rtLighting.anchorMax = vt;
        }));
        sq.InsertCallback(0, () =>
        {
            OnSelectIcon(index);
        });
        sq.OnComplete(() =>
        {
            rtLighting.anchorMin = targetRtLightingMin;
            rtLighting.anchorMax = targetRtLightingMax;
            rtLighting.offsetMin = new Vector2(0, offsetMin.y);
            rtLighting.offsetMax = new Vector2(0, offsetMax.y);
            icons[indexCache].anchoredPosition3D = new Vector3(0, 0, 0);
            icons[indexCache].localScale = new Vector3(1, 1, 1);
            icons[index].anchoredPosition3D = new Vector3(0, 42, 0);
            icons[index].localScale = new Vector3(1.25f, 1.25f, 1.25f);
            indexCache = index;
        });

    }
    private void OnSelectIcon(int index)
    {
        icons[indexCache].DOAnchorPos3D(new Vector3(0, 0, 0), timeMove);
        icons[indexCache].DOScale(new Vector3(1, 1, 1), timeMove);
        icons[index].DOAnchorPos3D(new Vector3(0, 60, 0), timeMove).SetEase(Ease.InBack);
        icons[index].DOScale(new Vector3(1.1f, 1.1f, 1.1f), timeMove).SetEase(Ease.InBack);

    }
}
