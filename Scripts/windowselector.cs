using Godot;
using System;
using System.Diagnostics;
using System.Transactions;

public partial class windowselector : Control
{
	private String[] modes;
	public OptionButton optionButton;

	public override void _Ready()
	{
		optionButton = GetNode<OptionButton>("HBoxContainer/OptionButton");
		modes = new String[]{"Windowed", "Windowed Borderless", "Fullscreen"};

		// Sets the top neighbour of the options menu to the top neighbour of the selector node for smoother navigation
		optionButton.FocusNeighborTop = "../../" + FocusNeighborTop;

		// Add items to the options via code
		foreach (String mode in modes) 
		{
			optionButton.AddItem(mode);
		}
	}

    private void _on_option_button_item_selected(int index)
	{
		// Change the window mode according to the mode selected
		switch(index)
		{
			case 0:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                break;
			
			case 1:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
                break;				

			case 2:
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
                DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
                break;
		}
	}

	public void customGrabFocus()
	{
		// Sets the focus of the selector node to the options button
		optionButton.GrabFocus();
	}
}
