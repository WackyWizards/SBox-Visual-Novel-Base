using Sandbox.Diagnostics;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	public static Logger Log { get => _log; }
	private readonly static Logger _log = new( "VNBase" );
}
