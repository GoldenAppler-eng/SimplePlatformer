using Godot;
using System;
using System.Diagnostics;

public partial class AnimationHandler : Node2D
{
    [Export]
    public PackedScene enemyPrefab;

	private loboEnemy backgroundEnemy;
	private loboPlayer backgroundPlayer;
	private loboPlayer foregroundPlayer;

    private Timer animationResetTimer;
    private Timer playerReturnTimer;
    private Timer instantiateEnemyTimer;

    private bool foregroundPlayerDone = false;
    private bool backgroundEnemyDone = false;

    private bool foregroundPlayerReset = false;

	public override void _Ready()
	{
		backgroundEnemy = GetNode<CharacterBody2D>("Background/Darkened/loboEnemy") as loboEnemy;
		backgroundPlayer = GetNode<CharacterBody2D>("Background/Darkened/BackgroundPlayer") as loboPlayer;
		foregroundPlayer = GetNode<CharacterBody2D>("Foreground/ForegroundPlayer") as loboPlayer;

        animationResetTimer = GetNode<Timer>("AnimationResetTimer");
        playerReturnTimer = GetNode<Timer>("PlayerReturnTimer");
        instantiateEnemyTimer = GetNode<Timer>("InstantiateEnemyTimer");

        backgroundEnemy.Connect("reachedStopPoint", new Callable(this, MethodName._on_enemy_reached_stop_point));
        backgroundEnemy.Connect("reachedDeathPoint", new Callable(this, MethodName._on_enemy_reached_death_point));

        backgroundPlayer.Connect("reachedEndPoint", new Callable(this, MethodName._on_background_player_reached_end_point));

        foregroundPlayer.Connect("reachedEndPoint", new Callable(this, MethodName._on_foreground_player_reached_end_point));
        foregroundPlayer.Connect("reachedStopPoint", new Callable(this, MethodName._on_foreground_player_reached_stop_point));

        animationResetTimer.Start();
        playerReturnTimer.Stop();
        instantiateEnemyTimer.Stop();
    }

    public override void _Process(double delta)
	{
        // Tell the backround player to start running once the loboEnemy is at its death point and the foreground player is out of frame
        if (backgroundEnemyDone && foregroundPlayerDone)
        {
            backgroundPlayer.startRunning();

            backgroundEnemyDone = false;
            foregroundPlayerDone = false;
        }

        // Start the timer to respawn the loboEnemy once the foreground player has reset its position
        if (foregroundPlayerReset) 
        {
            instantiateEnemyTimer.Start();
            foregroundPlayerReset = false;
        }
	}

    // Start the animation
    private void _on_animation_reset_timer_timeout()
    {
        animationResetTimer.Stop();

        foregroundPlayer.startRunning();
        backgroundEnemy.goToDeath();
    }

    // Flag that the enemy has reached its death point
    private void _on_enemy_reached_death_point()
    {
        backgroundEnemyDone = true;
    }

    // Stop the foreground player and flag that the foreground player has reached its end point
    private void _on_foreground_player_reached_end_point()
    {
        foregroundPlayer.stopRunning();

        foregroundPlayerDone = true;
    }

    // Stop the background player and start the timer for the foreground player to start running
    private void _on_background_player_reached_end_point()
    {
        backgroundPlayer.stopRunning();

        playerReturnTimer.Start();
    }

    // Tell the foreground player to start running
    private void _on_player_return_timer_timeout()
    {
        playerReturnTimer.Stop();

        foregroundPlayer.startRunning();
    }

    // Stop the foreground player and flag that the foreground player has reset its position
    private void _on_foreground_player_reached_stop_point()
    {
        foregroundPlayerReset = true;

        foregroundPlayer.stopRunning();
    }
    
    // Spawn a new enemy
    private void _on_instantiate_enemy_timer_timeout()
    {
        instantiateEnemyTimer.Stop();

        instantiateEnemy();
    }

    // Start the timer to replay the animation once the respawned loboEnemy reaches the stop point
    private void _on_enemy_reached_stop_point()
    {
        animationResetTimer.Start();
    }

    // Function to instantiate a new loboEnemy
    private void instantiateEnemy()
    {
        Node newNode = ResourceLoader.Load<PackedScene>(enemyPrefab.ResourcePath).Instantiate();
        loboEnemy newEnemy = newNode as loboEnemy;

        newEnemy.spawnpoint = GetNode<Marker2D>("CuePoints/BackgroundEnemySpawnPoint");
        newEnemy.deathpoint = GetNode<Marker2D>("CuePoints/BackgroundEnemyDeathPoint");
        newEnemy.stoppoint = GetNode<Marker2D>("CuePoints/BackgroundEnemyStopPoint");

        newEnemy.hopCount = 3;

        backgroundEnemy = newEnemy;

        // Add the new loboEnemy to the tree
        GetNode<Node2D>("Background/Darkened").AddChild(newNode);

        backgroundEnemy.Connect("reachedStopPoint", new Callable(this, MethodName._on_enemy_reached_stop_point));
        backgroundEnemy.Connect("reachedDeathPoint", new Callable(this, MethodName._on_enemy_reached_death_point));

        // Start the initializing sequence for the new loboEnemy
        newEnemy.startInitalSequence();
    }
}
