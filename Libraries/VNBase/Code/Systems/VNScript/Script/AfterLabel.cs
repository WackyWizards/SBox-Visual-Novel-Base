using Sandbox;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VNScript;

public partial class Script
{
	public class After
	{
		public List<SParen> CodeBlocks { get; set; } = [];
		
		[JsonPropertyName( "isLastLabel" )]
		public bool IsLastLabel { get; set; }
		
		[FilePath]
		[JsonPropertyName( "scriptPath" )]
		public string? ScriptPath { get; set; }
		
		[JsonPropertyName( "targetLabel" )]
		public string? TargetLabel { get; set; }
	}
}
