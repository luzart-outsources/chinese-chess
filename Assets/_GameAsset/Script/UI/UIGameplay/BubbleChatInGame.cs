using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BubbleChatInGame : MonoBehaviour
{
    public BaseSelect bsIcon;
    public Image imIcon;
    public BubbleAutoResize bubbleAutoResize;
    private void Awake()
    {
        gameObject.SetActive(false);
    }
    public void SetText(string s)
    {
        Sprite sp = ResourcesManager.Instance.GetIconByString(s);
        bool isIcon = sp != null;
        bsIcon.Select(isIcon);
        if (isIcon)
        {
            imIcon.sprite = sp;
        }
        else
        {
            bubbleAutoResize.SetText(s);
        }

    }
    private Tween twDelay;
    public void ShowBubble(string s)
    {
        SetText(s);
        gameObject.SetActive(true);
        twDelay?.Kill(true);
        twDelay = DOVirtual.DelayedCall(3f, () =>
        {
            gameObject.SetActive(false);
        }).SetTarget(this);
    }
    private void OnDisable()
    {
        twDelay?.Kill();
    }
}
