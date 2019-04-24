# Contributing to Gorgon

Thank you for taking the time to contribute to Gorgon.

The following is a set of guidelines for contributing. These are mostly guidelines, not rules. So, use your best judgment, and feel free to propose changes to this document in a pull request.

#### Table Of Contents

[Code of Conduct](#code-of-conduct)

[How Can I Contribute?](#how-can-i-contribute)
  * [Reporting Bugs](#reporting-bugs)
  * [Suggesting Enhancements](#suggesting-enhancements)
  * [Your First Code Contribution](#your-first-code-contribution)
  * [Pull Requests](#pull-requests)

[Style](#style)
  * [Git Commit Messages](#git-commit-messages)
  * [C#](#c)
  * [Documentation Styleguide](#documentation-styleguide)

[Additional Notes](#additional-notes)
  * [Issue and Pull Request Labels](#issue-and-pull-request-labels)

## Code of Conduct

This project and everyone participating in it is governed by the [Gorgon Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to [gorgon@tape-worm.net](mailto:gorgon@tape-worm.net).

## How Can I Contribute?

### Reporting Bugs

This section guides you through submitting a bug report for Gorgon. Following these guidelines helps maintainers and the community understand your report, reproduce the behavior, and find related reports.

Before creating bug reports, please check [the issues tab](https://github.com/Tape-Worm/Gorgon/issues) as you might find out that you don't need to create one. When you are creating a bug report, please [include as many details as possible](#how-do-i-submit-a-good-bug-report).

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

#### How Do I Submit A (Good) Bug Report?

Bugs are tracked as [GitHub issues](https://guides.github.com/features/issues/).

Explain the problem and include additional details to help maintainers reproduce the problem:

* **Use a clear and descriptive title** for the issue to identify the problem.
* **Describe the exact steps which reproduce the problem** in as many details as possible. 
* **Provide specific examples to demonstrate the steps**. Include links to files or GitHub projects, or copy/pasteable snippets, which you use in those examples. If you're providing snippets in the issue, use [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines).
* **Describe the behavior you observed after following the steps** and point out what exactly is the problem with that behavior.
* **Explain which behavior you expected to see instead and why.**
* **Include screenshots** which show you following the described steps and clearly demonstrate the problem. 
* **If the problem wasn't triggered by a specific action**, describe what you were doing before the problem happened and share more information using the guidelines below.

### Suggesting Enhancements

This section guides you through submitting an enhancement suggestion for Gorgon, including completely new features and minor improvements to existing functionality. Following these guidelines helps maintainers and the community understand your suggestion and find related suggestions.

#### How Do I Submit A (Good) Enhancement Suggestion?

Enhancement suggestions are tracked as [GitHub issues](https://guides.github.com/features/issues/). Create a new issue and provide the following information:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Provide specific examples to demonstrate the steps**. Include copy/pasteable snippets which you use in those examples, as [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines).
* **Describe the current behavior** and **explain which behavior you expected to see instead** and why.
* **Include screenshots (if possible)** which help you demonstrate the steps or point out the part of Gorgon which the suggestion is related to. 
* **Explain why this enhancement would be useful** to most Gorgon users and isn't something that can or should be implemented separately.
* **Specify which version of Gorgon you're using.** 

### Your First Code Contribution

 You can start by looking through these `beginner` and `help-wanted` issues:

* [Beginner issues][beginner] - issues which should only require a few lines of code, and a test or two.
* [Help wanted issues][help-wanted] - issues which should be a bit more involved than `beginner` issues.

Both issue lists are sorted by total number of comments. 

### Pull Requests

The process described here has several goals:

- Fix problems that are important to users
- Enable a sustainable system for Gorgon's maintainers to review contributions

Please follow these steps to have your contribution considered by the maintainers:

1. Follow the [Style](#style) information.
2. After you submit your pull request, verify that all [status checks](https://help.github.com/articles/about-status-checks/) are passing <details><summary>What if the status checks are failing?</summary>If a status check is failing, and you believe that the failure is unrelated to your change, please leave a comment on the pull request explaining why you believe the failure is unrelated. A maintainer will re-run the status check for you. If we conclude that the failure was a false positive, then we will open an issue to track that problem with our status check suite.</details>

While the prerequisites above must be satisfied prior to having your pull request reviewed, the reviewer(s) may ask you to complete additional design work, tests, or other changes before your pull request can be ultimately accepted.

## Style

### Git Commit Messages

* Limit the first line to 72 characters or less
* Reference issues and pull requests liberally after the first line

### C#

Code will be subject to code review before pushing into the root branch(es). 

All C# code must adhere to the [best practices](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions) as outlined by Microsoft. Deviations may be acceptable upon review by a maintainer on a case by case basis. 

Additionally, code must be formatted, as closely as possible, in the style present throughout the library code base.  This means:

* Use Tabs - not spaces. 
* Only use #region blocks to organize your code. Do _NOT_ use them to hide code.  Types of acceptable region blocks are littered throughout the code.
* Try to adhere as closely to [SOLID](https://en.wikipedia.org/wiki/SOLID) principles as possible. 
* When building user interfaces, try to use [MVVM](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93viewmodel) (or something approaching my bastardization of it - see the Editor code).
* Try to use small, concise methods. Do not write massive methods that take more than 1.5 pages to get through.
* Try to keep method/constructor parameters down to 4-6 parameters. Sometimes this is not possible, so this will depend on context.

### Documentation Styleguide

When writing the documentation for your code, please try to be concise and try to convey your intent clearly. 

* Always comment your code. You don't need to write War & Peace, but something to describe your thinking is required.
* Comments for events, methods, properties, classes, structs, and non-private variables must use XML commenting. Private variables can use standard commenting.
* Ensure comments are up to date. Don't make a function that returns the sum of two numbers and have a comment that describes drawing a pixel on the screen.

## Additional Notes

If your code does something strange to get around a problem, please write comments either in the body, or <remarks> section explaining why the code is written the way it is. Many of the libraries (including .NET itself) in use in Gorgon have quirks, it's good to know where there are.

Gorgon has been in evolving and developed over years, and as such, styles and methodologies have changed over time. This means that the code isn't always compliant with the guidelines in this document. Do not take that to mean that these guidelines are optional, they are not. This is very much a case of do as I say, not as I once did when I was young and dumb.

### Issue and Pull Request Labels

This section lists the labels we use to help us track and manage issues and pull requests. 

[GitHub search](https://help.github.com/articles/searching-issues/) makes it easy to use labels for finding groups of issues or pull requests you're interested in.

The labels are loosely grouped by their purpose, but it's not required that every issue have a label from every group or that an issue can't have more than one label from the same group.
