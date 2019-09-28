// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See License.txt in the project root for license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MethodCheck.Core;
using MethodCheck.Core.Data;
using MethodCheck.Core.Parsing;
using NUnit.Framework;

namespace MethodCheck.Test
{
	[TestFixture]
	class MethodFormatterTest
	{
		[Test]
		public void ParseAndFormat([ValueSource(nameof(Samples))] string name)
		{
			var source = Load(name);
			var match = Splitter.Match(source);
			Assert.That(match.Success, Is.True);

			var group = match.Groups["splitter"];
			var blob = BinaryProcessor.Parse(source.AsSpan(0, group.Index));

			MethodData? data;

			if (name.StartsWith("Body.", StringComparison.InvariantCulture))
			{
				data = MethodParser.ParseBody(blob);
			}
			else if (name.StartsWith("IL.", StringComparison.InvariantCulture))
			{
				data = MethodParser.ParseIL(blob);
			}
			else
			{
				throw new ArgumentException();
			}

			var actual = data == null ? null : MethodFormatter.Format(data);
			var expected = source.Substring(group.Index + group.Length);

			Assert.That(actual, Is.EqualTo(expected));
		}

		#region Implementation

		protected static IEnumerable<string> Samples
		{
			get
			{
				return
					from resource in typeof(MethodFormatterTest).Assembly.GetManifestResourceNames()
					where resource.StartsWith(ResourcePrefix, StringComparison.InvariantCulture)
					select resource.Substring(ResourcePrefix.Length);
			}
		}

		static string Load(string name)
		{
			using var stream = typeof(MethodFormatterTest).Assembly.GetManifestResourceStream(ResourcePrefix + name);
			using var reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}

		const string ResourcePrefix = "MethodCheck.Test.Samples.";
		static readonly Regex Splitter = new Regex(@"(?<splitter>===+\r?\n)", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

		#endregion
	}
}
