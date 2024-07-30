using Sirenix.OdinInspector;
using uGuiFramework.View;
using UniRx;
using UnityEngine;

namespace uGuiFramework.Component.Base {
    public abstract class FwInstantiateBase<T> : ViewComponentBase where T : MonoBehaviour {
        [SerializeField] private bool _isDefaultSet;

        [SerializeField] [ShowIf("_isDefaultSet")]
        private T _instantiateObject;

        private T _currentRootObject;
        public T instantiateObject => _instantiateObject;

        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
            _subscriptions.Add(data._rootObject.Subscribe(InstantiateObject));
        }

        private void InstantiateObject(T rootObject) {
            if (!(_viewData is ViewData data)) return;

            if (_currentRootObject == rootObject) return;

            if (_instantiateObject != null) Destroy(_instantiateObject.gameObject);

            if (rootObject == null) return;

            _instantiateObject = Instantiate(rootObject, transform);
            _currentRootObject = rootObject;
            ExtendInstantiateProcess(_instantiateObject);
        }

        protected abstract void ExtendInstantiateProcess(T instantiateObject);

        public class ViewData : ViewDataBase {
            public readonly ReactiveProperty<T> _rootObject;

            public ViewData(T rootObject, bool isVisible = true) : base(isVisible) {
                _rootObject = new ReactiveProperty<T> {
                    Value = rootObject
                };
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                _rootObject.Value = data._rootObject.Value;
            }
        }
    }
}