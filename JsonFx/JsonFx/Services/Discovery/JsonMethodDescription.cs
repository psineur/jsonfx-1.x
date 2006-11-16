using System;
using System.ComponentModel;
using System.Reflection;

using JsonFx.Serialization;

namespace JsonFx.Services.Discovery
{
	public class JsonMethodDescription : JsonDescriptionBase
	{
		#region Fields

		private string name;
		private string summary;
		private string help;
		private bool idempotent;
		private JsonNamedParameterDescription[] paramArgs;
		private JsonParameterDescription returnVal;

		#endregion Fields

		#region Init

		public JsonMethodDescription() { }

		/// <summary>
		/// Ctor.
		/// </summary>
		internal JsonMethodDescription(MethodInfo method)
		{
			//TODO: clean up JsonMethodDescription efficiency

			if (method == null)
				return;

			if (!JsonMethodAttribute.IsJsonMethod(method))
				throw new JsonServiceException("Specified method is not marked as a JsonMethod.");

			this.Name = JsonMethodAttribute.GetJsonName(method);
			if (String.IsNullOrEmpty(this.Name))
				this.name = method.Name;

			ParameterInfo[] parameters = method.GetParameters();
			this.Params = new JsonNamedParameterDescription[parameters.Length];
			for (int i=0; i<parameters.Length; i++)
			{
				this.Params[i] = new JsonNamedParameterDescription(parameters[i]);
			}

			this.Return = new JsonParameterDescription(method.ReturnParameter);

			this.Help = JsonMethodAttribute.GetHelpUrl(method);
			this.Idempotent = JsonMethodAttribute.IsIdempotent(method);

			if (Attribute.IsDefined(method, typeof(DescriptionAttribute)))
			{
				DescriptionAttribute description = Attribute.GetCustomAttribute(method, typeof(DescriptionAttribute), true) as DescriptionAttribute;
				this.Summary = description.Description;
			}
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets and sets a simple name for the method.
		/// </summary>
		/// <remarks>
		/// REQUIRED. A String value that provides a simple name for the method.
		/// </remarks>
		[JsonName("name")]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Gets and sets a summary of the purpose of the service.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that summarizes the purpose of the service.
		/// This SHOULD be kept to a maximum of 3 sentences and often limited to
		/// a single phrase like, "Lets you search for hyperlinks that have been
		/// tagged by particular tags."
		/// </remarks>
		[JsonName("summary")]
		public string Summary
		{
			get { return this.summary; }
			set { this.summary = value; }
		}

		/// <summary>
		/// Gets and sets a URL from where human-readable documentation about the procedure may be obtained.
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A String value that is a URL from where human-readable
		/// documentation about the procedure may be obtained.
		/// </remarks>
		[JsonName("help")]
		public string Help
		{
			get { return this.help; }
			set { this.help = value; }
		}

		/// <summary>
		/// Gets and sets 
		/// </summary>
		/// <remarks>
		/// OPTIONAL. A Boolean value that indicates whether the procedure is
		/// idempotent and therefore essentially safe to invoke over an HTTP GET
		/// transaction. This member MUST be present and true for the procedure
		/// to be considered idempotent.
		/// http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.1
		/// </remarks>
		[JsonName("idempotent")]
		public bool Idempotent
		{
			get { return this.idempotent; }
			set { this.idempotent = value; }
		}

		/// <summary>
		/// Gets and sets 
		/// </summary>
		/// <remarks>
		/// OPTIONAL. An Array value whose elements are either Procedure Parameter Description
		/// objects or String values. If an element each of uniquely describes a single
		/// parameter of the procedure. If the only description that is available of each
		/// parameter is its name, then a service MAY instead supply an Array of String elements
		/// for this member and where each element uniquely names a parameter and the parameter
		/// is assumed to be typed as "any". In either case, the elements of the array MUST be
		/// ordered after the formal argument list of the procedure being described. If this
		/// member is missing or the Null value then the procedure does not expect any parameters.
		/// </remarks>
		[JsonName("params")]
		public JsonNamedParameterDescription[] Params
		{
			get { return this.paramArgs; }
			set { this.paramArgs = value; }
		}

		/// <summary>
		/// Gets and sets 
		/// </summary>
		/// <remarks>
		/// OPTIONAL. An Object value that is structured after the Procedure Parameter Description
		/// and which describes the output from the procedure. Otherwise, if it is a String value,
		/// then it defines the type of the return value. If this member is missing or is the Null
		/// value then the return type of the procedure is defined to be "any".
		/// </remarks>
		[JsonName("return")]
		public JsonParameterDescription Return
		{
			get { return this.returnVal; }
			set { this.returnVal = value; }
		}

		#endregion Properties
	}
}
