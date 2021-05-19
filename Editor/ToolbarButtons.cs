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
        private const string scenesFolder = "Scenes";
        static ScriptableObject m_currentToolbar;
        static Type m_toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        static VisualElement parent;

        static ToolbarSceneButtons()
        {
            m_currentToolbar = null;
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static int lastInstanceID;

        static void OnUpdate()
        {
            if (m_currentToolbar == null)
            {
                var toolbars = Resources.FindObjectsOfTypeAll(m_toolbarType);
                m_currentToolbar = toolbars.Length > 0 ? (ScriptableObject)toolbars[0] : null;
                // UnityEngine.Debug.Log(m_currentToolbar != null);
            }

            // If the windows layour reloaded, we need to re create our GUI
            if (m_currentToolbar != null && parent != null && m_currentToolbar.GetInstanceID() != lastInstanceID)
            {
                parent.RemoveFromHierarchy();
                parent = null;
                lastInstanceID = m_currentToolbar.GetInstanceID();
            }

            if (m_currentToolbar != null && parent == null)
            {
                // foreach (var item in m_currentToolbar.GetType().GetRuntimeFields())
                //     UnityEngine.Debug.Log(item.Name + " " + item.FieldType + " " + item.IsPublic);

                var root = m_currentToolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
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

                        parent = null;

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
        }

        static void OnAttachToToolbar(VisualElement parent)
        {
            // #if !UNITY_2021_1_OR_NEWER
            //             parent.Add(CreateToolbarButton("Search On Icon", ShowQuickSearch));
            // #endif
            parent.Add(CreateToolbarButton("Package Manager", ShowPackageManager));
            parent.Add(CreateToolbarButton("Settings", ShowSettings));
            parent.Add(CreateToolbarButton("UnityEditor.SceneHierarchyWindow", ShowScenes));
            parent.Add(CreateToolbarButton("UnityEditor.GameView", ShowBootstrapScene));
        }

        static VisualElement CreateToolbarButton(string icon, Action onClick)
        {
            Button buttonVE = new Button(onClick);
            FitChildrenStyle(buttonVE);

            VisualElement iconVE = new VisualElement();
            iconVE.AddToClassList("unity-editor-toolbar-element__icon");
#if UNITY_2021_2_OR_NEWER
            iconVE.style.backgroundImage = Background.FromTexture2D((Texture2D)EditorGUIUtility.IconContent(icon).image);
            iconVE.style.height = 16;
            iconVE.style.width = 16;
            iconVE.style.alignSelf = Align.Center;
#else
            iconVE.style.backgroundImage = Background.FromTexture2D(EditorGUIUtility.FindTexture(icon));
#endif
            buttonVE.Add(iconVE);

            return buttonVE;
        }

        static void FitChildrenStyle(VisualElement element)
        {
            element.AddToClassList("unity-toolbar-button");
            element.AddToClassList("unity-editor-toolbar-element");
            element.RemoveFromClassList("unity-button");
#if UNITY_2021_2_OR_NEWER
            element.style.paddingRight = 8;
            element.style.paddingLeft = 8;
            element.style.justifyContent = Justify.Center;
            element.style.display = DisplayStyle.Flex;
            element.style.borderTopLeftRadius = 2;
            element.style.borderTopRightRadius = 2;
            element.style.borderBottomLeftRadius = 2;
            element.style.borderBottomRightRadius = 2;

            element.style.marginRight = 1;
            element.style.marginLeft = 1;
#else
            element.style.marginRight = 2;
            element.style.marginLeft = 2;
#endif
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

        private static void ShowScenes()
        {
            var a = new GenericMenu();
            var ls = GetAtPath<UnityEngine.Object>(scenesFolder);
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

        private static void ShowBootstrapScene()
        {
            var bootstrapPath = "Assets/" + scenesFolder + "/bootstrap.unity";
            if (!Application.isPlaying && File.Exists(bootstrapPath))
                EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Additive);
            Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
            SceneView.FrameLastActiveSceneView();
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
