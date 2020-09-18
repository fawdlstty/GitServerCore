# GitServer

[English](./README.md) | 简体中文

通过 ASP.Net Core 搭建一个 Git 服务器

## 实验步骤

1. 安装git <https://git-scm.com/>，确保`git`命令可用
2. 打开GitServer.sln，运行ExampleWeb
3. 浏览器访问 <http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb>，确保返回内容为 `success`
4. 执行命令 `git clone http://127.0.0.1:5000/aaa/bbb.git`
5. 输入账号 `hello`，密码 `world`

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
