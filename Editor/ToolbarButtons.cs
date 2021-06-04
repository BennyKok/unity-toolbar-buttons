using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.UIElements;
using System.Linq;

namespace BennyKok.ToolbarButtons
{
    [Conditional("UNITY_EDITOR"), AttributeUsage(System.AttributeTargets.Method)]
    public class ToolbarButtonAttribute : Attribute
    {
        public string tooltip;
        public string iconName;

        public int order = 0;

        public ToolbarButtonAttribute() { }

        public ToolbarButtonAttribute(string iconName, string tooltip = null, int order = 0)
        {
            this.tooltip = tooltip;
            this.iconName = iconName;
            this.order = order;
        }
    }

    [InitializeOnLoad]
    public class ToolbarSceneButtons
    {
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
            var methods = TypeCache.GetMethodsWithAttribute<ToolbarButtonAttribute>();
            var allAttributes = new Dictionary<MethodInfo, ToolbarButtonAttribute>();
            foreach (var method in methods)
            {
                var attribute = (ToolbarButtonAttribute)method.GetCustomAttributes(typeof(ToolbarButtonAttribute), false).First();
                if (attribute != null)
                    allAttributes.Add(method, attribute);
            }

            foreach (var attr in allAttributes.OrderByDescending(x => x.Value.order))
            {
                parent.Add(CreateToolbarButton(attr.Value.iconName, () => attr.Key.Invoke(null, null)));
            }
        }

        static VisualElement CreateToolbarButton(string icon, Action onClick, string tooltip = null)
        {
            Button buttonVE = new Button(onClick);
            buttonVE.tooltip = tooltip;
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
    }
}
