using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class Enemy : CharacterBody2D
{
	public float Speed = 150.0f;
	public float JumpVelocity = -350.0f;

	[Export]
	public int hopCount = 0;

	[Export]
	public bool disabled = false;

	[Export]
	public bool initDirectionLeft = true;

	private CharacterBody2D player;

	public AnimatedSprite2D animatedSprite2D;

    public Timer waitTimer;
    public Timer idleTimer;

	// Flags whether the enemy is in moving state (true) or idle state (false)
    public bool moveMode = false;

	// Flags whether the enemy needs to jump during moving state
    public bool jumpSignal = false;

    public int currentHopCount = 0;

    public Vector2 direction;

	public bool isDead = false;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	public AudioStreamPlayer2D jumpSfx;

	public override void _Ready()
	{
		animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        waitTimer = GetNode<Timer>("WaitTimer");
		idleTimer = GetNode<Timer>("IdleTimer");

		jumpSfx = GetNode<AudioStreamPlayer2D>("JumpSfx");

		direction = initDirectionLeft ? new Vector2(1, 0) : new Vector2(-1, 0);

        idleTimer.Start();
		waitTimer.Stop();
    }

	public override void _Process(double delta)
	{
		if (!isDead && !disabled)
		{
			Vector2 velocity = Velocity;
			
			handleJumpControls();

			velocity = Velocity;

            // Add the gravity
            if (!IsOnFloor())
			{
				velocity.Y += gravity * (float)delta;

				// Only move horizontally when in moving state
				if (moveMode) velocity.X = Speed * direction.X;

				// Handles animation based on vertical velocity
				if (velocity.Y > 0)
				{
					animatedSprite2D.Play("fall");
				}
				else if (velocity.Y < 0)
				{
					animatedSprite2D.Play("jump");
				}
			}

			if (IsOnFloor())
			{
				velocity.X = 0;

				animatedSprite2D.Play("idle");

				// Unpause the wait timer if the enemy has not been signalled to jump yet
				if (!jumpSignal)
				{
					waitTimer.Paused = false;
				}

				// Tells the enemy to jump when it is in moving state and the jump signal is on
				if (moveMode && jumpSignal)
				{
					velocity.Y = JumpVelocity;
					jumpSfx.Play();

					// Increment the number of hops the enemy has done
					currentHopCount++;

					// Stop telling the enemy to jump 
					jumpSignal = false;
				}
			}

			// Handles horizontal orientation of sprite
			if (direction.X > 0)
			{
				animatedSprite2D.FlipH = true;
			}
			else
			{
				animatedSprite2D.FlipH = false;
			}

			Velocity = velocity;
			MoveAndSlide();

			handleCollisionWithPlayer();
		}
	}

    private void _on_wait_timer_timeout()
    {
		// Pause the timer and allow the enemy to jump when the wait timer runs out
		waitTimer.Paused = true;

        jumpSignal = true;
    }

    private void _on_idle_timer_timeout()
    {
		// Exit idle state when idle timer runs out
		idleTimer.Stop();
		waitTimer.Start();

		moveMode = true;
		jumpSignal = true;

		// Flip horizontal direction
		direction.X = -direction.X;
		
		// Reset number of hops
		currentHopCount = 0;
    }

	// Handles when the enemy jumps
	public virtual void handleJumpControls()
	{
		// Stop jumping adn enter idle state when the number of hops is equal to the maximum number of hops it has
        if (currentHopCount >= hopCount && IsOnFloor() && moveMode && !jumpSignal)
        {
            moveMode = false;
            jumpSignal = false;

            waitTimer.Stop();
            idleTimer.Start();
        }
    }

    public virtual void handleCollisionWithPlayer()
	{
		// Kill any player that the enemy collides with from the top
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            GodotObject obj = GetSlideCollision(i).GetCollider();

            if (obj is CharacterController)
            {
                CharacterController player = obj as CharacterController;

                player.die();
            }
        }
    }

	public virtual void die()
	{
		if (!isDead)
		{
			// Create an explosion at the global position of the enemy on death
            Core core = GetNode<Node>("/root/Core") as Core;
			core.addExplosionAt(GlobalPosition.X, GlobalPosition.Y);

            isDead = true;

			// Remove the enemy from the tree on death
			QueueFree();
        }
	}
}
