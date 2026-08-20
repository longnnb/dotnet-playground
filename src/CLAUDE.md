# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A personal .NET learning/reference repository targeting .NET 10 / C# 14. Each top-level directory is an independent, self-contained project exploring one specific C# or .NET concept (threading primitives, HttpClient patterns, the Options pattern, DI, cryptography, design patterns, C# language features, etc.). There is no shared business domain tying these together, and no `ProjectReference` between any of them — treat each project folder as its own isolated sandbox unless told otherwise.

## Commands

Build/run from the `src/` directory (where `DotNet.Playground.sln` lives).

```
dotnet build DotNet.Playground.sln           # build everything
dotnet build <Project>/<Project>.csproj      # build a single project
dotnet run --project <Project>               # run every demo in a project
dotnet run --project <Project> -- <name>     # run one demo, e.g. dotnet run --project ThreadSynchronization -- mutex
dotnet run --project <Project> -- --list     # print a project's available demo names
```

There are no test projects in the solution. The compiler is the enforcement instead: `TreatWarningsAsErrors` is on solution-wide (via `Directory.Build.props`), so an unused using, a missing XML doc comment on a public member, a non-`_camelCase` private field, or a namespace that doesn't match its folder all fail the build. When changing a demo, "does `dotnet build` succeed and does the output match the file's `/* Expected output */` block" is the bar, not unit tests.

`global.json` pins the SDK to `10.0.400` with `rollForward: latestFeature` (not `latestMajor`) — a future .NET 11 preview SDK on the machine won't silently get picked up and change `LangVersion`/analyzer behavior out from under the build.

## Structure

`Directory.Build.props` defines `$(PlaygroundTfm)` (currently `net10.0`) as the one shared target-framework knob; every csproj writes `<TargetFramework>$(PlaygroundTfm)</TargetFramework>` (or `$(PlaygroundTfm)-windows` for the WPF project) explicitly rather than inheriting it silently. `Directory.Packages.props` centrally manages NuGet package versions — csprojs use versionless `<PackageReference Include="..." />`. `src/.editorconfig` encodes the naming/style conventions below at `warning` severity, which `TreatWarningsAsErrors` escalates uniformly to build errors.

Current projects and their focus:
- `AsyncDisposable` — the full `IAsyncDisposable`/`IDisposable` pattern (`DisposeAsyncCore`, LIFO disposal order), `await foreach`/`IAsyncEnumerable<T>`, `CreateAsyncScope`
- `CancellationTokenWpf` — the one WPF app (`net10.0-windows`, `UseWPF`); `CancellationTokenSource` with a configurable timeout, manual cancellation via `CreateLinkedTokenSource`, and CPU-bound vs I/O-bound cancellable work. Named `...Wpf` (not just `CancellationToken`) because that name shadows `System.Threading.CancellationToken` as an identifier
- `Challenges` — Codewars-style katas, each with a primary implementation and one or more alternatives whose results are compared and printed with an `[agree]`/`[DIFFER]` marker
- `CSharpFeatures` — pattern matching (legacy `is`+cast through switch expressions with positional patterns), `out` variables, list patterns, tuples (structural equality, deconstruction, name erasure)
- `Cryptography` — RSA encrypt/sign/verify, AES-GCM, hashing (SHA-256/HMAC/PBKDF2), self-signed X509 certs (in-memory and file-based round trips), PEM export/import
- `Decorator` — the decorator pattern via Scrutor + `Microsoft.Extensions.Hosting`, plus Scrutor's assembly-scanning (`Scan()`) and open-generic decoration features
- `DependencyInjection` — service lifetimes (singleton/scoped/transient), the captive-dependency problem and its fix, keyed services, `ActivatorUtilities`
- `DesignPatterns` — nine GoF patterns (three creational, three structural, three behavioral); Decorator is deliberately not duplicated here since it has its own project
- `HttpClientTest` — `IHttpClientFactory` patterns (basic/named/typed clients), `DelegatingHandler` pipelines, Polly v7 retry policies vs `Microsoft.Extensions.Http.Resilience` (Polly v8), and a FHIR client (`Hl7.Fhir.R4`) hitting the public `hapi.fhir.org` test server
- `OptionsPattern` — every `IOptions<T>` registration strategy (JSON binding, `IConfigureOptions<T>`, provider lambda, dependency-driven configuration), plus validation (`ValidateDataAnnotations().ValidateOnStart()`), `IOptions` vs `IOptionsMonitor`, and async configuration sources
- `ThreadSynchronization` — `lock`/`Monitor`/`System.Threading.Lock`, `Mutex`, `ManualResetEvent`/`AutoResetEvent`, `SemaphoreSlim` (sync and async)

