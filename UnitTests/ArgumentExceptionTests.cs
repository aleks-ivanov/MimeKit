﻿//
// ArgumentExceptionTests.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2016 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.IO;
using System.Reflection;

using NUnit.Framework;

using MimeKit;
using MimeKit.IO;
using MimeKit.IO.Filters;
using MimeKit.Cryptography;

namespace UnitTests {
	[TestFixture]
	public class ArgumentExceptionTests
	{
		static void AssertFilterArguments (IMimeFilter filter)
		{
			int outputIndex, outputLength;
			var input = new byte[1024];
			ArgumentException ex;

			// Filter
			Assert.Throws<ArgumentNullException> (() => filter.Filter (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Filter (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			// Flush
			Assert.Throws<ArgumentNullException> (() => filter.Flush (null, 0, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentNullException when input was null.", filter.GetType ().Name);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, -1, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was -1.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, -1, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was -1.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 1025, 0, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when startIndex was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("startIndex", ex.ParamName);

			ex = Assert.Throws<ArgumentOutOfRangeException> (() => filter.Flush (input, 0, 1025, out outputIndex, out outputLength),
				"{0}.Filter did not throw ArgumentOutOfRangeException when length was > 1024.", filter.GetType ().Name);
			Assert.AreEqual ("length", ex.ParamName);
		}

		[Test]
		public void TestFilterArguments ()
		{
			AssertFilterArguments (new Dos2UnixFilter ());
			AssertFilterArguments (new Unix2DosFilter ());
			AssertFilterArguments (new ArmoredFromFilter ());
			AssertFilterArguments (new BestEncodingFilter ());
			AssertFilterArguments (new CharsetFilter ("iso-8859-1", "utf-8"));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.Base64));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.Base64));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.QuotedPrintable));
			AssertFilterArguments (DecoderFilter.Create (ContentEncoding.UUEncode));
			AssertFilterArguments (EncoderFilter.Create (ContentEncoding.UUEncode));
			AssertFilterArguments (new TrailingWhitespaceFilter ());
			AssertFilterArguments (new DkimRelaxedBodyFilter ());
			AssertFilterArguments (new DkimSimpleBodyFilter ());
		}

		static void AssertParseArguments (Type type)
		{
			const string text = "this is a dummy text buffer";
			var options = ParserOptions.Default;
			var buffer = new byte[1024];

			foreach (var method in type.GetMethods (BindingFlags.Public | BindingFlags.Static)) {
				if (method.Name != "Parse")
					continue;

				var parameters = method.GetParameters ();
				var args = new object[parameters.Length];
				TargetInvocationException tie;
				ArgumentException ex;
				int bufferIndex = 0;
				int idx = 0;
				int length;

				if (parameters[idx].ParameterType == typeof (ParserOptions))
					args[idx++] = null;

				// this is either a byte[] or string buffer
				bufferIndex = idx;
				if (parameters[idx].ParameterType == typeof (byte[])) {
					length = buffer.Length;
					args[idx++] = buffer;
				} else {
					length = text.Length;
					args[idx++] = text;
				}

				for (int i = idx; i < parameters.Length; i++) {
					switch (parameters[i].Name) {
					case "startIndex": args[i] = 0; break;
					case "length": args[i] = length; break;
					default:
						Assert.Fail ("Unknown parameter: {0} for {1}.Parse", parameters[i].Name, type.Name);
						break;
					}
				}

				if (bufferIndex == 1) {
					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when options was null.", type.Name);
					Assert.IsInstanceOf<ArgumentNullException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual ("options", ex.ParamName);

					args[0] = options;
				}

				var buf = args[bufferIndex];
				args[bufferIndex] = null;
				tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
					"{0}.Parse did not throw an exception when {1} was null.", type.Name, parameters[bufferIndex].Name);
				Assert.IsInstanceOf<ArgumentNullException> (tie.InnerException);
				ex = (ArgumentException) tie.InnerException;
				Assert.AreEqual (parameters[bufferIndex].Name, ex.ParamName);
				args[bufferIndex] = buf;

				if (idx < parameters.Length) {
					// startIndex
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw ArgumentOutOfRangeException when {1} was -1.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was > length.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx++] = 0;
				}

				if (idx < parameters.Length) {
					// length
					args[idx] = -1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was -1.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					args[idx] = length + 1;

					tie = Assert.Throws<TargetInvocationException> (() => method.Invoke (null, args),
						"{0}.Parse did not throw an exception when {1} was > length.", type.Name, parameters[idx].Name);
					Assert.IsInstanceOf<ArgumentOutOfRangeException> (tie.InnerException);
					ex = (ArgumentException) tie.InnerException;
					Assert.AreEqual (parameters[idx].Name, ex.ParamName);

					idx++;
				}
			}
		}

		[Test]
		public void TestParseArguments ()
		{
			AssertParseArguments (typeof (GroupAddress));
			AssertParseArguments (typeof (MailboxAddress));
			AssertParseArguments (typeof (InternetAddress));
			AssertParseArguments (typeof (InternetAddressList));

			AssertParseArguments (typeof (ContentDisposition));
			AssertParseArguments (typeof (ContentType));
		}

		static void AssertStreamArguments (Stream stream)
		{
			var buffer = new byte[1024];
			ArgumentException ex;

			if (stream.CanRead) {
				ex = Assert.Throws<ArgumentNullException> (() => stream.Read (null, 0, 0),
					"{0}.Read() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.AreEqual ("buffer", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, -1, 0),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.AreEqual ("offset", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, buffer.Length + 1, 0),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.AreEqual ("offset", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, 0, -1),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.AreEqual ("count", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Read (buffer, 0, buffer.Length + 1),
					"{0}.Read() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.AreEqual ("count", ex.ParamName);
			}

			if (stream.CanWrite) {
				ex = Assert.Throws<ArgumentNullException> (() => stream.Write (null, 0, 0),
					"{0}.Write() does not throw an ArgumentNullException when buffer is null.", stream.GetType ().Name);
				Assert.AreEqual ("buffer", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, -1, 0),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when offset is -1.", stream.GetType ().Name);
				Assert.AreEqual ("offset", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, buffer.Length + 1, 0),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when offset > buffer length.", stream.GetType ().Name);
				Assert.AreEqual ("offset", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, -1),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when count is -1.", stream.GetType ().Name);
				Assert.AreEqual ("count", ex.ParamName);

				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Write (buffer, 0, buffer.Length + 1),
					"{0}.Write() does not throw an ArgumentOutOfRangeException when count > buffer length.", stream.GetType ().Name);
				Assert.AreEqual ("count", ex.ParamName);
			}

			if (stream.CanSeek) {
				ex = Assert.Throws<ArgumentOutOfRangeException> (() => stream.Seek (0, (SeekOrigin) 255),
					"{0}.Seek() does not throw an ArgumentOutOfRangeException when origin is invalid.", stream.GetType ().Name);
				Assert.AreEqual ("origin", ex.ParamName);
			}
		}

		[Test]
		public void TestStreamArguments ()
		{
			using (var stream = new MeasuringStream ())
				AssertStreamArguments (stream);

			using (var stream = new MemoryBlockStream ())
				AssertStreamArguments (stream);

			using (var memory = new MemoryStream ()) {
				using (var stream = new FilteredStream (memory))
					AssertStreamArguments (stream);
			}

			using (var memory = new MemoryStream ()) {
				using (var stream = new BoundStream (memory, 0, -1, true))
					AssertStreamArguments (stream);
			}

			using (var memory = new MemoryStream ()) {
				using (var stream = new ChainedStream ())
					AssertStreamArguments (stream);
			}
		}
	}
}