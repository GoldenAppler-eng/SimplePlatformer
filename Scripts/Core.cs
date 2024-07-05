using Godot;
using System;
using System.Diagnostics;

public partial class Core : Node
{
	private Control sceneContainer;
    private Node2D explosionRenderer;

    private Control pauseMenu;
    private mainmenu mainMenu;

    private ParallaxBackground background;

    /** For debug purposes only
     * -> Hides and disables the player 
     * -> Allows free control of the camera
     * -> Spacebar pauses the game but does not open the pause menu
     */
    [Export]
    public bool camMode = false;

	public override void _Ready()
	{
		sceneContainer = GetNode<Control>("Scenes/SceneContainer");

        explosionRenderer = GetNode<Node2D>("Scenes/ExplosionRenderer");

        pauseMenu = GetNode<Control>("Scenes/UI/Pausemenu");
        mainMenu = GetNode<Control>("Scenes/UI/Mainmenu") as mainmenu;

        background = GetNode<ParallaxBackground>("Scenes/Background/ParallaxBackground");

        background.Visible = false;

        mainMenu.customGrabFocus();
        pauseMenu.ProcessMode = ProcessModeEnum.Disabled;
    }

    // Function to change scene in scene container node (Also used for restarting levels)
    public async void switchScene(String nextScene, Boolean isRestart)
    {
        Godot.Collections.Array<Node> currentScenes = sceneContainer.GetChildren();
        int sceneCount = currentScenes.Count;

        // Disable the pause menu so the player cannot pause the game while switching scenes
        pauseMenu.ProcessMode = ProcessModeEnum.Disabled;

        // Reset the global camera and player position variables on entering a new level
        if (!isRestart)
        {
            Global.playerPos = new Vector2(-1000, -1000);
            Global.cameraPos = new Vector2(-1000, -1000);
        }

        // Load the first level if there are no scenes in scene container
        if (sceneCount == 0)
        {
            startGame(nextScene);
        }

        // Transitions to the next level if there are one or more scenes in scene container
        if (sceneCount > 0)
        {
            level_transition transition = GetNode<CanvasLayer>("/root/LevelTransition") as level_transition;

            // Only show the transition animation on entering a new level
            if (!isRestart)
            {
                transition.slideToBlack();

                // Wait for the first transition animation to finish
                await ToSignal(transition, level_transition.SignalName.transition);
            }

            Node newScene = ResourceLoader.Load<PackedScene>(nextScene).Instantiate();

            // Set the new level scene invisible before adding it to the tree
            if (newScene is Node2D) 
            {
                Node2D scene2D = newScene as Node2D;
                scene2D.Visible = false;
            }

            sceneContainer.AddChild(newScene);

            // Pause and hide all levels previously in scene container 
            foreach (Node child in currentScenes)
            {
                child.GetTree().Paused = true;
                (child as Node2D).Visible = false;
            }

            // Show the new level scene
            if (newScene is Node2D)
            {
                Node2D scene2D = newScene as Node2D;
                scene2D.Visible = true;
            }

            // Wait a few milliseconds to allow the canvas to load 
            await ToSignal(GetTree().CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);

            // Remove all scenes except the new scene
            foreach (Node child in currentScenes)
            {
                sceneContainer.RemoveChild(child);
                child.QueueFree();
            }

            // Only show the transition animation on entering a new level
            if (!isRestart)
            {
                transition.slideFromBlack();
            }

            // Unpause the scene tree
            newScene.GetTree().Paused = false;
        }

        // Enable the pause menu
        pauseMenu.ProcessMode = ProcessModeEnum.Always;
    }

    // Uses switchScene() to load the same level as the current one
    public void restartLevel()
	{
        Godot.Collections.Array<Node> currentScenes = sceneContainer.GetChildren();
        int sceneCount = currentScenes.Count;

		if (sceneCount > 0)
		{
			foreach (Node child in currentScenes)
			{
				switchScene(child.SceneFilePath, true);

				break;
			}
		}
	}

    // Loads an explosion animation at a specified global position
    public void addExplosionAt(float x, float y)
    {
        Node2D explosion = ResourceLoader.Load<PackedScene>("res://Prefabs/explosioneffect.tscn").Instantiate() as Node2D;

        explosion.GlobalPosition = new Vector2(x, y);

        explosionRenderer.AddChild(explosion);
    }

    // Loads a specified starting scene when there are no scenes currently in scene container
    public async void startGame(String startScene)
    {
        // Disable the pause menu so the player cannot pause the game while switching scenes
        pauseMenu.ProcessMode = ProcessModeEnum.Disabled;

        // Reset the global camera and player global positions
        Global.playerPos = new Vector2(-1000, -1000);
        Global.cameraPos = new Vector2(-1000, -1000);

        level_transition transition = GetNode<CanvasLayer>("/root/LevelTransition") as level_transition;

        transition.slideToBlack();
        
        // Wait for the first transition to finish before proceeding
        await ToSignal(transition, level_transition.SignalName.transition);

        // Disable the main menu
        disableMainMenu();

        Node newScene = ResourceLoader.Load<PackedScene>(startScene).Instantiate();

        // Set the new scene invisible before adding it into the tree
        Node2D scene2D = newScene as Node2D;
        scene2D.Visible = false;

        sceneContainer.AddChild(newScene);
        
        // Pause the game
        newScene.GetTree().Paused = true;

        // Show the new scene
        scene2D.Visible = true;

        // Show the parallax background for levels
        background.Visible = true;

        // Wait for a few milliseconds to allow canvas to load
        await ToSignal(GetTree().CreateTimer(0.1), SceneTreeTimer.SignalName.Timeout);

        transition.slideFromBlack();

        //Unpause the game
        newScene.GetTree().Paused = false;

        // Enable the pause menu
        pauseMenu.ProcessMode = ProcessModeEnum.Always;
    }

    // Disables and hides main menu
    public void disableMainMenu()
    {
        mainMenu.hideMenu();

        mainMenu.ProcessMode = ProcessModeEnum.Disabled;
    }

    // Enables and shows main menu
    public void enableMainMenu()
    {
        mainMenu.showMenu();

        mainMenu.ProcessMode = ProcessModeEnum.Inherit;
    }
}
