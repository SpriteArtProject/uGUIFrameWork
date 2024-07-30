#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using uGuiFramework.View;
using UnityEditor;
using UnityEngine;

namespace uGuiFramework.Generate {
    public static class AutoGeneratorUIClass {
        private const string ProjectName = "FairyExplorer";
        private const string GenerateCsPath = "Assets/_FairyExplorer/UI/";

        public static string nameSpace => ProjectName + ".View";

        // Get file path for a game object
        private static string GetFilePathForGameObject(GameObject gameObject, bool isNewPath = false, bool isExtension = false) {
            var uiClass = gameObject.GetComponent<ViewBase>();
            if (uiClass == null || isNewPath || isExtension) return GenerateCsPath + gameObject.name + "/" + gameObject.name + (isExtension ? "Extension" : "") + ".cs";
            var monoScript = MonoScript.FromMonoBehaviour(uiClass);
            if (monoScript != null) return AssetDatabase.GetAssetPath(monoScript);
            return GenerateCsPath + gameObject.name + "/" + gameObject.name + (isExtension ? "Extension" : "") + ".cs";
        }


        public static bool Generate(ViewBase component, bool isNewPass) {
            // Get file path for game object
            var generateCsFilePath = GetFilePathForGameObject(component.gameObject, isNewPass);
            var generateCsFoldPath = Path.GetDirectoryName(generateCsFilePath);
            var className = Path.GetFileNameWithoutExtension(generateCsFilePath);
            // Create directory if it does not exist
            if (!Directory.Exists(generateCsFoldPath)) {
                Directory.CreateDirectory(generateCsFoldPath);
                Debug.LogWarning("Folder " + generateCsFoldPath + " created");
            }

            // Get view components
            var data = component.GetIViewComponents();
            // Validate object name
            if (!ObjectNameValidate(data)) return false;
            // Generate code
            var generate = new GeneratedCode();
            CreateUsing(generate);
            CreateNameSpace(generate);
            CreateClassName(generate, className);
            CreateViewData(generate, component, className);
            CreateField(generate, component);
            EndParenthesis(generate);
            // Write code to file
            File.WriteAllText(generateCsFilePath, generate.code, Encoding.GetEncoding("UTF-8"));
            // Get extension path
            var extensionPath = GetFilePathForGameObject(component.gameObject, isNewPass, true);
            // Check if file exists
            if (File.Exists(extensionPath)) {
                Debug.Log(extensionPath);
                return true;
            }

            // Generate extension code
            var generateExtension = new GeneratedCode();
            CreateUsing(generateExtension);
            CreateNameSpace(generateExtension);
            CreateClassName(generateExtension, className);
            CreateExtensionSet(generateExtension);
            EndParenthesis(generateExtension);
            // Write extension code to file
            File.WriteAllText(extensionPath, generateExtension.code, Encoding.GetEncoding("UTF-8"));
            return true;
        }

        private static void CreateUsing(GeneratedCode generatedCode) {
            generatedCode.Generate("using uGuiFramework.Component;");
            generatedCode.Generate("using uGuiFramework.View;");
            generatedCode.Generate("using UniRx;");
            generatedCode.Generate("using UnityEngine;");
        }

        private static void CreateNameSpace(GeneratedCode generatedCode) {
            generatedCode.Generate("namespace " + ProjectName + ".View {");
        }

        private static void CreateClassName(GeneratedCode generatedCode, string name) {
            generatedCode.Generate("public partial class " + name + " : ViewBase {");
        }


        /// <summary>
        ///     ViewDataの定義
        /// </summary>
        /// <param name="generatedCode"></param>
        /// <param name="component"></param>
        /// <param name="className"></param>
        private static void CreateViewData(GeneratedCode generatedCode, ViewBase component, string className) {
            generatedCode.Generate("public class ViewData : ViewDataBase {");


            var list = component.GetIViewComponents();

            string CreateTypeAndName(IViewParts parts) {
                var str = parts.GetType().Name;
                str += ".ViewData " + GetPartsName(parts);
                return str;
            }

            string GetPartsName(IViewParts parts) {
                return parts.objName.Substring(0, 1).ToLower() + parts.objName.Substring(1);
            }


            list.ForEach(parts => {
                    var str = "public readonly " + CreateTypeAndName(parts) + ";";
                    generatedCode.Generate(str);
                }
            );


            var signature = "public ViewData(";
            list.ForEach((parts, i) => {
                    signature += CreateTypeAndName(parts);
                    signature += ", ";
                }
            );
            signature += " bool isVisible = true) : base(isVisible) {";
            generatedCode.Generate(signature);


            list.ForEach(parts => {
                    var str = "this." + GetPartsName(parts) + " = " + GetPartsName(parts) + ";";
                    generatedCode.Generate(str);
                }
            );


            generatedCode.Generate("}");

            // Copyメソッド実装

            generatedCode.Generate("");
            generatedCode.Generate("protected override void Copy(IViewData rootData) {");
            generatedCode.Generate("var data = rootData as ViewData;");

            list.ForEach(parts => {
                    var str = GetPartsName(parts) + ".DoCopy(data." + GetPartsName(parts) + ");";
                    generatedCode.Generate(str);
                }
            );

            generatedCode.Generate("}");

            // コピペ用コメント生成
            generatedCode.Generate("");
            generatedCode.Generate("/*");

            generatedCode.Generate(className + ".ViewData Crete" + className + "Data() {");
            generatedCode.Generate("return new " + className + ".ViewData(");

            list.ForEach((parts, i) => {
                    if (parts.description.IsNullOrWhitespace())
                        generatedCode.Generate("// " + parts.objName);
                    else
                        generatedCode.Generate("// " + parts.description);
                    var str = "new " + parts.GetType().Name + ".ViewData()";
                    if (i != list.Count - 1) str += ",";
                    generatedCode.Generate(str);
                }
            );

            generatedCode.Generate(");");
            generatedCode.Generate("}");

            generatedCode.Generate("*/");


            generatedCode.Generate("}");
        }


