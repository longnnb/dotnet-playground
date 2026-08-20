# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A personal .NET learning/playground repository. Each top-level directory is an independent, self-contained project exploring one specific C# or .NET concept (threading primitives, HttpClient patterns, the Options pattern, DI, cryptography, design patterns, C# language features, etc.). There is no shared business domain tying these together — treat each project folder as its own isolated sandbox unless told otherwise.

## Commands

Build/run from the `src/` directory (where `DotNet.Playground.sln` lives).

```
dotnet build DotNet.Playground.sln          # build everything
dotnet build <Project>/<Project>.csproj      # build a single project
dotnet run --project <Project>               # run a single project (e.g. dotnet run --project ThreadSynchronization)
```

There are no test projects in the solution — do not assume a test runner exists unless one is added.

`global.json` pins the SDK with `rollForward: latestMajor` and allows prerelease SDKs, so target framework versions vary per project (see below) rather than being fixed solution-wide.

## Structure

Each folder is a separate `.csproj` referenced by `DotNet.Playground.sln`; there are no cross-project references. Target framework and `OutputType` are set per project, not centrally:

- Most projects are `net8.0` console apps (`OutputType: Exe`).
- `Cryptography` targets `net9.0`.
- `CancellationToken` is the one WPF app (`net8.0-windows`, `UseWPF`) — different execution model from the rest (App.xaml/MainWindow.xaml), despite the folder name suggesting a threading topic.

Current projects and their focus:
- `AsyncDisposable` — `IAsyncDisposable` patterns
- `CancellationToken` — WPF app (not a console demo like the others)
- `Challenges` — coding kata/exercises
- `CSharpFeatures` — language features (pattern matching, tuples, out variables)
- `Cryptography` — RSA encrypt/sign/verify, self-signed X509 certs
- `Decorator` — decorator pattern via Scrutor + `Microsoft.Extensions.Hosting`
- `DependencyInjection` — `Microsoft.Extensions.DependencyInjection` basics
- `DesignPatterns` — general GoF pattern demos
- `HttpClientTest` — `IHttpClientFactory` patterns: basic/named/typed clients, `DelegatingHandler` (`LoggingHandler`), Polly retry policies, a custom proxy `HttpClientHandler`, and a FHIR client (`Hl7.Fhir.R4`) hitting the public `hapi.fhir.org` test server
- `OptionsPattern` — `IOptions<T>` configuration binding: from JSON, via `IConfigureOptions<T>`, via provider/action, via a dependency-driven configurer
- `ThreadSynchronization` — `lock`, `ManualResetEvent`/`AutoResetEvent`, `Mutex`, `Semaphore` examples

## Conventions to follow when editing

- **`Program.cs` files are experimentation logs, not clean entry points.** They're full of commented-out alternative approaches (e.g. `OptionsPattern/Program.cs` has 5+ commented-out ways to register the same options class; `ThreadSynchronization/Program.cs` and `HttpClientTest/Program.cs` comment out whichever demo isn't currently active). When adding a new demo variant, follow this pattern — comment out the previous approach rather than deleting it, unless asked to clean up.
- Each project is meant to be run and observed via console output, not verified by an automated test suite. When changing a demo, "did it run and print the expected thing" is the bar, not unit tests.
- `HttpClientTest` and `OptionsPattern` use `Host.CreateApplicationBuilder` + `Microsoft.Extensions.DependencyInjection` as the composition root — service registration lives in `Program.ConfigureServices` (HttpClientTest) or inline in `Main` (OptionsPattern).
- The FHIR demo in `HttpClientTest` talks to a live public test server (`https://hapi.fhir.org/baseR4`) — no local/mock server. Be aware that running it makes real network calls and can create/mutate data on that shared instance (`CreatePatient`, `UpdatePatient`).
- `Cryptography/Program.cs` writes a `selfsigned.pfx` file to disk when run — a side effect to be aware of, not a bug.
