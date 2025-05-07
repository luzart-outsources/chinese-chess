namespace Luzart
{
    using DG.Tweening;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UIReceiveRewardEventTicket : UIBase
    {
        public GameObject fxFirework;
        public ListResUI listResUI;
        public void Initialize(params DataResource[] dataResources)
        {
            int lengthDataRes = dataResources.Length;
            if(listResUI != null) 
            listResUI.InitResUI(dataResources);
            Sequence sq = DOTween.Sequence();
            sq.AppendInterval(0.05f);
            sq.AppendCallback(() => fxFirework.SetActive(true));
            for (int i = 0; i < listResUI.listResUI.Count; i++)
            {
                int index = i;
                listResUI.listResUI[index].gameObject.SetActive(false);
                sq.AppendCallback(() => listResUI.listResUI[index].gameObject.SetActive(true));
                sq.AppendInterval(0.1f);
            }
            sq.OnComplete(() => closeBtn.gameObject.SetActive(true));

        }
        public override void Hide()
        {
            base.Hide();
            UIManager.Instance.RefreshUI();
        }
    }

}