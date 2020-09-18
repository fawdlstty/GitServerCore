# GitServer

English | [简体中文](./README-zh.md)

Create a git server by ASP.Net Core

## Test Steps

1. install git <https://git-scm.com/>, make sure the 'git' command is available.
2. run ExampleWeb project from GitServer.sln
3. Access in a browser <http://127.0.0.1:5000/MyGitCommand/Create?path=aaa/bbb>, to ensure that the returned content is `success`
4. executive command `git clone http://127.0.0.1:5000/aaa/bbb.git`
5. Enter the account 'hello', password 'world'

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
