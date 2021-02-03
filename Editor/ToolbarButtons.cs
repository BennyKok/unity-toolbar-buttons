using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace BennyKok.ToolbarButtons
{
    [InitializeOnLoad]
    public class ToolbarSceneButtons
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle commandButtonStyle;
            public static readonly GUIStyle commandButtonStylePadding;

            static ToolbarStyles()
            {
                commandButtonStyle = new GUIStyle("Command")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    margin = new RectOffset(),
                    padding = new RectOffset()
                };

                commandButtonStylePadding = new GUIStyle("Command")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    margin = new RectOffset(),
                    padding = new RectOffset(2, 2, 2, 2)
                };
            }
        }

        static ToolbarSceneButtons()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
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

        static void OnToolbarGUI()
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorGUIUtility.IconContent("Search On Icon", "Quick Search"), ToolbarStyles.commandButtonStylePadding))
            {
                EditorApplication.ExecuteMenuItem("Help/Quick Search");
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("Package Manager", "Package Manager"), ToolbarStyles.commandButtonStyle))
            {
                UnityEditor.PackageManager.UI.Window.Open("");
            }
            if (GUILayout.Button(EditorGUIUtility.IconContent("Settings", "Settings"), ToolbarStyles.commandButtonStyle))
            {
                var a = new GenericMenu();
                a.AddItem(new GUIContent("Project"), false, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
                a.AddItem(new GUIContent("Preferences"), false, () => EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
                a.ShowAsContext();
            }

            GUILayout.Space(20);

            if (GUILayout.Button(EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow"), ToolbarStyles.commandButtonStyle))
            {
                // Vector2 position = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                // SearchWindow.Open<SceneSearch>(new SearchWindowContext(position), SceneSearch.Instance);
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

            // if (GUILayout.Button(EditorGUIUtility.IconContent("UnityEditor.GameView"), ToolbarStyles.commandButtonStyle))
            // {
            //     if (!Application.isPlaying)
            //         EditorSceneManager.OpenScene("Assets/#Scenes/bootstrap.unity", OpenSceneMode.Additive);
            //     Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
            //     SceneView.FrameLastActiveSceneView();
            // }

            GUILayout.Space(20);

        }
    }
}
