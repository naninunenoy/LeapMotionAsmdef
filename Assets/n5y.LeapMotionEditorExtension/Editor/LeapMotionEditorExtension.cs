using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace n5y.LeapMotionEditorExtension.Editor
{
    
    public static class LeapMotionEditorExtension
    {
        const string EditorAsmdefTemplateGuid = "518ece1b4d38ca84bbc66a7ee831f52c";
        const string EditorTestsAsmdefTemplateGuid = "63f2a6a16f979b648b68756d9332168a";
        
        [MenuItem("Assets/n5y/GenerateLeapMotionAsmdef")]
        public static void GenerateLeapMotionAsmdef()
        {
            // LeapMotionのディレクトリからEditor or Tests を含むものを抽出
            var sep = Path.DirectorySeparatorChar;
            var pluginPath = $"{Application.dataPath}{sep}Plugins";
            var leapMotionPath = $"{pluginPath}{sep}LeapMotion";
            var leapMotionDirectories = Directory.GetDirectories(
                leapMotionPath, "*", SearchOption.AllDirectories);
            var editorDirectories = leapMotionDirectories
                .Where(x => x.EndsWith($"{sep}Editor"))
                .Where(x => !x.Contains("Tests"));
            var testDirectories = leapMotionDirectories
                .Where(x => x.EndsWith($"{sep}Tests"));

            Debug.Log(string.Join("\n", editorDirectories));
            Debug.Log(string.Join("\n", testDirectories));

            // pathと作成するasmdefのmapを作成
            var editorAsmdefDict = editorDirectories
                .ToDictionary(x => x, x => Path2AsmdefName(x, pluginPath));
            var testsAsmdefDict = testDirectories
                .ToDictionary(x => x, x => Path2AsmdefName(x, pluginPath));
            
            // 指定のpathに空のasmdefファイルを作成
            var editorAsmdefTemplate = Guid2TextAsset(EditorAsmdefTemplateGuid);
            var editorTestsAsmdefTemplate = Guid2TextAsset(EditorTestsAsmdefTemplateGuid);
            foreach (var pair in editorAsmdefDict)
            {
                CreateAsmdefJson(pair.Key, pair.Value, editorAsmdefTemplate.text);
            }
            foreach (var pair in testsAsmdefDict)
            {
                CreateAsmdefJson(pair.Key, pair.Value, editorTestsAsmdefTemplate.text);
            }
        }

        [MenuItem("Assets/n5y/EvacuateLeapMotionAsmdef")]
        public static void EvacuateLeapMotionAsmdef()
        {
            // LeapMotionのディレクトリからasmdefを抽出
            var sep = Path.DirectorySeparatorChar;
            var dist = $"{Application.dataPath}{sep}dst";
            var pluginPath = $"{Application.dataPath}{sep}Plugins";
            var leapMotionPath = $"{pluginPath}{sep}LeapMotion";
            var leapMotionDirectories = Directory.GetDirectories(
                leapMotionPath, "*", SearchOption.AllDirectories);
            // root ディレクトリを最初に作成
            var root = Directory.CreateDirectory($"{dist}{sep}LeapMotionAsmdef");
            // ディレクトリとasmdefをコピーしていく
            foreach (var dirPath in leapMotionDirectories)
            {
                var asmdef = Directory.GetFiles(dirPath, "*.asmdef", SearchOption.TopDirectoryOnly);
                if (asmdef.Length == 0) continue;
                // asmdefとディレクトリをコピー
                var relativePath = dirPath.Replace(leapMotionPath, "");
                var dir = Directory.CreateDirectory($"{root.FullName}{sep}{relativePath}");
                var fileName = asmdef[0].Split(sep).Last();
                File.Copy(asmdef[0], $"{dir.FullName}{sep}{fileName}");
            }
        }
        
        static TextAsset Guid2TextAsset(string guid)
        {
            var path =  AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
        }

        static string Path2AsmdefName(string fullPath, string pluginPath)
        {
            return string.Concat(fullPath
                .Replace(pluginPath, "")
                .Replace($"{Path.DirectorySeparatorChar}", ".")
                .Skip(1));
        }

        static void CreateAsmdefJson(string path, string name, string template)
        {
            var emptyJson = JsonUtility.FromJson<AsmdefJson>(template);
            emptyJson.name = name;
            var newJson = JsonUtility.ToJson(emptyJson, true);
            var fullPath = $"{path}{Path.DirectorySeparatorChar}{name}.asmdef";
            using (TextWriter writer = new StreamWriter(fullPath, false, Encoding.UTF8))
            {
                writer.Write(newJson);
                writer.Close();
            }
        }
    }
}
