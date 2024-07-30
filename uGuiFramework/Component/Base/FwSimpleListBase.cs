using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using uGuiFramework.View;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace uGuiFramework.Component.Base {
    public class FwSimpleListBase<T, U> : ViewComponentBase, IEnhancedScrollerDelegate where T : ICellData where U : FwSimpleListCellViewBase<T> {
        [PropertyOrder(1)] [SerializeField] protected EnhancedScroller _scroller;

        [PropertySpace(16)] [PropertyOrder(3)] [SerializeField]
        protected U _cellView;

        private void Awake() {
            _cellView.transform.parent.gameObject.SetActive(false);
        }

        public int GetNumberOfCells(EnhancedScroller scroller) {
            var viewData = _viewData as ViewData;
            return viewData.listData.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) {
            var viewData = _viewData as ViewData;
            if (_scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical) return _cellView.size.y;
            return _cellView.size.x;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex) {
            var viewData = _viewData as ViewData;
            var cellView = scroller.GetCellView(_cellView) as U;
            SetCellViewExtend(cellView);

            cellView.name = "Cell " + dataIndex;

            cellView.SetData(viewData.listData[dataIndex]);

            return cellView;
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
            _scroller._scrollPosition = 0;
        }

        private void CreateCellDataList() {
            if (!gameObject.activeInHierarchy) return;
            _scroller.ReloadData(_scroller.NormalizedScrollPosition);
        }

        protected virtual void SetCellViewExtend(U cellView) {
        }

        public class ViewData : ViewDataBase, IListViewData<T> {
            public ViewData(IReadOnlyList<T> listData, bool isVisible = true) : base(isVisible) {
                this.listData = new ReactiveCollection<T>(listData);
            }

            public ReactiveCollection<T> listData { get; }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
                listData.Clear();
                listData.AddRange(data.listData);
            }
        }

#if UNITY_EDITOR
        [PropertyOrder(4)]
        [Button]
        private void GetCellView() {
            _cellView = GetComponentInChildren<U>();
        }

        [PropertyOrder(2)]
        [Button]
        private void GetReferenceLayout() {
            var layout = GetComponentInChildren<HorizontalOrVerticalLayoutGroup>();
            if (layout == null) return;

            _scroller.padding = layout.padding;
            _scroller.spacing = layout.spacing;
        }
#endif
    }

    public interface IListViewData<T> where T : ICellData {
        public ReactiveCollection<T> listData { get; }
    }
}