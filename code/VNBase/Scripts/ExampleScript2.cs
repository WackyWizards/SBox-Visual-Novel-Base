namespace VNBase;

/// <summary>
/// This is an example character definition.
/// It is generally recommended to put this in a separate file.
/// This is just for demonstration purposes.
/// </summary>
public class John : CharacterBase
{
	public override string Name => "John MacAvitch";
	public override string Title => "Office Employee";
	public override string Images => "/materials/vnbase/images/John";
	public override Color NameColor => Color.White;
	public override Color TitleColor => Color.White;
}

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
	(character John exp Thinking)
)

(label questioning
  (text ""My apologies, I wasn't expecting it. Hopefully we can learn from our mistakes."")
  (character John exp Smile)
  (after end-dialogue)
)

(label cocky
  (text ""I should've believed you from the start.. I sincerely apologize."")
  (character John exp Smile)
  (after end-dialogue)
)

(start-dialogue movingon)
	";
}
