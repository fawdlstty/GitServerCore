using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fawdlstty.GitServerCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ExampleWeb.Controllers {
	[ApiController]
	[Route ("[controller]/[action]")]
	public class MyGitCommandController: ControllerBase {
		private readonly ILogger<MyGitCommandController> _logger;

		public MyGitCommandController (ILogger<MyGitCommandController> logger) {
			_logger = logger;
		}

		// http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb
		[HttpGet]
		public string Create (string path) {
			_logger.LogInformation ($"Create git bare repo by path[{path}]");
			if (GitServerAPI.CreateRepo (path)) {
				return "success";
			} else {
				return "failure";
			}
		}
	}
}
