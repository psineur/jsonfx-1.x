#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion License

using System;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Xml;

namespace JsonFx.Json
{
	/// <summary>
	/// Represents a proxy method for serialization of types which do not implement IJsonSerializable.
	/// </summary>
	/// <typeparam name="T">the type for this proxy</typeparam>
	/// <param name="writer">the JsonWriter to serialize to</param>
	/// <param name="value">the value to serialize</param>
	public delegate void WriteDelegate<T>(JsonWriter writer, T value);

	/// <summary>
	/// Writer for producing JSON data.
	/// </summary>
	public class JsonWriter : IDisposable
	{
		#region Constants

		public const string JsonMimeType = "application/json";

		private static readonly DateTime EcmaScriptEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
		private const string EcmaScriptDateCtor = "new Date({0})";

		private const string AnonymousTypePrefix = "<>f__AnonymousType";

		#endregion Constants
		
		#region Fields

		private readonly TextWriter writer = null;
		private string typeHintName = null;
		private bool strictConformance = true;

		private bool prettyPrint = false;
		private bool useXmlSerializationAttributes = false;
		private int depth = 0;
		private string tab = "\t";
		private WriteDelegate<DateTime> dateTimeSerializer = null;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">TextWriter for writing</param>
		public JsonWriter(TextWriter output)
		{
			this.writer = output;
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">Stream for writing</param>
		public JsonWriter(Stream output)
		{
			this.writer = new StreamWriter(output, Encoding.UTF8);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">File name for writing</param>
		public JsonWriter(string outputFileName)
		{
			Stream stream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
			this.writer = new StreamWriter(stream, Encoding.UTF8);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="output">StringBuilder for appending</param>
		public JsonWriter(StringBuilder output)
		{
			this.writer = new StringWriter(output, System.Globalization.CultureInfo.InvariantCulture);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets the property name used for type hinting.
		/// </summary>
		public string TypeHintName
		{
			get { return this.typeHintName; }
			set { this.typeHintName = value; }
		}

		/// <summary>
		/// Gets and sets if JSON will be formatted for human reading.
		/// </summary>
		public bool PrettyPrint
		{
			get { return this.prettyPrint; }
			set { this.prettyPrint = value; }
		}

		/// <summary>
		/// Gets and sets the string to use for indentation
		/// </summary>
		public string Tab
		{
			get { return this.tab; }
			set { this.tab = value; }
		}

		/// <summary>
		/// Gets and sets the lien terminator string
		/// </summary>
		public string NewLine
		{
			get { return this.writer.NewLine; }
			set { this.writer.NewLine = value; }
		}

		/// <summary>
		/// Gets and sets if should use XmlSerialization Attributes.
		/// </summary>
		/// <remarks>
		/// Respects XmlIgnoreAttribute, ...
		/// </remarks>
		public bool UseXmlSerializationAttributes
		{
			get { return this.useXmlSerializationAttributes; }
			set { this.useXmlSerializationAttributes = value; }
		}

		/// <summary>
		/// Gets and sets if should conform strictly to JSON spec.
		/// </summary>
		/// <remarks>
		/// Setting to true causes NaN, Infinity, -Infinity to serialize as null.
		/// </remarks>
		public bool StrictConformance
		{
			get { return this.strictConformance; }
			set { this.strictConformance = value; }
		}

		/// <summary>
		/// Gets and sets a proxy formatter to use for DateTime serialization
		/// </summary>
		public WriteDelegate<DateTime> DateTimeSerializer
		{
			get { return this.dateTimeSerializer; }
			set { this.dateTimeSerializer = value; }
		}

		/// <summary>
		/// Gets the underlying TextWriter.
		/// </summary>
		public TextWriter TextWriter
		{
			get { return this.writer; }
		}

		#endregion Properties

		#region Static Methods

		/// <summary>
		/// A fast method for serializing an object to JSON
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Serialize(object value)
		{
			StringBuilder output = new StringBuilder();

			using (JsonWriter writer = new JsonWriter(output))
			{
				writer.Write(value);
			}

			return output.ToString();
		}

		#endregion Static Methods

		#region Public Methods

		public virtual void Write(object value)
		{
			this.Write(value, false);
		}

		private void Write(object value, bool isProperty)
		{
			if (isProperty && this.prettyPrint)
			{
				this.writer.Write(' ');
			}

			if (value == null)
			{
				this.writer.Write(JsonReader.LiteralNull);
				return;
			}

			if (value is IJsonSerializable)
			{
				try
				{
					if (isProperty)
					{
						this.depth++;
						this.WriteLine();
					}
					((IJsonSerializable)value).WriteJson(this);
				}
				finally
				{
					if (isProperty)
					{
						this.depth--;
					}
				}
				return;
			}

			// must test enumerations before value types
			if (value is Enum)
			{
				this.Write((Enum)value);
				return;
			}

			// Type.GetTypeCode() allows us to more efficiently switch type
			// plus cannot use 'is' for ValueTypes
			Type type = value.GetType();
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
				{
					this.Write((Boolean)value);
					return;
				}
				case TypeCode.Byte:
				{
					this.Write((Byte)value);
					return;
				}
				case TypeCode.Char:
				{
					this.Write((Char)value);
					return;
				}
				case TypeCode.DateTime:
				{
					this.Write((DateTime)value);
					return;
				}
				case TypeCode.DBNull:
				case TypeCode.Empty:
				{
					this.writer.Write(JsonReader.LiteralNull);
					return;
				}
				case TypeCode.Decimal:
				{
					// From MSDN:
					// Conversions from Char, SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, and UInt64
					// to Decimal are widening conversions that never lose information or throw exceptions.
					// Conversions from Single or Double to Decimal throw an OverflowException
					// if the result of the conversion is not representable as a Decimal.
					this.Write((Decimal)value);
					return;
				}
				case TypeCode.Double:
				{
					this.Write((Double)value);
					return;
				}
				case TypeCode.Int16:
				{
					this.Write((Int16)value);
					return;
				}
				case TypeCode.Int32:
				{
					this.Write((Int32)value);
					return;
				}
				case TypeCode.Int64:
				{
					this.Write((Int64)value);
					return;
				}
				case TypeCode.SByte:
				{
					this.Write((SByte)value);
					return;
				}
				case TypeCode.Single:
				{
					this.Write((Single)value);
					return;
				}
				case TypeCode.String:
				{
					this.Write((String)value);
					return;
				}
				case TypeCode.UInt16:
				{
					this.Write((UInt16)value);
					return;
				}
				case TypeCode.UInt32:
				{
					this.Write((UInt32)value);
					return;
				}
				case TypeCode.UInt64:
				{
					this.Write((UInt64)value);
					return;
				}
				default:
				case TypeCode.Object:
				{
					// all others must be explicitly tested
					break;
				}
			}

			if (value is Guid)
			{
				this.Write((Guid)value);
				return;
			}

			if (value is Uri)
			{
				this.Write((Uri)value);
				return;
			}

			if (value is TimeSpan)
			{
				this.Write((TimeSpan)value);
				return;
			}

			if (value is Version)
			{
				this.Write((Version)value);
				return;
			}

			// IDictionary test must happen BEFORE IEnumerable test
			// since IDictionary implements IEnumerable
			if (value is IDictionary)
			{
				try
				{
					if (isProperty)
					{
						this.depth++;
						this.WriteLine();
					}
					this.WriteObject((IDictionary)value);
				}
				finally
				{
					if (isProperty)
					{
						this.depth--;
					}
				}
				return;
			}

			if (type.GetInterface(JsonReader.TypeGenericIDictionary) != null)
			{
				throw new JsonSerializationException(JsonReader.ErrorGenericIDictionary);
			}

			// IDictionary test must happen BEFORE IEnumerable test
			// since IDictionary implements IEnumerable
			if (value is IEnumerable)
			{
				if (value is XmlNode)
				{
					this.Write((System.Xml.XmlNode)value);
					return;
				}

				try
				{
					if (isProperty)
					{
						this.depth++;
						this.WriteLine();
					}
					this.WriteArray((IEnumerable)value);
				}
				finally
				{
					if (isProperty)
					{
						this.depth--;
					}
				}
				return;
			}

			// structs and classes
			try
			{
				if (isProperty)
				{
					this.depth++;
					this.WriteLine();
				}
				this.WriteObject(value);
			}
			finally
			{
				if (isProperty)
				{
					this.depth--;
				}
			}
		}

		public virtual void WriteBase64(byte[] value)
		{
			this.Write(Convert.ToBase64String(value));
		}

		public virtual void Write(DateTime value)
		{
			if (this.dateTimeSerializer != null)
			{
				this.dateTimeSerializer(this, value);
				return;
			}

			// UTC DateTime in ISO-8601
			value = value.ToUniversalTime();
			this.Write(String.Format("{0:s}Z", value));
		}

		public virtual void Write(Guid value)
		{
			this.Write(value.ToString("D"));
		}

		public virtual void Write(Enum value)
		{
			string enumName = null;

			Type type = value.GetType();

			if (type.IsDefined(typeof(FlagsAttribute), true) && !Enum.IsDefined(type, value))
			{
				Enum[] flags = JsonWriter.GetFlagList(type, value);
				string[] flagNames = new string[flags.Length];
				for (int i=0; i<flags.Length; i++)
				{
					flagNames[i] = JsonNameAttribute.GetJsonName(flags[i]);
					if (String.IsNullOrEmpty(flagNames[i]))
					{
						flagNames[i] = flags[i].ToString("f");
					}
				}
				enumName = String.Join(", ", flagNames);
			}
			else
			{
				enumName = JsonNameAttribute.GetJsonName(value);
				if (String.IsNullOrEmpty(enumName))
				{
					enumName = value.ToString("f");
				}
			}

			this.Write(enumName);
		}

		public virtual void Write(string value)
		{
			if (value == null)
			{
				this.writer.Write(JsonReader.LiteralNull);
				return;
			}

			int length = value.Length;
			int start = 0;

			this.writer.Write(JsonReader.OperatorStringDelim);

			for (int i = start; i < length; i++)
			{
				if (value[i] <= '\u001F' ||
					value[i] >= '\u007F' ||
					value[i] == '<' ||
					value[i] == '\t' ||
					value[i] == JsonReader.OperatorStringDelim ||
					value[i] == JsonReader.OperatorCharEscape)
				{
					this.writer.Write(value.Substring(start, i-start));
					start = i+1;

					switch (value[i])
					{
						case JsonReader.OperatorStringDelim:
						case JsonReader.OperatorCharEscape:
						{
							this.writer.Write(JsonReader.OperatorCharEscape);
							this.writer.Write(value[i]);
							continue;
						}
						case '\b':
						{
							this.writer.Write("\\b");
							continue;
						}
						case '\f':
						{
							this.writer.Write("\\f");
							continue;
						}
						case '\n':
						{
							this.writer.Write("\\n");
							continue;
						}
						case '\r':
						{
							this.writer.Write("\\r");
							continue;
						}
						case '\t':
						{
							this.writer.Write("\\t");
							continue;
						}
						default:
						{
							this.writer.Write("\\u{0:X4}", Char.ConvertToUtf32(value, i));
							continue;
						}
					}
				}
			}

			this.writer.Write(value.Substring(start, length-start));

			this.writer.Write(JsonReader.OperatorStringDelim);
		}

		public static void WriteEcmaScriptDate(JsonWriter writer, DateTime value)
		{
			// find the time since Jan 1, 1970
			TimeSpan duration = value.ToUniversalTime().Subtract(EcmaScriptEpoch);

			// get the total milliseconds
			long ticks = (long)duration.TotalMilliseconds;

			// write out as a Date constructor
			writer.TextWriter.Write(EcmaScriptDateCtor, ticks);
		}

		#endregion Public Methods

		#region Primative Writer Methods

		public virtual void Write(bool value)
		{
			this.writer.Write(value ? JsonReader.LiteralTrue : JsonReader.LiteralFalse);
		}

		public virtual void Write(byte value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(sbyte value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(short value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(ushort value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(int value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(uint value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(long value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(ulong value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(float value)
		{
			if (this.StrictConformance && (Single.IsNaN(value) || Single.IsInfinity(value)))
			{
				this.writer.Write(JsonReader.LiteralNull);
			}
			else
			{
				this.writer.Write("{0:r}", value);
			}
		}

		public virtual void Write(double value)
		{
			if (this.StrictConformance && (Double.IsNaN(value) || Double.IsInfinity(value)))
			{
				this.writer.Write(JsonReader.LiteralNull);
			}
			else
			{
				this.writer.Write("{0:r}", value);
			}
		}

		public virtual void Write(decimal value)
		{
			this.writer.Write("{0:g}", value);
		}

		public virtual void Write(char value)
		{
			this.Write(new String(value, 1));
		}

		public virtual void Write(TimeSpan value)
		{
			this.Write(value.Ticks);
		}

		public virtual void Write(Uri value)
		{
			this.Write(value.ToString());
		}

		public virtual void Write(Version value)
		{
			this.Write(value.ToString());
		}

		public virtual void Write(XmlNode value)
		{
			// TODO: translate XML to JSON
			this.Write(value.OuterXml);
		}

		#endregion Primative Writer Methods

		#region Writer Methods

		protected internal virtual void WriteArray(IEnumerable value)
		{
			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorArrayStart);

			this.depth++;
			try
			{
				foreach (object item in value)
				{
					if (appendDelim)
					{
						this.writer.Write(JsonReader.OperatorValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					this.WriteLine();
					this.Write(item, false);
				}
			}
			finally
			{
				this.depth--;
			}

			if (appendDelim)
			{
				this.WriteLine();
			}
			this.writer.Write(JsonReader.OperatorArrayEnd);
		}

		protected virtual void WriteObject(IDictionary value)
		{
			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorObjectStart);

			this.depth++;
			try
			{
				foreach (object name in value.Keys)
				{
					if (appendDelim)
					{
						this.writer.Write(JsonReader.OperatorValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					this.WriteLine();
					this.Write((String)name);
					this.writer.Write(JsonReader.OperatorNameDelim);
					this.Write(value[name], true);
				}
			}
			finally
			{
				this.depth--;
			}

			if (appendDelim)
			{
				this.WriteLine();
			}
			this.writer.Write(JsonReader.OperatorObjectEnd);
		}

		protected virtual void WriteObject(object value)
		{
			bool appendDelim = false;

			this.writer.Write(JsonReader.OperatorObjectStart);

			this.depth++;
			try
			{
				Type objType = value.GetType();

				if (!String.IsNullOrEmpty(this.TypeHintName))
				{
					if (appendDelim)
					{
						this.writer.Write(JsonReader.OperatorValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					this.WriteLine();
					this.Write(this.TypeHintName);
					this.writer.Write(JsonReader.OperatorNameDelim);
					this.Write(objType.FullName, true);
				}

				// serialize public properties
				PropertyInfo[] properties = objType.GetProperties();
				foreach (PropertyInfo property in properties)
				{
					if (!property.CanWrite || !property.CanRead)
					{
						continue;
					}

					if (this.IsIgnored(objType, property, value))
					{
						continue;
					}

					object propertyValue = property.GetValue(value, null);
					if (this.IsDefaultValue(property, propertyValue))
					{
						continue;
					}

					if (appendDelim)
					{
						this.writer.Write(JsonReader.OperatorValueDelim);
					}
					else
					{
						appendDelim = true;
					}

					string propertyName = JsonNameAttribute.GetJsonName(property);
					if (String.IsNullOrEmpty(propertyName))
					{
						propertyName = property.Name;
					}

					this.WriteLine();
					this.Write(propertyName);
					this.writer.Write(JsonReader.OperatorNameDelim);
					this.Write(propertyValue, true);
				}

				// serialize public fields
				FieldInfo[] fields = objType.GetFields();
				foreach (FieldInfo field in fields)
				{
					if (!field.IsPublic || field.IsStatic)
					{
						continue;
					}

					if (this.IsIgnored(objType, field, value))
					{
						continue;
					}

					object fieldValue = field.GetValue(value);
					if (this.IsDefaultValue(field, fieldValue))
					{
						continue;
					}

					if (appendDelim)
					{
						this.writer.Write(JsonReader.OperatorValueDelim);
						this.WriteLine();
					}
					else
					{
						appendDelim = true;
					}

					string fieldName = JsonNameAttribute.GetJsonName(field);
					if (String.IsNullOrEmpty(fieldName))
					{
						fieldName = field.Name;
					}

					// use Attributes here to control naming
					this.Write(fieldName);
					this.writer.Write(JsonReader.OperatorNameDelim);
					this.Write(fieldValue, true);
				}
			}
			finally
			{
				this.depth--;
			}

			if (appendDelim)
			{
				this.WriteLine();
			}
			this.writer.Write(JsonReader.OperatorObjectEnd);
		}

		protected virtual void WriteLine()
		{
			if (!this.prettyPrint)
			{
				return;
			}

			this.writer.WriteLine();
			for (int i=0; i<this.depth; i++)
			{
				this.writer.Write(this.tab);
			}
		}

		#endregion Writer Methods

		#region Private Methods

		/// <summary>
		/// Determines if the property or field should not be serialized.
		/// </summary>
		/// <param name="objType"></param>
		/// <param name="member"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>
		/// Checks these in order, if any returns true then this is true:
		/// - is flagged with the JsonIgnoreAttribute property
		/// - has a JsonSpecifiedProperty which returns false
		/// </remarks>
		private bool IsIgnored(Type objType, MemberInfo member, object obj)
		{
			if (JsonIgnoreAttribute.IsJsonIgnore(member))
			{
				return true;
			}

			string specifiedProperty = JsonSpecifiedPropertyAttribute.GetJsonSpecifiedProperty(member);
			if (!String.IsNullOrEmpty(specifiedProperty))
			{
				PropertyInfo specProp = objType.GetProperty(specifiedProperty);
				if (specProp != null)
				{
					object isSpecified = specProp.GetValue(obj, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
					{
						return true;
					}
				}
			}

			if (this.UseXmlSerializationAttributes)
			{
				if (JsonIgnoreAttribute.IsXmlIgnore(member))
				{
					return true;
				}

				PropertyInfo specProp = objType.GetProperty(member.Name+"Specified");
				if (specProp != null)
				{
					object isSpecified = specProp.GetValue(obj, null);
					if (isSpecified is Boolean && !Convert.ToBoolean(isSpecified))
					{
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Determines if the member value matches the DefaultValue attribute
		/// </summary>
		/// <returns>if has a value equivalent to the DefaultValueAttribute</returns>
		private bool IsDefaultValue(MemberInfo member, object value)
		{
			DefaultValueAttribute attribute = Attribute.GetCustomAttribute(member, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
			if (attribute == null)
			{
				return false;
			}

			if (attribute.Value == null)
			{
				return (value == null);
			}

			return (attribute.Value.Equals(value));
		}

		#region GetFlagList

		/// <summary>
		/// Splits a bitwise-OR'd set of enums into a list.
		/// </summary>
		/// <param name="enumType">the enum type</param>
		/// <param name="value">the combined value</param>
		/// <returns>list of flag enums</returns>
		/// <remarks>
		/// from PseudoCode.EnumHelper
		/// </remarks>
		private static Enum[] GetFlagList(Type enumType, object value)
		{
			ulong longVal = Convert.ToUInt64(value);
			string[] enumNames = Enum.GetNames(enumType);
			Array enumValues = Enum.GetValues(enumType);

			List<Enum> enums = new List<Enum>(enumValues.Length);

			// check for empty
			if (longVal == 0L)
			{
				// Return the value of empty, or zero if none exists
				if (Convert.ToUInt64(enumValues.GetValue(0)) == 0L)
					enums.Add(enumValues.GetValue(0) as Enum);
				else
					enums.Add(null);
				return enums.ToArray();
			}

			for (int i = enumValues.Length-1; i >= 0; i--)
			{
				ulong enumValue = Convert.ToUInt64(enumValues.GetValue(i));

				if ((i == 0) && (enumValue == 0L))
					continue;

				// matches a value in enumeration
				if ((longVal & enumValue) == enumValue)
				{
					// remove from val
					longVal -= enumValue;

					// add enum to list
					enums.Add(enumValues.GetValue(i) as Enum);
				}
			}

			if (longVal != 0x0L)
				enums.Add(Enum.ToObject(enumType, longVal) as Enum);

			return enums.ToArray();
		}

		#endregion GetFlagList

		#endregion Private Methods

		#region IDisposable Members

		void IDisposable.Dispose()
		{
			if (this.writer != null)
			{
				this.writer.Dispose();
			}
		}

		#endregion IDisposable Members
	}
}
