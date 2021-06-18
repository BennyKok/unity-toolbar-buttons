using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// Little help from https://qiita.com/shogo281/items/fb24cf7d28f06822527e

namespace BennyKok.ToolbarButtons
{
    public class GenericAdvancedDropdown : AdvancedDropdown
    {
        private Dictionary<string, Action> pathToActionMap = new Dictionary<string, Action>();
        private Dictionary<int, Action> idToActionMap = new Dictionary<int, Action>();
        private string title = "Items";

        public GenericAdvancedDropdown(AdvancedDropdownState state) : base(state) { }
        public GenericAdvancedDropdown() : base(new AdvancedDropdownState()) { }
        public GenericAdvancedDropdown(string title) : base(new AdvancedDropdownState())
        {
            this.title = title;
        }

        public void ShowAsContext(int minLineCount = -1)
        {
            Vector2 size = new Vector2(200, 100);

            if (minLineCount > 0)
            {
                size.y = minLineCount * EditorGUIUtility.singleLineHeight;
                this.minimumSize = size;
            }

            var r = new Rect(Event.current.mousePosition - new Vector2(size.x / 2, size.y), size);
            Show(r);
        }

        public void AddItem(string label, Action action)
        {
            pathToActionMap.Add(label, action);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(title);

            foreach (var item in pathToActionMap)
            {
                var splitStrings = item.Key.Split('/');
                var parent = root;
                AdvancedDropdownItem lastItem = null;

                foreach (var str in splitStrings)
                {
                    var foundChildItem = parent.children.FirstOrDefault(item => item.name == str);

                    if (foundChildItem != null)
                    {
                        parent = foundChildItem;
                        lastItem = foundChildItem;
                        continue;
                    }

                    var child = new AdvancedDropdownItem(str);
                    parent.AddChild(child);

                    parent = child;
                    lastItem = child;
                }

                if (lastItem != null)
                {
                    idToActionMap.Add(lastItem.id, item.Value);
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);

            if (idToActionMap.TryGetValue(item.id, out var action))
            {
                action();
            }
        }
    }
}