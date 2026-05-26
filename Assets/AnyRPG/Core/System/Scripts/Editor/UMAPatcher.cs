using UnityEngine;
using UnityEditor;
using System.IO;

namespace AnyRPG {
    public class UMAPatcher : EditorWindow {
        [MenuItem("Tools/UMA/Patch Fresh Install (Unity 6.3)")]
        public static void Patch() {
            string folderPath = "Assets/UMA/Core/ShaderPackages";

            if (!Directory.Exists(folderPath)) {
                EditorUtility.DisplayDialog("Error", "UMA Shader folder not found! Please ensure UMA is installed in Assets/UMA.", "OK");
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.umaShaderPack", SearchOption.AllDirectories);
            int count = 0;

            // Fresh UMA install (1-parameter)
            string oldLine = "outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);";
            // Unity 6.3 Fix (0-parameter)
            string newLine = "outRenderingLayers = float4(EncodeMeshRenderingLayer(), 0, 0, 0);";

            foreach (string file in files) {
                string content = File.ReadAllText(file);
                if (content.Contains(oldLine)) {
                    content = content.Replace(oldLine, newLine);
                    File.WriteAllText(file, content);
                    count++;
                }
            }

            AssetDatabase.Refresh();

            // Updated popup with Reimport instructions
            EditorUtility.DisplayDialog("Patch Complete",
                $"Successfully patched {count} UMA shader files for Unity 6.3.\n\n" +
                "ACTION REQUIRED:\n" +
                "1. Locate the 'Assets/UMA/Core/ShaderPackages' folder.\n" +
                "2. Right-click the folder and select 'REIMPORT'.\n\n" +
                "Note: If you have already attempted a build and it failed, you must also delete your 'Library' folder to clear the error cache.",
                "I've Got It!");
        }
    }

}