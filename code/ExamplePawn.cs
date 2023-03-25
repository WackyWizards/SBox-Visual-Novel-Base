using Sandbox;
using System;
using VNBase;

namespace Sandbox;

public partial class Pawn : AnimatedEntity
{
	[Net] public ScriptPlayer VNScriptPlayer { get; private set; }

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if ( Game.IsServer )
		{
			VNScriptPlayer = new ScriptPlayer
			{
				Owner = this
			};
			VNScriptPlayer.LoadScript( new ExampleScript() );
		}
	}

	[Event.Client.BuildInput]
	private void SkipEffects()
	{
		bool skipPressed = Input.Pressed( InputButton.Jump );
		if ( !skipPressed ) return;

		ScriptPlayer.SkipDialogue();
	}

	/// <summary>
	/// Called every tick, clientside and serverside.
	/// </summary>
	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		Rotation = Angles.Zero.ToRotation();
	}

	/// <summary>
	/// Called every frame on the client
	/// </summary>
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		// Update rotation every frame, to keep things smooth
		Rotation = Angles.Zero.ToRotation();

		Camera.Position = Position;
		Camera.Rotation = Rotation;

		// Set field of view to whatever the user chose in options
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		// Set the first person viewer to this, so it won't render our model
		Camera.FirstPersonViewer = this;
	}
}
