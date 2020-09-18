using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace Fawdlstty.GitServerCore.internals {
	internal class GitRepoController {
		private static string _get_base_workdir (string _repo_path) => Path.Combine (AspNetExtensions.s_config.GitRepoBareDir, _repo_path).Replace ('\\', '/');

		[Display (Name = "拉取详情", GroupName = "GET", Description = "/info/refs")]
		public static IActionResult _info_refs (string _repo_path, string service) {
			Console.WriteLine ("call _info_refs");
			return new GitCommandResult ("git", new GitCommandOptions {
				RepoPath = _get_base_workdir (_repo_path),
				Service = service,
				AdvertiseRefs = true,
				EndStreamWithNull = true,
			});
		}

		[Display (Name = "提交内容", GroupName = "POST", Description = "/git-receive-pack")]
		public static IActionResult _git_receive_pack (string _repo_path) {
			Console.WriteLine ("call _git_receive_pack");
			return new GitCommandResult ("git", new GitCommandOptions {
				RepoPath = _get_base_workdir (_repo_path),
				Service = "git-receive-pack",
				AdvertiseRefs = false,
				EndStreamWithNull = true,
			});
		}

		[Display (Name = "克隆仓库", GroupName = "POST", Description = "/git-upload-pack")]
		public static IActionResult _git_upload_pack (string _repo_path) {
			Console.WriteLine ("call _git_upload_pack");
			return new GitCommandResult ("git", new GitCommandOptions {
				RepoPath = _get_base_workdir (_repo_path),
				Service = "git-upload-pack",
				AdvertiseRefs = false,
				EndStreamWithNull = false,
			});
		}
	}
}
