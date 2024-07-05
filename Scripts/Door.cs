using Godot;
using System;

public partial class Door : Activatable
{
	private AnimatedSprite2D animatedSprite;

	private AnimatedSprite2D button;

	private AudioStreamPlayer2D enterSfx;

    // Determine whether the door is opened at the beginning of the level
	[Export]
	public bool localDisabled;

    // Stores the path of the scene that the door directs to
	[Export]
	public String loadScene;

    private bool interacted = false;

	public override void _Ready()
	{
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        button = GetNode<AnimatedSprite2D>("ZButton");

		enterSfx = GetNode<AudioStreamPlayer2D>("EnterSfx");

        disabled = true;
        
        // Open the door if it should be opened in the beginning
		if (!localDisabled) activate();
	}

    public override void activate()
    {
        // Set the door sprite to open
		animatedSprite.Frame = 1;

        // Allow the player to interact with the door
		disabled = false;
    }

    public override void interact()
    {
        // Only allow the player to interact with the door once
        if (interacted) return;

        enterSfx.Play();

        interacted = true;

        // Change the scene during a period of idle
        CallDeferred(MethodName.changeScene);
    }

    private async void changeScene()
	{
        CharacterController player = GetParent().GetNode<CharacterBody2D>("Player") as CharacterController;

        // Play the animation for the player entering a door
        player.enterDoor();

        // Wait for the player animation to finish
        await ToSignal(player, CharacterController.SignalName.playerFadeOut);

        Core core = GetTree().Root.GetNode<Node>("Core") as Core;

        // Change the scene 
        core.switchScene(loadScene, false);
    }

    public override void highlight()
    {
        // Show the Z button animation
        button.Visible = true;
    }

    public override void unHighlight()
    {
        // Hide the Z button animation
        button.Visible = false;
    }
}
