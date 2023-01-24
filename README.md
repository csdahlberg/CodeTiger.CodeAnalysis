# CodeTiger.CodeAnalysis
[![Gitter](https://badges.gitter.im/csdahlberg/CodeTiger.CodeAnalysis.svg)](https://gitter.im/csdahlberg/CodeTiger.CodeAnalysis?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

CodeTiger.CodeAnalysis is a set of Roslyn-based code analyzers that I use in my personal projects. It analyzes coding style (naming, layout, etc.) to ensure it matches my own preferred coding style, as well as various maintainability and functional issues.

It currently supports Roslyn 2.6.1 (Visual Studio 2017 / 15.5) and newer.

## Source code

Clone the sources: `git clone https://github.com/csdahlberg/CodeTiger.CodeAnalysis.git`

## Building

CodeTiger.CodeAnalysis is primarily developed using [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/). It can also be built using `msbuild`, `dotnet build`, or the `Build.ps1` script (with the `-SkipVsix` switch under linux).

## Contributing
Before you contribute, please read through the [Contributing Guide](CONTRIBUTING.md).

You are also encouraged to join the chat on [Gitter](https://gitter.im/csdahlberg/CodeTiger.CodeAnalysis) or start a discussion by filing an issue.

## License

CodeTiger.CodeAnalysis is licensed under the [MIT license](LICENSE).
