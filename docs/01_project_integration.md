# MTS Integration Guide

`MTS` = `Modular Testing Suite`

Use `MTS` when a project has an HTTP-accessible service or test surface and you want a reusable operator-driven harness instead of project-specific throwaway test UI.

## Canonical Location

- App root: `C:\dev\MTS`
- Solution: `C:\dev\MTS\MTS.sln`
- Desktop app project: `C:\dev\MTS\src\Mts.App\Mts.App.csproj`
- Module folder: `C:\dev\MTS\modules`

## What "use MTS" should mean

When working on a project, if the user says `use MTS` or `use the Modular Testing Suite`, the expected workflow is:

1. Identify the project's testable endpoints or commands.
2. Add or update a module manifest in `C:\dev\MTS\modules`.
3. Keep the module project-specific, but keep the harness generic.
4. Use the MTS app to exercise the project instead of building one-off test controls into the product UI unless there is a product reason to do so.

## Feature Combination Testing

MTS now supports a reusable `feature matrix` model.

That means a module can define named feature toggles such as:

- `feature_alpha`
- `feature_bravo`
- `feature_charlie`

In the dashboard, those show up as selectable checkboxes.

This supports the dev-cycle workflow where you may want to test:

- feature 1 only
- feature 2 only
- feature 3 only
- features 1 and 2
- features 1 and 3
- features 2 and 3
- all 3 together
- baseline with none enabled

In practice, that means each sprint feature can be run on a feature-by-feature basis or in any combination you choose.

## When to integrate a project with MTS

MTS is a good fit when the project has:

- REST or HTTP endpoints
- repeatable setup flows
- request/response testing needs
- environment variables like `sessionId`, `jobId`, `runId`, `token`, or `scenarioId`
- manual QA workflows that need to be reused later
- feature-flag or sprint-feature combination testing

MTS is less useful when the project only exposes:

- local-only CLI behavior with no stable machine interface
- highly interactive graphics flows that need a dedicated in-app debug overlay
- authentication or transport requirements that the current MTS module format does not support yet

## Integration Pattern

### 1. Create a module manifest

Add a JSON file under:

- `C:\dev\MTS\modules\<project-name>.json`

Start from:

- `C:\dev\MTS\modules\project-http-template.json`

### 2. Define variables

Variables should represent values that need to be reused across actions.

Common examples:

- `baseUrl`
- `sessionId`
- `seed`
- `scenarioId`
- `userId`
- `token`
- `runMode`

### 3. Define features

Use the `features` block for sprint or dev-cycle features that should be tested in combinations.

Common examples:

- `feature_newRoutePlanner`
- `feature_realWeather`
- `feature_dashboardV2`
- `feature_incidentPackB`

Those are exposed as checkboxes in the MTS dashboard and are automatically available to request templates as boolean tokens.

### 4. Define actions

Each action should represent one meaningful test step.

Examples:

- create session
- fetch current state
- trigger refresh
- run health check
- reset simulation
- apply feature toggles

### 5. Capture values back into the suite

Use `captureJsonFields` to pull values from responses and save them back into variables.

Examples:

- `sessionId`
- `jobId`
- `uploadId`
- `conversationId`

## Design Rule

Keep the harness generic.

That means:

- new project behavior should usually be added as a `module manifest`
- only change `MTS core/app code` when many projects would benefit from the new capability

## Current Module Capabilities

MTS currently supports:

- module manifests loaded from JSON
- variable substitution using `{token}` placeholders
- feature-toggle substitution using checkbox-backed boolean tokens
- HTTP methods against a base URL
- JSON body templates
- request preview
- response preview
- capture of JSON values back into variables

## Example Cases

### Example: simulation project

Use MTS for:

- create session
- set feature flags
- refresh world data
- refresh weather
- poll state
- compare behavior with one sprint feature on versus multiple sprint features on together

### Example: backend service

Use MTS for:

- create test user
- run job
- inspect job result
- toggle debug mode
- call admin endpoints
- compare combinations of feature flags during a sprint

## File Map

- `C:\dev\MTS\README.md`
  - high-level overview
- `C:\dev\MTS\docs\01_project_integration.md`
  - this guide
- `C:\dev\MTS\modules\project-http-template.json`
  - starter module template
- `C:\dev\MTS\modules\military-logistics-sim.json`
  - example real module

## Suggested Phrase To Use Later

If you want Codex to reach for this system while working in another project, say:

- `use MTS for testing`
- `add an MTS module for this project`
- `wire this project into the Modular Testing Suite`
- `add sprint feature toggles to the MTS module`

That should imply:

- update or add a module in `C:\dev\MTS\modules`
- keep the reusable harness in `C:\dev\MTS`
- expose feature combinations in the dashboard when the project benefits from feature-by-feature testing
- avoid building throwaway duplicate testing UI when MTS can handle it
