﻿//
// WorkspaceSerializationExtension.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
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
using System;
using System.IO;
using System.Threading.Tasks;
using MonoDevelop.Core;
using System.Xml;
using MonoDevelop.Core.Serialization;
using MonoDevelop.Projects.Formats.MD1;

namespace MonoDevelop.Projects
{
	class WorkspaceSerializationExtension: ProjectServiceExtension
	{
		public override bool FileIsObjectOfType (string file, Type type)
		{
			if (type.IsAssignableFrom (typeof(Workspace))) {
				string ext = Path.GetExtension (file).ToLower ();
				if (ext == ".mdw")
					return true;
			}
			return base.FileIsObjectOfType (file, type);
		}

		public override async Task<WorkspaceItem> LoadWorkspaceItem (ProgressMonitor monitor, string fileName)
		{
			string ext = Path.GetExtension (fileName).ToLower ();
			if (ext == ".mdw")
				return await ReadWorkspaceItemFile (fileName, monitor);

			return await base.LoadWorkspaceItem (monitor, fileName);
		}

		Task<WorkspaceItem> ReadWorkspaceItemFile (FilePath fileName, ProgressMonitor monitor)
		{
			return Task<WorkspaceItem>.Factory.StartNew (delegate {
				XmlTextReader reader = new XmlTextReader (new StreamReader (fileName));
				try {
					monitor.BeginTask (string.Format (GettextCatalog.GetString ("Loading workspace item: {0}"), fileName), 1);
					reader.MoveToContent ();
					XmlDataSerializer ser = new XmlDataSerializer (MD1ProjectService.DataContext);
					ser.SerializationContext.BaseFile = fileName;
					ser.SerializationContext.ProgressMonitor = monitor;
					WorkspaceItem entry = (WorkspaceItem)ser.Deserialize (reader, typeof(WorkspaceItem));
					entry.FileName = fileName;
					return entry;
				} catch (Exception ex) {
					monitor.ReportError (string.Format (GettextCatalog.GetString ("Could not load solution item: {0}"), fileName), ex);
					throw;
				} finally {
					monitor.EndTask ();
					reader.Close ();
				}
			});
		}
	}
}

