using Godot;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Reflection;

public partial class levelselectmenu : Control
{
    private Button selectButton;
    private Button backButton;

    private TextureRect preview;

    private ItemList list;

    [Export]
    public Texture2D[] previewList;

    public int numOfLevel;

    [Export]
    public int defaultLevel = 1;

    private int lastIndex = 0;

    private float[] levelName = new float[] {1.1f, 1.2f, 2.1f, 2.3f, 2.4f, 3.1f, 3.2f, 3.3f, 3.4f, 4.1f};

    public override void _Ready()
    {
        AddUserSignal("exit_menu");

        AddUserSignal("exit_level_select_menu");

        selectButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/SelectButton");
        backButton = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/BackButton");

        preview = GetNode<TextureRect>("MarginContainer/HBoxContainer/VBoxContainer/TextureRect");
        list = GetNode<ItemList>("MarginContainer/HBoxContainer/VBoxContainer2/ItemList");

        numOfLevel = previewList.Length;

        for (int i = 0; i < numOfLevel; i++)
        {
            list.AddItem("Level " + (i + 1).ToString());
        }

        customGrabFocus();

        _on_item_list_item_selected(defaultLevel - 1);
    }

    public override void _Process(double delta)
    {
        /**
         * Removed code that deselected all items on the list when either button in the menu was selected
         * 
         * if (!list.HasFocus())
         * {
         *     list.DeselectAll();
         * }
         *
         * Removed code that set the last index to either the first or last item when either button in the menu was selected
         * 
         *   if (backButton.HasFocus())
         *   {
         *      lastIndex = 0;
         *   }
         *
         *   if (selectButton.HasFocus())
         *   {
         *       lastIndex = numOfLevel - 1;
         *   }
         **/
    }

    private void _on_back_button_pressed()
    {
        // Reset the focus of the menu to the first level
        lastIndex = 0;

        // Signal that the level select menu has been exited
        EmitSignal("exit_level_select_menu");
    }

    private void _on_item_list_item_selected(int index)
    {
        // Change the preview image to corresponding preview of the selected level 
        preview.Texture = previewList[index];
    }

    private void _on_item_list_item_activated(int index)
    {
        // Get the path of the selected level via the array
        String levelPath = "res://Levels/Scenes/level" + levelName[index].ToString() + ".tscn";

        Core core = GetNode<Node>("/root/Core") as Core;
        
        // Change scenes to the level selected
        core.switchScene(levelPath, false);

        // Reset the focus of the menu to the first level
        lastIndex = 0;

        // Signal that all menus need to be exited
        EmitSignal("exit_menu");
    }

    private void _on_select_button_pressed()
    {
        // Select the selected level when select button is pressed
        _on_item_list_item_activated(list.GetSelectedItems()[0]);
    }

    private void _on_item_list_focus_entered()
    {
        // Sets the focus of the item list to the last index selected
        customGrabFocus();
    }

    public void customGrabFocus()
    {
        // Sets the focus of the menu to the last selected item in the item list
        list.GrabFocus();
        list.Select(lastIndex);

        // Change the preview image to the corresponding level
        preview.Texture = previewList[lastIndex];
    }
}
