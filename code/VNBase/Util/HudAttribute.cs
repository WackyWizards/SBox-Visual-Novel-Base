using Sandbox;
using System;

namespace VNBase.UI;

/// <summary>
/// Marks a class as part of the HUD. These get rebuilt every hotload.
/// </summary>
[AttributeUsage( AttributeTargets.Class )]
internal class HudAttribute : LibraryAttribute, ITypeAttribute
{
	public Type TargetType { get; set; }
}
