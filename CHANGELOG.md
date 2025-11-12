# Changelog

## [1.1.2] - 2025-11-12

**Notable Changes**:
+ Fix app version being 1.0.0 in pre-built binaries (#19).
+ Add log message about server starting, its version and commit (#20).
+ Add generation of sha256sums.txt for pre-built binaries (#21).
+ Add CHANGELOG.md and use it as a source of truth for releases (#22).

[All Changes](https://github.com/AndreyTeets/FileServer/compare/v1.1.1...v1.1.2)

## [1.1.1] - 2025-11-11

**Notable Changes**:
+ Add automatic release creation with pre-built binaries (#16).

[All Changes](https://github.com/AndreyTeets/FileServer/compare/v1.1.0...v1.1.1)

## [1.1.0] - 2025-11-09

**Notable Changes**:
+ Add upload progress tracking and cancellation (#10).
+ Keep last upload info when switching pages.
+ Change server error responses to more appropriate HTTP status codes instead of BadRequest everywhere (1062d4c36eda).
+ Rewrite client to React-like architecture with VDOM that renders only a minimal changed element in DOM (#11).

[All Changes](https://github.com/AndreyTeets/FileServer/compare/v1.0.3...v1.1.0)

## [1.0.3] - 2025-10-18

**Notable Changes**:
+ Update vulnerable container base images (#9).
+ Fix several missing disposes for PhysicalFileProvider and FileStream objects.
+ Very minor improvements and fixes in error handling.
+ Refactoring (mostly style-only) to improve code readability and maintainability.

[All Changes](https://github.com/AndreyTeets/FileServer/compare/v1.0.2...v1.0.3)

## [1.0.2] - 2025-04-12

**Notable Changes**:
+ Improve compatibility of Dockerfile with other build tools (#2). This only applies to building the container image from the source code.

[All Changes](https://github.com/AndreyTeets/FileServer/compare/v1.0.1...v1.0.2)

## [1.0.1] - 2025-04-11

**Initial Release**:\
A minimalistic https file server with a simple password-only authentication.\
It starts an https server at the specified `listen address` and `listen port` using the provided `server certificate`. The server hosts a simple single page application (SPA) which provides a browser UI to:
+ Login using the `login key` (password) specified in server settings.
+ Logout.
+ Show the list of files in the `anonymous downloads folder` specified in server settings.
+ Download or view in text mode any file in the `anonymous downloads folder` specified in server settings.
+ Show the list of files in the `downloads folder` specified in server settings (only if authorized).
+ Download or view in text mode any file in the `downloads folder` specified in server settings (only if authorized).
+ Upload a file to the `uploads folder` specified in server settings (only if authorized).

[All Commits](https://github.com/AndreyTeets/FileServer/commits/v1.0.1)
