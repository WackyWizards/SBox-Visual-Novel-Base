namespace VNBase;

/// <summary>
/// This is an example character definition.
/// It is generally recommended to put this in a separate file.
/// </summary>
public class John : CharacterBase
{
	public override string Name => "John MacAvitch";
	public override string Title => "Office Employee";
	public override string Images => "/materials/vnbase/images/John";
}

/// <summary>
/// This is an example script.
/// </summary>
public class ExampleCharacterScript : ScriptBase
{
	// We will write our script here.
	public override string Dialogue { get; set; } = @"
(label moving-on
	(text ""So in the end it wasn't you, huh."" say John)
	(choice ""Why didn't you believe me?"" jump questioning)
	(choice ""Yeah, I told you."" jump cocky)
	(character John exp Smile.png)
	(character John exp Smile.png)
	(bg Meadow.jpg)
)

(label questioning
	(text ""My apologies, I wasn't expecting it. Hopefully we can learn from our mistakes."" say John)
	(character John exp Thinking.png)
	(bg Meadow.jpg)
	(after end-dialogue)
)

(label cocky
	(text ""I should've believed you from the start.. I sincerely apologize."" say John)
	(character John exp Thinking.png)
	(bg Meadow.jpg)
	(after end-dialogue)
)

(start-dialogue moving-on)
	";
}
