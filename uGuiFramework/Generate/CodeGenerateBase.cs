using uGuiFramework.View;
using UnityEngine;

namespace uGuiFramework.Generate {
    public class CodeGenerateBase : ViewBase {
#if UNITY_EDITOR
        [ContextMenu("CreateNewUICode")]
#endif
        public override void Set(IViewData viewData) {
        }
    }
}