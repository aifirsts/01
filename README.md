# aifirsts/01 — SOCpit workspace

This repository hosts the **SOCpit** Security Operations Center project together with the research
artifacts produced for NEO's infrastructure.

## Contents

| Path | Description |
|---|---|
| [`SOCpit2026/`](SOCpit2026/) | SOCpit v2026.10.15 — .NET 10 / WPF Security Operations Center (source, tests, docs, audit reports) |
| [`SOCpit2026/src/`](SOCpit2026/src) | 5-layer clean architecture: `SOCpit.Core`, `SOCpit.Application`, `SOCpit.Storage`, `SOCpit.Import`, `SOCpit.App` |
| [`SOCpit2026/tests/`](SOCpit2026/tests) | 4 xUnit test projects + `SOCpit.TestSupport` harness — 261 tests, 93.71% line coverage |
| [`SOCpit2026/audit/`](SOCpit2026/audit) | Quality-gate status, findings register and final validation reports |
| [`.github/workflows/`](.github/workflows) | CI/CD: build, test, coverage gate, single-file EXE packaging and GitHub Release publishing |
| [`SOCpit_2026_[FINAL.MD]`](SOCpit_2026_%5BFINAL.MD%5D) | Delivery report for the original 2026.10.11 request |
| [`fp-audit/`](fp-audit/) | Browser-fingerprint research harness and captures |
| [`SOCpit-2026.10.09-FINAL.zip`](SOCpit-2026.10.09-FINAL.zip) | Archived first delivery package (historical) |

## SOCpit at a glance

- **Latest release**: [`v2026.10.15`](https://github.com/aifirsts/01/releases/tag/v2026.10.15) — self-contained
  single-file `SOCpit.App.exe` for Windows 10/11 x64 (no .NET installation required).
- **Data location**: `C:\SOCpit2026\socpit.db` (SQLite) and `C:\SOCpit2026\logs\`.
- **Security policy**: conservative `ADR-V2-SEC-001` — unknown, stale, revoked or superseded evidence never
  produces `PASS`.
- **Verification**: `261 / 261` automated tests green in CI, `93.71%` line coverage, 12/12 quality gates
  `PASS-1`; the only deferred item is final on-PC validation by NEO.

## Build from source

```bash
dotnet restore SOCpit2026/SOCpit.sln
dotnet build   SOCpit2026/SOCpit.sln --configuration Release
dotnet test    SOCpit2026/SOCpit.sln --configuration Release --no-build --collect:"XPlat Code Coverage"
```

## Documentation map

- [`SOCpit2026/README.md`](SOCpit2026/README.md) — product overview, features, quick start
- [`SOCpit2026/INSTALL.md`](SOCpit2026/INSTALL.md) — installation guide (RU / EN)
- [`SOCpit2026/ARCHITECTURE.md`](SOCpit2026/ARCHITECTURE.md) — 5-layer architecture
- [`SOCpit2026/DATA-MODEL.md`](SOCpit2026/DATA-MODEL.md) — entities and schema
- [`SOCpit2026/SECURITY.md`](SOCpit2026/SECURITY.md) — security model and policy
- [`SOCpit2026/CHANGELOG.md`](SOCpit2026/CHANGELOG.md) — release history
- [`SOCpit2026/audit/FINAL-VALIDATION-2026-10-15.md`](SOCpit2026/audit/FINAL-VALIDATION-2026-10-15.md) — current validation report

## License

Proprietary — all rights reserved (see [`SOCpit2026/LICENSE`](SOCpit2026/LICENSE)).
