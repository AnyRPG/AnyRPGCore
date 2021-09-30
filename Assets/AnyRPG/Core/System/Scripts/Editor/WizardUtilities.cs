using AnyRPG;
using UnityEditor;

namespace AnyRPG {
    public class WizardUtilities {

        public static string GetFileSystemGameName(string gameName) {
            return gameName.Replace(" ", "");
        }

        public static void CreateFolderIfNotExists(string folderName) {
            if (!System.IO.Directory.Exists(folderName)) {
                System.IO.Directory.CreateDirectory(folderName);
            }

            AssetDatabase.Refresh();
        }

    }
}
