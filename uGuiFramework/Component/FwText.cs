using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component {
    public class FwText : ViewComponentBase {
        [SerializeField] private Text _text;
        public Text text => _text;

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            if (data.isDefaultColor) data.color.Value = _text.color;

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(data.text.Subscribe(SetText));
            _subscriptions.Add(data.color.Subscribe(color => _text.color = color));
        }

        private void SetText(string s) {
            _text.text = s;
        }

        public class ViewData : ViewDataBase {
            public readonly ColorReactiveProperty color;
            public readonly bool isDefaultColor;
            public readonly ReactiveProperty<string> text;

            public ViewData(string text, bool isVisible = true) : base(isVisible) {
                color = new ColorReactiveProperty();
                this.text = new ReactiveProperty<string> {
                    Value = text
                };
                isDefaultColor = true;
            }

            public ViewData(string text, Color color, bool isVisible = true) : base(isVisible) {
                this.color = new ColorReactiveProperty {
                    Value = color
                };
                this.text = new ReactiveProperty<string> {
                    Value = text
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                text.Value = data.text.Value;
                color.Value = data.color.Value;
            }
        }
    }
}