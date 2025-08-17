using System;
using System.Text.Json.Serialization;
using Sandbox;

namespace VNBase;

public class Input : IEquatable<InputAction>
{
	// This needs to be public, apparently.
	// ReSharper disable once MemberCanBePrivate.Global
	[InputAction]
	public string Action { get; set; } = string.Empty;

	public bool Equals( InputAction? other )
	{
		return Action == other?.Name;
	}

	[Hide, JsonIgnore] public bool Pressed => Sandbox.Input.Pressed( this );

	[Hide, JsonIgnore] public bool Down => Sandbox.Input.Down( this );

	public static implicit operator string( Input input ) => input.Action;
	public static implicit operator Input( string action ) => new() { Action = action };
}
