using uGuiFramework.Component;
using UnityEngine;

namespace uGuiFramework.View.Test {
    public partial class ExUI : ViewBase {
        [SerializeField] private FwImage _image;
        [SerializeField] private FwText _text;
        [SerializeField] private FwGameObject _gameObject;
        [SerializeField] private FwButton _button;
        [SerializeField] private FwButton _button2;

        private ViewData _viewData;

        public override void Set(IViewData viewData) {
            _viewData = viewData as ViewData;
            _image.Set(_viewData.imageData);
            _text.Set(_viewData.textData);
            _gameObject.Set(_viewData.gameObjectData);
            _button.Set(_viewData.buttonData);
            _button2.Set(_viewData.button2Data);
            ExtensionSet();
        }

        partial void ExtensionSet();

        public class ViewData : IViewData {
            public readonly FwButton.ViewData button2Data;
            public readonly FwButton.ViewData buttonData;
            public readonly FwGameObject.ViewData gameObjectData;
            public readonly FwImage.ViewData imageData;
            public readonly FwText.ViewData textData;

            public ViewData(FwImage.ViewData imageData, FwText.ViewData textData, FwGameObject.ViewData gameObjectData, FwButton.ViewData buttonData, FwButton.ViewData button2Data) {
                this.imageData = imageData;
                this.textData = textData;
                this.gameObjectData = gameObjectData;
                this.buttonData = buttonData;
                this.button2Data = button2Data;
            }

            /*
            ExUI.ViewData CreteExUIViewData() {
                    return new ExUI.ViewData(
                    // イメージ1
                    new FwImage.ViewData(),
                    // テキスト
                    new FwText.ViewData(),
                    // 非表示切り替え
                    new FwGameObject.ViewData(),
                    // 切り替えボタン
                    new FwButton.ViewData(),
                    // 画像変更ボタン
                    new FwButton.ViewData()
                );
            }
             */
        }
    }

    public partial class ExUI : ViewBase {
        partial void ExtensionSet() {
        }
    }
}