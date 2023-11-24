using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace HJJJJ.DeskReach.Plugins.PluginMenu
{
    /// <summary>
    /// 菜单位置枚举
    /// </summary>
    [Flags]
    public enum MenuPosition
    {
        TopMenu = 1,
        Screen = 2,
        ToolBar = 4,
        ToolMenu = 5,
        StatusMenu = 6
    }

    /// <summary>
    /// 菜单项基类
    /// </summary>
    public abstract class MenuItem
    {
        public abstract MenuItemType ItemType { get; }
        public string Name { get; set; }

        public bool Enable { get; set; } = true;

        public Color FontColor { get; set; }
    }

    /// <summary>
    /// 下拉菜单项
    /// </summary>
    public class DropMenuItem : MenuItem
    {
        public List<MenuItem> DropItems { get; set; }

        public override MenuItemType ItemType => MenuItemType.Drop;
    }

    /// <summary>
    /// 按钮菜单项
    /// </summary>
    public class ButtonMenuItem : MenuItem
    {
        public Action<ButtonMenuItem, EventArgs> OnClick { get; set; }

        public override MenuItemType ItemType => MenuItemType.Button;
    }

    /// <summary>
    /// 输入菜单项
    /// <br/>调用输入框
    /// </summary>
    public class InputMenuItem : MenuItem
    {
        public string Pronpt { get; set; }
        public Action<string> OnInput { get; set; }

        public override MenuItemType ItemType => MenuItemType.Input;
    }

    /// <summary>
    /// 输入菜单项
    /// <br/>调用输入框
    /// </summary>
    public class ComboBoxMenuItem : MenuItem
    {
        public object[] Items { get; set; }
        public Action<MenuItem, ComboBoxSelectedEvnetArgs> OnIndexChanged { get; set; }
        public override MenuItemType ItemType => MenuItemType.ComboBox;
    }

    public class ColorSelectorMenuItem : MenuItem
    {
        public Action<MenuItem, ColorSelectedEventArgs> OnColorSelected { get; set; }
        public override MenuItemType ItemType => MenuItemType.ColorSelector;
    }

    public class LabelMenuItem : MenuItem
    {
        public override MenuItemType ItemType => MenuItemType.Lable;
    }

    public enum MenuItemType
    {
        Lable,
        Button,
        Input,
        Drop,
        ComboBox,
        ColorSelector,
    }
    public class ComboBoxSelectedEvnetArgs : EventArgs {
        public string Text { get; set; }

    }

    public class ButtonClickEventArgs : EventArgs{
        public int MyProperty { get; set; }

    }

    public class ColorSelectedEventArgs : EventArgs
    {
        public Color Color { get; set; }
    }
}
