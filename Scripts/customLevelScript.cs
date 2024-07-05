using Godot;
using System;
using System.Diagnostics;

public partial class customLevelScript : Node2D
{
    private CharacterBody2D player;
    private Camera2D camera;

    private bool camMode = false;

	public override void _Ready()
	{
        Core core = GetTree().Root.GetNode<Node>("Core") as Core;

        camMode = core.camMode;

        camera = GetNode<Camera2D>("Camera2D");
        player = GetNode<CharacterBody2D>("Player");

        //float cameraOffsetX = GetViewportRect().Size.X / camera.Zoom.X / 2;
        //float cameraOffsetY = GetViewportRect().Size.Y / camera.Zoom.Y / 2;

        //camera.GlobalPosition = new Vector2(customClamp(player.GlobalPosition.X, camera.LimitLeft + cameraOffsetX, camera.LimitRight - cameraOffsetX), customClamp(player.GlobalPosition.Y, camera.LimitTop + cameraOffsetY, camera.LimitBottom - cameraOffsetY));

        if (Global.cameraPos != new Vector2(-1000f, -1000f))
        {
            camera.GlobalPosition = Global.cameraPos;
        }

        if (Global.playerPos != new Vector2(-1000f, -1000f))
        {
            core.addExplosionAt(Global.playerPos.X, Global.playerPos.Y);
        }

        if (camMode)
        {
            player.ProcessMode = ProcessModeEnum.Disabled;
            player.Hide();
        }
    }

    public override void _Process(double delta)
    {
        if (!camMode) return;

        if (Input.IsActionJustPressed("jump")) GetTree().Paused = !GetTree().Paused;

        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        float cameraSpeed = 300.0f;

        if (Input.IsActionPressed("slowdown")) cameraSpeed /= 2;

        float camX = camera.GlobalPosition.X + direction.X * cameraSpeed * (float)delta;
        float camY = camera.GlobalPosition.Y + direction.Y * cameraSpeed * (float)delta;


        float cameraOffsetX = GetViewportRect().Size.X / camera.Zoom.X / 2;
        float cameraOffsetY = GetViewportRect().Size.Y / camera.Zoom.Y / 2;

        camera.GlobalPosition = new Vector2(customClamp(camX, camera.LimitLeft + cameraOffsetX, camera.LimitRight - cameraOffsetX), customClamp(camY, camera.LimitTop + cameraOffsetY, camera.LimitBottom - cameraOffsetY));
    }

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
}
