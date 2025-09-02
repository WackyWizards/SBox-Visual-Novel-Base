using Sandbox;
using System.Collections.Generic;

namespace VNScript;

public partial class Script
{
	public class After
	{
		public List<SParen> CodeBlocks { get; set; } = [];

		public bool IsLastLabel { get; set; }

		[FilePath]
		public string? ScriptPath { get; set; }

		public string? TargetLabel { get; set; }
	}
}
