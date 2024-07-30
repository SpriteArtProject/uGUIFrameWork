using uGuiFramework.View;
using UniRx;

namespace uGuiFramework.Component {
    public class FwGameObject : ViewComponentBase {
        public override void Set(IViewData viewData) {
            _viewData = viewData;
            var data = viewData as ViewData;

            ResetSubscriptions();

            _subscriptions.Add(data.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));
        }

        public class ViewData : ViewDataBase {
            public ViewData(bool isVisible = true) : base(isVisible) {
            }

            protected override void Copy(IViewData rootData) {
                var data = rootData as ViewData;
            }
        }
    }
}