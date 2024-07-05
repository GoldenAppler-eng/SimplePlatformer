using Godot;
using System;

public partial class Global : Node
{
	// The last camera position recorded before restarting the level
	public static Vector2 cameraPos = new Vector2(-1000f, -1000f);
	
	// The last player position recorded before restarting the level
	public static Vector2 playerPos = new Vector2(-1000f, -1000f);
}
