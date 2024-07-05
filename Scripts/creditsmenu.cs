using Godot;
using System;

public partial class creditsmenu : Control
{
	private Button backButton;

	public override void _Ready()
	{
		AddUserSignal("exit_credits_menu");

		backButton = GetNode<Button>("MarginContainer/VBoxContainer/BackButton");

		customGrabFocus();
	}

	public void customGrabFocus()
	{
		// Sets the focus of the menu to the back button
		backButton.GrabFocus();
	}

	private void _on_back_button_pressed()
	{
		// Signal that the credits menu has been exited
		EmitSignal("exit_credits_menu");
	}
}
