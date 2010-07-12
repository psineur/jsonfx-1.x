﻿#region License
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
using System.Linq;

using JsonFx.Serialization;
using Xunit;

using Assert=JsonFx.AssertPatched;

namespace JsonFx.Json
{
	public class JsonAnalyzerTests
	{
		#region Array Tests

		[Fact]
		public void Parse_ArrayEmpty_ReturnsEmptyArray()
		{
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[0];

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayOneItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[] { null };

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayMultiItem_ReturnsExpectedArray()
		{
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[]
			{
				0,
				null,
				false,
				true
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayNestedDeeply_ReturnsExpectedArray()
		{
			// input from pass2.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Not too deep"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenArrayEnd
			};

			var expected = new []
			{
				new []
				{
					new []
					{
						new []
						{
							new []
							{
								new []
								{
									new []
									{
										new []
										{
											new []
											{
												new []
												{
													new []
													{
														new []
														{
															new []
															{
																new []
																{
																	new []
																	{
																		new []
																		{
																			new []
																			{
																				new []
																				{
																					new []
																					{
																						"Not too deep"
																					}
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze((input)).Cast<string[][][][][][][][][][][][][][][][][][][]>().Single();

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void Parse_ArrayUnclosed_ThrowsAnalyzerException()
		{
			// input from fail2.json in test suite at http://www.json.org/JSON_checker/
			var input = new []
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Unclosed array")
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNone, ex.Token);
		}

		[Fact]
		public void Parse_ArrayExtraComma_ThrowsAnalyzerException()
		{
			// input from fail4.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenArrayEnd, ex.Token);
		}

		[Fact]
		public void Parse_ArrayDoubleExtraComma_ThrowsAnalyzerException()
		{
			// input from fail5.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("double extra comma"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayMissingValue_ThrowsAnalyzerException()
		{
			// input from fail6.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("<-- missing value"),
				JsonGrammar.TokenArrayEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayColonInsteadOfComma_ThrowsAnalyzerException()
		{
			// input from fail22.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Colon instead of comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenArrayEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenPairDelim, ex.Token);
		}

		[Fact]
		public void Parse_ArrayBadValue_ThrowsAnalyzerException()
		{
			// input from fail23.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("Bad value"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenLiteral("truth"),
				JsonGrammar.TokenArrayEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenLiteral("truth"), ex.Token);
		}

		[Fact]
		public void Parse_ArrayCloseMismatch_ThrowsAnalyzerException()
		{
			// input from fail33.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("mismatch"),
				JsonGrammar.TokenObjectEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenObjectEnd, ex.Token);
		}

		#endregion Array Tests

		#region Object Tests

		[Fact]
		public void Parse_ObjectEmpty_ReturnsEmptyObject()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>();

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectOneProperty_ReturnsSimpleObject()
		{
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("key"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("value"),
				JsonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{ "key", "value" }
				};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectNested_ReturnsNestedObject()
		{
			// input from pass3.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass3"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("The outermost value"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("must be an object or array."),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("In this test"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("It is an object."),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenObjectEnd
			};

			var expected = new Dictionary<string, object>
				{
					{
						"JSON Test Pattern pass3",
						new Dictionary<string, object>
						{
							{ "The outermost value", "must be an object or array." },
							{ "In this test", "It is an object." }
						}
					}
				};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<IDictionary<string, object>>().Single();

			Assert.Equal(expected, actual, false);
		}

		[Fact]
		public void Parse_ObjectExtraComma_ThrowsAnalyzerException()
		{
			// input from fail9.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Extra comma"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenObjectEnd, ex.Token);
		}

		[Fact]
		public void Parse_ObjectMissingColon_ThrowsAnalyzerException()
		{
			// input from fail19.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Missing colon"),
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNull, ex.Token);
		}

		[Fact]
		public void Parse_ObjectDoubleColon_ThrowsAnalyzerException()
		{
			// input from fail20.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Double colon"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenPairDelim, ex.Token);
		}

		[Fact]
		public void Parse_ObjectCommaInsteadOfColon_ThrowsAnalyzerException()
		{
			// input from fail21.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead of colon"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenObjectEnd
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenValueDelim, ex.Token);
		}

		[Fact]
		public void Parse_ObjectCommaInsteadOfClose_ThrowsAnalyzerException()
		{
			// input from fail32.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("Comma instead if closing brace"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			AnalyzerException<JsonTokenType> ex = Assert.Throws<AnalyzerException<JsonTokenType>>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected token
			Assert.Equal(JsonGrammar.TokenNone, ex.Token);
		}

		#endregion Object Tests

		#region Complex Graph Tests

		[Fact]
		public void Parse_GraphComplex_ReturnsGraph()
		{
			// input from pass1.json in test suite at http://www.json.org/JSON_checker/
			var input = new[]
			{
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("JSON Test Pattern pass1"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("object with 1 member"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenString("array with 1 element"),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(-42),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenString("integer"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1234567890),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("real"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(-9876.543210),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("e"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0.123456789e-12),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("E"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1.234567890E+34),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(""),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(23456789012E66),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("zero"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("one"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("space"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quote"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\""),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("backslash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\\"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("controls"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\b\f\n\r\t"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("slash"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("/ & /"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("alpha"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("abcdefghijklmnopqrstuvwyz"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("ALPHA"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("ABCDEFGHIJKLMNOPQRSTUVWYZ"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("0123456789"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("digit"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("special"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("`1~!@#$%^&*()_+-={':[,]}|;.</>?"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("hex"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("\u0123\u4567\u89AB\uCDEF\uabcd\uef4A"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("true"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenTrue,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("false"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenFalse,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("null"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenNull,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("array"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("object"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenObjectBegin,
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("address"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("50 St. James Street"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("url"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("http://www.JSON.org/"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("comment"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("// /* <!-- --"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("# -- --> */"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString(" "),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString(" s p a c e d "),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("compact"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenArrayBegin,
				JsonGrammar.TokenNumber(1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(3),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(4),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(7),
				JsonGrammar.TokenArrayEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("jsontext"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("{\"object with 1 member\":[\"array with 1 element\"]}"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("quotes"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("&#34; \u0022 %22 0x22 034 &#x22;"),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?"),
				JsonGrammar.TokenPairDelim,
				JsonGrammar.TokenString("A key can be any string"),
				JsonGrammar.TokenObjectEnd,
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.5),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(98.6),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(99.44),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1066),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(10.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(0.1),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(1.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenNumber(2.0),
				JsonGrammar.TokenValueDelim,
				JsonGrammar.TokenString("rosebud"),
				JsonGrammar.TokenArrayEnd
			};

			var expected = new object[] {
				"JSON Test Pattern pass1",
				new Dictionary<string, object>
				{
					{ "object with 1 member", new[] { "array with 1 element" } },
				},
				new Dictionary<string, object>(),
				new object[0],
				-42,
				true,
				false,
				null,
				new Dictionary<string, object> {
					{ "integer", 1234567890 },
					{ "real", -9876.543210 },
					{ "e", 0.123456789e-12 },
					{ "E", 1.234567890E+34 },
					{ "", 23456789012E66 },
					{ "zero", 0 },
					{ "one", 1 },
					{ "space", " " },
					{ "quote", "\"" },
					{ "backslash", "\\" },
					{ "controls", "\b\f\n\r\t" },
					{ "slash", "/ & /" },
					{ "alpha", "abcdefghijklmnopqrstuvwyz" },
					{ "ALPHA", "ABCDEFGHIJKLMNOPQRSTUVWYZ" },
					{ "digit", "0123456789" },
					{ "0123456789", "digit" },
					{ "special", "`1~!@#$%^&*()_+-={':[,]}|;.</>?" },
					{ "hex", "\u0123\u4567\u89AB\uCDEF\uabcd\uef4A" },
					{ "true", true },
					{ "false", false },
					{ "null", null },
					{ "array", new object[0] },
					{ "object", new Dictionary<string, object>() },
					{ "address", "50 St. James Street" },
					{ "url", "http://www.JSON.org/" },
					{ "comment", "// /* <!-- --" },
					{ "# -- --> */", " " },
					{ " s p a c e d ", new [] { 1,2,3,4,5,6,7 } },
					{ "compact", new [] { 1,2,3,4,5,6,7 } },
					{ "jsontext", "{\"object with 1 member\":[\"array with 1 element\"]}" },
					{ "quotes", "&#34; \u0022 %22 0x22 034 &#x22;" },
					{ "/\\\"\uCAFE\uBABE\uAB98\uFCDE\ubcda\uef4A\b\f\n\r\t`1~!@#$%^&*()_+-=[]{}|;:',./<>?", "A key can be any string" }
				},
				0.5,
				98.6,
				99.44,
				1066,
				1e1,
				0.1e1,
				1e-1,
				1e00,
				2e+00,
				2e-00,
				"rosebud"
			};

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			var actual = analyzer.Analyze(input).Cast<object[]>().Single();

			Assert.Equal(expected, actual, false);
		}

		#endregion Complex Graph Tests

		#region Input Edge Case Tests

		[Fact]
		public void Parse_EmptyInput_ReturnsNothing()
		{
			var input = Enumerable.Empty<Token<JsonTokenType>>();

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());
			Assert.False(analyzer.Analyze<object>(input).Any());
		}

		[Fact]
		public void Parse_NullInput_ThrowsArgumentNullException()
		{
			var input = (IEnumerable<Token<JsonTokenType>>)null;

			var analyzer = new JsonReader.JsonAnalyzer(new DataReaderSettings());

			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var actual = analyzer.Analyze<object>(input).Single();
				});

			// verify exception is coming from expected param
			Assert.Equal("tokens", ex.ParamName);
		}

		[Fact]
		public void Ctor_NullSettings_ThrowsArgumentNullException()
		{
			ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
				delegate
				{
					var analyzer = new JsonReader.JsonAnalyzer(null);
				});

			// verify exception is coming from expected param
			Assert.Equal("settings", ex.ParamName);
		}

		#endregion Input Edge Case Tests
	}
}