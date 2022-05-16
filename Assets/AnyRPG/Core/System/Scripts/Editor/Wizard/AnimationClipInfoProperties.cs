using UnityEngine;
using UnityEditor;
using System.Collections;

class AnimationClipInfoProperties {
	SerializedProperty m_Property;

	private SerializedProperty Get(string property) { return m_Property.FindPropertyRelative(property); }

	public AnimationClipInfoProperties(SerializedProperty prop) { m_Property = prop; }

	public string name { get { return Get("name").stringValue; } set { Get("name").stringValue = value; } }
	public string takeName { get { return Get("takeName").stringValue; } set { Get("takeName").stringValue = value; } }
	public float firstFrame { get { return Get("firstFrame").floatValue; } set { Get("firstFrame").floatValue = value; } }
	public float lastFrame { get { return Get("lastFrame").floatValue; } set { Get("lastFrame").floatValue = value; } }
	public int wrapMode { get { return Get("wrapMode").intValue; } set { Get("wrapMode").intValue = value; } }
	public bool loop { get { return Get("loop").boolValue; } set { Get("loop").boolValue = value; } }

	// Mecanim animation properties
	public float orientationOffsetY { get { return Get("orientationOffsetY").floatValue; } set { Get("orientationOffsetY").floatValue = value; } }
	public float level { get { return Get("level").floatValue; } set { Get("level").floatValue = value; } }
	public float cycleOffset { get { return Get("cycleOffset").floatValue; } set { Get("cycleOffset").floatValue = value; } }
	public bool loopTime { get { return Get("loopTime").boolValue; } set { Get("loopTime").boolValue = value; } }
	public bool loopBlend { get { return Get("loopBlend").boolValue; } set { Get("loopBlend").boolValue = value; } }
	public bool loopBlendOrientation { get { return Get("loopBlendOrientation").boolValue; } set { Get("loopBlendOrientation").boolValue = value; } }
	public bool loopBlendPositionY { get { return Get("loopBlendPositionY").boolValue; } set { Get("loopBlendPositionY").boolValue = value; } }
	public bool loopBlendPositionXZ { get { return Get("loopBlendPositionXZ").boolValue; } set { Get("loopBlendPositionXZ").boolValue = value; } }
	public bool keepOriginalOrientation { get { return Get("keepOriginalOrientation").boolValue; } set { Get("keepOriginalOrientation").boolValue = value; } }
	public bool keepOriginalPositionY { get { return Get("keepOriginalPositionY").boolValue; } set { Get("keepOriginalPositionY").boolValue = value; } }
	public bool keepOriginalPositionXZ { get { return Get("keepOriginalPositionXZ").boolValue; } set { Get("keepOriginalPositionXZ").boolValue = value; } }
	public bool heightFromFeet { get { return Get("heightFromFeet").boolValue; } set { Get("heightFromFeet").boolValue = value; } }
	public bool mirror { get { return Get("mirror").boolValue; } set { Get("mirror").boolValue = value; } }

	public AnimationEvent GetEvent(int index) {
		AnimationEvent evt = new AnimationEvent();
		SerializedProperty events = Get("events");

		if (events != null && events.isArray) {
			if (index < events.arraySize) {
				evt.floatParameter = events.GetArrayElementAtIndex(index).FindPropertyRelative("floatParameter").floatValue;
				evt.functionName = events.GetArrayElementAtIndex(index).FindPropertyRelative("functionName").stringValue;
				evt.intParameter = events.GetArrayElementAtIndex(index).FindPropertyRelative("intParameter").intValue;
				evt.objectReferenceParameter = events.GetArrayElementAtIndex(index).FindPropertyRelative("objectReferenceParameter").objectReferenceValue;
				evt.stringParameter = events.GetArrayElementAtIndex(index).FindPropertyRelative("data").stringValue;
				evt.time = events.GetArrayElementAtIndex(index).FindPropertyRelative("time").floatValue;
			} else {
				Debug.LogWarning("Invalid Event Index");
			}
		}

		return evt;
	}

