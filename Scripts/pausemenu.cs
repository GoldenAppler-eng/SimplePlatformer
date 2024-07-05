using Godot;
using System;

public partial class pausemenu : Control
{
	private optionsmenu optionsMenu;
	private levelselectmenu levelSelectMenu;
	private controlsmenu controlsMenu;

	private MarginContainer marginContainer;
    
	private Button resumeButton;
	
	public override void _Ready()
    {
		optionsMenu = GetNode<Control>("Panel/Optionsmenu") as optionsmenu;
        levelSelectMenu = GetNode<Control>("Panel/Levelselectmenu") as levelselectmenu;
		controlsMenu = GetNode<Control>("Panel/ControlsMenu") as controlsmenu;
	
        marginContainer = GetNode<MarginContainer>("Panel/MarginContainer");

        resumeButton = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ResumeButton");

        optionsMenu.Connect("exit_options_menu", new Callable(this, MethodName._on_exit_options_menu));
        
		levelSelectMenu.Connect("exit_level_select_menu", new Callable(this, MethodName._on_exit_level_select_menu));
		levelSelectMenu.Connect("exit_menu", new Callable(this, MethodName._on_exit_menu));

		controlsMenu.Connect("exit_controls_menu", new Callable(this, MethodName._on_exit_controls_menu));

		customGrabFocus();

        optionsMenu.Visible = false;
		levelSelectMenu.Visible = false;
        Visible = false;
    }

    public override void _Process(double delta)
	{
		// Pause or unpause the game when 'esc' is pressed
		if (Input.IsActionJustPressed("pause"))
		{
			if (!GetTree().Paused) {
                GetTree().Paused = true;

                showMenu();
            }
			else
			{
				_on_resume_button_pressed();
			}
				
		}

		// Restart the level when 'R' is pressed
		if (Input.IsActionJustPressed("restart"))
		{
			_on_restart_button_pressed();
		}
	}

	// Resume the game and hide the pause menu
	private void _on_resume_button_pressed()
	{
		GetTree().Paused = false;

		hideMenu();
    }

	// Restart the level
	private void _on_restart_button_pressed()
	{
		Control sceneContainer = GetNode<Control>("/root/Core/Scenes/SceneContainer");
        
		Godot.Collections.Array<Node> currentScenes = sceneContainer.GetChildren();
        int sceneCount = currentScenes.Count;

        if (sceneCount > 0)
		{	
			// Look for the player node and activate its restart function
			foreach (Node child in currentScenes)
			{
                CharacterController player = child.GetNode<CharacterBody2D>("Player") as CharacterController;

                player.restart();

				hideMenu();
            }
        }
	}

  
	// Hide pause menu and set all other menus invisible again
	private void hideMenu()
	{
        optionsMenu.Visible = false;
		levelSelectMenu.Visible = false;
		controlsMenu.Visible = false;
		marginContainer.Visible = true;

        Visible = false;
    }


	// Show pause menu and set all other menus invisible
	private void showMenu()
	{
        _on_exit_options_menu();
        _on_exit_level_select_menu();

        Visible = true;
		customGrabFocus();
    }

	// Open options menu
    private void _on_options_button_pressed()
	{
		optionsMenu.Visible = true;
		marginContainer.Visible = false;

        optionsMenu.customGrabFocus();
    }

	// Exit options menu
    private void _on_exit_options_menu()
	{
        optionsMenu.Visible = false;
        marginContainer.Visible = true;

		customGrabFocus();
    }

	// Open level select menu
	private void _on_level_select_button_pressed()
	{
        levelSelectMenu.Visible = true;
        marginContainer.Visible = false;

		levelSelectMenu.customGrabFocus();
    }

	// Exit level select menu
    private void _on_exit_level_select_menu()
    {
        levelSelectMenu.Visible = false;
        marginContainer.Visible = true;

		customGrabFocus();
    }

	// Open controls menu
	private void _on_controls_button_pressed()
	{
		controlsMenu.Visible = true;
		marginContainer.Visible = false;

		controlsMenu.customGrabFocus();
	}

	// Exit controls menu
	private void _on_exit_controls_menu()
	{
        controlsMenu.Visible = false;
        marginContainer.Visible = true;

		customGrabFocus();
    }

	// Quit the game
    private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

	// Exit all the menus
	private void _on_exit_menu()
	{
		_on_resume_button_pressed();
	}

	public void customGrabFocus()
	{
		// Sets the focus of the pause menu to the resume button
		resumeButton.GrabFocus();
	}
}