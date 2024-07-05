using Godot;
using System;

public partial class gameovermenu : Control
{
	private Button backButton;

	public override void _Ready()
	{
		backButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/BackButton");

		customGrabFocus();
	}

	public void customGrabFocus()
	{
		// Set the focus of the menu to the back button
		backButton.GrabFocus();
	}

	private void _on_back_button_pressed()
	{
		Core core = GetNode<Node>("/root/Core") as Core;

		// Return to the main menu
		core.enableMainMenu();

		// Delete the game over menu after back button is pressed
		QueueFree();
	}
}
