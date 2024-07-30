using System;
using System.Collections.Generic;
using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace uGuiFramework.Component {
    [RequireComponent(typeof(CanvasRenderer))]
    public class FwButton : MyButton, IViewComponent, IViewParts {
        public string _description;

        private List<IDisposable> _subscriptions;

        private ViewData _viewData;

        protected override void OnDestroy() {
            base.OnDestroy();
            Dispose();
        }

        public T GetViewData<T>() where T : class, IViewData {
            return _viewData as T;
        }

        public GameObject GetGameObject() {
            return gameObject;
        }

        public void Set(IViewData viewData) {
            _viewData = viewData as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(_viewData.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));

            onClick.RemoveAllListeners();
            onClickWhenInactive.RemoveAllListeners();
            onLongClick.RemoveAllListeners();

            if (_viewData.onClick != null) onClick.AddListener(_viewData.onClick);
            if (_viewData.onClickInactive != null) onClickWhenInactive.AddListener(_viewData.onClickInactive);
            if (_viewData.onLongClick != null) onLongClick.AddListener(_viewData.onLongClick);


            var buttonEnable = _viewData.onClick != null || _viewData.onClickInactive != null || _viewData.onLongClick != null;
            enabled = buttonEnable;
        }


        public string objName => gameObject.name.Replace(" ", "");

        public string description => _description;

        private void ResetSubscriptions() {
            _subscriptions?.ForEach(_ => _.Dispose());
            _subscriptions = new List<IDisposable>();
        }


        public void Dispose() {
            _subscriptions?.ForEach(_ => _.Dispose());
            _subscriptions?.Clear();
        }

        public class ViewData : ViewDataBase {
            public ViewData(UnityAction onClick = null, UnityAction onClickInactive = null, UnityAction onLongClick = null, bool isVisible = true) : base(isVisible) {
                this.onClick = onClick;
                this.onClickInactive = onClickInactive;
                this.onLongClick = onLongClick;
            }

            public UnityAction onClick { get; private set; }
            public UnityAction onClickInactive { get; private set; }
            public UnityAction onLongClick { get; private set; }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;

                onClick = data.onClick;
                onClickInactive = data.onClickInactive;
                onLongClick = data.onLongClick;
            }
        }
    }
}