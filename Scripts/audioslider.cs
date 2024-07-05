using Godot;
using System;
using System.Diagnostics;

public partial class audioslider : Control
{
	[Export(PropertyHint.Enum, "Master,Music,Sfx,UI")] 
	public String busName = "Master";

	private Label audioName;
	private Label audioLevel;
	private HSlider slider;

	private int busIndex;

	public override void _Ready()
	{
        audioName = GetNode<Label>("HBoxContainer/AudioName");
        audioLevel = GetNode<Label>("HBoxContainer/AudioLevel");
        slider = GetNode<HSlider>("HBoxContainer/HSlider");

		audioName.Text = busName + " Volume";

		busIndex = AudioServer.GetBusIndex(busName);

		slider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
    }

	private void _on_h_slider_value_changed(float value)
	{
		// Change the display text 
		audioLevel.Text = (Mathf.Round(value * 1000)/10).ToString() + "%";

		// Change the volume of the corresponding bus
		AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(value));
	}
}
