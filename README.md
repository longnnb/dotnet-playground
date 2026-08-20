# dotnet-playground

A personal .NET learning and reference repository targeting **.NET 10 / C# 14**. Each project under [`src/`](src/) is an independent, self-contained demo of one specific C# or .NET concept — there's no shared business domain and no project references between them.

Every project is runnable on its own and prints a documented, verifiable output.

## Getting started

```
cd src
dotnet build DotNet.Playground.sln           # build everything
dotnet run --project <Project>               # run every demo in a project
dotnet run --project <Project> -- <name>     # run one demo by name
dotnet run --project <Project> -- --list     # list a project's demo names
```

There are no test projects — the compiler is the safety net instead (`TreatWarningsAsErrors` is on solution-wide), and each demo file ends with a `/* Expected output */` comment you can diff your own run against.

## Projects

| Project | Topic |
|---|---|
| [`AsyncDisposable`](src/AsyncDisposable) | `IAsyncDisposable`/`IDisposable`, `DisposeAsyncCore`, `await foreach`, `CreateAsyncScope` |
| [`CancellationTokenWpf`](src/CancellationTokenWpf) | `CancellationTokenSource` with timeout + manual cancel (WPF app) |
| [`Challenges`](src/Challenges) | Codewars-style katas, with alternative implementations compared side by side |
| [`CSharpFeatures`](src/CSharpFeatures) | Pattern matching, `out` variables, list patterns, tuples |
| [`Cryptography`](src/Cryptography) | RSA, AES-GCM, hashing, X509 certificates, PEM |
| [`Decorator`](src/Decorator) | The decorator pattern via Scrutor, plus assembly scanning |
| [`DependencyInjection`](src/DependencyInjection) | Service lifetimes, the captive-dependency problem, keyed services |
| [`DesignPatterns`](src/DesignPatterns) | Nine GoF patterns (creational, structural, behavioral) |
| [`HttpClientTest`](src/HttpClientTest) | `IHttpClientFactory`, `DelegatingHandler` pipelines, Polly retry/resilience, a live FHIR client |
| [`OptionsPattern`](src/OptionsPattern) | Every `IOptions<T>` registration strategy, validation, `IOptions` vs `IOptionsMonitor` |
| [`ThreadSynchronization`](src/ThreadSynchronization) | `lock`/`Monitor`/`Lock`, `Mutex`, reset events, `SemaphoreSlim` |

See [`src/CLAUDE.md`](src/CLAUDE.md) for the conventions every project follows and more detail on each one.
