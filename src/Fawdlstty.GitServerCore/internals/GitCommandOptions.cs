using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fawdlstty.GitServerCore.internals {
	public class GitCommandOptions {
		public string RepoPath { get; set; }
		public string Service { get; set; }
		public bool AdvertiseRefs { get; set; } = true;
		public bool EndStreamWithNull { get; set; } = true;

		public override string ToString () {
			if (!Service.StartsWith ("git-"))
				throw new InvalidOperationException ();

			StringBuilder _sb = new StringBuilder ();
			_sb.Append (Service [4..]);
			_sb.Append (" --stateless-rpc");
			if (AdvertiseRefs)
				_sb.Append (" --advertise-refs");
			_sb.Append ($@" ""{RepoPath.TrimEnd (Path.DirectorySeparatorChar)}""".Replace ('/', Path.DirectorySeparatorChar));
			return _sb.ToString ();
		}
	}
}
