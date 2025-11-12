# FileServer
Is a minimalistic https file server with a simple password-only authentication. It provides a simple, secure, universal, cross-platform method to transfer files over network (or just view them in text mode in a browser).

It's useful, for example, to transfer files over LAN directly between 2 devices if enabling/installing things like SSH, Samba, FTP, Windows folder sharing, e.t.c. on them is undesired. Also mobile devices often miss the client tools necessary to use those, but they do have a browser.

It's implemented as a dotnet AspNetCore server with a plain JavaScript single page mini-site (which basically consists of only several buttons). Server code base is also very small - about 10 files containing actual logic around 100 lines each, half of which are related to startup/settings/logging. There are no dependencies except for aspnetcore runtime, which is included when published as self-contained.

## What can it do?
It starts an https server at the specified `listen address` and `listen port` using the provided `server certificate`. The server hosts a simple single page application (SPA) which provides a browser UI to:
+ Login using the `login key` (password) specified in server settings.
+ Logout.
+ Show the list of files in the `anonymous downloads folder` specified in server settings.
+ Download or view in text mode any file in the `anonymous downloads folder` specified in server settings.
+ Show the list of files in the `downloads folder` specified in server settings (only if authorized).
+ Download or view in text mode any file in the `downloads folder` specified in server settings (only if authorized).
+ Upload a file to the `uploads folder` specified in server settings (only if authorized).

