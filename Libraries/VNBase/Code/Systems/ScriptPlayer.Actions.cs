using System;
using SandLang;
using VNBase.Assets;

namespace VNBase;

public sealed partial class ScriptPlayer
{
	public Action<Script>? OnScriptLoad { get; set; }
	
	public Action<Script>? OnScriptUnload { get; set; }
	
	public Action<Dialogue.Label>? OnLabelSet { get; set; }
}
