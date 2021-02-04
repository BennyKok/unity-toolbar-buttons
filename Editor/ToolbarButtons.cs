using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace BennyKok.ToolbarButtons
{
    [InitializeOnLoad]
    public class ToolbarSceneButtons
    {
        static ScriptableObject m_currentToolbar;
        static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static VisualElement parent;

        static void OnUpdate()
        {
            // Find toolbar
            if (m_currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                // UnityEngine.Debug.Log(m_currentToolbar != null);
            }

            if (m_currentToolbar != null)
            {
                // foreach (var item in m_currentToolbar.GetType().GetRuntimeFields())
                //     UnityEngine.Debug.Log(item.Name + " " + item.FieldType + " " + item.IsPublic);

                var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic |
                         BindingFlags.Instance);
                // UnityEngine.Debug.Log(root);
                if (root != null)
                {
                    var rawRoot = root.GetValue(m_currentToolbar);
                    // UnityEngine.Debug.Log(rawRoot != null);
                    if (rawRoot != null)
                    {
                        // UnityEngine.Debug.Log("Attaching");
                        // UnityEngine.Debug.Log(rawRoot.GetType());
                        var mRoot = rawRoot as VisualElement;
                        // UnityEngine.Debug.Log(mRoot.name);

                        var toolbarZoneLeftAlign = mRoot.Q("ToolbarZoneLeftAlign");

                        if (parent != null)
                            parent.RemoveFromHierarchy();

                        parent = new VisualElement()
                        {
                            style ={
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };
                        parent.Add(new VisualElement()
                        {
                            style = {
                                flexGrow = 1,
                            }
                        });
                        OnAttachToToolbar(parent);
                        toolbarZoneLeftAlign.Add(parent);
                    }
                }
            }

            EditorApplication.update -= OnUpdate;
        }

        static void OnAttachToToolbar(VisualElement parent)
        {
#if !UNITY_2021_OR_NEWER
            parent.Add(CreateToolbarButton("Search On Icon", ShowQuickSearch));
#endif
            parent.Add(CreateToolbarButton("Package Manager", ShowPackageManager));
            parent.Add(CreateToolbarButton("Settings", ShowSettings));
            parent.Add(CreateToolbarButton("UnityEditor.SceneHierarchyWindow", ShowScenes));
        }

        static VisualElement CreateToolbarButton(string icon, Action onClick)
        {
            Button buttonVE = new Button(onClick);
            FitChildrenStyle(buttonVE);

            VisualElement iconVE = new VisualElement();
            iconVE.AddToClassList("unity-editor-toolbar-element__icon");
            iconVE.style.backgroundImage = Background.FromTexture2D(EditorGUIUtility.FindTexture(icon));
            buttonVE.Add(iconVE);

            return buttonVE;
        }

        static void FitChildrenStyle(VisualElement element)
        {
            element.AddToClassList("unity-toolbar-button");
            element.AddToClassList("unity-editor-toolbar-element");
            element.RemoveFromClassList("unity-button");
            element.style.marginRight = 0;
            element.style.marginLeft = 0;
        }

        static ToolbarSceneButtons()
        {
            m_currentToolbar = null;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        public static T[] GetAtPath<T>(string path)
        {
            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

            foreach (string fileName in fileEntries)
            {
                string temp = fileName.Replace("\\", "/");
                int index = temp.LastIndexOf("/");
                string localPath = "Assets/" + path;

                if (index > 0)
                    localPath += temp.Substring(index);

                System.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }

            T[] result = new T[al.Count];

            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }

        // static void OnToolbarGUI()
        // {
        //     GUILayout.FlexibleSpace();

        //     if (GUILayout.Button(EditorGUIUtility.IconContent("Search On Icon", "Quick Search"), ToolbarStyles.commandButtonStylePadding))
        //         ShowQuickSearch();
        //     if (GUILayout.Button(EditorGUIUtility.IconContent("Package Manager", "Package Manager"), ToolbarStyles.commandButtonStyle))
        //         ShowPackageManager();
        //     if (GUILayout.Button(EditorGUIUtility.IconContent("Settings", "Settings"), ToolbarStyles.commandButtonStyle))
        //         ShowSettings();

        //     GUILayout.Space(20);

        //     if (GUILayout.Button(EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow"), ToolbarStyles.commandButtonStyle))
        //         ShowScenes();

        //     GUILayout.Space(20);
        // }

        private static void ShowScenes()
        {
            var a = new GenericMenu();
            var ls = GetAtPath<UnityEngine.Object>("Scenes");
            foreach (var l in ls)
            {
                var p = AssetDatabase.GetAssetPath(l);
                var n = Path.GetFileName(p);
                if (n.EndsWith(".unity"))
                {
                    a.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(p)), false, () =>
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(p, OpenSceneMode.Single);
                            if (p == "bootstrap")
                            {
                                Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
                                SceneView.FrameLastActiveSceneView();
                            }
                        }
                    });
                }
            }
            a.ShowAsContext();
        }

        private static void ShowQuickSearch()
        {
            EditorApplication.ExecuteMenuItem("Help/Quick Search");
        }

        private static void ShowSettings()
        {
            var a = new GenericMenu();
            a.AddItem(new GUIContent("Project"), false, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
            a.AddItem(new GUIContent("Preferences"), false, () => EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
            a.ShowAsContext();
        }

        private static void ShowPackageManager()
        {
            UnityEditor.PackageManager.UI.Window.Open("");
        }
    }
}
