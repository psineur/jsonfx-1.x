using System;

namespace JsonFx.JSON
{
	/// <summary>
	/// Allows classes to control their own JSON serialization
	/// </summary>
	public interface IJsonSerializable
	{
		void ReadJson(JsonReader reader);
		void WriteJson(JsonWriter writer);
	}
}
