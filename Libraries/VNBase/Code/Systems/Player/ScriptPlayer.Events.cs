using System;
using Script = VNScript.Script;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	public event Action<Assets.Script>? OnScriptLoad;
	
	public event Action<Assets.Script>? OnScriptUnload;
	
	public event Action<Script.Label>? OnLabelSet;
	
	public event Action<int>? OnTextAdvanced;
}
