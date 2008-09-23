using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

using JsonFx.Handlers;
using JsonFx.Compilation;

namespace JsonFx.Client
{
	/// <summary>
	/// Base control for referencing a ResourceHandler
	/// </summary>
	public class ResourceInclude : Control
	{
		#region Constants

		private const string StyleImport = "<style type=\"{0}\">@import url({1});</style>";
		private const string ScriptInclude = "<script type=\"{0}\" src=\"{1}\"></script>";

		#endregion Constants

		#region Fields

		private bool isDebug = false;
		private string sourceUrl = String.Empty;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets and sets if should render a debuggable ("Pretty-Print") reference.
		/// </summary>
		[DefaultValue(false)]
		public bool IsDebug
		{
			get { return this.isDebug; }
			set { this.isDebug = value; }
		}

		/// <summary>
		/// Gets and sets resource url.
		/// </summary>
		[DefaultValue("")]
		public string SourceUrl
		{
			get { return this.sourceUrl; }
			set { this.sourceUrl = value; }
		}

		#endregion Properties

		#region Page Event Handlers

		protected override void Render(HtmlTextWriter writer)
		{
			string url = this.SourceUrl;
			CompiledBuildResult info = CompiledBuildResult.Create(url);
			if (info == null)
			{
				throw new ArgumentException(String.Format(
					"Error loading \"{0}\".  Either not found or a build error occurred.",
					url));
			}

			url = ResourceHandler.GetResourceUrl(url, this.isDebug);
			url = this.ResolveUrl(url);
			string type =
				String.IsNullOrEmpty(info.ContentType) ?
				String.Empty :
				info.ContentType.ToLowerInvariant();

			switch (type)
			{
				case CssResourceCodeProvider.MimeType:
				{
					this.RenderStyleImport(writer, url, info.ContentType);
					break;
				}
				default:
				{
					this.RenderScriptInclude(writer, url, info.ContentType);
					break;
				}
			}

			if (info is GlobalizedCompiledBuildResult)
			{
				url = ResourceHandler.GetLocalizationUrl(this.SourceUrl, this.isDebug);
				url = this.ResolveUrl(url);
				this.RenderScriptInclude(writer, url, ScriptResourceCodeProvider.MimeType);
			}
		}

		private void RenderStyleImport(HtmlTextWriter writer, string url, string mimeType)
		{
			writer.Write(StyleImport, mimeType, url);
		}

		private void RenderScriptInclude(HtmlTextWriter writer, string url, string mimeType)
		{
			writer.Write(ScriptInclude, mimeType, url);
		}

		#endregion Page Event Handlers
	}
}