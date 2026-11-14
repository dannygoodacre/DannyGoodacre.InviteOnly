# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository

A .NET 10 library, `DannyGoodacre.InviteOnly` (accounts, invite code, and admins on ASP.NET Core Identity), shipped as one NuGet package, plus the `DannyGoodacre.InviteOnly.Example` web API (SQLite, EF migrations, Scalar at `/scalar` in Development). Only projects in `DannyGoodacre.InviteOnly.slnx` are part of the codebase; other folders under `src/` and `tests/` are untracked leftovers.

## Commands

```shell
dotnet build
dotnet test
dotnet test --project tests/DannyGoodacre.InviteOnly.Tests --filter-method "*WhenUserIsDisabled*"   # or --filter-class "<FullName>"
dotnet test --coverage --coverage-output-format cobertura --results-directory ./coverage_data
dotnet run --project src/DannyGoodacre.InviteOnly.Example
```

Tests use xUnit v3 with `Microsoft.Testing.Extensions.CodeCoverage` for coverage; VSTest collectors such as `coverlet.collector` do not work. There is no `global.json` yet, so if the .NET 10 SDK rejects `dotnet test` for these projects, one must opt in to Microsoft.Testing.Platform.

## Architecture

- **Identity:** a thin layer over stock ASP.NET Core Identity; configuration stays in Identity's own options, and the package changes only the security stamp interval (from its `SecurityStampValidationIntervalMinutes` option) and the cookie's redirect events. Registration requires the invite code (one row in the internal `InviteCodes` table). Disabling an account uses Identity lockout until a far-future date, kept private to `UserExtensions` behind `Disable`/`Enable`/`IsDisabled`; disabling must be saved with a security stamp change. Admins are the `Admin` role, exposed as the `Admin` policy (`IdentityDefaults`); at least one enabled admin must remain. Folders match namespaces: the public API lives in the root `DannyGoodacre.InviteOnly` namespace so consumers need one `using`; internals go in `Accounts/`, `Data/`, `Endpoints/`, and `Models/`. Tests run each case on its own `TestApp` (test server, SQLite in-memory) rather than `WebApplicationFactory`, so options can differ per test.
- **Internals:** `DannyGoodacre.InviteOnly.Tests` sees internals via `InternalsVisibleTo`.

## Documentation

- The root `README.md` is the single README and the reference for the package's API. The package packs it from there, so it shows on nuget.org: avoid relative links in it.
- Docs quote the example project by file. Keep quotes in sync with the code.
- Docs cover using the package, not general EF Core, testing, or coding practice.

# .NET & C# Conventions

## Language & C# Features
- **Modern C# Standards:** Target C# 14. Use primary constructors, target-typed `new()`, pattern matching, `field` property accessors, and file-scoped namespaces (`namespace MyNamespace;`).
- **Types & Immutability:** Prefer `record` or `readonly struct` for DTOs and value objects. Use `init`-only properties over standard setters.
- **Nullability:** Treat nullable reference types strictly (`<Nullable>enable</Nullable>`). Avoid suppressing warnings with `!` unless proven safe.

## Async & Resource Management
- **Async Signature:** Always append `Async` to asynchronous method names (`GetUserAsync`).
- **Cancellation:** Pass `CancellationToken cancellationToken = default` as the last parameter in all async/I-O methods.
- **ValueTask:** Use `ValueTask` for async methods that frequently complete synchronously (e.g., hot path cache lookups).
- **Resource Disposal:** Use `await using` or `using` statement scope declarations rather than nested `using (...) { }` blocks.

## Architecture & EF Core
- **No Abstraction Over EF Core:** Do not create generic repository (`IRepository<T>`) or Unit of Work wrappers over `DbContext`. Use EF Core directly or via MediatR / Vertical Slices.
- **EF Core Performance:** Always use `.AsNoTracking()` for read-only queries. Avoid `Include()` over-fetching; use `.Select()` projections to map directly to DTOs.
- **Result Pattern:** Prefer returning a strongly-typed `Result<T>` model for validation/domain failures rather than throwing control-flow exceptions.

## Dependency Injection & API Design
- **Registration:** Use primary constructors for dependency injection. Do not write explicit private `readonly` fields and constructor assignments.
- **API Endpoints:** Prefer Minimal APIs / FastEndpoints / Carter over fat MVC controllers unless working in a legacy project.
- **Logging:** Use `ILogger<T>` with structured logging message templates (e.g., `_logger.LogInformation("Processing order {OrderId}", order.Id)`—do not use string interpolation `$""`).

## Comments
- **Only when necessary:** Write a comment only when the code cannot show *why* something was done, for example a non-obvious constraint, a workaround, or a decision that a future change could easily break. Most code needs no comment.
- **Never how:** Do not describe what the code does or how it works; the code shows that.
- **Never about changes:** Do not write comments that narrate an edit, its history, or how the code used to be (for example "now uses keyed services", "moved from X", "fixed: …"). That belongs in the commit message or PR description.
- **File overviews:** A short overview at the top of a file whose logic only makes sense across several methods is acceptable. Keep it to the design decisions a change could break.
