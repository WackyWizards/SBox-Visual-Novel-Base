using System;
using System.IO;

namespace VNBase;

/// <summary>
/// The exception that is thrown if a required resource is unable to be found.
/// </summary>
public class ResourceNotFoundException : FileNotFoundException
{
	public string ResourceName { get; private set; } = string.Empty;

	public ResourceNotFoundException() { }

	public ResourceNotFoundException( string message )
		: base( message ) { }

	public ResourceNotFoundException( string message, Exception innerException )
		: base( message, innerException ) { }

	public ResourceNotFoundException( string message, string resourceName )
		: base( message )
	{
		ResourceName = resourceName;
	}

	public ResourceNotFoundException( string message, string resourceName, string fileName )
		: base( message, fileName )
	{
		ResourceName = resourceName;
	}
}
