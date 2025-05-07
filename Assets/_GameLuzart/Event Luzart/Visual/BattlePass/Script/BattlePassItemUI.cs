namespace Luzart
{
    using DG.Tweening;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    
    public class BattlePassItemUI : MonoBehaviour
    {
        public ProgressBarUI progress;
        private RectTransform _rectTransform;
        public RectTransform rectTransform
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
        public ItemResBattlePass itemResFree;
        public ItemResBattlePass itemResIAP;
        public TMP_Text txtLineLevel;
    
        public BaseSelect bsWillPass;
        public RectTransform rtLineDown;

        public int index { get; set; } = 0;

        public void InitItemFree(DataResource[] itemRes, EStateClaim state, Action<ItemResBattlePass> action, DataTypeResource chest)
        {
            itemResFree.InitVisual(index, chest,state, action, itemRes);
        }
        public void InitItemIAP(DataResource[] itemRes, EStateClaim state, Action<ItemResBattlePass> action, DataTypeResource chest)
        {
            itemResIAP.InitVisual(index,chest, state, action, itemRes);
        }
        public Tween InitSliderBattlePass(EStatePass eStatePass, int index ,float percent)
        {
            this.index = index;
            txtLineLevel.text = (index+1).ToString();
            SwitchMedal(eStatePass);
            rtLineDown?.gameObject?.SetActive(false);
            return progress.SetSliderTween(percent, percent, 0);

        }
        public void SetSliderOpen(bool isActive)
        {
            rtLineDown?.gameObject?.SetActive(isActive);
        }
        public Tween InitSliderBattlePass(float prePercent, float percent, float timeMove)
        {
            return progress.SetSliderTween(prePercent, percent, timeMove);
        }
        private void SwitchMedal(EStatePass eState)
        {
            bool isPass = eState == EStatePass.Pass;
            bsWillPass.Select(!isPass);
        }
        public Tween SetAnimLineDown(bool isActive,float timeScale)
        {
            float originScale = !isActive ? 1f : 0f;
            float scale = isActive ? 1f : 0f;
            rtLineDown?.gameObject?.SetActive(true);
            rtLineDown.transform.localScale = new Vector3(originScale, 1, 1);

            return rtLineDown.DOScaleX(scale, timeScale);
        }

    }
    public enum EStatePass
    {
        Pass,
        Current,
        WillPass,
    }
}
