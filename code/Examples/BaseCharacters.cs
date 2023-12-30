namespace VNBase.Examples;

// This file contains a bunch of example character definitions.

public class Unknown : CharacterBase
{
	public override string Name => "???";
}

public class John : CharacterBase
{
	public override string Name => "John MacAvitch";
	public override string Title => "Office Employee";
	public override string Images => "/materials/vnbase/scripts/john";
}
