# FileServer
Is a minimalistic https file server with a simple password-only authentication. It provides a simple, secure, universal, cross-platform method to transfer files over network (or just view them in text mode in a browser).

It's useful, for example, to transfer files over LAN directly between 2 devices if enabling/installing things like SSH, Samba, FTP, Windows folder sharing, e.t.c. on them is undesired. Also mobile devices often miss the client tools necessary to use those, but they do have a browser.

It's implemented as a dotnet AspNetCore Kestrel server with a plain JavaScript single page mini-site (which basically consists of only several buttons). The codebase is pretty small - excluding server-side files that don't contain actual logic (mostly DTO-models) - about 20 C# files for the server-side (around 50 lines each, half of which are related to startup/settings/logging) + about 10 JS files for the client-side (around 100 lines each). There are no dependencies except for AspNetCore runtime, which is included when built (published) as self-contained.

## What can it do?
It starts an https server at the specified `listen address` and `listen port` using the provided `server certificate`. The server hosts a simple single page application (SPA) which provides a browser UI to:
+ Login using the `login key` (password) specified in server settings.
+ Logout.
+ Show a list of files in the `anonymous downloads folder` specified in server settings.
+ Download or view in text mode any file in the `anonymous downloads folder` specified in server settings.
+ Show a list of files in the `downloads folder` specified in server settings (only if authorized).
+ Download or view in text mode any file in the `downloads folder` specified in server settings (only if authorized).
+ Upload a file to the `uploads folder` specified in server settings (only if authorized).

