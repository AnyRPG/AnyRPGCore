using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using AnyRPG;

namespace AnyRPG.EditorTools {

    public class ResourceSelector : EditorWindow {
        System.Type resourceType;
        string selectedItemName = "";
        SerializedProperty editedProperty;
        Label header;
        Label fileTypeLabel;
        Label busyLabel;
        TextField nameFilter;
        PopupField<Type> typeFilter;
        ListView listView;
        List<ResourceProfile> listElements;
        List<Type> classNames = new List<Type>();
        bool listInitialized = false;
        string namePattern;
        Type classPattern;

        [MenuItem("Tools/AnyRPG/Browse Resources")]
        public static void ShowResources() {
            ResourceSelector wnd = GetWindow<ResourceSelector>();
            wnd.titleContent = new GUIContent("ResourceSelector");
            wnd.resourceType = typeof(ResourceProfile);
            wnd.Show();
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
            if (editedProperty != null) {
                foreach (System.Object obj in selected) {
                    if (obj != null) {
                        editedProperty.stringValue = (obj as ResourceProfile).ResourceName;
                    } else {
                        editedProperty.stringValue = "";
                    }
                    editedProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        // fill listElements with all resources for the type in fileType
        void InitializeList() {
            listElements.Clear();
            bool classFilterNeedsReset = false;
            DummyResourceManager manager = new DummyResourceManager(resourceType);
            if (manager.GetResourceList().Count == 0) {
                manager.LoadResourceList();
            }
            List<ResourceProfile> namesList = new List<ResourceProfile>();
            classNames.Clear();
            if (editedProperty == null) {
                classNames.Add(typeof(ResourceProfile));
            } else if (typeFilter.value == typeof(ResourceProfile)) {
                classFilterNeedsReset = true;
            }
            foreach (ResourceProfile item in manager.GetResourceList()) {
                if (!classNames.Contains(item.GetType())) {
                    classNames.Add(item.GetType());
                }
                if (MatchesFilter(item)) {
                    namesList.Add(item);
                }
            }
            namesList.Sort((a, b) => a.ResourceName.CompareTo(b.ResourceName));
            listElements.AddRange(namesList);
            if (classFilterNeedsReset && classNames.Contains(resourceType)) {
                typeFilter.SetValueWithoutNotify(resourceType);
            }
        }

        bool MatchesFilter(ResourceProfile item) {
            bool exclude = false;
            if (namePattern != null && namePattern != "") {
                exclude = !item.ResourceName.ToLower().Contains(namePattern.ToLower());
            }
            if (!exclude && classPattern != null) {
                exclude = !classPattern.IsAssignableFrom(item.GetType());
            }
            return !exclude;
        }

        void ReloadList() {
            listElements.Clear();
            InitializeList();
            listView.Rebuild();
        }

        void ApplyFilter(string namePattern) {
            this.namePattern = namePattern;
            InitializeList();
            listView.Rebuild();
        }

        void ApplyClassFilter(Type classPattern) {
            this.classPattern = classPattern;
            InitializeList();
            listView.Rebuild();
        }

        public void OnGUI() {
            if (editedProperty != null) {
                try {
                    header.text = editedProperty.propertyPath + " in " + editedProperty.serializedObject.targetObject;
                } catch (NullReferenceException) {
                    // that means the serialized object has been disposed and gone out of scope while the editor window is still open
                    // let's just close it and act as nothing happened
                    Debug.Log("edited property is gone.");
                    Close();
                }
                fileTypeLabel.text = "Resource name: " + resourceType.Name;
            }
            if (!listInitialized && resourceType != null) {
                InitializeList();
                listView.Rebuild();
                listInitialized = true;
                ClearBusyLabel();
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

        void ClearBusyLabel() {
            busyLabel.text = "";
        }

        // this function is heavily influenced by the API docs example at
        // https://docs.unity3d.com/Packages/com.unity.ui@1.0/api/UnityEngine.UIElements.ListView.html
        public void CreateGUI() {
            classNames.Add(typeof(ResourceProfile));

            VisualElement root = rootVisualElement;

            header = new Label("");
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(header);

            fileTypeLabel = new Label("any");
            fileTypeLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(fileTypeLabel);

            VisualElement filtersElement = new VisualElement() { style = { flexDirection = FlexDirection.Row } };
            root.Add(filtersElement);

            typeFilter = new UnityEditor.UIElements.PopupField<Type>(classNames, 0) { style = { flexGrow = 0.4f } };
            typeFilter.RegisterCallback<ChangeEvent<Type>>(x => ApplyClassFilter(x.newValue));
            filtersElement.Add(typeFilter);

            nameFilter = new TextField() { style = { flexGrow = 1 } };
            nameFilter.RegisterValueChangedCallback<string>(x => ApplyFilter(x.newValue));
            filtersElement.Add(nameFilter);

            busyLabel = new Label("Loading... Please wait.");
            busyLabel.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            busyLabel.style.color = Color.red;
            root.Add(busyLabel);

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
                l.tooltip = "Type: " + listElements[i].GetType().Name;
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

            nameFilter.Focus();
        }
    }

}

