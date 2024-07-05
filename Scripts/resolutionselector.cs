using Godot;
using System;

public partial class resolutionselector : Control
{
    Vector2I[] modes;
    OptionButton optionButton;

    public override void _Ready()
    {
        optionButton = GetNode<OptionButton>("HBoxContainer/OptionButton");

        // Define an array of integer vectors to represent different resolutions
        modes = new Vector2I[]{new Vector2I(1152, 648), new Vector2I(1280, 720) , new Vector2I(1920, 1080) };

        // Format text and add items to the options 
        for (int i = 0; i < modes.Length; i++)
        {
            optionButton.AddItem(modes[i].X.ToString() + " x " + modes[i].Y.ToString());
        }
    }

    private void _on_option_button_item_selected(int index)
    {
        // Sets the window size according to the selected resolution
        DisplayServer.WindowSetSize(modes[index]);
    }
}