## Conventions to follow when editing

- **Every project is an args-driven dispatcher.** `Program.cs` holds a `Dictionary<string, Func<Task>>` (or `Func<IServiceProvider, Task>` in `HttpClientTest`, which shares one host across demos) mapping demo names to `RunAsync` methods. No arguments runs every demo in order; one or more names runs just those; `--list` enumerates them. **Never comment out a demo to "switch" which one runs** — that was the old convention in this repo and it's exactly what left most demos unreachable before the .NET 10 rewrite. A demo not wired into the dispatcher's dictionary is a demo nobody will ever see run.
- **One public type per file, file named after the type**, with one deliberate exception: a demo file may declare its supporting participants with the C# `file` access modifier in the same file (see `DesignPatterns/*/`) so a whole pattern's cast stays visible in one place. `file`-scoped types get a compiler-mangled name — never let one appear in output a demo prints (via `GetType().Name`, a DI validation exception message, etc.); if a type's name needs to show up in console output, it can't be `file`-scoped.
- **Every public demo type/method needs an XML `<summary>`** stating the concept, with a `<remarks>` for the non-obvious point — this is enforced by the build (CS1591), not just a style preference.
- **Every demo file ends with a `/* Expected output */` block comment**, copied from an actual `dotnet run` — not written from prediction. Mark anything nondeterministic in the header, e.g. `/* Expected output (thread ids and interleaving vary between runs) */`, and prefer asserting an invariant ("never more than 2 concurrent") over a literal transcript when output can't be pinned. Several bugs during the .NET 10 rewrite were only found by actually running the code and comparing against what the doc comment predicted — don't skip this step when adding a demo.
- **Compared alternatives must actually run and compare, not just exist.** Where an old version of a demo had a commented-out alternative implementation, it's now a second real method, invoked from the dispatcher, with its result compared against the primary implementation and printed with `[agree]`/`[DIFFER]` (see `Challenges/Kata.cs`). If a variant doesn't diverge from the primary for any real input, that's worth verifying empirically (as `Challenges/Jaden.cs`'s doc comments explain) rather than assumed.
- File-scoped namespaces matching the folder/assembly; `_camelCase` private fields — both enforced by `.editorconfig` + `TreatWarningsAsErrors`.
- `HttpClientTest` and `OptionsPattern` are the two projects with `Microsoft.Extensions.Hosting`-based composition roots. `HttpClientTest` builds **one shared host** across all its demos (matching how a real app registers its HttpClients once at startup); `OptionsPattern` builds **one host per demo**, since its whole point is comparing registration strategies that would otherwise clobber each other in a shared container. `OptionsPattern/DemoHost.cs` roots the builder at `AppContext.BaseDirectory` rather than the default working directory — `Host.CreateApplicationBuilder()`'s default `appsettings.json` loader is rooted at the process's current directory, which silently breaks when running via `dotnet run --project OptionsPattern` from `src/` (this repo's own documented usage) rather than from inside the project folder.
- The FHIR demos in `HttpClientTest` talk to a live public test server (`https://hapi.fhir.org/baseR4`) — no mock. Reads (`fhir-search`, `fhir-read`) run by default; writes (`fhir-create`, `fhir-update`) are opt-in only (must be named explicitly) since they add data other people on the shared server can see. All FHIR demos wrap their call in a 30s timeout and print a graceful skip message on failure rather than throwing, since the server is occasionally slow.
- `Cryptography`'s certificate demos do not leave files behind: `SelfSignedCertificateDemo` round-trips a PFX entirely in memory, and `CertificateFileRoundTripDemo` writes to `Path.GetTempPath()` and deletes it in a `finally` block.
