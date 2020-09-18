using Fawdlstty.GitServerCore.internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fawdlstty.GitServerCore {
	public static class AspNetExtensions {
		private static Dictionary<(string, string), string> s_ctrl_funcs = new Dictionary<(string, string), string> ();
		private static List<string> s_end_withs = new List<string> ();
		internal static GitServerConfig s_config = new GitServerConfig ();

		public static IServiceCollection AddGitServerCore (this IServiceCollection services, Action<GitServerConfig> _func) {
			foreach (var _method_info in typeof (GitRepoController).GetMethods ()) {
				var _display = _method_info.GetCustomAttribute<DisplayAttribute> ();
				if (string.IsNullOrEmpty (_display?.GroupName) || string.IsNullOrEmpty (_display?.Description))
					continue;
				s_ctrl_funcs.Add ((_display.GroupName, _display.Description), _method_info.Name);
				s_end_withs.Add (_display.Description);
			}
			_func (s_config);
			return services;
		}

		// 实际处理
		private static async Task _process_git_command (HttpContext _ctx, string _repo_path, string _path_cmd) {
			do {
				// 获取对应方法
				if (!s_ctrl_funcs.ContainsKey ((_ctx.Request.Method, _path_cmd)))
					break;
				var _func_name = s_ctrl_funcs [(_ctx.Request.Method, _path_cmd)];
				var _method_info = (from p in typeof (GitRepoController).GetMethods () where p.Name == _func_name && p.IsStatic select p).FirstOrDefault ();
				if (_method_info == null)
					break;

				// 生成参数
				var _param_infos = _method_info.GetParameters ();
				object[] _call_params = null;
				if ((_param_infos?.Length ?? 0) >= 1) {
					_call_params = new object [_param_infos.Length];
					for (int i = 0; i < _param_infos.Length; ++i) {
						if (_param_infos [i].Name == "_repo_path") {
							_call_params [i] = Path.Combine (s_config.GitRepoBareDir, _repo_path).Replace ('\\', '/');
						} else {
							// 读取参数并写入
							_call_params [i] = _ctx.Request.Query [_param_infos [i].Name].ToString ();
						}
						//} else if (_param_infos [i].Name == "_path_cmd") {
						//	_call_params [i] = _path_cmd;
					}
				}

				// 调用并计算返回值
				var _ret = _method_info.Invoke (null, _call_params);
				if (_method_info.ReturnType == null) {
					// do nothing
				} else if (_method_info.ReturnType == typeof (Task)) {
					await (Task) _ret;
				} else {
					if (_method_info.ReturnType.IsSubclassOf (typeof (Task))) {
						await (Task) _ret;
						_ret = _ret.GetType ().InvokeMember ("Result", BindingFlags.GetProperty, null, _ret, Array.Empty<object> ());
					}
					if (_ret is IActionResult _iar) {
						var _ac = new ActionContext { HttpContext = _ctx };
						await _iar.ExecuteResultAsync (_ac);
					} else if (_ret is string _s) {
						await _ctx.Response.WriteAsync (_s);
					} else {
						throw new NotSupportedException ();
					}
					//_ret.GetType () switch {
					//	var _t when _t.IsSubclassOf (typeof (IActionResult)) => 
					//};
				}
				return;
			} while (false);
			_ctx.Response.StatusCode = 404;
			//_ctx.Response.Body.Write (Encoding.UTF8.GetBytes ("404 Not Found"));
		}

		public static IApplicationBuilder UseGitServerCore (this IApplicationBuilder _app) {
			_app.Use (async (_ctx, _next) => {
				string _path = _ctx.Request.Path;
				// git command
				foreach (var _path_cmd in s_end_withs) {
					if (_path.EndsWith (_path_cmd)) {
						string _tmp_path = _path[..^_path_cmd.Length];
						var _match = new Regex (s_config.GitUrlRegex).Match (_tmp_path);
						if (_match.Success) {
							// 识别操作方式
							_tmp_path = s_config.GitUrlSimplize (_match.Value);
							var _oper_type = _path_cmd switch {
								"/info/refs" => $"{_ctx.Request.Query["service"]}" switch {
									"git-upload-pack" => GitOperType.Clone,
									"git-receive-pack" => GitOperType.Commit,
									_ => GitOperType.Unknown,
								},
								"/git-upload-pack" => GitOperType.Clone,
								"/git-receive-pack" => GitOperType.Commit,
								_ => GitOperType.Unknown,
							};

							// 获取用户名和密码
							string _username = "", _password = "";
							if (_ctx.Request.Headers.ContainsKey ("Authorization")) {
								string _s = _ctx.Request.Headers ["Authorization"];
								if (_s.StartsWith ("Basic ")) {
									_s = Encoding.UTF8.GetString (Convert.FromBase64String (_s [6..]));
									int _p = _s.IndexOf (':');
									_username = _s [.._p];
									_password = _s [(_p + 1)..];
								}
							}

							// 操作仓库
							var _ret_oper = await s_config.CheckAllowAsync (_tmp_path, _oper_type, _username, _password);
							if (_ret_oper == GitOperReturnType.NeedAuth) {
								_ctx.Response.StatusCode = 401;
								_ctx.Response.Headers.Add ("WWW-Authenticate", @"Basic realm=""FaGitServer""");
							} else if (_ret_oper == GitOperReturnType.Allow) {
								await _process_git_command (_ctx, _tmp_path, _path_cmd);
								await s_config.HasBeenOperationAsync (_tmp_path, _oper_type, _username);
							} else {
								_ctx.Response.StatusCode = 403;
							}
						} else {
							break;
						}
						return;
					}
				}
				await _next ();
			});
			return _app;
		}
	}
}
