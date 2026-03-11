# SVN 迁移至 Git 操作手册（LOA-Client）

本文档说明如何将 LOA-Client 从 SVN 迁移到 Git，并保留完整提交历史。迁移后不再使用 SVN，日常开发改用 Git。

---

## 一、迁移步骤概览

1. 在 SVN 工作副本的**父目录**执行 `git svn clone <SVN仓库根URL> --trunk=trunk --no-metadata LOA-Client-git`，得到带完整历史的本地 Git 仓库。
2. 在 GitHub/Gitee 等创建**空**远程仓库（不要勾选初始化 README/.gitignore）。
3. 在克隆出的 `LOA-Client-git` 中执行 `git branch -M main`、`git remote add origin <url>` 与 `git push -u origin main`。
4. 之后所有开发在 **LOA-Client-git** 目录进行，不再使用 SVN。

若推送时出现超过 100MB 的大文件，使用 `git filter-branch` 或 `git filter-repo` 从历史中移除该文件，并在 `.gitignore` 中忽略，再推送。

---

## 二、当前 SVN 仓库信息

- **仓库根 URL**：`https://106.15.248.25/svn/LOA-Client`
- **trunk URL**：`https://106.15.248.25/svn/LOA-Client/trunk`

---

## 三、迁移后以 LOA-Client-git 为开发目录

日常开发、提交、推送均在 **LOA-Client-git** 下完成。原 SVN 工作副本（如 `LOA-Client/trunk`）可仅作备份或删除。

---

## 四、SVN 残留清理

在**原 SVN 工作副本目录**（例如 `LOA-Client/trunk` 或 `LOA-Client`）：

- 删除 `.svn/` 与 `.svn_auto/`。
- 若确认不再需要旧副本：整目录备份后删除，仅保留 **LOA-Client-git**。

---

## 五、任务完成用 Git 提交并推送

- 任务完成后的自动提交已改为 Git：见本仓库 `.cursor/rules/git-commit-on-task-complete.mdc`，使用 `.git_commit_msg.txt` 与 `.cursor/scripts/git_commit_and_push.sh`。
- 在仓库根执行：`bash .cursor/scripts/git_commit_and_push.sh`，或手动：`git add -A && git commit -F .git_commit_msg.txt && git push`。

---

## 六、用户规则替换为 Git 版

请在 Cursor 的「用户规则」中，将原先的 SVN 自动提交规则（涉及 `.svn_auto/commit_msg.txt`、`start_auto.sh`）**整段替换**为 Git 版规则。完整条文见本仓库 `Documents/Cursor-User-Rule-Git-Commit.md`，复制到 Cursor 用户规则中即可，避免与 Git 流程冲突。