	public void SetEvent(int index, AnimationEvent animationEvent) {
		SerializedProperty events = Get("events");

		if (events != null && events.isArray) {
			if (index < events.arraySize) {
				events.GetArrayElementAtIndex(index).FindPropertyRelative("floatParameter").floatValue = animationEvent.floatParameter;
				events.GetArrayElementAtIndex(index).FindPropertyRelative("functionName").stringValue = animationEvent.functionName;
				events.GetArrayElementAtIndex(index).FindPropertyRelative("intParameter").intValue = animationEvent.intParameter;
				events.GetArrayElementAtIndex(index).FindPropertyRelative("objectReferenceParameter").objectReferenceValue = animationEvent.objectReferenceParameter;
				events.GetArrayElementAtIndex(index).FindPropertyRelative("data").stringValue = animationEvent.stringParameter;
				events.GetArrayElementAtIndex(index).FindPropertyRelative("time").floatValue = animationEvent.time;
			} else {
				Debug.LogWarning("Invalid Event Index");
			}
		}
	}


	public void ClearEvents() {
		SerializedProperty events = Get("events");

		if (events != null && events.isArray) {
			events.ClearArray();
		}
	}

	public int GetEventCount() {
		int ret = 0;

		SerializedProperty curves = Get("events");

		if (curves != null && curves.isArray) {
			ret = curves.arraySize;
		}

		return ret;
	}

	public void SetEvents(AnimationEvent[] newEvents) {
		SerializedProperty events = Get("events");

		if (events != null && events.isArray) {
			events.ClearArray();

			foreach (AnimationEvent evt in newEvents) {
				events.InsertArrayElementAtIndex(events.arraySize);
				SetEvent(events.arraySize - 1, evt);
			}
		}
	}

	public AnimationEvent[] GetEvents() {
		AnimationEvent[] ret = new AnimationEvent[GetEventCount()];
		SerializedProperty events = Get("events");

		if (events != null && events.isArray) {
			for (int i = 0; i < GetEventCount(); ++i) {
				ret[i] = GetEvent(i);
			}
		}

		return ret;

	}

}
public class AddEvent {

	//[MenuItem("Mecanim/Copy events")]
	static void DoAddEvent() {
		Object[] objs = Selection.GetFiltered(typeof(AnimationClip), SelectionMode.Unfiltered);
		if (objs.Length < 2)
			return;

		AnimationClip sourceAnimClip = objs[1] as AnimationClip;
		AnimationClip targetAnimClip = objs[0] as AnimationClip;
		if ((targetAnimClip.hideFlags & HideFlags.NotEditable) != 0)
			DoAddEventImportedClip(sourceAnimClip, targetAnimClip);
		else
			DoAddEventClip(sourceAnimClip, targetAnimClip);
	}

	static void DoAddEventClip(AnimationClip sourceAnimClip, AnimationClip targetAnimClip) {
		if (sourceAnimClip != targetAnimClip) {
			AnimationEvent[] sourceAnimEvents = AnimationUtility.GetAnimationEvents(sourceAnimClip);
			AnimationUtility.SetAnimationEvents(targetAnimClip, sourceAnimEvents);
		}
	}

	static void DoAddEventImportedClip(AnimationClip sourceAnimClip, AnimationClip targetAnimClip) {
		ModelImporter modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(targetAnimClip)) as ModelImporter;
		if (modelImporter == null)
			return;

		SerializedObject serializedObject = new SerializedObject(modelImporter);
		SerializedProperty clipAnimations = serializedObject.FindProperty("m_ClipAnimations");

		if (!clipAnimations.isArray)
			return;

		for (int i = 0; i < clipAnimations.arraySize; i++) {
			AnimationClipInfoProperties clipInfoProperties = new AnimationClipInfoProperties(clipAnimations.GetArrayElementAtIndex(i));
			if (clipInfoProperties.name == targetAnimClip.name) {
				AnimationEvent[] sourceAnimEvents = AnimationUtility.GetAnimationEvents(sourceAnimClip);

				clipInfoProperties.SetEvents(sourceAnimEvents);
				serializedObject.ApplyModifiedProperties();
				AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(targetAnimClip));
				break;
			}
		}



	}
}
