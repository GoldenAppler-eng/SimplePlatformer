using Godot;
using System;

public partial class explosioneffect : AnimatedSprite2D
{
    private void _on_animation_finished()
	{
		// Delete the node once the animation has finished
		QueueFree();
	}
}
