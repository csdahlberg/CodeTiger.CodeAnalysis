# Instructions for Logging Issues

## 1. Search for Duplicates

[Search the existing issues](https://github.com/csdahlberg/CodeTiger.CodeAnalysis/issues?utf8=%E2%9C%93&q=is%3Aissue) before logging a new one.

## 2. Do you have a question?

The issue tracker is for **issues**, in other words, bugs and suggestions.
If you have a *question*, please ask on [Gitter](https://gitter.im/csdahlberg/CodeTiger.CodeAnalysis) or [start a discussion](https://github.com/csdahlberg/CodeTiger.CodeAnalysis/discussions) on GitHub.

## 3. Did you find a bug?

When logging a bug, please be sure to include the following:
 * What version of CodeTiger.CodeAnalysis you're using
 * If at all possible, an *isolated* way to reproduce the behavior
 * The behavior you expect to see, and the actual behavior

## 4. Do you have a suggestion?

Suggestions are also accepted in the issue tracker.

In general, the following are useful when reviewing suggestions:
* A description of the problem you're trying to solve
* An overview of the suggested solution
* Examples of how the suggestion would work in various places

# Instructions for Contributing Code
To be accepted, a pull request must:
* Include a description of what your change intends to do
* Have clear commit messages
    * e.g. "Fixed several possible null references in ParenthesisLayoutAnalyzer."
* Build (using Build.ps1) without any errors or warnings
* Roughly follow existing coding style
* Include adequate tests 
    * At least one test should fail in the absence of your non-test code changes. If your PR does not match this criteria, please specify why
    * Tests should include reasonable permutations of the target fix/change
    * Include baseline changes with your change
    * All changed code must have 100% code coverage
* Not introduce any new dependencies
