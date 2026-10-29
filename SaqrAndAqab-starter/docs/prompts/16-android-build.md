# Task 16 — Android build setup
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. `Editor/AndroidSetup.cs`: menu `Saqr/Apply Android Settings` that sets via the PlayerSettings API: application id `com.saqrandaqab.game`, product name `صقر وعقاب` (and company `SaqrAndAqab`), default orientation Landscape Left with auto-rotation limited to landscape, min SDK API 24, ARM64 only, scripting backend IL2CPP. If any API name is uncertain for Unity 6000.0, print the manual steps instead of guessing.
2. Write `docs/android-build.md` (in Arabic) with step-by-step instructions: File > Build Profiles > Android > Switch Platform; enable USB debugging on the phone; Build And Run; where the APK is; what to do if Gradle fails (copy the error to OpenCode).
3. Check that SampleScene is in the build list; if not, tell the user how to add it.

## Done when (the user checks)
`Saqr > Apply Android Settings` runs without errors; following docs/android-build.md produces an APK that starts on the phone and shows the Arabic menu correctly.

## Finish
Commit `feat: android build setup`.
