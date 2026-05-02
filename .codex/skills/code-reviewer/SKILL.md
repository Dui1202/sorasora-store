---
name: code-reviewer
description: General-purpose code review workflow for reviewing diffs, pull requests, commits, patches, or selected files across any language or framework. Use when Codex is asked to review code, assess implementation quality, find bugs, evaluate security or performance risk, check tests, or provide actionable PR feedback.
---

# Code Reviewer

## Overview

Review code like a senior engineer: prioritize correctness, security, maintainability, performance, and test coverage. Produce specific, actionable findings instead of style-only commentary.

## Review Workflow

1. Identify the review target: current diff, branch comparison, commit, PR, selected files, or pasted code.
2. Load local instructions first when available, especially `AGENTS.md`, repository docs, linters, test conventions, and architecture notes.
3. Inspect the actual changed code and the surrounding call sites, types, contracts, and tests needed to understand behavior.
4. Trace data flow across boundaries where relevant: input validation, auth, persistence, external calls, serialization, state transitions, and error handling.
5. Verify risky assumptions with tests, type checks, or focused commands when feasible.
6. Report findings first, ordered by severity. Keep summaries secondary.

## What To Prioritize

- Correctness bugs, edge cases, broken contracts, regressions, and data corruption risks.
- Security issues: auth bypass, injection, XSS, unsafe deserialization, secret exposure, insecure file or network handling, and missing authorization checks.
- Reliability issues: missing error handling, partial writes, transaction gaps, race conditions, retries, idempotency, and cleanup.
- Performance issues with clear impact: N+1 queries, unbounded loads, expensive render paths, unnecessary network calls, leaks, or avoidable allocations.
- Maintainability issues that affect future changes: unclear ownership boundaries, duplicated non-trivial logic, confusing control flow, overly broad types, and hidden side effects.
- Test gaps for important behavior, especially changed business logic, mappings, validators, error handling, and integration boundaries.

Avoid spending review budget on formatting or preference issues when linters or formatters can handle them.

## Severity

- `Blocker`: Must fix before merge. Use for exploitable security flaws, data loss, broken API contracts, failed critical workflows, race conditions, or production-breaking regressions.
- `Suggestion`: Should fix. Use for meaningful correctness risk, missing validation, missing important tests, maintainability problems with real cost, and performance issues with likely impact.
- `Nit`: Nice to have. Use sparingly for small clarity improvements, minor docs gaps, or naming improvements that are not correctness risks.

If severity is uncertain, explain the assumption and choose the lower severity unless the failure mode is clearly costly.

## Finding Format

Use this shape for each issue:

```markdown
**Blocker: Short Title**
[path/to/file.ext](/absolute/path/to/file.ext:42): Specific code location and problem.

**Why:** Explain the concrete failure mode and impact.

**Suggestion:** Describe the smallest practical fix or point to the existing pattern to reuse.
```

Use `Suggestion` and `Nit` the same way. Prefer plain labels over symbols so review output works in terminals, issue trackers, and chat clients.

## Output Rules

- Start with findings, ordered by severity.
- Include exact file and line references whenever possible.
- Explain why each issue matters and how to fix it.
- Ask questions only when intent is genuinely unclear and affects correctness.
- Say clearly when no issues are found, then mention residual risk or unrun checks.
- Include a brief summary only after findings.
- Do not invent issues from code that was not inspected.

## Review Heuristics

- Compare changed contracts against callers and consumers.
- Check that validation exists at the boundary that receives untrusted input.
- Check that authorization is enforced server-side, not only in UI.
- Check persistence changes for transactions, concurrency, migrations, and backwards compatibility.
- Check API responses for stable error shapes and non-leakage of internal details.
- Check async code for missed awaits, cancellation, ordering assumptions, and cleanup.
- Check UI code for state consistency, accessible controls, loading and error states, and unexpected re-render or fetch loops.
- Check tests by behavior, not only by coverage count.

## Local Project Instructions

Treat repository instructions as part of the review contract when they exist. For example, if `AGENTS.md`, `CONTRIBUTING.md`, architecture docs, or local style guides define boundaries, validation rules, error formats, testing expectations, or dependency direction, review changed code against those rules and cite the specific local rule when it affects a finding.
