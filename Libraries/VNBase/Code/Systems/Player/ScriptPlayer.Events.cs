using System;
using SandLang;
using VNBase.Assets;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	public event Action<Script>? OnScriptLoad;
	
	public event Action<Script>? OnScriptUnload;
	
	public event Action<Dialogue.Label>? OnLabelSet;
}
