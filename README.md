# Json.Path

Json.Path is a lightweight library that provides straightforward helpers for resolving simple JSONPath expressions against `System.Text.Json` documents. The project follows the engineering conventions used in [Json.Masker](https://github.com/myarichuk/Json.Masker) so it is ready to build, test, and publish to NuGet using GitHub Actions.

## Features

- Minimal API for resolving object and array segments in JSON payloads.
- SourceLink, StyleCop, and nullable reference types enabled by default.
- Automated CI/CD with GitHub Actions for pull requests and releases.

## Getting Started

Install the package from NuGet:

```bash
dotnet add package Json.Path
```

Then call the `JsonPath.TryResolve` helper:

```csharp
using System.Text.Json;
using Json.Path;

var json = JsonDocument.Parse("""{""name"":""Ada"",""skills"":{""0"":""math""}}""");

if (JsonPath.TryResolve(json.RootElement, "$.name", out var result))
{
    Console.WriteLine(result.GetString());
}
```

## Development

```bash
dotnet restore
dotnet build
dotnet test
```

## Release Process

Merging a pull request into `main` runs the publish workflow, which

1. Determines the semantic version using GitVersion.
2. Builds and tests the library in Release configuration.
3. Packs and publishes the NuGet package when the `NUGET_TOKEN` secret is supplied.
4. Updates the changelog and creates a GitHub release using git-chglog.

## License

This project is licensed under the terms of the [MIT license](LICENSE).
