using Godot;
using System;
using System.Diagnostics;

public partial class Ladder : Node2D
{
	private void _on_area_2d_body_entered(Node2D body)
	{
        // Set the isClimbable boolean to true for the player
        if (body.Name == "Player")
		{
			CharacterController ct = body as CharacterController;

            ct.setClimbable(true);
        }
	}

    private void _on_area_2d_body_exited(Node2D body)
    {
        // Set the isClimbable boolean to false for the player
        if (body.Name == "Player")
        {
            CharacterController ct = body as CharacterController;

			ct.setClimbable(false);
        }
    }
}