        /// <summary>
        ///     フィールド定義と初期化
        /// </summary>
        /// <param name="generatedCode"></param>
        /// <param name="component"></param>
        private static void CreateField(GeneratedCode generatedCode, ViewBase component) {
            generatedCode.Generate("");

            var list = component.GetIViewComponents();

            string CreateTypeAndName(IViewParts parts) {
                var str = parts.GetType().Name;
                str += " " + GetPartsName(parts, "_");
                return str;
            }

            string GetPartsName(IViewParts parts, string prefix) {
                return prefix + parts.objName.Substring(0, 1).ToLower() + parts.objName.Substring(1);
            }


            list.ForEach(parts => {
                    var str = "[SerializeField] private " + CreateTypeAndName(parts) + ";";
                    generatedCode.Generate(str);
                }
            );

            generatedCode.Generate("");
            generatedCode.Generate("private ViewData _viewData;");
            generatedCode.Generate("");

            generatedCode.Generate("public override void Set(IViewData viewData) {");

            generatedCode.Generate("_viewData = viewData as ViewData;");

            generatedCode.Generate("ResetSubscriptions();");

            list.ForEach(parts => {
                    var str = GetPartsName(parts, "_") + ".Set(_viewData." + GetPartsName(parts, "") + ");";
                    generatedCode.Generate(str);
                }
            );

            generatedCode.Generate("_subscriptions.Add(_viewData.isVisible.Subscribe(isVisible => gameObject.SetActive(isVisible)));");

            generatedCode.Generate("ExtensionSet();");

            generatedCode.Generate("}");

            generatedCode.Generate("");

            generatedCode.Generate("partial void ExtensionSet();");
        }

        private static void CreateExtensionSet(GeneratedCode generatedCode) {
            generatedCode.Generate("partial void ExtensionSet() { }");
        }

        private static void EndParenthesis(GeneratedCode generatedCode) {
            generatedCode.Generate("}");
            generatedCode.Generate("}");
        }

        private static bool ObjectNameValidate(IReadOnlyList<IViewParts> viewPartsList) {
            var isSafe = true;

            if (viewPartsList.GroupBy(_ => _.objName).Any(_ => _.Count() > 1)) {
                Debug.LogError("重複したオブジェクト名があります");
                Debug.LogError(viewPartsList.GroupBy(_ => _.objName).FirstOrDefault(_ => _.Count() > 1)?.Key);
                isSafe = false;
            }

            var regex = new Regex(@"^[0-9a-zA-Z]+$");

            IViewParts target = null;

            if (viewPartsList.Any(parts => {
                        var symbol = !regex.IsMatch(parts.objName);
                        if (symbol) target = parts;
                        return symbol;
                    }
                )) {
                Debug.LogError("オブジェクト名に使用できない文字が含まれています: " + target.objName);
                isSafe = false;
            }

            return isSafe;
        }

        /// <summary>
        ///     コード生成用クラス
        /// </summary>
        public class GeneratedCode {
            private const string IndentCode = "	   ";
            public int indent;
            public string code { get; private set; }

            /// <summary>
            ///     行単位でcodeに足していく
            /// </summary>
            /// <param name="code"></param>
            public void Generate(string code) {
                var str = new StringBuilder();

                if (code.Contains("}")) indent -= 1;

                for (var i = 0; i < indent; i++) str.Append(IndentCode);

                if (code.Contains("{")) indent += 1;

                str.Append(code);

                str.Append("\n");

                this.code += str.ToString();
            }
        }
    }
}
#endif