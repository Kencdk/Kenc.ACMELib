# Contributing to Kenc.ACMELib

:+1::tada: First off, thanks for taking the time to contribute! :tada::+1:

The following is a set of guidelines for contributing to Kenc.ACMELib and its examples, which are hosted in [Kencdk/Kenc.ACMELib](https://github.com/Kencdk/Kenc.ACMELib) on GitHub. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

#### Table Of Contents
<!-- TOC -->

- [Contributing to Kenc.ACMELib](#contributing-to-kencacmelib)
            - [Table Of Contents](#table-of-contents)
    - [Code of Conduct](#code-of-conduct)
    - [I don't want to read this whole thing I just have a question!!!](#i-dont-want-to-read-this-whole-thing-i-just-have-a-question)
    - [What should I know before I get started?](#what-should-i-know-before-i-get-started)
    - [How Can I Contribute?](#how-can-i-contribute)
        - [Reporting Bugs](#reporting-bugs)
            - [Before Submitting A Bug Report](#before-submitting-a-bug-report)
            - [How Do I Submit A (Good) Bug Report?](#how-do-i-submit-a-good-bug-report)
        - [Suggesting Enhancements](#suggesting-enhancements)
            - [How Do I Submit A (Good) Enhancement Suggestion?](#how-do-i-submit-a-good-enhancement-suggestion)
        - [Pull Requests](#pull-requests)
    - [Styleguides](#styleguides)
        - [Git Commit Messages](#git-commit-messages)

<!-- /TOC -->

## Code of Conduct

This project and everyone participating in it is governed by the [Code of Conduct](https://github.com/Kencdk/Kenc.ACMELib/blob/master/.github/Code_of_Conduct.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [kenc@kenc.dk](mailto:kenc@kenc.dk).

## I don't want to read this whole thing I just have a question!!!

> **Note:** Please don't file an issue to ask a question. You'll get faster results by using the resources below.

* Tweet at [@kenmandk](https://twitter.com/kenmandk)
* Email at [kenc@kenc.dk](mailto:kenc@kenc.dk)

## What should I know before I get started?

Kenc.ACMELib is designed relying on interfaces for all integration points, to ensure testability.

## How Can I Contribute?

### Reporting Bugs

When you are creating a bug report, please [include as many details as possible](#how-do-i-submit-a-good-bug-report). Fill out [the required template](https://github.com/Kencdk/Kenc.ACMELib/blob/master/.github/ISSUE_TEMPLATE/bug_report.md), the information it asks for helps us resolve issues faster.

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

#### Before Submitting A Bug Report

* **Perform a [cursory search](https://github.com/search?q=+is:issue+user:kencdk)** to see if the problem has already been reported. If it has **and the issue is still open**, add a comment to the existing issue instead of opening a new one.

#### How Do I Submit A (Good) Bug Report?

Bugs are tracked as [GitHub issues](https://guides.github.com/features/issues/). Create an issue and provide the following information by filling in [the template](https://github.com/Kencdk/Kenc.ACMELib/blob/master/.github/ISSUE_TEMPLATE/bug_report.md).

Explain the problem and include additional details to help maintainers reproduce the problem:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the exact steps which reproduce the problem** in as many details as possible. For example, start by explaining which actions you took in order which leads to the problem
* **Provide specific examples to demonstrate the steps**. Include example domain names and combinations of actions that leads to the unexpected behavior.
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
* **Explain which behavior you expected to see instead and why.**
* **If possible, include network traces using fiddler or similar.**

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion for Kenc.ACMELib, including completely new features and minor improvements to existing functionality. Following these guidelines helps maintainers and the community understand your suggestion :pencil: and find related suggestions :mag_right:.

#### How Do I Submit A (Good) Enhancement Suggestion?

Enhancement suggestions are tracked as [GitHub issues](https://guides.github.com/features/issues/). Create an issue and provide the following information by filling in [the template](https://github.com/Kencdk/Kenc.ACMELib/blob/master/.github/ISSUE_TEMPLATE/feature_request.md).

Explain the feature and include additional details to help understand the feature:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Describe the current behavior** and **explain which behavior you expected to see instead** and why.
* **Explain why this enhancement would be useful.** 
* **List which section of the RFC the feature is described in** for features in the protocol itself.
* **List some other lets encrypt libraries or clients where this enhancement exists.**

### Pull Requests

The process described here has several goals:

- Maintain/improve code quality
- Add new features available in the ACME protocol.
- Fix issues in the implementation.

Please follow these steps to have your contribution considered by the maintainers:

1. Follow all instructions in [the template](pull_request_template.md)
2. After you submit your pull request, verify that all [status checks](https://help.github.com/articles/about-status-checks/) are passing <details><summary>What if the status checks are failing?</summary>If a status check is failing, and you believe that the failure is unrelated to your change, please leave a comment on the pull request explaining why you believe the failure is unrelated. A maintainer will re-run the status check for you. If we conclude that the failure was a false positive, then we will open an issue to track that problem with our status check suite.</details>

While the prerequisites above must be satisfied prior to having your pull request reviewed, the reviewer(s) may ask you to complete additional design work, tests, or other changes before your pull request can be ultimately accepted.

## Styleguides

### Git Commit Messages

* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line