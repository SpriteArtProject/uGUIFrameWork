using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace uGuiFramework.Component.Base {
    public abstract class FwSimpleListCellViewBase<T> : EnhancedScrollerCellView where T : ICellData {
        [ValidateInput("Validate")] [SerializeField] [SceneObjectsOnly]
        private GameObject _target;

        protected T _data;
        private RectTransform _rectTransform;
        protected List<IDisposable> _subscriptions;

        private IViewParts _viewParts;

        public Vector2 size {
            get {
                _rectTransform ??= GetComponent<RectTransform>();
                return _rectTransform.sizeDelta;
            }
        }

        private void OnDestroy() {
            Dispose();
        }

        [Button]
        private void GetTarget() {
            _target = GetComponentsInChildren<IViewParts>().FirstOrDefault(comp => comp.GetGameObject() != gameObject)?.GetGameObject();
        }

        public void SetData(T data) {
            _viewParts ??= _target.GetComponent<IViewParts>();

            if (_viewParts == null) {
                Debug.LogError("FwScrollerCellView : リストの要素に使用するViewが存在しません");
                return;
            }

            _data = data;

            _viewParts.Set(data.viewData);
            SetDataExtend(data);
        }

        public void SetActive(bool isActive) {
            gameObject.SetActive(isActive);
        }

        protected virtual void SetDataExtend(T data) {
        }

        private bool Validate(GameObject target, ref string message) {
            var obj = target != null ? target.GetComponent<IViewParts>() : null;
            if (obj != null) return true;
            message = "対象のViewが存在しません";
            return false;
        }

        public override void RefreshCellView() {
            SetData(_data);
        }

        protected void ResetSubscriptions() {
            _subscriptions?.ForEach(_ => _.Dispose());
            _subscriptions = new List<IDisposable>();
        }


        public void Dispose() {
            _subscriptions?.ForEach(_ => _.Dispose());
            _subscriptions?.Clear();
        }
    }

    public interface ICellData {
        public IViewData viewData { get; }
        public int index { get; }
    }

    public class CellData : ICellData {
        public CellData(IViewData viewData, int index) {
            this.viewData = viewData;
            this.index = index;
        }

        public IViewData viewData { get; }

        public int index { get; }
    }

    /// <summary>
    ///     明示的なIView継承クラス指定用
    /// </summary>
    public class CellData<T> : CellData where T : class, IViewData {
        public CellData(T viewData, int index) : base(viewData, index) {
        }

        public new T viewData => base.viewData as T;
    }

    public class SelectableCellData : ICellData {
        public readonly ReactiveProperty<bool> isSelect;
        public readonly UnityAction<SelectableCellData> onClick;
        public readonly UnityAction<SelectableCellData> onLongClick;

        public SelectableCellData(IViewData viewData, int index, UnityAction<SelectableCellData> onClick = null, bool isSelect = false, UnityAction<SelectableCellData> onLongClick = null) {
            this.viewData = viewData;
            this.index = index;
            this.onClick = onClick ?? DefaultOnClick;
            this.onLongClick = onLongClick ?? DefaultOnLongClick;
            this.isSelect = new ReactiveProperty<bool> {
                Value = isSelect
            };
        }

        public virtual IViewData viewData { get; }

        public int index { get; }

        private void DefaultOnClick(SelectableCellData selectableCellData) {
            selectableCellData.isSelect.Value = !selectableCellData.isSelect.Value;
        }

        private void DefaultOnLongClick(SelectableCellData selectableCellData) {
        }
    }

    /// <summary>
    ///     明示的なIView継承クラス指定用
    /// </summary>
    public class SelectableCellData<T> : SelectableCellData where T : class, IViewData {
        public SelectableCellData(T viewData, int index, UnityAction<SelectableCellData<T>> onClick = null, bool isSelect = false, UnityAction<SelectableCellData<T>> onLongClick = null) : base(viewData, index, OverrideOnClick(onClick), isSelect, OverrideOnClick(onLongClick)) {
        }

        public new T viewData => base.viewData as T;

        // 継承先からの型変換
        private static UnityAction<SelectableCellData> OverrideOnClick(UnityAction<SelectableCellData<T>> action) {
            return data => action?.Invoke((SelectableCellData<T>)data);
        }
    }
}