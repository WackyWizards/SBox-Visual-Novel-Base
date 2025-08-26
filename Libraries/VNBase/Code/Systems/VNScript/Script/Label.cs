using System.Collections.Generic;
using System.Linq;
using VNBase.Assets;

namespace VNScript;

public partial class Script
{
	/// <summary>
	/// Represents a dialogue step.
	/// </summary>
	public class Label
	{
		public string Name { get; set; } = string.Empty;

		public List<string> Dialogue => FormattableDialogue.Select( x => x.Format( Environment ) ).ToList();
		
		internal List<FormattableText> FormattableDialogue { get; set; } = [];

		public Input? ActiveInput { get; set; }

		public Character? SpeakingCharacter { get; set; }

		public List<Character> Characters { get; set; } = [];

		public List<Choice> Choices { get; set; } = [];

		public List<IAsset> Assets { get; set; } = [];

		public AfterLabel? AfterLabel { get; set; }

		internal IEnvironment Environment { get; set; } = new EnvironmentMap();
	}
}
