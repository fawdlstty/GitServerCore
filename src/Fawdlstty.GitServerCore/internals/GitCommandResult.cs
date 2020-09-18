using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.GitServerCore.internals {
	internal class GitCommandResult: IActionResult {
		private string _gitPath;

		public GitCommandOptions Options { get; set; }

		public GitCommandResult (string gitPath, GitCommandOptions options) {
			_gitPath = gitPath;
			Options = options;
		}

		public async Task ExecuteResultAsync (ActionContext context) {
			var _feature = context.HttpContext.Features.Get<IHttpBodyControlFeature> ();
			if (_feature != null)
				_feature.AllowSynchronousIO = true;
			var _allow_gzip = context.HttpContext.Request.Headers ["Accept-Encoding"].First ().Contains ("gzip");
			Stream _res_stream = context.HttpContext.Response.Body;
			if (_allow_gzip) {
				context.HttpContext.Response.Headers.Add ("Content-Encoding", "gzip");
				_res_stream = new GZipStream (context.HttpContext.Response.Body, CompressionMode.Compress);
			}

			var _res = context.HttpContext.Response;
			_res.ContentType = $"application/x-{Options.Service}{(Options.AdvertiseRefs ? "-advertisement" : "")}";
			_res.Headers.Add ("Expires", "Fri, 01 Jan 1980 00:00:00 GMT");
			_res.Headers.Add ("Pragma", "no-cache");
			_res.Headers.Add ("Cache-Control", "no-cache, max-age=0, must-revalidate");

			////info.Environment.Add("AUTH_USER", userName);
			////info.Environment.Add("REMOTE_USER", userName);
			////info.Environment.Add("GIT_COMMITTER_EMAIL", email);

			using var _ps = Process.Start (new ProcessStartInfo (_gitPath, Options.ToString ()) {
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			});
			await (context.HttpContext.Request.Headers ["Content-Encoding"].Equals ("gzip") switch
			{
				true => new GZipStream (context.HttpContext.Request.Body, CompressionMode.Decompress),
				false => context.HttpContext.Request.Body,
			}).CopyToAsync (_ps.StandardInput.BaseStream);

			if (Options.EndStreamWithNull)
				await _ps.StandardInput.WriteAsync ('\0');
			_ps.StandardInput.Dispose ();

			if (Options.AdvertiseRefs) {
				string service = $"# service={Options.Service}\n";
				var _content = Encoding.UTF8.GetBytes ($"{service.Length + 4:x4}{service}0000");
				await _res_stream.WriteAsync (_content);
			}
			await _ps.StandardOutput.BaseStream.CopyToAsync (_res_stream);
			if (_res_stream is GZipStream)
				_res_stream.Dispose ();
			//_ps.WaitForExit ();
		}
	}
}
