using System;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using Sirenix.OdinInspector;
using UnityEngine;

namespace uGuiFramework.Component.Base {
    public class FwGridListCellViewBase<T, U> : EnhancedScrollerCellView where T : FwSimpleListCellViewBase<U> where U : ICellData {
        [SceneObjectsOnly] [SerializeField] protected T[] _rowCellViews;

        private RectTransform _rectTransform;
        public int cellRow => _rowCellViews.Length;

        public Vector2 size {
            get {
                _rectTransform ??= GetComponent<RectTransform>();
                return _rectTransform.sizeDelta;
            }
        }

        [Button]
        private void GetCells() {
            _rowCellViews = GetComponentsInChildren<T>();
        }

        public void SetData(IReadOnlyList<U> data, int startingIndex, Action<T> setCellViewExtend) {
            // loop through the sub cells to display their data (or disable them if they are outside the bounds of the data)
            for (var i = 0; i < _rowCellViews.Length; i++) {
                var dataIndex = startingIndex + i;

                _rowCellViews[i].SetActive(dataIndex < data.Count);
                if (dataIndex >= data.Count) continue;
                _rowCellViews[i].SetData(data[dataIndex]);
                setCellViewExtend(_rowCellViews[i]);
            }
        }
    }
}