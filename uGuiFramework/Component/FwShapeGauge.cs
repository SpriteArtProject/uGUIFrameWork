using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Shapes;
using uGuiFramework.View;
using UniRx;
using UnityEngine;

namespace uGuiFramework.Component {
    public class FwShapeGauge : ViewComponentBase {
        [SerializeField] private Line _line;
        private Vector3 _defaultLineEnd;

        private Vector3 _defaultLineStart;
        public Line line => _line;

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            _defaultLineStart = _line.Start;
            _defaultLineEnd = _line.End;

            ResetSubscriptions();

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(data.value.Subscribe(SetValue));
        }

        private void SetValue(float value) {
            _line.End = _defaultLineStart + _defaultLineEnd * Mathf.Clamp01(value);
        }

        public class ViewData : ViewDataBase {
            public readonly ReactiveProperty<float> value;

            private TweenerCore<float, float, FloatOptions> _tween;

            public ViewData(float value, bool isVisible = true) : base(isVisible) {
                this.value = new ReactiveProperty<float> {
                    Value = value
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                value.Value = data.value.Value;
                _tween?.Kill();
            }

            public void TweenValue(float duration, float toValue, Ease ease = Ease.InOutExpo, float fromValue = -1) {
                value.Value = fromValue < 0 ? value.Value : fromValue;

                _tween?.Kill();
                _tween = null;

                _tween = DOTween.To(() => value.Value, x => value.Value = x, toValue, duration).SetEase(ease);
            }
        }
    }
}