## Nuances
+ There are no file size restrictions for downloading and uploading.
+ Uploaded files names are sanitized before the server saves them: any characters except the whitelisted (which are any ASCII letter, any digit, space and any of ``` !#$%&'()+,-.;=@[]^_`{}~ ```) are replaced with `_`, then the name is shortened to the first 120 symbols, and then the result is prefixed and postfixed with `upl.` and `.oad` respectively.
+ The server won't accept an upload if a file with the same (sanitized) name already exists.
+ Some browsers on mobile devices (e.g. Safari on IPhone/IPad) ignore the "Content-Type" header and may try to interpret the content based on the file name extension, which may lead to "Download" button viewing the file or "View" button downloading the file. This can be solved by renaming the file on the server to a name without extension.
+ Volume mounting Windows directories (for use as downloads/uploads folders) into container running in WSL can result in slow download/upload speeds. It's a common issue and isn't specific to this server. This can be solved by keeping the downloads/uploads folders within the Linux file system or by using native Windows binaries instead of container images.

## Authentication system
To authenticate an incoming request that requires authorization, the server expects a client to provide 2 tokens: an auth token in the request HttpOnly cookie and an antiforgery token in the request header/query-parameter. The antiforgery token is kept by the SPA client in a browser local storage (which is scoped to protocol://host:port) and the cookie is managed by the browser. Both tokens have to be a HMACSHA256-signed-claim for "user name;token type;expiration time". Authentication succeeds only if both tokens are valid (that is, they are signed with the server `signing key`, have the correct token types, and are not expired) and have the same user name (the server issues tokens during a login operation only for one hardcoded `Main` user).

For a login operation to succeed, the server expects a client to provide a login request with the same password as the server `login key`. If successful, the server responds with a pair of newly created tokens: an auth token in the "Set-Cookie ...;Secure;HttpOnly;SameSite=Strict" header, and an antiforgery token in the response body. Two types of tokens are used because cookies generally provide more secure browser-handled storage, while an antiforgery token addresses one of the main weaknesses of cookies: that browsers may automatically send them, even if the request originated from another site (even if the SameSite cookie setting is supported, what browsers consider the same site is a confusing concept, much broader than the same origin).

To perform a logout operation, the SPA client sends a logout request to the server, which simply responds with the "Set-Cookie token=empty;expired" header, which is then handled by the browser. Then the SPA client removes the antiforgery token from the browser local storage. As a consequence it's still possible to access the server with those tokens until they expire (for example, if they got intercepted or got saved somewhere else or got not deleted). This shouldn't be a problem if the `tokens ttl` is configured short. To completely invalidate all issued tokens, the server `signing key` has to be changed.

As for authorization - the server requires the authenticated user name to be `Main`, but it's kind of redundant since the server doesn't issue tokens for any other users.

## Configuration
Server settings are configured using the `appsettings.json` file. The file must exist at the path specified in the `FileServer_SettingsFilePath` environmental variable (which is by default set to `/app/settings/appsettings.json` in the provided container images) or in the working directory of the server if this environmental variable is unset. The file is hot-reloaded on change (including the logging configuration section).

Template settings file can be found here [src/FileServer/appsettings.template.json](src/FileServer/appsettings.template.json). At a bare minimum the `signing key` and `login key` have to be changed, as they are empty in the template and the server will refuse to start with invalid settings.

It's also possible to override the settings specified in the file using environmental variables. For example, setting the `FileServer__Settings__ListenAddress` environmental variable will override the `Settings.ListenAddress` setting specified in the `appsettings.json` file.

Settings that can be configured:
+ `Settings.ListenAddress` which is referred to in this readme file as `listen address`.
+ `Settings.ListenPort` which is referred to in this readme file as `listen port`.
+ `Settings.CertFilePath` which is referred to in this readme file as `server certificate`.
+ `Settings.CertKeyPath` which is referred to in this readme file as `server certificate`.
+ `Settings.DownloadAnonDir` which is referred to in this readme file as `anonymous downloads folder`.
+ `Settings.DownloadDir` which is referred to in this readme file as `downloads folder`.
+ `Settings.UploadDir` which is referred to in this readme file as `uploads folder`.
+ `Settings.SigningKey` which is referred to in this readme file as `signing key`.
+ `Settings.LoginKey` which is referred to in this readme file as `login key`.
+ `Settings.TokensTtlSeconds` which is referred to in this readme file as `tokens ttl`.

## Usage requirements
+ To build container images and/or run them - docker or an alternative container engine (e.g. podman).
+ To build (publish) binaries - dotnet SDK 10.0 or higher.
+ To run cross-platform binaries - AspNetCore runtime 10.0.
+ To run self-contained binaries on Linux - dotnet runtime dependencies.
+ To run self-contained binaries on Windows/macOS - nothing.

The information above doesn't take into account the AOT variant of binaries. Running AOT binaries should work out of the box on most systems without any special requirements, other than having the appropriate libc and OpenSSL libraries on Linux. As for requirements to build AOT binaries - refer to the dotnet documentation (also commands in AOT-related dockerfiles may be informative examples).

## Usage (container images)
+ ###### 1. Get the source code (skip if using pre-built).
    For example, to get the latest stable sources:
    ```
    git clone -b stable https://github.com/AndreyTeets/FileServer.git
    cd FileServer
    ```

+ ###### 2. Build the server (skip if using pre-built).
    For example, to build the JIT variant using docker:
    ```
    docker build . --pull -f src/FileServer/Dockerfile-simple-jit -t my_file_server:latest
    ```
    More examples can be found in the [_commands.txt](_commands.txt) file.

+ ###### 3. Create/prepare the `server certificate` in PEM format.
    For example, to create a self signed certificate:
    ```
    mkdir server_cert
    openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes -keyout server_cert/cert.key -out server_cert/cert.crt -subj "/CN=localhost"
    ```
    Note: An HTTPS connection still provides encryption even if the certificate is untrusted by the browser (e.g. if it's self-signed). But to ensure that the connection is not intercepted, the certificate must still be verified by other means, such as manually via the browser "view certificate" menu and comparing its fingerprints with those printed in the server logs during startup.

+ ###### 4. Create/prepare the `anonymous downloads folder`, `downloads folder`, `uploads folder`.
    For example:
    ```
    mkdir fs_data
    mkdir fs_data/anonymous_downloads
    mkdir fs_data/downloads
    mkdir fs_data/uploads
    ```
    On Linux set up permissions if necessary.

+ ###### 5. Create/prepare the settings file.
    For example, assuming steps 3 and 4 have been strictly followed, copy the template settings file to `appsettings.json` in the current directory and change:
    ```
    "CertFilePath": "/server_cert/cert.crt",
    "CertKeyPath": "/server_cert/cert.key",
    "SigningKey": "<some string from 20 to 64 symbols>",
    "LoginKey": "<some password, 12 symbols minimum>",
    ```

+ ###### 6. Start the server.
    For example, assuming steps 3, 4, 5 have been strictly followed, to run the image built in step 2 using docker on Linux:
    ```
    docker run --rm -it --pull=never --name fs \
        --security-opt no-new-privileges \
        --cap-drop=all \
        -u "$(id -u):$(id -g)" \
        -p 8443:8443/tcp \
        -e "FileServer_SettingsFilePath=/app/appsettings.json" \
        -v "`pwd`/appsettings.json:/app/appsettings.json:ro" \
        -v "`pwd`/server_cert:/server_cert:ro" \
        -v "`pwd`/fs_data:/fs_data" \
        my_file_server:latest
    ```
    + Adjust or remove the `-u` option on Windows.
    + Replace ``` `pwd` ``` with `$pwd` or `%cd%` when using pwsh or Windows cmd respectively.
    + Replace `\` with ``` ` ``` or `^` when using pwsh or Windows cmd respectively.
    + To run in non-interactive (detached) mode use `-d --restart=unless-stopped` instead of `--rm -it`.

    Permissions:
    + To avoid confusion: "root-ness outside the container" is referred to as "rootful mode"/"rootless mode", and "root-ness inside the container" as "root user"/"non-root user".
    + The provided container images don't set any custom users and will run as the root user unless it's explicitly specified in the run command otherwise.
    + The server does not require root permissions and can run with an arbitrary user UID. The only requirement is for it to have read access to the settings file, all the files and directories specified in settings, and, if upload functionality is to be used, write access to the uploads directory (from the perspective of the user running inside the container). Neither does it require any capabilities and the `--cap-drop=all` option can be used if the previous condition is met.
    + When running as the root user in rootful mode (which docker by default does), uploaded files will be owned by the root user on the host, which is at least inconvenient. It also poses additional security risks. So it is highly recommended to use the `-u "$(id -u):$(id -g)"` option, which will make uploaded files being owned by the same user who launched the container.
    + When running as the root user in rootless mode (which e.g. podman by default does), there may be no problem with uploaded files ownership (e.g. with podman, there isn't, as it by default maps the host user who launched the container to the root user inside the container). But changing to a non-root user is still recommended to reduce security risks. For podman it can be done using the `--userns=keep-id` option, which will change the previously mentioned mapping to the same UID inside the container as on the host, or the `--userns=keep-id:uid=12345,gid=12345` option, which will change it to the specified UID.
    + On systems with SELinux it may be necessary to use the `:z` volume mount option (with `!care!`, as it will recursively relabel the mapped directory on the host, which may break the host system if used on directories that are used elsewhere besides the container). In case of relabeling, uploaded files will have the "system_u:object_r:container_file_t:s0" context. To restore the default context, the `restorecon -RFv /path/to/fs_data` command can be used. If relabeling is an issue and has to be avoided the `--security-opt label=disable` option can be used instead (which will basically disable SELinux for the containerized process).

+ ###### 7. Open the appropriate tcp port in firewall if necessary.

+ ###### 8. Visit the site using a browser.
    In this example, to do it from the same machine, the address will be https://127.0.0.1:8443 or https://localhost:8443

## Usage (binaries)
Replace these steps from the container images usage example:
+ ###### 2. Build the server (skip if using pre-built).
    For example:
    ```
    dotnet publish src/FileServer -o artifacts/publish
    ```
    + Use the `--no-self-contained` option to publish as framework-dependent (add `-p:UseAppHost=false` to only publish DLL without executable).
    + Use the `-p:PublishTrimmed=true` option to publish as self-contained (trimming implicitly enables self-contained).
    + Use the `-p:FsUseEmbeddedStaticFiles=true` option to embed wwwroot static files into the published DLL and serve them from there.
    + Use the `-p:FsPublishSingleFile=true` or `-p:FsPublishAot=true` options to publish as a single-file JIT-compiled executable or as a single-file AOT-compiled executable respectively (they implicitly enable the `FsUseEmbeddedStaticFiles` option, disable IIS web.config generation, and set the appropriate publish mode with embedded(JIT)/removed(AOT) debug symbols). When publishing as a self-contained single-file JIT-compiled executable, additionally use the `-p:EnableCompressionInSingleFile=true` option to significantly reduce its size.
    + Use the `-r <RID>` option to publish for another platform. Some commonly used RIDs: `linux-x64`, `linux-musl-x64`, `osx-arm64`, `win-x64`.

    Examples of commonly used option combinations can be found in the [_commands.txt](_commands.txt) file.

+ ###### 5. Create/prepare the settings file.
    For example, assuming steps 3 and 4 have been strictly followed, copy the template settings file to `appsettings.json` in the publish directory and change:
    ```
    "CertFilePath": "/full/path/to/server_cert/cert.crt",
    "CertKeyPath": "/full/path/to/server_cert/cert.key",
    "DownloadAnonDir": "/full/path/to/fs_data/anonymous_downloads",
    "DownloadDir": "/full/path/to/fs_data/downloads",
    "UploadDir": "C:\\windows_full_path_example\\to\\fs_data\\uploads",
    "SigningKey": "<some string from 20 to 64 symbols>",
    "LoginKey": "<some password, 12 symbols minimum>",
    ```

+ ###### 6. Start the server.
    For example, assuming the working directory is set to the publish directory, and the settings file is in there, to run the server built in step 2:
    + `dotnet FileServer.dll` for cross-platform (framework-dependent).
    + `"./FileServer.exe"` for self-contained when using Windows cmd.
    + `./FileServer.exe` for self-contained when using Windows pwsh.
    + `./FileServer` for self-contained when using Linux/macOS.

    If wwwroot static files are embedded (e.g. it's a single-file or an AOT executable), there's no need to change the working directory to where the executable is and e.g. the `./artifacts/publish/FileServer` command can be used instead (assuming the settings file is in the current directory or its path is specified in the environmental variable).

A practical end-to-end working example of setting up, publishing and running a cross-platform server for local development can be found in the [_setup-server.bat](_setup-server.bat) and [_run-server.bat](_run-server.bat) scripts.

## Additional information
The `master` branch is where the development happens. It may be unstable and/or contain mismatched documentation (e.g. for something not yet released). Use the `stable` branch or one of the `v*` tags (releases) to build from the source code and/or to view the relevant documentation (the `stable` branch always points to the latest release, so it is essentially an analog to the `latest` tag for container images).

Pre-built binaries can be found [here](https://github.com/AndreyTeets/FileServer/releases) in GitHub Releases.\
Pre-built container images can be found [here](https://hub.docker.com/r/andreyteets/fileserver) on Docker Hub, and also in GitHub Releases.\
Changes for each release can be found in the [CHANGELOG.md](CHANGELOG.md) file, and are also duplicated in GitHub Releases.\
Changes for upcoming releases can be found in GitHub Pull requests, filtered by the corresponding Milestone and the `noteworthy` label (e.g. `milestone:2.0.0 label:noteworthy`).

Some clarifications on pre-built binaries, container images and their variants:
+ The `jit-cross-platform` variant of binaries is the result of the standard framework-dependent dotnet publish with the `-p:UseAppHost=false` option. It can only be run using the `dotnet FileServer.dll` command and requires the appropriate dotnet runtime installed.
+ The `jitsf` variant of binaries stands for "JIT single-file".
+ The `aot` variant of pre-built container images is the result of building the main Dockerfile, which is a bit different from the Dockerfile-simple-aot. The latter uses static linking for the libc and OpenSSL libraries and the `scratch` pseudo-image as a base, thus producing an image containing only a single self-sufficient executable, which may be preferable (in which case it should be built from the source code).
+ The `latest` tag for container images published on Docker Hub points to the same image as the `latest-jit` tag.
+ Container images published in GitHub Releases are the result of the `docker save` command, which has then been gzipped. They can be loaded, for example, using the `gunzip < image.tar.gz | docker load` command in the bash shell.

# License
MIT License. See [LICENSE](LICENSE) file.
