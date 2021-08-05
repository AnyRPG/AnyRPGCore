using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using AnyRPG;

public class ResourceSelector : EditorWindow
{
    System.Type resourceType;
    string selectedItemName = "";
    SerializedProperty editedProperty;
    Label header;
    Label fileTypeLabel;
    //Toggle includeCoreContent;
    ListView listView;
    List<ResourceProfile> listElements;
    bool listInitialized = false;

//    [MenuItem("Tools/AnyRPG/ResourceSelector")]

    public static void ShowResources() {
        ResourceSelector wnd = GetWindow<ResourceSelector>();
        wnd.titleContent = new GUIContent("ResourceSelector");
    }

    public static void DisplaySelectionDialog(System.Type resourceType, SerializedProperty property) {
        ResourceSelector window = GetWindow<ResourceSelector>();
        window.titleContent = new GUIContent("ResourceSelector");
        window.resourceType = resourceType;
        window.selectedItemName = property.stringValue;
        window.editedProperty = property;
        window.Show();
    }

    void SetSelected(IEnumerable<System.Object> selected) {
        foreach (System.Object obj in selected) {
            //Debug.Log("set selected: " + obj);
            if (obj != null) {
                editedProperty.stringValue = (obj as ResourceProfile).ResourceName;
            } else {
                editedProperty.stringValue = "";
            }
            editedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    // fill listElements with all resources for the type in fileType
    void InitializeList() {
        //DummyResourceManager manager = new DummyResourceManager(resourceType, includeCoreContent.value);
        DummyResourceManager manager = new DummyResourceManager(resourceType);
        if (manager.GetResourceList().Count == 0) {
            manager.LoadResourceList();
        }
        List<ResourceProfile> namesList = new List<ResourceProfile>();
        foreach (ResourceProfile item in manager.GetResourceList()) {
            namesList.Add(item);
        }
        namesList.Sort((a,b) => a.ResourceName.CompareTo(b.ResourceName));
        listElements.AddRange(namesList);
    }

    void ReloadList() {
        listElements.Clear();
        InitializeList();
        listView.Refresh();
    }

    public void OnGUI() {
        if (editedProperty != null) {
            try {
                header.text = editedProperty.propertyPath + " in " + editedProperty.serializedObject.targetObject;
            } catch (NullReferenceException) {
                // that means the serialized object has been disposed and gone out of scope while the editor window is still open
                // let's just close it and act as nothing happened
                Close();
            }
            fileTypeLabel.text = "Resource name: " + resourceType.Name;
            if (!listInitialized) {
                InitializeList();
                listView.Refresh();
                listInitialized = true;
                int idx = -1;
                for (int i = 0; i < listElements.Count; i++) {
                    if (listElements[i].ResourceName == selectedItemName) {
                        idx = i;
                        break;
                    }
                }
                if (idx >= 0) {
                    listView.selectedIndex = idx;
                }
            }
        }
    }

    // this function is heavily influenced by the API docs example at
    // https://docs.unity3d.com/Packages/com.unity.ui@1.0/api/UnityEngine.UIElements.ListView.html
    public void CreateGUI() {
        VisualElement root = rootVisualElement;

        header = new Label("");
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(header);

        fileTypeLabel = new Label("any");
        fileTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        root.Add(fileTypeLabel);

        /*
        includeCoreContent = new Toggle("include core content");
        includeCoreContent.value = true;
        includeCoreContent.RegisterValueChangedCallback<bool>( x => ReloadList());
        root.Add(includeCoreContent);
        */

        listElements = new List<ResourceProfile>();

        // The "makeItem" function is called when the
        // ListView needs more items to render.
        Func<VisualElement> makeItem = () => new Label();

        // As the user scrolls through the list, the ListView object
        // recycles elements created by the "makeItem" function,
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list).
        //Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = listElements[i];
        Action<VisualElement, int> bindItem = (e, i) => {
            Label l = e as Label;
            l.text = listElements[i].ResourceName;
            // I would love to put the path in a tooltip but there is currently no way to get it from Resources
            //l.tooltip = "Path: " + AssetDatabase.GetAssetPath(listElements[i]);
        };

        // Provide the list view with an explict height for every row
        // so it can calculate how many items to actually display
        const int itemHeight = 16;

        listView = new ListView(listElements, itemHeight, makeItem, bindItem);

        listView.selectionType = SelectionType.Single;

        listView.onItemsChosen += objects => SetSelected(objects);
        listView.onSelectionChange += objects => SetSelected(objects);

        listView.style.flexGrow = 1.0f;

        root.Add(listView);
    }
}