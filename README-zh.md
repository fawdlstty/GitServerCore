# GitServerCore

![Nuget](https://buildstats.info/nuget/Fawdlstty.GitServerCore)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/fawdlstty/GitServerCore/master/LICENSE)

[English](./README.md) | 简体中文

通过 ASP.Net Core 搭建一个 Git 服务器

## 操作指南

第一步：安装git <https://git-scm.com/>，确保`git`命令可用，包括项目部署环境与客户端测试环境  
第二步：新建WebAPI项目，并在`nuget`上面引用最新版本的`Fawdlstty.GitServerCore`  
第三步：`Configure`加入如下代码：

```csharp
public void ConfigureServices (IServiceCollection services) {
    // ...
    services.AddGitServerCore (_cfg => {
        // URL匹配正则（比如：/username/reponame.git）
        _cfg.GitUrlRegex = "^/(\\S*)\\.git$";

        // URL地址如何转为仓库相对路径
        //比如如上正则匹配的URL将会转为：username/reponame
        _cfg.GitUrlSimplize = _url => _url [1..^4];

        // git裸仓库路径
        _cfg.GitRepoBareDir = "/data/repo_bare";

        // git文件释放路径
        _cfg.GitRepoExtractDir = "/data/repo_extract";

        // 登录校验
        _cfg.CheckAllowAsync = async (_path, _oper, _username, _password) => {
            if (string.IsNullOrEmpty (_username)) {
                // 用户名为空那么强制要求输入用户名
                return GitOperReturnType.NeedAuth;
            } else if (_username == "hello" && _password == "world") {
                // 用户名和密码验证通过
                return GitOperReturnType.Allow;
            } else {
                // 拒绝此次访问
                return GitOperReturnType.Block;
            }
        };

        // 操作完成通知
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

第四步：新建控制器，示例代码：

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

第五步：浏览器访问 <http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb>，确保返回内容为 `success`  
第六步：执行命令 `git clone http://127.0.0.1:5000/aaa/bbb.git`，提示输入用户名密码，输入账号 `hello`，密码 `world`，即成功克隆

## 功能

- 创建仓库（API）
- 提取仓库文件（API）
- 克隆仓库（HTTP管道）
	+ 正则指定路径
	+ 用户权限控制
- 提交更改（HTTP管道）
	+ 正则指定路径
	+ 用户权限控制

## 备注

灵感及部分代码源于：<https://github.com/linezero/GitServer> (MIT License)
