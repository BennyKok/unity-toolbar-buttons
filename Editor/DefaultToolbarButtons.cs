using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace BennyKok.ToolbarButtons
{
    public class DefaultToolbarButtons
    {
        private const string scenesFolder = "Scenes";
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

        [ToolbarButton("d_winbtn_win_max", "Open Terminal")]
        public static void OpenTerminal()
        {
            var projectPath = Directory.GetParent(Application.dataPath).FullName;

            Process cmd = new Process();
#if UNITY_EDITOR_WIN
            cmd.StartInfo.FileName = "cmd.exe";
#endif
#if UNITY_EDITOR_OSX
            cmd.StartInfo.FileName = "/Applications/Utilities/Terminal.app/Contents/MacOS/Terminal";
#endif
            cmd.StartInfo.WorkingDirectory = projectPath;
            cmd.Start();
        }

        [ToolbarButton("Folder Icon", "Open Folder")]
        public static void OpenFolder()
        {
            var projectPath = Directory.GetParent(Application.dataPath).FullName;

            Process cmd = new Process();
#if UNITY_EDITOR_WIN
            cmd.StartInfo.FileName = "explorer.exe";
#endif
            // #if UNITY_EDITOR_OSX
            //             cmd.StartInfo.FileName = "/bin/bash";
            // #endif
            cmd.StartInfo.Arguments = projectPath;
            cmd.Start();
        }

        [ToolbarButton(iconName = "Package Manager", tooltip = "Package Manager")]
        public static void ShowPackageManager()
        {
            UnityEditor.PackageManager.UI.Window.Open("");
        }


        [ToolbarButton("Settings", "Show Settings")]
        public static void ShowSettings()
        {
            var a = new GenericMenu();
            a.AddItem(new GUIContent("Project"), false, () => EditorApplication.ExecuteMenuItem("Edit/Project Settings..."));
            a.AddItem(new GUIContent("Preferences"), false, () => EditorApplication.ExecuteMenuItem("Edit/Preferences..."));
            a.ShowAsContext();
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(string Source, string Find, string Replace)
        {
            int place = Source.LastIndexOf(Find);

            if (place == -1)
                return Source;

            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        [ToolbarButton("UnityEditor.SceneHierarchyWindow", "Show Scenes")]
        [Shortcut("Show Scenes", KeyCode.S)]
        public static void ShowScenes()
        {
            var sceneList = AssetDatabase.GetAllAssetPaths().Where(s => s.EndsWith(".unity")).ToList();
            sceneList.Sort();

            var a = new GenericAdvancedDropdown("Scenes");
            foreach (var p in sceneList)
            {
                string label = ReplaceLast(p, ".unity", "");
                label = ReplaceFirst(label, "Assets/", "");
                a.AddItem(label, () =>
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
            // a.AddSeparator("");
            a.AddItem("New Scene +", () =>
            {
                EditorApplication.ExecuteMenuItem("File/New Scene");
            });
            a.ShowAsContext(10);
        }

        [ToolbarButton("UnityEditor.GameView", "Show Bootstrap Scene")]
        public static void ShowBootstrapScene()
        {
            var bootstrapPath = "Assets/" + scenesFolder + "/bootstrap.unity";
            if (!Application.isPlaying && File.Exists(bootstrapPath))
                EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Additive);
            Selection.activeGameObject = GameObject.FindGameObjectWithTag("Player");
            SceneView.FrameLastActiveSceneView();
        }

        public static void ShowQuickSearch()
        {
            EditorApplication.ExecuteMenuItem("Help/Quick Search");
        }

    }
}