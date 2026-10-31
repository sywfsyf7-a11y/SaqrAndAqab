# Task 25 — Android build
Read AGENTS.md, docs/decisions.md, docs/v2/decisions-v2.md and docs/v2/screens-spec.md first.
FIRST inspect what already exists in the code for this topic and EXTEND it; never create duplicates. Do ONLY this task.

## Steps
1. `Editor/AndroidSetup.cs` menu `Saqr/Apply Android Settings` (id `com.saqrandaqab.game`, landscape, min API 24, ARM64, IL2CPP). If an API name is uncertain print the manual steps instead of guessing.
2. Write `docs/android-build.md` in Arabic with every click from Build Profiles to a running APK, and what to do when Gradle fails.
3. Verify the scene is in the build list.

## Done when (the user checks)
An APK runs on the phone, Arabic text correct, touch controls work. 0 red errors.

## Finish
Update docs/progress.md, `git add -A`, commit `feat: android build`, `git push`. Reply to the user in simple Arabic: what you did, files changed, how to verify. Ask at most ONE question and only if blocked.
