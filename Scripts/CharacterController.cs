using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class CharacterController : CharacterBody2D
{
	public float Speed = 300.0f;
    public const float ClimbSpeed = 200.0f;
	public const float JumpVelocity = -400.0f;
    public const float CameraSpeed = 15.0f;
    public float CameraFirstSpeed = 150.0f;
    public const float maxSpeed = 300.0f;

    [Export]
    public float coyoteTime = 0.07f;

	// Get the gravity from the project settings to be synced with RigidBody nodes
	public float gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    private AnimatedSprite2D animatedSprite2D;
    private CollisionShape2D collisionShape2D;

    // Defines the raycasts beside the player to detect interactable objects
    private RayCast2D lInteractionRay;
    private RayCast2D rInteractionRay;

    // Defines the raycasts underneath the player to detect enemies
    private RayCast2D killCast1;
    private RayCast2D killCast2;

    private Interactive highlighted;

    // Stores the y-value below that the player will die
    [Export]
    public Marker2D deathline;

    // Stores the camera that follows the player
    [Export]
    public Camera2D camera;

    private AudioStreamPlayer jumpSfx;
    private AudioStreamPlayer deathSfx;
    private AudioStreamPlayer runSfx;

    private AnimationPlayer animPlayer;

    private Timer coyoteTimer;

    private Timer runSfxTimer;

    private bool isCrouching = false;
    private bool isDead = false;
    private bool isClimbable = false;
    private bool isClimbing = false;

    private bool hasJumped = false;

    private bool cameraFirstContact = false;

    private bool disabledMovement = false;

    private bool playRunSfx = true;

    // Custom signal to tell other functions when the player's fade out animation has finished
    [Signal]
    public delegate void playerFadeOutEventHandler();

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

        lInteractionRay = GetNode<RayCast2D>("LeftInteractionDetection");
        rInteractionRay = GetNode<RayCast2D>("RightInteractionDetection");

        killCast1 = GetNode<RayCast2D>("KillCast1");
        killCast2 = GetNode<RayCast2D>("KillCast2");

        collisionShape2D = GetNode<CollisionShape2D>("Collision");

        jumpSfx = GetNode<AudioStreamPlayer>("JumpSfx");
        deathSfx = GetNode<AudioStreamPlayer>("DeathSfx");
        runSfx = GetNode<AudioStreamPlayer>("RunSfx");

        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        coyoteTimer = GetNode<Timer>("CoyoteTimer");
        coyoteTimer.WaitTime = coyoteTime;

        runSfxTimer = GetNode<Timer>("RunSfxTimer");
       
        coyoteTimer.Stop();
    }

    public override void _Process(double delta)
    {
        // Stop processing if movement is disabled
        if (disabledMovement)
            return;

        // Kill the player if it goes below its death line
        if (this.GlobalPosition.Y > deathline.GlobalPosition.Y)
        {
            die();
        }

        handleInteraction();
   
        Vector2 velocity = Velocity;

        // Get the input direction 
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // Add the gravity
        if (!IsOnFloor() && !isClimbing)
            velocity.Y += gravity * (float)delta;

        if (!isDead)
        {
            handleCameraMovement(delta);
         
            // Jump only once when player presses 'space' when it is on the floor or while the coyote timer is running
            if (Input.IsActionPressed("jump") && (IsOnFloor() || !coyoteTimer.IsStopped()) && !hasJumped)
            {
                jumpSfx.Play();
                velocity.Y = JumpVelocity;

                hasJumped = true;
            }

            // Half the player's horizontal speed when the player holds down 'shift'
            if (Input.IsActionPressed("slowdown")) 
            {
                Speed = maxSpeed / 2;
            }
            else
            {
                Speed = maxSpeed;
            }

            // Move the player horizontally if it is not crouching
            if (!isCrouching)
            {
                if (direction != Vector2.Zero)
                {
                    velocity.X = Mathf.Sign(direction.X) * Speed;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                }
            }

            // Unused code for climbing
            if (isClimbing)
            {
                // Move the player up or down the climbable object
                if (direction != Vector2.Zero)
                {
                    velocity.Y = Mathf.Sign(direction.Y) * ClimbSpeed;
                }
                else
                {
                    velocity.Y = 0;
                }

                // Handle the player animation when stopping or moving on climbable object
                if (velocity.Y == 0)
                {
                    animatedSprite2D.Pause();
                }
                else 
                {
                    animatedSprite2D.Play();
                }
            }

            // Handle horizontal orientation of sprite
            if (direction.X > 0)
            {
                animatedSprite2D.FlipH = false;
            }
            else if (direction.X < 0)
            {
                animatedSprite2D.FlipH = true;
            }

            // (Unused code for climbing) Play climbing animation when climbing
            if (isClimbable && direction.Y < 0)
            {
                animatedSprite2D.Play("climb");
                isClimbing = true;
            }
            else if (IsOnFloor() || !isClimbable)
            {
                isClimbing = false;
            }

            // Crouch when player holds down key
            if (IsOnFloor() && direction.Y > 0)
            {
                animatedSprite2D.Play("crouch");
                isCrouching = true;

                // Stop the player from moving left or right while crouching
                velocity.X = 0;                

                // Reduce the collision box of the crouching player
                RectangleShape2D rect = collisionShape2D.Shape as RectangleShape2D;
                rect.Size = new Vector2(14, 13);
                
                // Offset the position of the collsion box when crouching
                collisionShape2D.Position = new Vector2(0, 9.5f);
            }
            else
            {
                isCrouching = false;

                // Reset the collision box and its position when uncrouching
                RectangleShape2D rect = collisionShape2D.Shape as RectangleShape2D;
                rect.Size = new Vector2(14, 21);

                collisionShape2D.Position = new Vector2(0, 5.5f);
            }

            // Handle animation for different horizontal and vertical velocities
            if (!isCrouching && !isClimbing)
            {
                if (velocity.Y == 0)
                {
                    if (velocity.X == 0)
                    {
                        animatedSprite2D.Play("idle");
                    }
                    else if (Mathf.Abs(velocity.X) > 0)
                    {
                        animatedSprite2D.Play("run");

                        if (!runSfx.Playing && playRunSfx)
                        {
                            runSfx.Play();
                            playRunSfx = false;
                            runSfxTimer.Start();
                        }
                    }
                }
                else if (velocity.Y < 0)
                {
                    animatedSprite2D.Play("jump");
                }
                else if (velocity.Y > 0)
                {
                    animatedSprite2D.Play("fall");
                }
            }
        }

        // Store whether the player was on the floor before moving
        bool wasOnFloor = IsOnFloor();

        Velocity = velocity;
        MoveAndSlide();

        // Start the coyote timer if the player is no longer on the floor after moving
        if (wasOnFloor && !IsOnFloor())
        {
            coyoteTimer.Start();
        }

        // Reset the flag for the player jumping when it reaches the floor
        if (!wasOnFloor && IsOnFloor())
        {
            hasJumped = false;
        }

        // Handle collision with enemies
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            GodotObject obj = GetSlideCollision(i).GetCollider();

            if (obj is Enemy)
            {
                if (killCast1.IsColliding() || killCast2.IsColliding())
                {   
                    // Kill the enemy if the player is on top of the enemy
                    Enemy en = obj as Enemy;

                    en.die();

                    // Accelerate the player upwards after killing an enemy
                    velocity.Y = JumpVelocity;
                    Velocity = velocity;

                    MoveAndSlide();
                }
                else
                {
                    // Kill the player if the player is not on top of the enemy
                    die();
                }
            }
        }
    }

    public void die()
    {
        if (!isDead)
        {
            isDead = true;
            
            // Play death animation and sound effect on death
            deathSfx.Play();
            animatedSprite2D.Play("death");

            // Accelerate the player upwards a bit on death
            Velocity = new Vector2(0, JumpVelocity);
            
            // Prevent the player from colliding with anything when falling
            CollisionLayer = 0;
            CollisionMask = 0;
        }
    }

    public void restart()
    {
        // Call reset function during idle time
        CallDeferred(MethodName.resetScene);
    }

    // Function to restart the level
    private void resetScene()
    {
        // Set the global camera and player position to the current camera and player position
        Global.cameraPos = camera.GlobalPosition;
        Global.playerPos = GlobalPosition;

        // Call the core's restart level function
        Core core = GetNode<Node>("/root/Core") as Core;

        core.restartLevel();
    }

    // (Unused code for climbing) Function to allow other objects to change isClimbable 
    public void setClimbable(bool b)
    {
        isClimbable = b;
    }

    private void handleCameraMovement(double delta)
    {
        // Get the offset to get to the center of the camera view
        float cameraOffsetX = GetViewportRect().Size.X / camera.Zoom.X / 2;
        float cameraOffsetY = GetViewportRect().Size.Y / camera.Zoom.Y / 2;

        // Gets the limits of the camera with respect to the camera view's center
        float leftLimit = camera.LimitLeft + cameraOffsetX, 
              rightLimit = camera.LimitRight - cameraOffsetX, 
              topLimit = camera.LimitTop + cameraOffsetY, 
              bottomLimit = camera.LimitBottom - cameraOffsetY;

        // Modify camera behaviour depending on whether camera has passed the player after entering/restarting level
        if (cameraFirstContact)
        {
            // Follow the player with drag
            camera.GlobalPosition = new Vector2(customClamp(Mathf.Lerp(camera.GlobalPosition.X, GlobalPosition.X, CameraSpeed * (float)delta), leftLimit, rightLimit), customClamp(Mathf.Lerp(camera.GlobalPosition.Y, GlobalPosition.Y, CameraSpeed * (float)delta), topLimit, bottomLimit));
        }
        else
        {
            // Speed up camera speed depending on distance between camera and player
            int multiplier = 1;

            if (Global.cameraPos.X - GlobalPosition.X >= 1000.0f)
            {
                multiplier = 3;
            }
            else if (Global.cameraPos.X - GlobalPosition.X >= 500.0f)
            {
                multiplier = 2;
            }

            // Move the camera with a constant speed
            float deltaX = Mathf.Sign(GlobalPosition.X - camera.GlobalPosition.X) * CameraFirstSpeed * multiplier * (float)delta,
                  deltaY = Mathf.Sign(GlobalPosition.Y - camera.GlobalPosition.Y) * CameraFirstSpeed * (float)delta;

            float x = customClamp(camera.GlobalPosition.X + deltaX, leftLimit, rightLimit);
            float y = customClamp(camera.GlobalPosition.Y + deltaY, topLimit, bottomLimit);

            camera.GlobalPosition = new Vector2(x, y);

            // Stop moving the camera with constant speed after it has passed the player for the first time
            if (camera.GlobalPosition.X <= customClamp(GlobalPosition.X, rightLimit, leftLimit))
            {
                cameraFirstContact = true; 
            }
        }
    }

    private void handleInteraction()
    {
        // Handle collision when either left or right raycast is colliding with an interactive object
        if (lInteractionRay.IsColliding())
        {
            if (lInteractionRay.GetCollider() is Node2D)
            {
                Node2D n = lInteractionRay.GetCollider() as Node2D;

                if (n.GetParent() is Interactive)
                {
                    Interactive i = n.GetParent() as Interactive;

                    if (!i.disabled)
                    {
                        // Highlight the interactive object if it is not being highlighted
                        if (highlighted == null || highlighted != i)
                        {
                            // Unhighlight the previously highlighted object if any
                            if (highlighted != null) highlighted.unHighlight();

                            // Highlight the new object
                            i.highlight();
                            highlighted = i;
                        }

                        // Interact with highlighted object when 'Z' is pressed
                        if (Input.IsActionJustPressed("interact") && highlighted == i)
                        {
                            i.interact();
                        }
                    }
                    else
                    {
                        // Unhighlight the interactive object if it is disabled
                        i.unHighlight();
                    }
                }
            }
        }
        else if (rInteractionRay.IsColliding())
        {
            if (rInteractionRay.GetCollider() is Node2D)
            {
                Node2D n = rInteractionRay.GetCollider() as Node2D;

                if (n.GetParent() is Interactive)
                {
                    Interactive i = n.GetParent() as Interactive;

                    if (!i.disabled)
                    {
                        // Highlight the interactive object if it is not being highlighted
                        if (highlighted == null || highlighted != i)
                        {
                            // Unhighlight the previously highlighted object if any
                            if (highlighted != null) highlighted.unHighlight();

                            // Highlight the new object
                            i.highlight();
                            highlighted = i;
                        }

                        // Interact with highlighted object when 'Z' is pressed
                        if (Input.IsActionJustPressed("interact") && highlighted == i)
                        {
                            i.interact();
                        }
                    }
                    else
                    {
                        // Unhighlight the interactive object if it is disabled
                        i.unHighlight();
                    }
                }
            }
        }
        else
        {
            // Unhighlight previously highlighted objects if it is not longer colliding with either raycasts
            if (highlighted != null)
            {
                highlighted.unHighlight();
                highlighted = null;
            }
        }
    }

    // Clamps a value in between two values (where the minimum and maximum are not strictly assigned to either values)
    private float customClamp(float value, float val1, float val2)
    {
        if (val1 >= val2)
        {
            return Mathf.Clamp(value, val2, val1);
        }
        else
        {
            return Mathf.Clamp(value, val1, val2);
        }
    }

    // Function to activate when player enters a door
    public async void enterDoor()
    {
        // Disable the movement of the player
        disabledMovement = true;

        // Play the fade out animation
        animPlayer.Play("fade_out");

        // Wait for the animation to finish
        await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);

        // Signal that the player has finished its fade out animation
        EmitSignal(SignalName.playerFadeOut);
    }

    // Regulates running sound effect
    private void _on_run_sfx_timer_timeout()
    {
        playRunSfx = true;
        runSfxTimer.Stop();
    }
}
