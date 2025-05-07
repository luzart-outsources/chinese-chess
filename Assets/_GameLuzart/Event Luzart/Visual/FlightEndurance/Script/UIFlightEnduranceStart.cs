namespace Luzart
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UIFlightEnduranceStart : UIBase
    {
        public TMP_Text txtCount;
        public  List<CanvasGroup> parentSpawns;
        private int count = 0;
        public override void Show(System.Action onHideDone)
        {
            base.Show(onHideDone);
            count = 0;
            closeBtn.gameObject.SetActive(false);
            SpawnAndWaitToText();
        }
    
        public void SpawnAndWaitToText()
        {
            parentSpawns.Shuffle();
            Sequence sq = DOTween.Sequence();
            int length = parentSpawns.Count;
            for (int i = 0; i < length; i++)
            {
                int index = i;
                var item = parentSpawns[index];
                sq.AppendCallback(() => SetItem(item));
                sq.AppendInterval(0.08f);
            }
            sq.AppendInterval(0.8f);
            sq.AppendCallback(() => ShowButtonHide());
        }
        private Tween twPunch;
        private void SetItem(CanvasGroup item)
        {
            item.alpha = 1f;
            var im = item.GetComponent<Image>();
            if(im != null)
            {
                int length = DataWrapperGame.AllSpriteAvatars.Length;
                int ran = Random.Range(0, length);
                im.sprite = DataWrapperGame.AllSpriteAvatars[ran];
            }
            count++;
            txtCount.text = $"{count}";
            twPunch?.Kill(true);
            twPunch = txtCount.transform.DOPunchScale(0.2f * Vector3.one, 0.08f, 1, 0.1f);
        }
        private void ShowButtonHide()
        {
            closeBtn.gameObject.SetActive(true);
        }
    }
}
