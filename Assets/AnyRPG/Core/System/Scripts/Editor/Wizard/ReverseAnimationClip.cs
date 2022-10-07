/*------------------------------------------------------------------------------
Original Author: Pikachuxxxx
Adapted By: Brandon Lyman

This script is an adaptation of Pikachuxxxx's utiltiy to reverse an animation 
clip in Unity. Please find the original Github Gist here:

https://gist.github.com/8101da6d14a5afde80c7c180e3a43644.git

ABSOLUTELY ALL CREDIT FOR THIS SCRIPT goes to Pikachuxxxx. Thank you so much for
your original script!

Unfortunately, their method that utilizes 
"AnimationUtility.GetAllCurves()" is obsolete, according to the official
unity documentation:

https://docs.unity3d.com/ScriptReference/AnimationUtility.GetAllCurves.html 

The editor suggests using "AnimationUtility.GetCurveBindings()" in its stead,
and this script reveals how that can be accomplished as it is slightly
different from the original methodology. I also added in some logic to 
differentiate between the original clip and the new clip being created, as 
I experienced null reference exceptions after the original "ClearAllCurves()" 
call. Additionally, I placed the script's logic in a ScriptableWizard class to 
fit the needs for my project. For more information on ScriptableWizards, please
refer to this Unity Learn Tutorial:

https://learn.unity.com/tutorial/creating-basic-editor-tools#5cf6c8f2edbc2a160a8a0951

Hope this helps and please comment with any questions. Thanks!

------------------------------------------------------------------------------*/

using UnityEngine;
using UnityEditor;
using System.IO;

namespace AnyRPG.Editor {
    public class ReverseAnimationClip : ScriptableWizard {
        public string NewFileName = "";

        [MenuItem("Tools/AnyRPG/Reverse Animation Clip...")]
        private static void ReverseAnimationClipWizard() {
            ScriptableWizard.DisplayWizard<ReverseAnimationClip>("Reverse Animation Clip...", "Reverse");
        }

        private void OnWizardCreate() {
            string directoryPath =
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(Selection.activeObject));
            string fileName =
                Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject));
            string fileExtension =
                Path.GetExtension(AssetDatabase.GetAssetPath(Selection.activeObject));
            fileName = fileName.Split('.')[0];

            string copiedFilePath = "";
            if (NewFileName != null && NewFileName != "") {
                copiedFilePath = directoryPath + Path.DirectorySeparatorChar + NewFileName + fileExtension;
            } else {
                copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;
            }

            AnimationClip originalClip = GetSelectedClip();

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

            AnimationClip reversedClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

            if (originalClip == null) {
                return;
            }

            float clipLength = originalClip.length;
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
            Debug.Log(curveBindings.Length);
            reversedClip.ClearCurves();
            foreach (EditorCurveBinding binding in curveBindings) {
                AnimationCurve curve = AnimationUtility.GetEditorCurve(originalClip, binding);
                Keyframe[] keys = curve.keys;
                int keyCount = keys.Length;
                WrapMode postWrapmode = curve.postWrapMode;
                curve.postWrapMode = curve.preWrapMode;
                curve.preWrapMode = postWrapmode;
                for (int i = 0; i < keyCount; i++) {
                    Keyframe K = keys[i];
                    K.time = clipLength - K.time;
                    float tmp = -K.inTangent;
                    K.inTangent = -K.outTangent;
                    K.outTangent = tmp;
                    keys[i] = K;
                }
                curve.keys = keys;
                reversedClip.SetCurve(binding.path, binding.type, binding.propertyName, curve);
            }

            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(originalClip);
            if (events.Length > 0) {
                for (int i = 0; i < events.Length; i++) {
                    events[i].time = clipLength - events[i].time;
                }
                AnimationUtility.SetAnimationEvents(reversedClip, events);
            }

            Debug.Log("[[ReverseAnimationClip.cs]] Successfully reversed " +
            "animation clip " + fileName + ".");
        }

        private AnimationClip GetSelectedClip() {
            Object[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
            if (clips.Length > 0) {
                return clips[0] as AnimationClip;
            }
            return null;
        }
    }
}