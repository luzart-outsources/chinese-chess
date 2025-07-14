using DG.Tweening;
using UnityEngine;

namespace Eco.TweenAnimation
{
    public class RotationAnimation : IAnimation
    {
        private AnimationFactory _factory;
        private Transform _transform;
        private BaseOptions _options;
        private Vector3Options _customOptions;
        
        public void Initialized(AnimationFactory animationFactory)
        {
            _factory = animationFactory;
            _transform = animationFactory.TweenAnimation.transform;
            _options = _factory.TweenAnimation.BaseOptions;
            _customOptions = _factory.TweenAnimation.Vector3Options;
            if(_customOptions.EndTo == Vector3.one * -1f)
                _customOptions.EndTo = _transform.localRotation.eulerAngles;
        }

        public void SetAnimationFrom()
        {
            _transform.rotation = Quaternion.Euler(_customOptions.From);
        }

        public Tweener Show()
        {
            SetAnimationFrom();
            return _transform
                .DORotate(_customOptions.EndTo, _options.Duration)
                .SetEase(_options.ShowEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }

        public Tweener Hide()
        {
            _transform.rotation = Quaternion.Euler(_customOptions.EndTo);
            return _transform
                .DORotate(_customOptions.From, _options.Duration)
                .SetEase(_options.HideEase)
                .SetUpdate(_options.IgnoreTimeScale)
                .SetDelay(_options.StartDelay);
        }
    }
}
