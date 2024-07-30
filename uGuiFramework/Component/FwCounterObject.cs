using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component {
    public class FwCounterObject : ViewComponentBase {
        [SerializeField] private bool _isVisibleOffImage;

        [SerializeField] private GameObject _onBase;

        [SerializeField] [ShowIf("_isVisibleOffImage")]
        private GameObject _offBase;

        [SerializeField] private LayoutGroup _layoutGroup;
        private readonly List<GameObject> _offBasePool = new();

        private readonly List<GameObject> _onBasePool = new();

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(data.quantity.Subscribe(_ => SetQuantity()));
            _subscriptions.Add(data.maxQuantity.Subscribe(_ => SetQuantity()));
            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
        }

        private void SetQuantity() {
            var data = _viewData as ViewData;
            var quantity = Mathf.Clamp(data.quantity.Value, 0, data.maxQuantity.Value);
            var maxQuantity = data.maxQuantity.Value;

            void Inactive(GameObject obj) {
                obj.SetActive(false);
            }

            foreach (Transform o in _layoutGroup.transform) Inactive(o.gameObject);

            for (var i = 0; i < quantity; i++) {
                var obj = _onBasePool.FirstOrDefault(activeObj => !activeObj.activeSelf);
                if (obj == null) {
                    obj = Instantiate(_onBase, _layoutGroup.transform);
                    _onBasePool.Add(obj);
                }

                obj.SetActive(true);
                obj.transform.SetAsLastSibling();
            }

            if (!_isVisibleOffImage || _offBase == null) return;

            for (var i = 0; i < maxQuantity - quantity; i++) {
                var obj = _offBasePool.FirstOrDefault(activeObj => !activeObj.activeSelf);
                if (obj == null) {
                    obj = Instantiate(_offBase, _layoutGroup.transform);
                    _offBasePool.Add(obj);
                }

                obj.SetActive(true);
                obj.transform.SetAsLastSibling();
            }
        }

        public class ViewData : ViewDataBase {
            public readonly ReactiveProperty<int> maxQuantity;
            public readonly ReactiveProperty<int> quantity;

            public ViewData(int current, int maxQuantity, bool isVisible = true) : base(isVisible) {
                quantity = new ReactiveProperty<int> {
                    Value = current
                };
                this.maxQuantity = new ReactiveProperty<int> {
                    Value = maxQuantity
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                quantity.Value = data.quantity.Value;
                maxQuantity.Value = data.maxQuantity.Value;
            }
        }
    }
}