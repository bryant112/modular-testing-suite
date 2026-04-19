# Modular Testing Suite

`MTS` is a standalone, reusable desktop harness for driving HTTP-based test modules from local JSON manifests.

## Why it exists

The goal is to keep useful testing workflows portable across projects instead of burying them inside one application UI.

## How it works

- Drop module manifests into `modules\*.json`
- Launch the desktop app
- Pick a module
- Fill in variables
- select any combination of feature toggles in the feature matrix
- run actions against a target base URL
- capture values such as `sessionId` back into the suite automatically

## Included first module

- `military-logistics-sim.json`
  - create session
  - inspect world-data status
  - inspect world state
  - refresh RWD
  - refresh weather
  - apply dev feature toggles

## Project integration

Start here when wiring a new project into MTS:

- [docs/01_project_integration.md](docs/01_project_integration.md)
- [modules/project-http-template.json](modules/project-http-template.json)
- [AGENTS.md](AGENTS.md)

## Structure

- `src\Mts.Core`
  - module definitions
  - module loader
  - template resolver
  - HTTP action executor
- `src\Mts.App`
  - Avalonia desktop shell
- `modules`
  - reusable test manifests
- `docs`
  - project integration and usage notes

## Reuse pattern

To use `MTS` with another project:

1. Add a new JSON file to `modules`
2. Define variables, features, and actions
3. Point the app at the target service base URL
4. Use the feature matrix to test single features or any subset combination during a dev cycle

## Build

```bash
cd ~/dev/work/modular-testing-suite
dotnet build ./MTS.sln
```

## Run

```bash
cd ~/dev/work/modular-testing-suite
dotnet run --project ./src/Mts.App/Mts.App.csproj
```

Windows fallback uses the same commands from your Windows repo path.

## Dev Toolbar

- Run common workflows from terminal:
  - `./scripts/dev-toolbar.sh start`
  - `./scripts/dev-toolbar.sh build`
  - `./scripts/dev-toolbar.sh test`
  - `./scripts/dev-toolbar.sh lint`
  - `./scripts/dev-toolbar.sh deploy` (safe stub)
  - `./scripts/dev-toolbar.sh git-pull`
  - `./scripts/dev-toolbar.sh git-push`
- VS Code task labels use the same command set: `dev:*` and `git:*`.
