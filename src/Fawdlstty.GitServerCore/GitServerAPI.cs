using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fawdlstty.GitServerCore {
	public class GitServerAPI {
		/// <summary>
		/// Create a bare repository
		/// </summary>
		/// <remarks>创建一个裸仓库</remarks>
		/// <param name="_path">example: user_name/repo_name</param>
		/// <returns></returns>
		public static bool CreateRepo (string _path) {
			string _path_src = Path.Combine (AspNetExtensions.s_config.GitRepoBareDir, _path).Replace ('\\', '/');
			if (Directory.Exists (_path_src))
				return false;
			Directory.CreateDirectory (_path_src);
			Repository.Init (_path_src, true);
			return true;
		}

		/// <summary>
		/// Copies files from the bare repository to the extract path
		/// </summary>
		/// <remarks>将裸仓库中的文件复制到提取路径</remarks>
		/// <param name="_path">example: user_name/repo_name</param>
		/// <returns></returns>
		public static async Task ExtractFilesAsync (string _path) {
			string _path_src = Path.Combine (AspNetExtensions.s_config.GitRepoBareDir, _path).Replace ('\\', '/');
			string _path_dest = Path.Combine (AspNetExtensions.s_config.GitRepoExtractDir, _path).Replace ('\\', '/');

			Func<Tree, Task> _extract_dir = null;
			_extract_dir = async (_tree) => {
				foreach (var _item in _tree) {
					if (_item.TargetType == TreeEntryTargetType.Tree) {
						string _tmp_path = Path.Combine (_path_dest, _item.Path).Replace ('\\', '/');
						if (!Directory.Exists (_tmp_path))
							Directory.CreateDirectory (_tmp_path);
						await _extract_dir ((Tree) _item.Target);
					} else if (_item.TargetType == TreeEntryTargetType.Blob) {
						using var _stream_src = ((Blob) _item.Target).GetContentStream ();
						using var _stream_dest = File.OpenWrite (Path.Combine (_path_dest, _item.Path).Replace ('\\', '/'));
						await _stream_src.CopyToAsync (_stream_dest);
					}
				}
			};

			using var _repo = new Repository (_path_src);
			if (!Directory.Exists (_path_dest))
				Directory.CreateDirectory (_path_dest);
			var _tree = _repo.Head.Tip.Tree;
			await _extract_dir (_tree);
		}
	}
}
