using Godot;
using Godot.NativeInterop;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Godot.TextServer;

public partial class loboPlayer : CharacterBody2D
{
    public float Speed = 300.0f;
    public float JumpVelocity = -400.0f;

    private float initialSpeed;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private AnimatedSprite2D animatedSprite2D;

    private AudioStreamPlayer jumpSfx;
    private AudioStreamPlayer runSfx;

    private Timer runSfxTimer;

    private int currentJumpIndex = 0;

    private bool goingLeft;

    private bool running = false;
    private bool stopped = true;

    private bool playRunSfx = true;

    // Spawnpoint marks the initial position of the loboPlayer
    [Export]
    public Marker2D spawnpoint;

    // Endpoint marks the end position of the loboPlayer
    [Export]
    public Marker2D endpoint;

    // Stop point (mainly the foreground loboPlayer) marks where the loboPlayer must pause at
    [Export]
    public Marker2D stopPoint;

    /** 
     * Jump points mark where the loboPlayer should jump at
     * Jump points should be sorted chronologically 
     **/
    [Export]
    public Marker2D[] jumppoints;

    // Determines whether the Physics constants scale by the scale of the loboPlayer or the scale of the parent of the loboPlayer
    [Export]
    public bool useParentScale = false;

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        jumpSfx = GetNode<AudioStreamPlayer>("JumpSfx");
        runSfx = GetNode<AudioStreamPlayer>("RunSfx");

        runSfxTimer = GetNode<Timer>("RunSfxTimer");

        animatedSprite2D.Play("idle");

        goingLeft = endpoint.GlobalPosition.X < spawnpoint.GlobalPosition.X;

        float scale = useParentScale ? (GetParent() as Node2D).Scale.X : Scale.X;

        Speed = Speed * scale;
        JumpVelocity = JumpVelocity * scale;
        gravity = gravity * scale;

        initialSpeed = Speed;

        runSfxTimer.Stop();

        AddUserSignal("reachedEndPoint");
        AddUserSignal("reachedStopPoint");
    }

    public override void _Process(double delta)
    {
        Vector2 velocity = Velocity;

        // Add the gravity
        if (!IsOnFloor())
            velocity.Y += gravity * (float)delta;

        // Jump when loboPlayer reaches a jump point
        if (jumppoints.Length > 0 && currentJumpIndex < jumppoints.Length)
        {
            if (goingLeft)
            {
                if (GlobalPosition.X <= jumppoints[currentJumpIndex].GlobalPosition.X)
                {
                    velocity.Y = JumpVelocity;
                    jumpSfx.Play();

                    currentJumpIndex++;
                }
            }
            else
            {
                if (GlobalPosition.X >= jumppoints[currentJumpIndex].GlobalPosition.X)
                {
                    velocity.Y = JumpVelocity;
                    jumpSfx.Play();

                    currentJumpIndex++;
                }
            }
        }

        float dirX = goingLeft ? -1 : 1;
        
        // Stop the loboPlayer moving in any horizontal direction if it is not running
        dirX = running ? dirX : 0;

        Vector2 direction = new Vector2(dirX, 0);

        // Handles running and decelerating on stop
        if (direction != Vector2.Zero)
        {
            velocity.X = direction.X * Speed;

            if (IsOnFloor())
            {
                animatedSprite2D.Play("run");
                if (playRunSfx)
                {
                    playRunSfx = false;
                    runSfx.Play();

                    runSfxTimer.Start();
                }
            }
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
        }

        // Handles horizontal orientation of sprite
        if (direction.X > 0 || (stopped && !running))
        {
            animatedSprite2D.FlipH = false;
        }
        else if (direction.X < 0)
        {
            animatedSprite2D.FlipH = true;
        }

        // Handles animations based on vertical velocity
        if (velocity.Y < 0)
        {
            animatedSprite2D.Play("jump");
        }
        else if (velocity.Y > 0)
        {
            animatedSprite2D.Play("fall");
        }
        else if (velocity.X == 0 && velocity.Y == 0)
        {
            animatedSprite2D.Play("idle");
        }

        Velocity = velocity;
        MoveAndSlide();

        // Kill any enemy that collides with loboPlayer
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            GodotObject obj = GetSlideCollision(i).GetCollider();

            if (obj is Enemy)
            {

                Enemy en = obj as Enemy;

                en.die();

                velocity.Y = JumpVelocity;
                Velocity = velocity;
                MoveAndSlide();

            }
        }

        // Handles the process when the actor has reached its stop point
        if (stopPoint != null && !stopped)
        {
            Speed = getSpecificSpeedToTarget(stopPoint.GlobalPosition, delta);

            if (goingLeft)
            {
                if (GlobalPosition.X <= stopPoint.GlobalPosition.X)
                {
                    reachedStopPoint();
                }
            }
            else
            {
                if (GlobalPosition.X >= stopPoint.GlobalPosition.X)
                {
                    reachedStopPoint();
                }
            }
        }
        else if (stopped)
        {
            Speed = initialSpeed;
        }

        // Handles the process when the actor has reached its end point
        if (goingLeft)
        {
            if (GlobalPosition.X <= endpoint.GlobalPosition.X)
            {
                reachedEndPoint();
            }
        }
        else
        {
            if (GlobalPosition.X >= endpoint.GlobalPosition.X)
            {
                reachedEndPoint();
            }
        }
    }

    // Function for other objects to set running to true
    public void startRunning()
    {
        running = true;
    }

    // Function for other objects to set running to false
    public void stopRunning()
    {
        running = false;
    }

    // Function to activate when the loboPlayer reaches its end point
    private void reachedEndPoint()
    {
        // Send the loboPlayer back to its spawnpoint
        GlobalPosition = new Vector2(spawnpoint.GlobalPosition.X, GlobalPosition.Y);
        
        // Reset the stored number of jumps the loboPlayer has performed
        currentJumpIndex = 0;

        stopped = false;

        // Tell the animation handler that the loboPlayer has reached its end point
        EmitSignal("reachedEndPoint");
    }

    // Function to activate when the loboPlayer has reaches its stop point
    private void reachedStopPoint()
    {
        stopped = true;

        // Tell the animation handler that the loboPlayer has reached its stop point
        EmitSignal("reachedStopPoint");
    }
    
    // Regulates running sound effect
    private void _on_run_sfx_timer_timeout()
    {
        playRunSfx = true;
    }

    // Returns a horizontal velocity to reach a specified horizontal distance
    private float getSpecificSpeedToTarget(Vector2 targetPosition, double delta)
    {
        float V = Mathf.Abs(targetPosition.X - GlobalPosition.X) / (float)delta;

        return Mathf.Min(initialSpeed, V);
    }
}
