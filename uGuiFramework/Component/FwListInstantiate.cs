using System.Collections.Generic;
using Sirenix.Utilities;
using uGuiFramework.View;
using UniRx;
using UnityEngine;

namespace uGuiFramework.Component {
    public class FwListInstantiate : ViewComponentBase {
        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;
            ResetSubscriptions();
            _subscriptions.Add(data.ObjectList.ObserveAdd().Subscribe(_ => CreateObjects()));
            _subscriptions.Add(data.ObjectList.ObserveRemove().Subscribe(_ => CreateObjects()));
            _subscriptions.Add(data.ObjectList.ObserveMove().Subscribe(_ => CreateObjects()));
            _subscriptions.Add(data.ObjectList.ObserveReplace().Subscribe(_ => CreateObjects()));
            _subscriptions.Add(data.ObjectList.ObserveReset().Subscribe(_ => CreateObjects()));
            CreateObjects();
            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
        }

        private void CreateObjects() {
            var data = _viewData as ViewData;
            transform.DestroyAllChild();
            data.ObjectList.ForEach(x => Instantiate(x, transform));
        }

        public class ViewData : ViewDataBase {
            public readonly ReactiveCollection<GameObject> ObjectList;

            public ViewData(IEnumerable<GameObject> baseGameObject, bool isVisible = true) : base(isVisible) {
                ObjectList = new ReactiveCollection<GameObject>(baseGameObject);
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                ObjectList.Clear();
                ObjectList.AddRange(data.ObjectList);
            }
        }
    }
}