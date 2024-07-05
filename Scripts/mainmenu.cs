using Godot;
using System;

public partial class mainmenu : Control
{
    private MarginContainer marginContainer;
   
    private optionsmenu optionsMenu;
    private levelselectmenu levelSelectMenu;
    private creditsmenu creditsMenu;

    private Panel panel;

    private ParallaxBackground bg;

    private Button startButton;

    public override void _Ready()
    {
        marginContainer = GetNode<MarginContainer>("MarginContainer");
        
        optionsMenu = GetNode<Control>("Panel/Optionsmenu") as optionsmenu;
        levelSelectMenu = GetNode<Control>("Panel/Levelselectmenu") as levelselectmenu;
        creditsMenu = GetNode<Control>("Panel/CreditsMenu") as creditsmenu;
        
        panel = GetNode<Panel>("Panel");

        startButton = GetNode<Button>("MarginContainer/VBoxContainer/StartButton");
        bg = GetNode<ParallaxBackground>("Animation/Background/ParallaxBackground");

        optionsMenu.Connect("exit_options_menu", new Callable(this, MethodName._on_exit_options_menu));
        
        levelSelectMenu.Connect("exit_level_select_menu", new Callable(this, MethodName._on_exit_level_select_menu));
        levelSelectMenu.Connect("exit_menu", new Callable(this, MethodName._on_exit_menu));

        creditsMenu.Connect("exit_credits_menu", new Callable(this, MethodName._on_exit_credits_menu));

        optionsMenu.Visible = false;
        levelSelectMenu.Visible = false;

        customGrabFocus();
    }

    public override void _Process(double delta)
    {
        // Close all other menus when 'esc' is pressed
        if (Input.IsActionPressed("pause"))
        {
            _on_exit_level_select_menu();
            _on_exit_options_menu();
            _on_exit_credits_menu();
        }
    }

    private void _on_start_button_pressed()
	{
        Core core = GetNode<Node>("/root/Core") as Core;
		
        // Load the first level
        core.startGame("res://Levels/Scenes/level1.1.tscn");

        //hideMenu();
	}


    private void _on_level_select_button_pressed()
	{
        // Hide main menu and show the level select menu
        panel.Visible = true;
        levelSelectMenu.Visible = true;
        marginContainer.Visible = false;

        levelSelectMenu.customGrabFocus();
    }
    private void _on_exit_level_select_menu()
    {
        // Hide level select menu and show the main menu
        panel.Visible = false;
        levelSelectMenu.Visible = false;
        marginContainer.Visible = true;

        customGrabFocus();
    }

    private void _on_options_button_pressed()
	{
        // Hide main menu and show the options menu
        panel.Visible = true;
        optionsMenu.Visible = true;
        marginContainer.Visible = false;

        optionsMenu.customGrabFocus();  
	}

    private void _on_exit_options_menu()
    {
        // Hide options menu and show the main menu
        panel.Visible = false;
        optionsMenu.Visible = false;
        marginContainer.Visible = true;

        customGrabFocus();
    }

    private void _on_credits_button_pressed()
    {
        // Hide main menu and show the credits menu
        panel.Visible = true;
        creditsMenu.Visible = true;
        marginContainer.Visible = false;

        creditsMenu.customGrabFocus();
    }

    private void _on_exit_credits_menu()
    {
        // Hide credits menu and show the main menu
        panel.Visible = false;
        creditsMenu.Visible = false;
        marginContainer.Visible = true;

        customGrabFocus();
    }
 
    private void _on_exit_menu()
    {
        //hideMenu();
    }

    private void _on_quit_button_pressed()
	{
		GetTree().Quit();
	}

    public void customGrabFocus()
    {
        // Sets the focus of the main menu to the start button
        startButton.GrabFocus();
    }

    // Funtion to hide the main menu and reset the view of the main menu for when it is shown again
    public void hideMenu()
    { 
        levelSelectMenu.Visible = false;
        optionsMenu.Visible= false;
        creditsMenu.Visible= false;

        marginContainer.Visible= true;

        Hide();

        // Hide the parallax background as well
        bg.Visible = false;
    }

    // Function to show the main menu and reset the view of the main menu
    public void showMenu()
    {
        Show();

        _on_exit_level_select_menu();
        _on_exit_options_menu();
        _on_exit_credits_menu();

        bg.Visible = true;
    }
}
