# Conventions
These conventions add to the shared [contributing guide](https://github.com/TomMaple/.github/blob/main/CONTRIBUTING.md) and win
over it. They follow the [conventions of the core](https://github.com/TomMaple/Maple.Result/blob/main/docs/conventions.md),
`Maple.Result`, adapted to this repository; a change of a shared rule is made in both.

## Git
1. Workflow: [GitHub flow](https://docs.github.com/en/get-started/using-github/github-flow)
1. Branch naming:
	* `main`,
	* `feature/{issue_id}-{kebab-case-name}`
	* `bugfix/{issue_id}-{kebab-case-name}`
1. Branches:
	* Create a branch from an up-to-date `main`, after the previous pull request is merged.
	* Delete a branch once its pull request is merged.
1. Commit messages:
	* [Conventional commits](https://www.conventionalcommits.org/)
	* Past tense
	* The issue number at the end of the subject, e.g., `feat: Added the retryable key, WithRetryable and IsRetryable (#19)`.
	* A breaking change is marked with `!` (e.g., `feat!:`) or a `BREAKING CHANGE:` footer; it drives the version bump
	  (see [Releasing](releasing.md)).
1. Commits:
	* The changes of a pull request are grouped into one commit per type (e.g., `build:`, `feat:`, `test:`, `ci:`).
	* Changes in the `docs` folder and of the READMEs go in a separate `docs:` commit, made after all the other changes
	  (code and tests) are committed.
1. Signing:
	* Commits are signed (SSH signing, `commit.gpgsign = true`); `main` accepts only signed commits.
	* A contributor’s unsigned commits are fine: the pull request is squashed, and GitHub signs the squash commit.
	* Before merging your own branch with a merge commit, check that every commit is signed
	  (`git log --format='%h %G? %s' main..`, all `G`); re-sign with `git rebase --force-rebase --gpg-sign main`.
1. Before opening a pull request, all of these pass:
	* `dotnet build -c Release`: no warnings (CI treats them as errors; this includes the code style, naming, trim and AOT
	  rules),
	* `dotnet test`,
	* `dotnet format --verify-no-changes`,
	* `python .github/scripts/check_reference_docs.py`: the reference pages agree with the code (see *Documentation*).

	The required checks are `build`, `newer-sdk`, `pr-title`, `Analyze (csharp)` and `Analyze (actions)`. On a pull
	request without code changes (e.g., only `docs/`), `build`, `newer-sdk` and `Analyze (csharp)` are skipped, which
	counts as passed. The job `reference` checks the reference pages on every pull request, also on one that changes
	only the documentation.

	`build` also fails when the library references a package other than `Maple.Result`, directly or transitively
	(`ALLOWED_PACKAGES` in `ci.yml`): the package must not bring *Polly* or `Microsoft.Extensions.*` to the packages that
	reference it. `newer-sdk` builds and tests with the .NET SDK after the one of `build` (now .NET 11), because consumers
	may compile against the package with a newer compiler than ours.
1. Pull requests:
	* The title follows the commit message rules: a squash merge turns the title into the subject of the commit on
	  `main` and the description into its body, so the release notes and the version bump come from them.
	* A breaking change is marked with `!` in the title (e.g., `feat!: …`) or a `BREAKING CHANGE:` footer at the end of
	  the description; a `!` on a branch commit alone is lost in a squash merge.
	* The description links the issue with `Closes #…`.
	* Every review thread is resolved before the merge, also the notes of CodeQL.
1. Merging:
	* Squash and merge by default, including contributor and Dependabot pull requests.
	* Create a merge commit only for your own branch whose every commit is a signed conventional commit; the commits
	  then reach `main` unchanged.
	* Rebase merging is disabled.
	* Dependabot pull requests for NuGet minor and patch updates merge automatically once the required checks pass,
	  also those of `Maple.Result`, the only runtime dependency: such an update raises the minimum version of the
	  package, so the notes of the next release mention it. NuGet majors and GitHub Actions updates are reviewed and merged
	  by hand.
1. Workflows:
	* Every action is pinned to a full commit SHA with the version in a comment (`uses: owner/action@<sha> # vX.Y.Z`);
	  the repository rejects unpinned actions.
	* A new third-party action must be added to the allowlist (Settings → Actions → General) first.
	* A workflow that provides a required check has no `paths` filter; its jobs are skipped instead
	  (`.github/scripts/changes.py`).

## C#
1. Source code
	* [Framework design guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/) (public API) and
	  [C# coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
	  (code style); where they differ from the rules below or from `.editorconfig`, the rules here win.
	* Indent character: *space*.
	* Constant naming style: *PascalCase*.
	* Formatting: `.editorconfig`; lines up to 160 characters.
	* Every source file starts with the SPDX license identifier and the copyright header.
	* A file has a `#region consts` at the top and a `#region helper methods` at the bottom where it needs them; members
	  are ordered alphabetically within each `#region`.
	* Extension members are declared in C# 14 extension blocks, one block per receiver (e.g., `extension(Error error)`),
	  with a `<summary>` and a `<param>` for the receiver. They compile to static extension methods, which every C# version
	  and other .NET languages can call; the reference pages show them in that form (see *Documentation*). The core does
	  not use extension blocks yet.
	* Chained calls go one per line, unless the whole statement fits on one line.
	* An expression-bodied method or local function is never written on one line: the `=>` starts the next line, indented
	  once. Any other `=>` that is followed by a line break (a lambda, a property, a switch expression arm) starts the next
	  line too, unless it opens a block (`{`).
	* An `if` whose condition spans several lines has braces (IDE0011).
	* Empty lines:
		* before a `return`, unless the block has only two one-line statements;
		* before the other control-flow statements (`throw`, `break`, `continue`, `goto`, `yield return`, `yield break`),
		  unless the statement directly follows the line that opens it (e.g., the body of an `if` without braces, or the
		  first statement after `{`); not in a `switch` section (no empty line before its `break`);
		* after each section of a `switch` statement (after its `break`), except the last one;
		* after a statement that spans several lines, unless the next line closes the block (`}`);
		* after `#region` and before `#endregion`, so that the region’s content is set off from both.
	* Conditional expressions (`?:`) go one operand per line, with `?` and `:` at the start of the line.
	* In `src`, an argument or an initializer value is a simple expression: a variable, a constant or a literal, a member
	  access, `nameof`, a lambda, a `??` (but not `?? throw …`), a simple arithmetic expression (e.g., `count + 1`), or a
	  string built for an exception message. Anything else is assigned to a well-named local variable first: to make it
	  readable, to give it a meaning, and to make a failure easy to locate. The exceptions are a `this(…)` or `base(…)`
	  constructor initializer and a field initializer, which no statement can precede. Tests may pass calls and other
	  expressions as arguments where it keeps them readable.
	* A regular expression is a `static readonly Regex` field, not a `[GeneratedRegex]`: the generated code is counted by
	  the coverage and cannot be covered by the tests.
	* Exceptions name the caller’s parameter (e.g., `delay` rather than the parameter of an internal helper).
	* Invalid input to a writer is rejected with an exception before anything is changed; it is never silently normalized
	  or dropped. A reader never throws for a stored value, because the values can come from another service: a value that
	  is not readable counts as absent.
1. XML documentation
	* Every public member is documented (`GenerateDocumentationFile` is on).
	* Documentation shared by several members is written once and inherited with `<inheritdoc cref="…" />`; the local
	  tags (e.g., `<summary>`, `<returns>`) go before it.
1. Tests
	* One test project per project of `src`, named `<project>.Tests.Unit`:
	  `tests/Maple.Result.Extensions.Resilience.Abstractions.Tests.Unit`.
	* Layout: the folders mirror the tested project; namespaces follow the folders, under the root namespace
	  `Maple.Results.Extensions.Resilience.Abstractions.Tests.Unit` (the project and folder names keep the package name
	  `Maple.Result`; the namespaces use `Maple.Results`, like the library).
	* Test class: `<TestedType><TestType>Tests` (e.g., `ErrorBuilderResilienceExtensionsUnitTests`). Tests of one concern
	  across many types are named after the concern.
	* Test method: `<Member>_<Scenario>_<ExpectedBehavior>`
	  ([naming convention](https://learn.microsoft.com/dotnet/core/testing/unit-testing-best-practices#naming-your-tests)),
	  e.g., `WithRetryAfterDelay_NegativeDelay_ThrowsArgumentOutOfRangeException`.
	* Every class that is not a test class goes under `TestingInfrastructure/`, by kind: `Fakes/` (test doubles, e.g.,
	  `FixedTimeProvider`), `Fixtures/`, `Helpers/` and `TestData/`; test data used by one test class stays in that class.
	* Each test is split into `// Arrange`, `// Act` and `// Assert` sections (`// Act & Assert` when they cannot be
	  separated).
	* Assertions use [Shouldly](https://docs.shouldly.org/); exceptions are captured with `Record.Exception(…)` and
	  then asserted.
	* No loops in assertions: one case per test, or per `[InlineData]` / `[MemberData]` row of a `[Theory]`. A `null` row
	  of a `TheoryData` is written `null!`.
	* A test that depends on the time uses a `TimeProvider` with a fixed “now” (`FixedTimeProvider`), never the system
	  clock, unless it tests the default of a `null` time provider.
	* Every bug fix comes with a test that fails without the fix.
	* Code coverage: CI collects it with `tests/CodeCoverage.config`, which leaves nothing out, and shows the summary in the
	  job summary. The `build` job fails when the line or the branch coverage is below 90% (`MINIMUM_COVERAGE` in
	  `ci.yml`). Every line of the code that runs is covered; a branch that cannot be reached is explained in the pull
	  request that leaves it. To measure it locally, pass the settings file with an absolute path (a relative one is
	  resolved against the output folder of the tests):
	  `dotnet test -c Release --coverage --coverage-output-format cobertura --coverage-settings "$PWD/tests/CodeCoverage.config"`.
	* Code written with an AI assistant is developed test-first
	  ([TDD](https://martinfowler.com/bliki/TestDrivenDevelopment.html)): first the tests that describe the change are
	  written and seen to fail, then the implementation makes them pass, then the code is refactored while they stay
	  green. The tests are reviewed before the implementation, so they state the intended behaviour rather than repeat
	  what the code happens to do.

## Compatibility
1. The metadata keys, the types and formats of their values, and the error types and titles are a contract with other
   packages and with non-.NET clients that read a serialized error ([Metadata format](metadata-format.md)): they never
   change. A new key or format is an addition, and the readers keep accepting what an earlier version wrote.
1. `dotnet pack` validates the public API against the baseline version (see [Releasing](releasing.md#package-validation)).

## Documentation
1. Typography: all texts (XML documentation, exception messages, Markdown documentation) use typographic characters:
   quotes (‘…’, “…”), apostrophes (’), em dashes (—), en dashes for ranges (e.g., 2–32) and the ellipsis (…).
   Code, identifiers, code spans and string values that are part of an API or a format (e.g., JSON) keep the ASCII
   characters.
1. The `README.md` of the repository is an overview for GitHub (relative links, GitHub alerts); the guides are in
   `docs/guides`, and the format of the metadata is in `docs/metadata-format.md`. The package README,
   `src/Maple.Result.Extensions.Resilience.Abstractions/README.md`, is shown on nuget.org, which renders CommonMark:
   it uses absolute links only, no GitHub alerts, and images only from the hosts that nuget.org allows. Keep the quick
   start of both in sync.
1. The reference pages in `docs/Reference` are written by hand, in the layout of the
   [.NET API reference](https://learn.microsoft.com/dotnet/api/) of Microsoft:
	* Every public member has a page, or a section of the page of its overloads; a constant or a property of a class of
	  constants is a row of the table of its type, with a source link.
	* A type page links to the file of its type, without a line. Its declaration is the one of the code. Its member
	  tables (*Constructors*, *Fields*, *Properties*, *Methods*, *Operators*) are in alphabetical order, static and
	  instance members together.
	* Every member has a `Source:` link that opens the line of its declaration (`…/blob/main/src/….cs#L<line>`) and is
	  labelled with the file name. A source link opens `main` or the tag of a release (`…/blob/v3.0.0/src/…`): the
	  pull request that prepares a release pins every link to its tag (see [Releasing](releasing.md)), and between
	  releases a link keeps that tag while the declaration that it opens stays the same.
	* The `csharp` block under the link is that declaration, with neither `async` nor `partial`. A member of an extension
	  block is shown as the static extension method that consumers see, with `static` and the receiver as the first
	  parameter: `public static bool IsRetryable(this Error error, TimeProvider? timeProvider = null);` for
	  `IsRetryable(TimeProvider? timeProvider = null)` in `extension(Error error)`. The block shows the attributes of the
	  code, except those that the .NET API reference leaves out: the attributes of the nullable analysis (e.g.,
	  `[NotNullWhen]`) and those for the debugger. The prose of the page says when a value is `null`.
	* A heading names a member with its type parameters and the types of its parameters, the receiver included, e.g.,
	  `WithRetryAfter(ErrorBuilder, TimeSpan, DateTimeOffset)`. Types are written with their .NET names (`String`,
	  `Boolean`), a nullable value type as `Nullable<T>` (e.g., `Nullable<TimeSpan>` for `TimeSpan?`), and a nullable
	  reference type without the `?`. The *Type Parameters*, *Parameters*, *Returns* and *Property Value* sections follow
	  the same rules, and a link that names a member with its parameters is written as the heading that it opens.
	* A type of the core (e.g., `Error`, `ErrorBuilder`) links to its page in the core’s reference, on GitHub.
	* A link to learn.microsoft.com has no locale (`/dotnet/api/…`, not `/en-us/dotnet/api/…`).
1. After a code change, update the pages of the changed members, and link each of them to `main` with the line of its
   declaration. A link to `main` below the change gets its new line; a link to a release keeps its tag and line, as
   it opens the code of that release. `python .github/scripts/check_reference_docs.py` checks the rules above: a
   link to a release against the code at its tag (a declaration that changed since is reported, so that the member is
   linked to `main`), and that every relative link and anchor of `README.md` and `docs/` resolves. It needs the tags
   of the repository (`git fetch --tags`). The script is a port of the core’s; a fix of a shared rule is made in both.
1. Every breaking change is documented in the migration guide of the major version it is released in, with what
   changed, why, and how to update the calling code.
