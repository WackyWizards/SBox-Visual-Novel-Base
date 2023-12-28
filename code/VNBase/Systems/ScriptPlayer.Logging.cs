namespace VNBase;

sealed partial class ScriptPlayer
{
	private static void ScriptLog( object msg, SeverityLevel level = SeverityLevel.Info )
	{
		switch ( level )
		{
			case SeverityLevel.Error:
				Log.Error( $"[VNBASE] {msg}" );
				break;
			case SeverityLevel.Warning:
				Log.Warning( $"[VNBASE] {msg}" );
				break;
			default:
				Log.Info( $"[VNBASE] {msg}" );
				break;
		}
	}

	private enum SeverityLevel
	{
		Info,
		Warning,
		Error
	}
}