## Nuances
+ There are no file size restrictions for downloading and uploading.
+ Uploaded files names are sanitized before the server saves them: any characters except the whitelisted (which are any ASCII letter, any digit, space and any of ``` !#$%&'()+,-.;=@[]^_`{}~ ```) are replaced with `_`, then the name is shortened to the first 120 symbols, and then the result is prefixed and postfixed with "upl." and ".oad" respectively.
+ The server won't accept an upload if a file with the same (sanitized) name already exists.
+ Some browsers on mobile devices (e.g. Safari on IPhone/IPad) ignore "Content-Type" header and may try to interpret the content based on the file name extension, which may lead to "Download" button viewing the file or "View" button downloading the file. This can be solved by renaming the file on the server to a name without extension.

## Authentication system
To authenticate an incoming request that requires authorization, the server expects a client to provide 2 tokens: an auth token in the request http-only cookie and an antiforgery token in the request header/query-parameter. The antiforgery token is kept by the SPA client in a browser local storage (which is scoped to protocol://host:port) and the cookie is managed by the browser. Both tokens have to be a HMACSHA256-signed-claim for [user name, corresponding token type, expiration time]. Authentication succeeds only if both tokens are valid (that is, they are signed by the server `signing key` and are not expired) and have the same user name (the server issues tokens during a login operation only for one hardcoded `Main` user).

For a login operation to succeed, the server expects a client to provide a login request with the same password as the server `login key`. The auth token ensures that the client has performed a successful login operation (and thus knows the `login key`). And the antiforgery token ensures that the incoming request was initiated by the SPA client site (protocol://host:port). The latter is necessary because browsers may automatically send cookies to the target site even if the request was initiated by another site.

To perform a logout operation the SPA client sends a logout request to the server which simply responds with "Set-Cookie token=empty;expired" header which is handled by the browser. Then the SPA client removes the antiforgery token from the browser local storage. As a consequence it's still possible to access the server with those tokens until they expire (for example, if they got intercepted or got saved somewhere else or got not deleted). This shouldn't be a problem if `tokens ttl` is configured short. To completely invalidate all issued tokens, the server `signing key` has to be changed.

As for authorization - the server requires the authenticated user name to be `Main`, but it's kind of redundant since the server doesn't issue tokens for any other users.

## Configuration
Server settings are configured using the `appsettings.json` file. The file must exist at the path specified in the `FileServer_SettingsFilePath` environmental variable (which is by default set to `/app/settings/appsettings.json` in the container image) or in the working directory of the application if this environmental variable is unset. The file is hot-reloaded on change (including the logging configuration section).

It's also possible to override the settings specified in the file using environmental variables. For example, setting `FileServer__Settings__ListenAddress` environmental variable will override `Settings.ListenAddress` specified in the `appsettings.json` file.

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
+ To build the container image and/or run it - docker or an alternative container engine (e.g. podman).
+ To build (publish) a dotnet application - dotnet sdk 10.0 or higher.
+ To run a portable dotnet application - aspnetcore runtime 10.0.
+ To run a self-contained dotnet application on linux - dotnet runtime dependencies.
+ To run a self-contained dotnet application on windows - nothing.

## Usage (container image)
+ ###### 1. Get the source code.
    For example, to get the latest stable sources:
    ```
    git clone -b stable https://github.com/AndreyTeets/FileServer.git
    cd FileServer
    ```

+ ###### 2. Build the container image.
    For example, using docker:
    ```
    docker build . -f ./FileServer/Dockerfile-simple -t my_file_server:latest
    ```

+ ###### 3. Create/prepare the `server certificate` in PEM format.
    For example, to create a self signed certificate:
    ```
    mkdir server_cert
    openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes -keyout server_cert/cert.key -out server_cert/cert.crt -subj "/CN=localhost"
    ```
    Note: An HTTPS connection still provides encryption even if the certificate is untrusted by the browser (e.g. if it's self-signed). But to ensure that the connection is not intercepted, the certificate must still be verified by other means, such as manually via browser "view certificate" menu and comparing its fingerprints with those printed in the server logs during startup.

+ ###### 4. Create/prepare the `anonymous downloads folder`, `downloads folder`, `uploads folder`.
    For example:
    ```
    mkdir fs_data
    mkdir fs_data/anonymous_downloads
    mkdir fs_data/downloads
    mkdir fs_data/uploads
    ```
    On linux set up permissions if necessary.

+ ###### 5. Create/prepare the settings file.
    Template settings file can be found here [FileServer/appsettings.template.json](FileServer/appsettings.template.json). At a bare minimum the `signing key` and `login key` have to be changed, as they are empty in the template and the server will refuse to start with invalid settings.

    The next step in this example assumes that the settings file is located in the current directory with the name `appsettings.json` and that the settings are set to:
    ```
    "CertFilePath": "/server_cert/cert.crt",
    "CertKeyPath": "/server_cert/cert.key",
    "SigningKey": "<some string from 20 to 64 symbols>",
    "LoginKey": "<some password, 12 symbols minimum>",
    ```
    with the rest left as is in the template.

+ ###### 6. Start the server.
    For example, using docker:
    ```
    docker run --rm -it --pull=never --name my_file_server \
        --security-opt no-new-privileges \
        -p 8443:8443/tcp \
        -e "FileServer_SettingsFilePath=/app/appsettings.json" \
        -v "`pwd`/appsettings.json:/app/appsettings.json:ro" \
        -v "`pwd`/server_cert:/server_cert:ro" \
        -v "`pwd`/fs_data:/fs_data" \
        my_file_server:latest
    ```
    + Replace ``` `pwd` ``` with `%cd%` or `$pwd` when using windows cmd or pwsh respectively.
    + Replace `\` with `^` or ``` ` ``` when using windows cmd or pwsh respectively.
    + To run in non-interactive (detached) mode use `-d --restart=unless-stopped` instead of `--rm -it`.

    Permissions:
    + To avoid confusion: "root-ness outside the container" is referred to as "rootful mode"/"rootless mode", and "root-ness inside the container" as "root user"/"non-root user".
    + The container image doesn't set any custom users and will run as the root user unless it's explicitly specified in the run command otherwise.
    + The server does not require root permissions and may run as any user UID. The only requirement is for it to have read access to the settings file, all the files and directories specified in settings, and, if upload functionality is to be used, write access to the uploads directory (from the perspective of the user running inside the container). Neither does it require any capabilities and `--cap-drop=all` option can be added if the previous condition is met.
    + When running as the root user in rootful mode (which docker by default does), uploaded files will be owned by the root user on the host, which is at least inconvenient. It also poses additional security risks. So it is highly recommended to add `-u $(id -u):$(id -g)` option, which will make uploaded files being owned by the same user who launched the container.
    + When running as the root user in rootless mode (which e.g. podman by default does), there may be no problem with uploaded files ownership (e.g. with podman, there isn't, as it by default maps the host user who launched the container to the root user inside the container). But changing to a non-root user is still recommended to reduce security risks. For podman it can be done using `--userns=keep-id` option, which will change the previously mentioned mapping to the same UID inside the container as on host, or `--userns=keep-id:uid=12345,gid=12345` option, which will change it to the specified UID.
    + On systems with SELinux it may be necessary to add `:z` to the volume mount options (with `!care!`, as it will recursively relabel the mapped directory on the host, which may break the host system if used on directories that are used elsewhere besides the container). In case of relabeling, uploaded files will have "system_u:object_r:container_file_t:s0" context. To restore default context `restorecon -RFv /path/to/fs_data` can be used. If relabeling is an issue and has to be avoided `--security-opt label=disable` option can be added instead (which will basically disable SELinux for the containerized process).

+ ###### 7. Open the corresponding tcp port in firewall if necessary.

+ ###### 8. Visit the site using a browser.
    In this example to do it from the same machine the address will be https://127.0.0.1:8443 or https://localhost:8443

## Usage (dotnet application directly)
Note: This is not recommended as it does not provide an extra layer of security through file-system and process isolation like containers do, and it is also much easier to misconfigure.

Replace these steps from the container image usage example:
+ ###### Step 2.
    ```
    dotnet publish "FileServer/FileServer.csproj" -c Release -o FileServer/bin/publish
    ```
    Add `-r <RID> --self-contained` to publish as self-contained.

    Commonly used RIDs: `win-x64`, `linux-x64`, `linux-musl-x64`.

+ ###### Step 5.
    Place the `appsettings.json` file from the container image usage example to FileServer/bin/publish and set correct full paths for:
    ```
    "CertFilePath": "/full/path/to/server_cert/cert.crt",
    "CertKeyPath": "/full/path/to/server_cert/cert.key",
    "DownloadAnonDir": "/full/path/to/fs_data/anonymous_downloads",
    "DownloadDir": "/full/path/to/fs_data/downloads",
    "UploadDir": "C:\\windows_path_example\\to\\fs_data\\uploads",
    ```

+ ###### Step 6.
    `cd FileServer/bin/publish` (current working directory is `!important!`).
    + `dotnet ./FileServer.dll` for portable (framework-dependent cross-platform).
    + `"./FileServer.exe"` for self-contained when using windows cmd.
    + `./FileServer.exe` for self-contained when using windows pwsh.
    + `./FileServer` for self-contained when using linux.

A practical end-to-end working example of setting up, publishing and running a portable server for local development can be found in [_setup-server.bat](_setup-server.bat) and [_run-server.bat](_run-server.bat) scripts.

## Additional information
The `master` branch is where the development happens, it may be unstable. Use the `stable` branch or one of the `v*` tags (releases) to build from the source code (the `stable` branch always points to the latest release, so it is essentially an analog to the "latest" tag for container images).

Pre-built container images can be found [here](https://hub.docker.com/r/andreyteets/fileserver) on Docker Hub.\
Pre-built binaries can be found [here](https://github.com/AndreyTeets/FileServer/releases) in GitHub Releases.

Changelog for each release can be found in [CHANGELOG.md](CHANGELOG.md) file, and is also duplicated in GitHub Releases.

# License
MIT License. See [LICENSE](LICENSE) file.
