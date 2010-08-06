#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2010 Stephen M. McKamey

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
using System.Collections.Generic;
using System.Text;

using JsonFx.Serialization;
using JsonFx.Serialization.Filters;

namespace JsonFx.Common
{
	/// <summary>
	/// Provides base implementation for standard serializers
	/// </summary>
	public abstract partial class CommonWriter : DataWriter<CommonTokenType>
	{
		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public CommonWriter()
			: this(new DataWriterSettings())
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		public CommonWriter(DataWriterSettings settings)
			: base(settings, null)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public CommonWriter(DataWriterSettings settings, params IDataFilter<CommonTokenType>[] filters)
			: base(settings, (IEnumerable<IDataFilter<CommonTokenType>>)filters)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="filters"></param>
		public CommonWriter(DataWriterSettings settings, IEnumerable<IDataFilter<CommonTokenType>> filters)
			: base(settings, filters)
		{
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the content encoding for the serialized data
		/// </summary>
		public override Encoding ContentEncoding
		{
			get { return Encoding.UTF8; }
		}

		#endregion Properties

		#region DataWriter<DataTokenType> Methods

		/// <summary>
		/// Gets a walker for JSON
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override IObjectWalker<CommonTokenType> GetWalker()
		{
			return new CommonWalker(this.Settings, this.Filters);
		}

		#endregion DataWriter<DataTokenType> Methods
	}
}
