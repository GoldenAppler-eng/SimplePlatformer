using Godot;
using System;
using System.Diagnostics;

public partial class Switch : Interactive
{
	private AnimatedSprite2D animatedSprite2D;

    private AnimatedSprite2D button;

    private AudioStreamPlayer2D crankSfx;

    // The object the switch activates
    [Export]
	public Activatable connectedComponent;

	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		button = GetNode<AnimatedSprite2D>("ZButton");

        crankSfx = GetNode<AudioStreamPlayer2D>("CrankSfx");
	}

	public override void interact()
	{
        // Set the sprite of the switch to the pulled state
		animatedSprite2D.Frame = 1;

        // Activate the corresponding object
		connectedComponent.activate();
		crankSfx.Play();

        // Disable the switch so it cannot be interacted with again
		disabled = true;
	}

    public override void highlight()
    {
        // Show the Z button animation
        button.Visible = true;

        ShaderMaterial mat = animatedSprite2D.Material as ShaderMaterial;

        // Draw an outline around the switch
        mat.SetShaderParameter("line_thickness", 1);
    }

    public override void unHighlight()
    {
        // Hide the Z button animation
        button.Visible = false;

        ShaderMaterial mat = animatedSprite2D.Material as ShaderMaterial;

        // Remove the outline aroind the switch
        mat.SetShaderParameter("line_thickness", 0);
    }
}
