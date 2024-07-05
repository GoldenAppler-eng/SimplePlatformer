using Godot;
using System;
using System.Diagnostics;

public partial class level_transition : CanvasLayer
{
	public AnimationPlayer animPlayer;

	public override void _Ready()
	{
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer.Play("RESET");
    }

	// Signal to tell other functions when the animation has finished playing
    [Signal]
    public delegate void transitionEventHandler(); 

    public async void slideToBlack()
	{
		animPlayer.Play("slideToBlack");

		// Wait for the animation to finish
		await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);

		// Signal that the animation has finished
		EmitSignal(SignalName.transition);
	}

	public async void slideFromBlack() 
	{
		animPlayer.Play("slideFromBlack");

		// Wait for the animation to finish
        await ToSignal(animPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }
}
