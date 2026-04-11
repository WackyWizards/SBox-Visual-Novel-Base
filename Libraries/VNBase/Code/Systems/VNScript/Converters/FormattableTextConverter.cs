using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VNScript.Converters;

public class FormattableTextConverter : JsonConverter<FormattableText>
{
	public override FormattableText Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
	{
		var str = reader.GetString();
		if ( reader.TokenType == JsonTokenType.String && !string.IsNullOrEmpty( str ) )
		{
			return new FormattableText( str );
		}
		
		throw new JsonException( "Expected string for FormattableText" );
	}
	
	public override void Write( Utf8JsonWriter writer, FormattableText value, JsonSerializerOptions options )
	{
		writer.WriteStringValue( value.Text );
	}
}
