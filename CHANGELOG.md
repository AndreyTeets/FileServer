# Changelog

## [2.0.3] - 2026-01-31

**Notable Changes**:
+ Remove unnecessary `domain` attribute from auth cookie (#64).
+ Fix file existence race condition when saving uploaded files (#65).
+ Improve certificate and settings information display in logs (#71).
+ Add settings validation for address/port/cert, add more checks for other (#69).
+ Add favicon, change page title to FileServer (#72).
+ Rebuild container images with updated os-packages containing a bunch of fixes for OpenSSL-related CVEs (none of which affect the server, since they are either CMS/PKCS12 related and the server doesn't use those, or they aren't applicable because the server doesn't use QUIC and doesn't request client certificates).

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A2.0.3%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v2.0.2...v2.0.3)

## [2.0.2] - 2026-01-19

**Notable Changes**:
+ **SECURITY**: Add rate limiting for login route (#60).
+ **SECURITY**: Use constant-time comparison for token/key related checks (#61).
+ Fix server throwing null reference exception on malformed auth tokens (#51).
+ Fix settings validation not failing on empty strings (#52).
+ Fix settings validation throwing null reference exception on null keys (#53).
+ Update pinned dotnet runtime to 10.0.2 (#58). This is only relevant for pre-built artifacts, excluding framework-dependent binaries.

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A2.0.2%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v2.0.1...v2.0.2)

## [2.0.1] - 2025-12-20

**Notable Changes**:
+ **SECURITY**: Don't pin patch-versions in dockerfiles by default (#46). This is mostly relevant for building from the source code.
+ **SECURITY**: Always upgrade all os-packages to latest in dockerfiles (#47).
+ Update pinned dotnet runtime to 10.0.1 (#48). This is only relevant for pre-built artifacts, excluding framework-dependent binaries.
+ Rebuild container images with the latest os-packages.

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A2.0.1%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v2.0.0...v2.0.1)

## [2.0.0] - 2025-11-23

**BREAKING CHANGES**:
+ Update dotnet target framework from 8.0 to 10.0 (#24). As a consequence, running a cross-platform (framework-dependent) dotnet application now requires AspNetCore runtime 10.0, and building it requires dotnet SDK 10.0 or higher.
+ Add `-jit` suffix for JIT variant of container images and pre-built binaries (#34). As a consequence, Dockerfile-simple is now called Dockerfile-simple-jit and `app` target in the main Dockerfile is now called `app-jit`. Docker `latest` tag still exists and points to the same image as `latest-jit`.
+ Move FileServer, FileServer.Tests, FileServer.E2ETests to src/tests subdirectories (#36). As a consequence, many file paths are now different.

**Notable Changes**:
+ Rewrite server from MVC to trimmable minimal API (#27).
+ Use trimming for all container images and self-contained pre-built binaries (#30).
+ Add build option to serve wwwroot static files from assembly resources (#28).
+ Add build options to publish as single file or as Native AOT (#29).
+ Add AOT variant for container images and pre-built binaries (#31).
+ Add JIT single file (JITSF) variant for pre-built binaries (#35).
+ Add pre-built binaries for linux-musl-armv7 (#37).
+ Upload pre-built container images to GitHub Releases (#32).
+ Get rid of warnings about DataProtection-Keys (which aren't used) (#26).
+ Fix commit info from SourceLink not being included in container images (#25).

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A2.0.0%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.1.2...v2.0.0)

## [1.1.2] - 2025-11-12

**Notable Changes**:
+ Fix app version being 1.0.0 in pre-built binaries (#19).
+ Add log message about server starting, its version and commit (#20).
+ Add generation of sha256sums.txt for pre-built binaries (#21).
+ Add CHANGELOG.md and use it as a source of truth for releases (#22).

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A1.1.2%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.1.1...v1.1.2)

## [1.1.1] - 2025-11-11

**Notable Changes**:
+ Add automatic release creation with pre-built binaries (#16).

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A1.1.1%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.1.0...v1.1.1)

## [1.1.0] - 2025-11-09

**Notable Changes**:
+ Add upload progress tracking and cancellation (#10).
+ Keep last upload info when switching pages.
+ Change server error responses to more appropriate HTTP status codes instead of BadRequest everywhere (1062d4c36eda).
+ Rewrite client to React-like architecture with VDOM that renders only a minimal changed element in DOM (#11).

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A1.1.0%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.0.3...v1.1.0)

## [1.0.3] - 2025-10-18

**Notable Changes**:
+ **SECURITY**: Update vulnerable container base images (#9). This fixes: CVE-2025-55315 (affected, CRITICAL), CVE-2025-9230 (not-affected), CVE-2025-9231 (not-affected), CVE-2025-9232 (not-affected).
+ Fix several missing disposes for PhysicalFileProvider and FileStream objects.
+ Very minor improvements and fixes in error handling.

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A1.0.3%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.0.2...v1.0.3)

## [1.0.2] - 2025-04-12

**Notable Changes**:
+ Improve compatibility of Dockerfile with other build tools (#2). This only applies to building the container image from the source code.

[Unnotable Changes](https://github.com/AndreyTeets/FileServer/pulls?q=milestone%3A1.0.2%20-label%3Anoteworthy)

[All Commits](https://github.com/AndreyTeets/FileServer/compare/v1.0.1...v1.0.2)

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

[2.0.3]: https://github.com/AndreyTeets/FileServer/releases/tag/v2.0.3
[2.0.2]: https://github.com/AndreyTeets/FileServer/releases/tag/v2.0.2
[2.0.1]: https://github.com/AndreyTeets/FileServer/releases/tag/v2.0.1
[2.0.0]: https://github.com/AndreyTeets/FileServer/releases/tag/v2.0.0
[1.1.2]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.1.2
[1.1.1]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.1.1
[1.1.0]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.1.0
[1.0.3]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.0.3
[1.0.2]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.0.2
[1.0.1]: https://github.com/AndreyTeets/FileServer/releases/tag/v1.0.1
