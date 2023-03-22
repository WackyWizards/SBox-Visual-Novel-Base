namespace VNBase;

/// <summary>
/// This is an example script.
/// </summary>
public class ExampleScript2 : ScriptBase
{
	// We will write our script here.
	public override string Dialogue { get; set; } = @"
(label movingon
	(text ""So in the end it wasn't you, huh."")
	(choice ""Why didn't you believe me?"" jump questioning)
	(choice ""Yeah, I told you."" jump cocky)
	(character John)
)

(label questioning
  (text ""My apologies, I wasn't expecting it. Hopefully we can learn from our mistakes."")
  (character John)
  (after end-dialogue)
)

(label cocky
  (text ""I should've believed you from the start.. I sincerely apologize."")
  (character John)
  (after end-dialogue)
)

(start-dialogue movingon)
	";
}
