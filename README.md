# unity-toolbar-buttons

Some extra pre-defined buttons injecting to the toolbar with VisualElement using reflection

Now able to add in custom button via Attribute with static methods

https://twitter.com/BennyKokMusic/status/1356818085643124737?s=20

![Imgur](https://i.imgur.com/qfiGWKh.gif)

Support from 2021.1, 2021.2!

Using VisualElement under the hood


Originally inspired by https://github.com/marijnz/unity-toolbar-extender

## Install

```
upm from git url -> https://github.com/BennyKok/unity-toolbar-buttons.git
```

## Custom Toolbar Button
Very simple usage, the iconName will using the Unity internal icon

```csharp
[ToolbarButton(iconName = "Package Manager", tooltip = "Package Manager", order = 0)]
public static void ShowPackageManager()
{
    UnityEditor.PackageManager.UI.Window.Open("");
}
```

## Explore

Feel free to check me out!! :)

[Twitter](https://twitter.com/BennyKokMusic) | [Website](https://bennykok.com) | [AssetStore](https://assetstore.unity.com/publishers/28510)
