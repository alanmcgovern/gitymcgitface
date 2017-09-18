// MIT License
// 
// Copyright (c) 2017 Alan McGovern
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace libgitface
{
	public sealed class Repository
	{
		public string Owner {
			get;
		}

		public string Name {
			get;
		}

		public Uri Uri {
			get;
		}

		public Repository (Uri uri)
			: this (uri, null)
		{

		}

		public Repository (Uri baseUri, string relativeUri)
		{
			if (relativeUri == null)
				Uri = baseUri;
			else
				Uri = new Uri (baseUri, relativeUri);

			var parts = Uri.PathAndQuery.Split (new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			Owner = parts [0];
			Name = parts [1];
		}
	}
}
