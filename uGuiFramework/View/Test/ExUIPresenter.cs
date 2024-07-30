using System.Collections.Generic;
using _FairyExplorer.DataBase;
using uGuiFramework.Component;
using uGuiFramework.Component.Base;
using UnityEngine;

namespace FairyExplorer.View {
    public class ExUIPresenter : MonoBehaviour {
        [SerializeField] private string _textA;
        [SerializeField] private Sprite _spriteA;
        [SerializeField] private Sprite _spriteB;
        [SerializeField] private ExView _exUI;

        private ExView.ViewData _viewData;

        private void Start() {
            Initialize();
        }

        private ExView.ViewData CreteExViewData() {
            return new ExView.ViewData(new FwText.ViewData("parent"),
                CreteExChildViewData(),
                new FwButton.ViewData(ChangeChildActive),
                CreateSimpleListViewData(),
                CreateGridListViewData()
            );
        }

        private FwSimpleListBase<SelectableCellData, FwSimpleSelectableListCellView>.ViewData CreateSimpleListViewData() {
            var viewData = new FwSimpleListBase<SelectableCellData, FwSimpleSelectableListCellView>.ViewData(CreateCellData());
            return viewData;
        }

        private FwGridListBase<SelectableCellData, FwSimpleSelectableListCellView>.ViewData CreateGridListViewData() {
            var viewData = new FwGridListBase<SelectableCellData, FwSimpleSelectableListCellView>.ViewData(CreateCellData());
            return viewData;
        }


        private IReadOnlyList<SelectableCellData<FairyIconView.ViewData>> CreateCellData() {
            var list = new List<SelectableCellData<FairyIconView.ViewData>>();
            for (var i = 0; i < 100; i++) {
                SelectableCellData<FairyIconView.ViewData> data = null;

                var buttonData = new FwButton.ViewData();
                var fairyData = FairyIconView.CreteFairyIconViewData(DataBaseMaster.instance.FairyMasters.failyMasters[Random.Range(0, 4)], true, buttonData);
                data = new SelectableCellData<FairyIconView.ViewData>(fairyData, i, OnClick);
                list.Add(data);

                void OnClick(SelectableCellData selectableCellData) {
                    selectableCellData.isSelect.Value = !selectableCellData.isSelect.Value;
                }
            }

            return list;
        }

        private ExChildView.ViewData CreteExChildViewData() {
            return new ExChildView.ViewData(
                // 切り替え可能イメージ
                new FwImage.ViewData(_spriteA),
                // 変更可能文字列
                new FwText.ViewData(_textA),
                // 表示切り替え可能オブジェクト
                new FwGameObject.ViewData(),
                // 文字切り替え用ボタン
                new FwButton.ViewData(ChangeActive),
                // 画像切り替え用ボタン
                new FwButton.ViewData(ChangeSprite),
                new FwShapeGauge.ViewData(0.5f),
                FairyIconView.CreteFairyIconViewData(DataBaseMaster.instance.FairyMasters.failyMasters[0], true)
            );
        }

        public void Initialize() {
            _viewData = CreteExViewData();
            _exUI.Set(_viewData);
        }

        private void ChangeSprite() {
            _viewData.exChildView.changeWindowImage.sprite.Value = _spriteB;
            _viewData.exChildView.fairyIconView.DoCopy(FairyIconView.CreteFairyIconViewData(DataBaseMaster.instance.FairyMasters.failyMasters[1], true));
        }

        private void ChangeActive() {
            _viewData.exChildView.activeChangeObject.isVisible.Value = !_viewData.exChildView.activeChangeObject.isVisible.Value;
            _viewData.exChildView.hpGauge.TweenValue(1, Random.Range(0, 1f));
        }

        private void ChangeChildActive() {
            _viewData.exChildView.isVisible.Value = !_viewData.exChildView.isVisible.Value;
        }
    }
}