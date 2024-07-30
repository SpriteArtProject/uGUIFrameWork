using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component {
    public class FwImage : ViewComponentBase {
        [SerializeField] private Image _image;
        public Image image => _image;

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            data.color.Value = _image.color;

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(data.sprite.Subscribe(SetImage));
            _subscriptions.Add(data.color.Subscribe(color => _image.color = color));
        }

        private void SetImage(Sprite sprite) {
            if (!(_viewData is ViewData data)) return;
            _image.sprite = sprite;
            _image.enabled = sprite != null;
        }

        public class ViewData : ViewDataBase {
            public readonly ColorReactiveProperty color;
            public readonly ReactiveProperty<Sprite> sprite;

            public ViewData(Sprite sprite, bool isVisible = true) : base(isVisible) {
                color = new ColorReactiveProperty();
                this.sprite = new ReactiveProperty<Sprite> {
                    Value = sprite
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                sprite.Value = data.sprite.Value;
                color.Value = data.color.Value;
            }
        }
    }
}