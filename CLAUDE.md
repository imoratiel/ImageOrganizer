# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

ImageOrganizer is a Windows-only WPF desktop app (.NET Core 3.1, `Microsoft.NET.Sdk.WindowsDesktop`, WPF + WinForms) for triaging photos: it shows images from a folder one at a time and lets the user move each into a subfolder, rotate, delete, skip, or undo.

## Commands

Run from the repo root (`ImageOrganizer.sln`):

```bash
dotnet build                                   # build the solution (this is also what CI runs)
dotnet run --project ImageOrganizer            # launch the app
```

There are no tests and no lint configuration. CI (`.github/workflows/build.yml`) only runs `dotnet build` wrapped in a SonarQube Cloud analysis (project `MrNachoM_ImageOrganizer`), on pushes to `main` and on PRs.

## Architecture

- **Startup / DI** — `App.xaml.cs` builds a `ServiceCollection`: loads `appsettings.json` (copied to output), registers `FileActionDbContext` (SQLite), NLog logging (`Nlog.config`), `IConfiguration`, and `MainWindow` as transient. `MainWindow` is resolved from DI in `OnStartup` (there is no `StartupUri`).
- **UI logic lives in `MainWindow.xaml.cs` code-behind** — no MVVM. Subfolder "move to" buttons are generated dynamically in `ScanFolder` into `SubFoldersButtonPanel`; the button's `Content` is the subfolder name used as the move destination. User feedback goes through the `ShowErrorMessage` / `ShowWarningMessage` / `ShowInfoMessage` toasts (Notification.Wpf).
- **`FileAction` table is the app's entire persisted state**, not just a history log. Each row has an `ActionType`:
  - `ScanFolder` rows = the list of known folders (repopulates the combo box on start; the most recent is auto-scanned; `timestamp` is bumped on rescan).
  - `Skip` rows = files to exclude when picking the next image (`GetSkippedItems` by folder prefix; "Reset Ignored Files" deletes them). The "N of M" counter is derived from the skip count.
  - `Move` / `Rotate*` / `Skip` rows form the undo stack. `GetLastItem` returns the newest row excluding `ScanFolder`/`GetDuplicated`, and `ButtonUndo_Click` reverses it and deletes the row.
- **Persistence** — SQLite DB at `%LOCALAPPDATA%\FileActionDB.db`. Schema is created with `Database.EnsureCreated()`; there are no EF migrations, so changing the `FileAction` model requires deleting the local DB file. `ActionType` is stored as an int, so reordering enum members (including the misspelled `RoteteRight`) corrupts existing data — append new values only.
- **Image pipeline** — `LoadNextImage` lists supported extensions (`.jpg .png .jpeg .bmp .gif .jfif`) in the top-level folder, sorted by name, minus skipped files, and shows the first. `LoadImage` copies the file to a temp path before decoding (avoids locking the original so it can be moved/rotated), applies EXIF orientation via `ImageUtil.GetRotateFromExif` (`Classes/Image.cs`), and compares a SHA-256 hash (`FileHash`) with the previously loaded image to warn about consecutive duplicates.
- **Rotation is destructive** — `RotateImage` uses `System.Drawing` and overwrites the original file (JPEG re-encoded at quality 100; PNG saved; BMP/GIF are silently not saved).

## Notes

- `appsettings.json` contains `Startup`/`EmailAddresses`/`ConnectionStrings` keys, but none are currently read by the code.
- Comments and commit messages mix Spanish and English.
