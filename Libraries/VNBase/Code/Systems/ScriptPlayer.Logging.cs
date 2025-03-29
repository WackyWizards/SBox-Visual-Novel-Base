using Sandbox;
using Sandbox.Diagnostics;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	[ConVar( "vnbase_logging" )]
	public static bool LoggingEnabled { get; set; } = false;

	public static readonly Logger Log = new( "VNBase" );
}
