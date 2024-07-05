using Godot;
using System;

public partial class controlsmenu : Control
{
	private Button backButton;

	public override void _Ready()
	{
		AddUserSignal("exit_controls_menu");

		backButton = GetNode<Button>("MarginContainer/VBoxContainer2/BackButton");

		customGrabFocus();
	}

	public void customGrabFocus()
	{
		// Sets the focus of the menu to the back button
		backButton.GrabFocus();
	}

	private void _on_back_button_pressed()
	{
		// Signal that the controls menu has been exited
		EmitSignal("exit_controls_menu");
	}
}
