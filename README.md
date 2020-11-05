# GitServerCore

![Nuget](https://buildstats.info/nuget/Fawdlstty.GitServerCore)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/fawdlstty/GitServerCore/master/LICENSE)

English | [简体中文](./README-zh.md)

Create a git server by ASP.Net Core

## Manuals

Step 1: install git <https://git-scm.com/>, ensure that the `git` command is available, including both the project deployment environment and the client test environment  
Step 2: Create a new WebAPI project and reference the latest version of `Fawdlstty.GitServerCore` above `nuget`  
Step 3: `Configure` add the following code:

```csharp
public void ConfigureServices (IServiceCollection services) {
    // ...
    services.AddGitServerCore (_cfg => {
        // URL regex（eg: /username/reponame.git）
        _cfg.GitUrlRegex = "^/(\\S*)\\.git$";

        // How is the URL address converted to a warehouse relative path
        // For example, the URL of the regular match above will be converted to username/reponame
        _cfg.GitUrlSimplize = _url => _url [1..^4];

        // The Git bare repository path
        _cfg.GitRepoBareDir = "/data/repo_bare";

        // The Git repo extract path
        _cfg.GitRepoExtractDir = "/data/repo_extract";

        // Log on to check
        _cfg.CheckAllowAsync = async (_path, _oper, _username, _password) => {
            if (string.IsNullOrEmpty (_username)) {
                // If the user name is empty, it is mandatory to enter the user name
                return GitOperReturnType.NeedAuth;
            } else if (_username == "hello" && _password == "world") {
                // The username and password are verified
                return GitOperReturnType.Allow;
            } else {
                // Denied this visit
                return GitOperReturnType.Block;
            }
        };

        // Operation completion notice
        _cfg.HasBeenOperationAsync = async (_path, _oper, _username) => await Task.Yield ();
    });
    // ...
}

public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
    // ...
    app.UseGitServerCore ();
    // ...
}
```

Step 4: Create new controller, sample code:

```csharp
[ApiController]
[Route ("[controller]/[action]")]
public class MyGitCommandController: ControllerBase {
    // http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb
    [HttpGet]
    public string Create (string path) {
        if (GitServerAPI.CreateRepo (path)) {
            return "success";
        } else {
            return "failure";
        }
    }

    [HttpGet]
    public async Task<string> Extract (string path) {
        if (await GitServerAPI.ExtractFilesAsync (path)) {
            return "success";
        } else {
            return "failure";
        }
    }
}
```

Step 5: browser to access <http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb>, make sure the return is `success`  
Step 6: execute the command `git clone http://127.0.0.1:5000/aaa/bbb.git`, prompt for user name password, enter account `hello`, password `world`, and then it was cloned

## Functions

- Creating a repository (API)
- Extract repository files (API)
- Clone repository (HTTP pipeline)
	+ The path is specified via a regular expression
	+ User permission control
- Commit changes (HTTP pipeline)
	+ The path is specified via a regular expression
	+ User permission control

## Remarks

Inspiration and part of the code comes from: <https://github.com/linezero/GitServer> (MIT License)
