using Godot;
using System;
using System.Diagnostics;

public partial class GravityPlatform : Node2D
{
    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private bool hasGravity = false;

    private Vector2 Velocity = new Vector2(0, 0);

    private AudioStreamPlayer2D fallingSfx;

    private AnimationPlayer animPlayer;

	public override void _Ready()
	{
        fallingSfx = GetNode<AudioStreamPlayer2D>("FallingSfx");

        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

	public override void _Process(double delta)
	{
        // Adds gravity if the platform has been triggered
        if (hasGravity)
        {
            Vector2 velocity = Velocity;

            velocity.Y += gravity * (float)delta;

            Velocity = velocity;

            this.GlobalPosition = new Vector2(this.GlobalPosition.X, this.GlobalPosition.Y + velocity.Y * (float)delta);
        }
        else
        {
            // Freeze the platform if it has not been triggered
            Velocity = Vector2.Zero;
        }
    }

    private void _on_area_2d_body_entered(Node2D body)
    {
        // When an object touches the platfrom, add gravity to the platform
        if (!hasGravity)
        {
            fallingSfx.Play();
            hasGravity = true;

            // Stop playing the platform animation
            animPlayer.Play("default");
        }
    }

    private void _on_dead_area_body_entered(Node2D body)
    {
        // Kill an enemy or player that is hit by the bottom of a falling platform
        if (hasGravity)
        {
            if (body is CharacterController)
            {
                CharacterController chr = body as CharacterController;
                chr.die();
            }
            else if (body is Enemy)
            {
                Enemy en = body as Enemy;
                en.die();
            }
        }
    }

}
