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
using System.Collections.Generic;
using System.Web.Compilation;

using JsonFx.JsonML.BST;

namespace JsonFx.Compilation
{
	public class JbstCodeProvider : JsonFx.Compilation.ResourceCodeProvider
	{
		#region Fields

		private string source = null;
		private string jsonp = null;
		private bool hasJsonp = false;
		private bool isJsonpVar = false;

		#endregion Fields

		#region ResourceCodeProvider

		public override string ContentType
		{
			get { return "text/javascript"; }
		}

		public override string FileExtension
		{
			get { return "js"; }
		}

		protected override IList<BuildTools.ParseException> PreProcess(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			TextWriter writer)
		{
			this.source = this.ParseDirective(sourceText, virtualPath);

			// parse JBST markup
			JbstCompiler parser = new JbstCompiler();
			parser.Parse(this.source);

			using (StringWriter sw = new StringWriter())
			{
				// render a pretty-print debug version
				this.Render(parser, sw, true);
				sw.Flush();
				writer.Write(sw.ToString());
			}

			using (StringWriter sw = new StringWriter())
			{
				// render the compacted version
				this.Render(parser, sw, false);
				sw.Flush();
				this.source = sw.ToString();
			}

			// report any errors
			return parser.Errors;
		}

		protected override IList<BuildTools.ParseException> Compact(
			IResourceBuildHelper helper,
			string virtualPath,
			string sourceText,
			TextWriter writer)
		{
			writer.Write(this.source);
			return null;
		}

		#endregion ResourceCodeProvider

		#region Methods

		private void Render(JbstCompiler parser, TextWriter writer, bool prettyPrint)
		{
			if (this.hasJsonp)
			{
				// wrap in JsonP
				writer.Write(jsonp);
				if (this.isJsonpVar)
				{
					if (prettyPrint)
					{
						writer.Write(" = ");
					}
					else
					{
						writer.Write('=');
					}
				}
				writer.Write('(');
			}

			parser.Render(writer, prettyPrint);

			if (this.hasJsonp)
			{
				writer.Write(");");
			}
			if (prettyPrint)
			{
				writer.WriteLine();
			}
		}

		private string ParseDirective(string sourceText, string virtualPath)
		{
			int lineNumber;

			DirectiveParser parser = new DirectiveParser(sourceText, virtualPath);
			parser.ProcessDirective += new DirectiveParser.ProcessDirectiveEvent(this.ProcessDirective);

			int index = parser.ParseDirectives(out lineNumber);

			this.hasJsonp = !String.IsNullOrEmpty(this.jsonp);

			return sourceText.Substring(index).Trim();
		}

		private void ProcessDirective(string directiveName, IDictionary<string, string> attribs, int lineNumber)
		{
			string name = attribs.ContainsKey("Name") ? attribs["Name"] : null;
			if (!String.IsNullOrEmpty(name))
			{
				this.jsonp = name;
				this.isJsonpVar = true;
				return;
			}

			string method = attribs.ContainsKey("Callback") ? attribs["Callback"] : null;
			if (!String.IsNullOrEmpty(method))
			{
				this.jsonp = method;
				this.isJsonpVar = false;
				return;
			}
		}

		#endregion Methods
	}
}