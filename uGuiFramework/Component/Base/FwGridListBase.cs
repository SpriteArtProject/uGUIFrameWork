using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component.Base {
    public class FwGridListBase<T, U> : ViewComponentBase, IEnhancedScrollerDelegate where T : ICellData where U : FwSimpleListCellViewBase<T> {
        [PropertyOrder(1)] [SerializeField] protected EnhancedScroller _scroller;

        [PropertySpace(16)] [ValidateInput("Validate")] [PropertyOrder(3)] [SerializeField]
        private EnhancedScrollerCellView _rowCellView;

        [SerializeField] [DisplayAsString] private int _cellRowDisplay;

        private int _numberOfCellsPerRow = 3;

        private void Awake() {
            _numberOfCellsPerRow = ((FwGridListCellViewBase<U, T>)_rowCellView).cellRow;
            _rowCellView.transform.parent.gameObject.SetActive(false);
        }

        private async void OnEnable() {
            await UniTask.DelayFrame(1);
            if (_viewData != null) CreateCellDataList();
        }

        public int GetNumberOfCells(EnhancedScroller scroller) {
            var viewData = _viewData as ViewData;
            return Mathf.CeilToInt((float)viewData.listData.Count / _numberOfCellsPerRow);
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
            var viewData = _viewData as ViewData;
            if (_scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical) return ((FwGridListCellViewBase<U, T>)_rowCellView).size.y;
            return ((FwGridListCellViewBase<U, T>)_rowCellView).size.x;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex) {
            var viewData = _viewData as ViewData;

            var cellRowView = scroller.GetCellView(_rowCellView) as FwGridListCellViewBase<U, T>;

            var di = dataIndex * _numberOfCellsPerRow;

            cellRowView.name = "Cell " + di + " to " + (di + _numberOfCellsPerRow - 1);
            cellRowView.SetData(viewData.listData, di, SetCellViewExtend);

            return cellRowView;
        }

        public override void Set(IViewData data) {
            _viewData = data;
            var viewData = data as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(viewData.listData.ObserveReplace().Subscribe(_ => CreateCellDataList()));
            _subscriptions.Add(viewData.listData.ObserveCountChanged().Subscribe(_ => CreateCellDataList()));
            _subscriptions.Add(viewData.listData.ObserveReset().Subscribe(_ => CreateCellDataList()));

            _subscriptions.Add(viewData.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));

            _scroller.Delegate = this;
        }

        private void CreateCellDataList() {
            if (!gameObject.activeInHierarchy) return;
            _scroller.ReloadData(_scroller.NormalizedScrollPosition);
        }

        protected virtual void SetCellViewExtend(U cellView) {
        }

        public class ViewData : FwSimpleListBase<T, U>.ViewData {
            public ViewData(IReadOnlyList<T> listData, bool isVisible = true) : base(listData, isVisible) {
            }
        }

#if UNITY_EDITOR
        [PropertyOrder(4)]
        [Button]
        private void GetCellView() {
            _rowCellView = GetComponentInChildren<EnhancedScrollerCellView>();
        }

        [PropertyOrder(2)]
        [Button]
        private void GetReferenceLayout() {
            var layout = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            if (layout == null) return;

            _scroller.padding = layout.padding;
            _scroller.spacing = layout.spacing;
        }

        private bool Validate(EnhancedScrollerCellView target, ref string message) {
            if (target is FwGridListCellViewBase<U, T> cellView) {
                _cellRowDisplay = cellView.cellRow;
                return true;
            }

            message = "CellViewのクラスが正しくありません";
            return false;
        }

#endif
    }
}