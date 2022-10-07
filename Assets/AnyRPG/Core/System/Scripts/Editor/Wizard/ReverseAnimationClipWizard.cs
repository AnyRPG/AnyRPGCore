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
    public class ReverseAnimationClipWizard : ScriptableWizard {
        
        public AnimationClip sourceAnimationClip = null;
        public string NewFileName = "";

        private const string wizardTitle = "Reverse Animation Clip Wizard";


        [MenuItem("Tools/AnyRPG/Reverse Animation Clip")]
        private static void CreateWizard() {
            ScriptableWizard.DisplayWizard<ReverseAnimationClipWizard>("Reverse Animation Clip", "Reverse");
        }

        private void OnWizardCreate() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Beginning Operation...", 0.2f);

            string filePath = ReverseAnimationClip();

            EditorUtility.ClearProgressBar();

            if (filePath != null) {
                EditorUtility.DisplayDialog(wizardTitle, wizardTitle + " Complete!\nThe new animation clip can be found at " + filePath, "OK");
            } else {
                EditorUtility.DisplayDialog(wizardTitle, wizardTitle + " Failed! There was an error!", "OK");
            }

        }

        private string ReverseAnimationClip() {

            EditorUtility.DisplayProgressBar(wizardTitle, "Getting Animation Clip Path...", 0.4f);

            string directoryPath =
                Path.GetDirectoryName(AssetDatabase.GetAssetPath(sourceAnimationClip));
            string fileName =
                Path.GetFileName(AssetDatabase.GetAssetPath(sourceAnimationClip));
            string fileExtension =
                Path.GetExtension(AssetDatabase.GetAssetPath(sourceAnimationClip));
            fileName = fileName.Split('.')[0];

            string copiedFilePath = "";
            if (NewFileName != null && NewFileName != "") {
                copiedFilePath = directoryPath + Path.DirectorySeparatorChar + NewFileName + fileExtension;
            } else {
                copiedFilePath = directoryPath + Path.DirectorySeparatorChar + fileName + "_Reversed" + fileExtension;
            }

            AnimationClip originalClip = GetSelectedClip();

            EditorUtility.DisplayProgressBar(wizardTitle, "Copying Animation Clip...", 0.6f);

            AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(Selection.activeObject), copiedFilePath);

            AnimationClip reversedClip = (AnimationClip)AssetDatabase.LoadAssetAtPath(copiedFilePath, typeof(AnimationClip));

            if (originalClip == null) {
                return null;
            }

            EditorUtility.DisplayProgressBar(wizardTitle, "Reversing Animation Clip...", 0.8f);

            float clipLength = originalClip.length;
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(originalClip);
            //Debug.Log(curveBindings.Length);
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

            EditorUtility.DisplayProgressBar(wizardTitle, "Setting Animation Events...", 1.0f);


            AnimationEvent[] events = AnimationUtility.GetAnimationEvents(originalClip);
            if (events.Length > 0) {
                for (int i = 0; i < events.Length; i++) {
                    events[i].time = clipLength - events[i].time;
                }
                AnimationUtility.SetAnimationEvents(reversedClip, events);
            }

            Debug.Log("Successfully reversed animation clip " + fileName + ".");

            return reversedClip.name;
        }

        private AnimationClip GetSelectedClip() {
            Object[] clips = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Assets);
            if (clips.Length > 0) {
                return clips[0] as AnimationClip;
            }
            return null;
        }
        private void OnEnable() {
            SetSelection();
            Selection.selectionChanged += SetSelection;
        }

        private void OnDisable() {
            Selection.selectionChanged -= SetSelection;
        }

        public void SetSelection() {
            sourceAnimationClip = (AnimationClip)Selection.activeObject;
            OnWizardUpdate();
        }


        void OnWizardUpdate() {
            helpString = "Reverses an animation clip";
            errorString = Validate();
            isValid = (errorString == null || errorString == "");
        }

        string Validate() {
            if (sourceAnimationClip == null) {
                return "An animation clip must be selected";
            }

            return null;
        }
    }
}