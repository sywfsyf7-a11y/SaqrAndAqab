# Task 00 — Repo setup
Read AGENTS.md and docs/decisions.md first. Do ONLY this task.

## Steps
1. Confirm the current folder is `C:\SaqrAndAqab` and contains `Assets`, `Packages`, `ProjectSettings`, `AGENTS.md`, `docs`. If anything is missing, STOP and tell the user (Arabic).
2. Run `git --version` and `git lfs version`. If either fails, STOP and tell the user to install Git for Windows from git-scm.com (it includes LFS).
3. Run `git config user.name` and `git config user.email`. If empty, ask the user ONCE for a name and an email, then set them for this repo only. Never invent them.
4. Read `Packages/manifest.json`. Report in Arabic: is `com.unity.render-pipelines.universal` present? Is `com.unity.inputsystem` present? Write both answers into `docs/progress.md` under "Environment".
5. Read `ProjectSettings/EditorSettings.asset`. Report whether serialization mode is Force Text (`m_SerializationMode: 2`) and version control is Visible Meta Files. Do not change them; just report.
6. Create `.gitignore` for Unity: `[Ll]ibrary/`, `[Tt]emp/`, `[Oo]bj/`, `[Bb]uild/`, `[Bb]uilds/`, `[Ll]ogs/`, `[Uu]ser[Ss]ettings/`, `.vs/`, `*.csproj`, `*.sln`, `*.suo`, `*.user`, `*.pidb`, `*.apk`, `*.aab`, `.idea/`, `.vscode/`, `Thumbs.db`, `.DS_Store`.
7. Create `.gitattributes` with Git LFS rules for: png jpg jpeg psd wav mp3 ogg mp4 (`filter=lfs diff=lfs merge=lfs -text`).
8. `git init -b main` if needed, `git lfs install`, add remote `origin` = `https://github.com/sywfsyf7-a11y/SaqrAndAqab.git` (skip if it exists).
9. Commit `chore: project setup` and `git push -u origin main`. Git Credential Manager may open a browser: tell the user (Arabic) to sign in to GitHub. NEVER ask the user to paste a token or password.

## Done when (the user checks)
The GitHub page shows AGENTS.md, docs, Assets, Packages, ProjectSettings. There is NO `Library` folder on GitHub.
