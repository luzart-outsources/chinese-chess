using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TruckSort.Other
{
    public class ButtonFeedBack : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private Transform _button;
        [SerializeField] private float _moveY = -17f;
        [SerializeField] private float _duration = 2f;
        [SerializeField] private float _loopDelay = 0f;

        public UnityEvent OnClick;

        private bool _isSelected;
        private Sequence _shinySequence;
        

        public void OnPointerUp(PointerEventData eventData)
        {
            _button.DOLocalMove(Vector3.zero, 0.025f).SetEase(Ease.OutQuad);
            if(_isSelected) OnClick?.Invoke();
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _button.DOLocalMove(Vector3.up * (_moveY * 0.69f), 0.025f).SetEase(Ease.OutQuad);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isSelected = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isSelected = false;
        }

        private void OnDisable()
        {
            _button.localPosition = Vector3.zero;
            _isSelected = false;
            _shinySequence?.Kill();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_button == null) _button = transform;
        }
#endif
    }
}