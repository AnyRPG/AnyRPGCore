#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AnyRPG.DefineSymbolsManager {
    [InitializeOnLoad]
    public class DefineSymbolsManager {
        static DefineSymbolsManager() {
            CreateDefinitions();
        }

        public static void CreateDefinitions() {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = definesString.Split(';').ToList();
            List<string> allAnyRPGDefines = new List<string>();

            var denitionsType = GetAllDefinitions();
            foreach (var t in denitionsType) {
                var value = t.InvokeMember(null, BindingFlags.DeclaredOnly |
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.CreateInstance, null, null, null);

                List<string> list = null;
                try {
                    list = (List<string>)t.InvokeMember("GetSymbols", BindingFlags.DeclaredOnly |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.GetProperty, null, value, null);
                    if (list != null) {
                        allAnyRPGDefines.AddRange(list.Except(allAnyRPGDefines));
                    }
                } catch {

                }
            }

            List<string> allDefinesToRemove = allDefines.FindAll(s => s.ToUpper().Contains("ANYRPG") && !allAnyRPGDefines.Contains(s));
            List<string> allDefinesToAdd = allAnyRPGDefines.FindAll(s => !allDefines.Contains(s));
            bool needUpdate = allDefinesToRemove.Count > 0 || allDefinesToAdd.Count > 0;
            if (needUpdate == true) {
                for (int i = 0; i < allDefinesToRemove.Count; i++)
                    if (allDefines.Contains(allDefinesToRemove[i])) {
                        allDefines.Remove(allDefinesToRemove[i]);
                    }

                AddDefinitionSymbols(allDefinesToAdd, allDefines);
            }
        }

        static void AddDefinitionSymbols(List<string> targetDefineSymbols, List<string> currentDefineSymbols) {
            currentDefineSymbols.AddRange(targetDefineSymbols.Except(currentDefineSymbols));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", currentDefineSymbols.ToArray()));
        }

        static List<System.Type> GetAllDefinitions() {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                 .Where(x => typeof(DefineSymbols).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract).ToList();
        }
    }
}

#endif
namespace AnyRPG.DefineSymbolsManager {
    public abstract class DefineSymbols {
        public abstract List<string> GetSymbols {
            get;
        }
    }
}