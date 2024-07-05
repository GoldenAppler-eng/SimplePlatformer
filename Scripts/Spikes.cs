using Godot;
using System;

public partial class Spikes : Node2D
{
    private void _on_area_2d_body_entered(Node2D body)
    {
		// Kill the player that touches this set of spikes
        if (body.Name == "Player")
		{
			CharacterController script = body as CharacterController;

			script.die();
		}
    }
}
