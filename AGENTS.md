# MTS Working Notes

`MTS` means `Modular Testing Suite`.

## Purpose

This repo is a reusable standalone test harness for multiple projects.

## Key rule

When a user says `use MTS`, prefer:

1. adding or updating a module manifest in `modules/`
2. reusing the existing harness UI and executor
3. keeping project-specific behavior in the manifest whenever possible
4. using the feature matrix when sprint features need to be tested one-by-one or in combinations

Avoid changing the MTS application code unless the capability will be broadly reusable across multiple projects.

## Important references

- Overview: `README.md`
- Project integration guide: `docs/01_project_integration.md`
- Starter template: `modules/project-http-template.json`
- Example live module: `modules/military-logistics-sim.json`

## User shorthand

- `DAYSF` means `do as you see fit`.
