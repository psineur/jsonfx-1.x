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
using System.Reflection;

namespace JsonFx.Serialization.Resolvers
{
	/// <summary>
	/// Controls name resolution for IDataReader / IDataWriter by using pluggable delegate callbacks
	/// </summary>
	public sealed class CallbackResolverStrategy : IResolverStrategy
	{
		#region Properties

		public delegate bool PropertyIgnoredDelegate(PropertyInfo propertyInfo, bool isAnonymous);

		/// <summary>
		/// Gets and sets the implementation for ignoring properties
		/// </summary>
		public PropertyIgnoredDelegate IsPropertyIgnored
		{
			get;
			set;
		}

		public delegate bool FieldIgnoredDelegate(FieldInfo fieldInfo);

		/// <summary>
		/// Gets and sets the implementation for ignoring fields
		/// </summary>
		public FieldIgnoredDelegate IsFieldIgnored
		{
			get;
			set;
		}

		public delegate ValueIgnoredDelegate GetValueIgnoredDelegate(MemberInfo memberInfo);

		/// <summary>
		/// Gets and sets the implementation for ignoring properties by value
		/// </summary>
		public GetValueIgnoredDelegate GetValueIgnored
		{
			get;
			set;
		}

		public delegate string GetNameDelegate(MemberInfo memberInfo);

		/// <summary>
		/// Gets and sets the implementation for naming members
		/// </summary>
		public GetNameDelegate GetName
		{
			get;
			set;
		}

		#endregion Properties

		#region IResolverStrategy Members

		bool IResolverStrategy.IsPropertyIgnored(PropertyInfo member, bool isAnonymousType)
		{
			if (this.IsPropertyIgnored == null)
			{
				return false;
			}

			return this.IsPropertyIgnored(member, isAnonymousType);
		}

		bool IResolverStrategy.IsFieldIgnored(FieldInfo member)
		{
			if (this.IsFieldIgnored == null)
			{
				return false;
			}

			return this.IsFieldIgnored(member);
		}

		ValueIgnoredDelegate IResolverStrategy.GetValueIgnoredCallback(MemberInfo member)
		{
			if (this.GetValueIgnored == null)
			{
				return null;
			}

			return this.GetValueIgnored(member);
		}

		string IResolverStrategy.GetName(MemberInfo member)
		{
			if (this.GetName == null)
			{
				return null;
			}

			return this.GetName(member);
		}

		#endregion IResolverStrategy Members
	}
}