using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fawdlstty.GitServerCore;
using Fawdlstty.GitServerCore.internals;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExampleWeb {
	public class Startup {
		public Startup (IConfiguration configuration) {
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices (IServiceCollection services) {
			services.AddControllers ();
			services.AddGitServerCore (_cfg => {
				// url regex
				_cfg.GitUrlRegex = "^/(\\S*)\\.git$";

				// source url to git path
				_cfg.GitUrlSimplize = _url => _url [1..^4];

				// git repository in where
				_cfg.GitRepoBareDir = "/repo_bare";
				_cfg.GitRepoExtractDir = "/repo_extract";

				// check auth
				_cfg.CheckAllowAsync = async (_path, _oper, _username, _password) => {
					if (string.IsNullOrEmpty (_username)) {
						return GitOperReturnType.NeedAuth;
					} else if (_username == "hello" && _password == "world") {
						return GitOperReturnType.Allow;
					} else {
						return GitOperReturnType.Block;
					}
				};

				// operation notify
				_cfg.HasBeenOperationAsync = async (_path, _oper, _username) => {};
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
			if (env.IsDevelopment ()) {
				app.UseDeveloperExceptionPage ();
			}

			app.UseGitServerCore ();

			app.UseRouting ();
			app.UseAuthorization ();
			app.UseEndpoints (endpoints => {
				endpoints.MapControllers ();
			});
		}
	}
}
