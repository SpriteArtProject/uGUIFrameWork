using System;
using Sirenix.OdinInspector;
using uGuiFramework.Component.Base;
using UniRx;
using UnityEngine;

namespace uGuiFramework.Component {
    [RequireComponent(typeof(FwButton))]
    public class FwSimpleSelectableListCellView : FwSimpleListCellViewBase<SelectableCellData> {
        [SerializeField] [Required] private GameObject _cursor;
        [SerializeField] [Required] private FwButton _button;

        private FwButton.ViewData _buttonData;
        private Action _scrollReload;


        protected override void SetDataExtend(SelectableCellData data) {
            ResetSubscriptions();
            _subscriptions.Add(data.isSelect.Subscribe(_cursor.SetActive));
            _buttonData ??= new FwButton.ViewData(OnClick, null, OnLongClick);
            _button.Set(_buttonData);
        }

        public void OnClick() {
            _data.onClick(_data);
            _scrollReload?.Invoke();
        }

        public void OnLongClick() {
            _data.onLongClick?.Invoke(_data);
        }

        public void SetScrollReloadAction(Action scrollReload) {
            _scrollReload = scrollReload;
        }
    }
}