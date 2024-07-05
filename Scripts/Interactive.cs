using Godot;
using System;
using System.Diagnostics;

public partial class Interactive : Node2D
{
	public bool disabled = false;

    // Tells the object what to do when interacted with  
	virtual public void interact() { }
    
    // The highlight effect signifying the player can interact with the object
    virtual public void highlight() { }
    
    // Undo the highlight effect when player cannot interact with the object
    virtual public void unHighlight() { }
}
