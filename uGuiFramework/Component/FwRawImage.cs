using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component {
    public class FwRawImage : ViewComponentBase {
        [SerializeField] private RawImage _image;
        public RawImage image => _image;

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            data.color.Value = _image.color;

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(data.texture.Subscribe(SetImage));
            _subscriptions.Add(data.color.Subscribe(color => _image.color = color));
        }

        private void SetImage(Texture2D texture) {
            if (!(_viewData is ViewData data)) return;
            _image.texture = texture;
            _image.enabled = texture != null;
        }

        public class ViewData : ViewDataBase {
            public readonly ColorReactiveProperty color;
            public readonly ReactiveProperty<Texture2D> texture;

            public ViewData(Texture2D texture, bool isVisible = true) : base(isVisible) {
                color = new ColorReactiveProperty();
                this.texture = new ReactiveProperty<Texture2D> {
                    Value = texture
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                texture.Value = data.texture.Value;
                color.Value = data.color.Value;
            }
        }
    }
}