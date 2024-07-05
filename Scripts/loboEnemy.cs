using Godot;
using System;
using System.Diagnostics;
using static Godot.DisplayServer;

public partial class loboEnemy : Enemy
{
    private AudioStreamPlayer2D explosionSfx;

    // Spawnpoint marks the initial position of the loboEnemy jumps out of
    [Export]
    public Marker2D spawnpoint;

    // Deathpoint marks the position the loboEnemy moves to for its death
    [Export]
    public Marker2D deathpoint;

    // Stop point marks the position the loboEnemy moves to and stops at after spawning
    [Export]
    public Marker2D stoppoint;

    // Flags whether the loboEnemy is in the state of initializing
    private bool initializing = false;

    // Flags whether the loboEnemy is in the state of setting up its death
    private bool goToDeathPoint = false;

    private float initialJumpVelocity;
    private float initialSpeed;
    private float initialGravity;

    private float riseJumpVelocity;

    public override void _Ready()
	{
		base._Ready();

        AddUserSignal("reachedStopPoint");
        AddUserSignal("reachedDeathPoint");

        Speed = Speed * (GetParent() as Node2D).Scale.X;
        JumpVelocity = JumpVelocity * (GetParent() as Node2D).Scale.Y;

        initialSpeed = Speed;
        initialJumpVelocity = JumpVelocity;
        riseJumpVelocity = JumpVelocity * 2.3f;

        gravity = gravity * (GetParent() as Node2D).Scale.Y;

        initialGravity = gravity;

        explosionSfx = GetNode<AudioStreamPlayer2D>("ExplosionSfx");
    }

    // Activates the initialization animation
    public void startInitalSequence()
    {
        // Spawn the loboEnemy at its spawnpoint
        GlobalPosition = spawnpoint.GlobalPosition;

        initializing = true;
    }

    // Tells the loboEnemy to head to its death point
    public void goToDeath()
    {
        goToDeathPoint = true;
    }

    // Handles when the loboEnemy jumps
    public override void handleJumpControls()
    {
        JumpVelocity = initialJumpVelocity;

        if (initializing)
        {
            // Set the jump velocity to reach the stop point
            JumpVelocity = getSpecificJumpVelocity(stoppoint.GlobalPosition);

            // Handles first jump right after spawning
            if (GlobalPosition.X == spawnpoint.GlobalPosition.X)
            {
                // Set the horizontal velocity to reach a distance from the spawnpoint
                float velocityX = getSpecificHorizontalVelocity(spawnpoint.GlobalPosition + new Vector2(100 * (GetParent() as Node2D).Scale.X, 0), riseJumpVelocity);

                Velocity = new Vector2(velocityX, riseJumpVelocity);

                Speed = velocityX;

                moveMode = true;
                MoveAndSlide();

                jumpSfx.Play();
            }
            else if (IsOnFloor())
            {
                // Set the horizontal speed back to normal once the loboEnemy has reached the ground
                Speed = initialSpeed;
            }

            // Set the loboEnemy's direction towards the stop point
            if (IsOnFloor()) direction = new Vector2(Mathf.Sign(stoppoint.GlobalPosition.X - GlobalPosition.X), 0);

            // Jump until the loboEnemy reaches near the stop point
            if ((Mathf.Abs(GlobalPosition.X - stoppoint.GlobalPosition.X) <= 25.0f) && IsOnFloor() && moveMode && !jumpSignal)
            {
                // Enter the idle state of enemy
                moveMode = false;
                jumpSignal = false;

                waitTimer.Stop();
                idleTimer.Start();

                // Flag that the loboEnemy is no longer initializing
                initializing = false;

                // Set the direction to the right to account for the loboEnemy switching directions
                direction = new Vector2(1, 0);

                // Tell the animation handler that the loboEnemy has reached the stop point
                EmitSignal("reachedStopPoint");
            }
        }
        else if (goToDeathPoint) 
        {
            // Set the jump velocity to reach the death point
            JumpVelocity = getSpecificJumpVelocity(deathpoint.GlobalPosition);

            // Set the direction towards the death point
            if (IsOnFloor()) direction = new Vector2(Mathf.Sign(deathpoint.GlobalPosition.X - GlobalPosition.X), 0);

            // Jump until the loboEnemy has reached near the death point
            if ((Mathf.Abs(GlobalPosition.X - deathpoint.GlobalPosition.X) <= 25.0f) && IsOnFloor() && moveMode && !jumpSignal)
            {
                // Disable the loboEnemy
                moveMode = false;
                jumpSignal = false;

                waitTimer.Stop();
                idleTimer.Stop();

                disabled = true;

                // Make the loboEnemy face the left
                direction = -direction;

                // Tell the animation handler that the loboEnemy has reached the death point
                EmitSignal("reachedDeathPoint");
            }
        }
        else
        {
            // Move like a regular enemy
            base.handleJumpControls();
        }
    }

    // Returns a horizontal velocity to reach a specified horizontal distance with a specified velocity for jumping
    private float getSpecificHorizontalVelocity(Vector2 targetPosition, float givenJumpVelocity) 
    {
        float R = Mathf.Abs(targetPosition.X - GlobalPosition.X);

        return R * gravity / (2 * -givenJumpVelocity);
    }

    // Returns a velocity for jumping to reach a specified horizontal distance
    private float getSpecificJumpVelocity(Vector2 targetPosition)
    {
        float R = Mathf.Abs(targetPosition.X - GlobalPosition.X);

        // If the distance is too far away, just jump with regular velocity
        if (R >= 2 * Speed * initialJumpVelocity / gravity)
        {
            return initialJumpVelocity;
        }

        return R * gravity / (2 * Speed);
    }

    // Overrides the base enemy class's function for colliding with players
    public override void handleCollisionWithPlayer()
    {
        return;
    }

    public override void die()
    {
        if (!isDead)
        {    
            // Create an explosion on death

            animatedSprite2D.Play("explosion");
            explosionSfx.Play();

            isDead = true;
        }
    }

    // Removes the loboEnemy from tree after it has exploded
    private void _on_animated_sprite_2d_animation_finished()
    {
        if (animatedSprite2D.Animation == "explosion") 
            QueueFree(); 
    }
}
