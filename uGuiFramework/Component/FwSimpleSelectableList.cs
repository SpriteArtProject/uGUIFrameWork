using uGuiFramework.Component.Base;

namespace uGuiFramework.Component {
    public class FwSimpleSelectableList : FwSimpleListBase<SelectableCellData, FwSimpleSelectableListCellView> {
        protected override void SetCellViewExtend(FwSimpleSelectableListCellView cellView) {
            //cellView.SetScrollReloadAction(() => _scroller.ReloadData(_scroller.NormalizedScrollPosition));
        }
    }
}