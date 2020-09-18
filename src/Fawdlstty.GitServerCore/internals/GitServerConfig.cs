using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.GitServerCore.internals {
	public enum GitOperType {
		Unknown,
		Clone,
		Commit,
	}

	public enum GitOperReturnType {
		NeedAuth,
		Allow,
		Block,
	}

	public class GitServerConfig {
		// URL路径正则
		public string GitUrlRegex { get; set; } = "^/(\\S*)\\.git$";

		// URL路径简化
		public Func<string, string> GitUrlSimplize { get; set; } = _url => _url [1..^4];

		// 裸仓库路径
		public string GitRepoBareDir { get; set; } = "/repo_bare";

		// 仓库内容解压路径
		public string GitRepoExtractDir { get; set; } = "/repo_extract";

		/// <summary>
		/// 访问仓库校验身份
		/// </summary>
		/// <remarks>当返回allow时代表可以使用仓库路径</remarks>
		public Func<string, GitOperType, string, string, Task<GitOperReturnType>> CheckAllowAsync { get; set; } = (_url_path, _oper, _username, _password) => Task.FromResult (GitOperReturnType.Block);

		/// <summary>
		/// 已操作完成事件
		/// </summary>
		public Func<string, GitOperType, string, Task> HasBeenOperationAsync { get; set; } = async (_path, _oper, _username) => await Task.Yield ();
	}
}
