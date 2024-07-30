using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using uGuiFramework.Generate;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace uGuiFramework.View {
    public interface IView {
        public IReadOnlyList<IViewParts> GetIViewComponents();
    }

    public interface IViewData {
    }

    public interface IViewComponent {
        public T GetViewData<T>() where T : class, IViewData;
    }

    public interface IViewParts {
        public string objName { get; }
        public string description { get; }
        public void Set(IViewData viewData);
        public GameObject GetGameObject();
    }

    public abstract class ViewComponentBase : MonoBehaviour, IViewComponent, IViewParts {
        [PropertySpace(16)] [PropertyOrder(98)]
        public string _description;

        protected List<IDisposable> _subscriptions;

        protected IViewData _viewData;

        private void OnDestroy() {
            Dispose();
        }

        public T GetViewData<T>() where T : class, IViewData {
            return _viewData as T;
        }

        public string objName => gameObject.name.Replace(" ", "");

        public string description => _description;

        public GameObject GetGameObject() {
            return gameObject;
        }

        public abstract void Set(IViewData viewData);

        protected void ResetSubscriptions() {
            _subscriptions?.ForEach(_ => _?.Dispose());
            _subscriptions = new List<IDisposable>();
        }


        public void Dispose() {
            _subscriptions?.ForEach(_ => _?.Dispose());
            _subscriptions?.Clear();
        }

#if UNITY_EDITOR
        [PropertySpace(8)]
        [PropertyOrder(99)]
        [Button(ButtonSizes.Medium, ButtonStyle.Box, Name = "Auto Reference")]
        private void GetReference() {
            foreach (var field in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                var fieldType = field.FieldType;

                if (typeof(IList).IsAssignableFrom(fieldType)) {
                    var elementType1 = fieldType.GetGenericArguments().Single();

                    if (!elementType1.IsCastableTo(typeof(MonoBehaviour))) continue;

                    if (field.GetValue(this) is IList list) {
                        list.Clear();
                        GetComponentsInChildren(elementType1, true).Where(comp => comp.gameObject != gameObject).ForEach(comp => { list.Add(comp); });
                    }
                }

                if (!fieldType.IsCastableTo(typeof(MonoBehaviour))) continue;

                var children = GetComponentInChildren(fieldType, true);

                if (children == null) continue;

                field.SetValue(this, children);
            }

            GetReferenceExtend();
        }

        protected virtual void GetReferenceExtend() {
        }

        private void Reset() {
            GetReference();
        }

#endif
    }

    public abstract class ViewDataBase : IViewData {
        public readonly ReactiveProperty<bool> isVisible;

        protected ViewDataBase(bool isVisible = true) {
            this.isVisible = new ReactiveProperty<bool> {
                Value = isVisible
            };
        }

        public void DoCopy(IViewData rootData) {
            var data = rootData as ViewDataBase;
            isVisible.Value = data.isVisible.Value;
            Copy(rootData);
        }

        protected abstract void Copy(IViewData rootData);
    }

    public abstract class ViewBase : MonoBehaviour, IView, IViewParts {
        public string _description;

        protected List<IDisposable> _subscriptions;

        private void OnDestroy() {
            Dispose();
        }

        public IReadOnlyList<IViewParts> GetIViewComponents() {
            var list = new List<IViewParts>();

            void GetChildren(GameObject obj) {
                var children = obj.GetComponentInChildren<Transform>();

                if (children.childCount == 0) return;

                foreach (Transform ob in children) {
                    if (ob.TryGetComponent(out IViewParts comp)) {
                        list.Add(comp);
                        continue;
                    }

                    GetChildren(ob.gameObject);
                }
            }

            GetChildren(gameObject);

            return list;
        }

        public string objName => gameObject.name.Replace(" ", "");

        public string description => _description;
        public abstract void Set(IViewData viewData);

        public GameObject GetGameObject() {
            return gameObject;
        }

        protected void ResetSubscriptions() {
            _subscriptions?.ForEach(_ => _?.Dispose());
            _subscriptions = new List<IDisposable>();
        }


        public void Dispose() {
            _subscriptions?.ForEach(_ => _?.Dispose());
            _subscriptions?.Clear();
        }

#if UNITY_EDITOR

        public class OnlyCodeGenerateView : ViewBase {
            [InitializeOnLoadMethod]
            private static void ReplacementViewComponent() {
                FindObjectsOfType<OnlyCodeGenerateView>().ForEach(comp => {
                        var type = comp.GetType().Assembly.GetType(AutoGeneratorUIClass.nameSpace + "." + comp.gameObject.name);
                        var newComp = comp.gameObject.AddComponent(type);
                        var mi = type.GetMethod("GetReference");
                        mi.Invoke(newComp, null);
                        DestroyImmediate(comp);
                    }
                );
            }

            public override void Set(IViewData viewData) {
            }
        }

        [PropertySpace(8)]
        [Button(ButtonSizes.Medium, ButtonStyle.Box, Name = "Auto Reference")]
        public void GetReference() {
            SetReferenceUtil.GetReferenceFromName(this);
        }

        [PropertySpace(8)]
        [Button(ButtonSizes.Medium, ButtonStyle.Box)]
        [ContextMenu("GenerateUICode")]
        protected virtual void GenerateUICode() {
            Debug.Log(gameObject.name + " GenerateCode");
            AutoGeneratorUIClass.Generate(this, false);
        }

        [MenuItem("GameObject/UI/GenerateNewUICode", false, 99)]
        public static void NewGenerateUICode() {
            var gameObject = Selection.activeGameObject;
            if (gameObject.GetComponent<ViewBase>() != null) return;
            var viewBase = gameObject.AddComponent<OnlyCodeGenerateView>();
            AutoGeneratorUIClass.Generate(viewBase, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorApplication.UnlockReloadAssemblies();
        }

#endif
    }

    internal static class SetReferenceUtil {
        public static void GetReferenceFromName(MonoBehaviour monoBehaviour) {
            foreach (var field in monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
                var fieldType = field.FieldType;
                if (!fieldType.IsCastableTo(typeof(MonoBehaviour))) continue;
                var children = monoBehaviour.GetComponentsInChildren<Transform>(true);
                var target = children.FirstOrDefault(tr => {
                        return
                            tr.gameObject.name.Equals(field.Name.Remove(0, 1), StringComparison.OrdinalIgnoreCase) && tr.parent.GetComponentInParent<ViewBase>() == monoBehaviour;
                    }
                )?.GetComponent(fieldType);

                if (target == null) continue;

                field.SetValue(monoBehaviour, target);
            }
        }
    }
}