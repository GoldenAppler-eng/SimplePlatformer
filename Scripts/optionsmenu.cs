using Godot;
using System;
using System.Diagnostics;

public partial class optionsmenu : Control
{
    private windowselector windowSelector;
    private Button backButton;

    public override void _Ready()
    {
        windowSelector = GetNode<Control>("MarginContainer/VBoxContainer/Windowselector") as windowselector;
        backButton = GetNode<Button>("MarginContainer/VBoxContainer2/BackButton");

        // Set the bottom neighbour of the back button to the window selector's option menu for smoother navigation
        backButton.FocusNeighborBottom = backButton.GetPathTo(windowSelector.optionButton);

        windowSelector.customGrabFocus();

        AddUserSignal("exit_options_menu");
    }  

    private void _on_back_button_pressed()
	{
        // Signal that the options menu has been exited
		EmitSignal("exit_options_menu");
	}

    public void customGrabFocus()
    {
        // Sets the focus of the menu to the window selector's focus
        windowSelector.customGrabFocus();
    }
}
