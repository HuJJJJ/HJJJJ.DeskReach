# libjpeg-turbo 3.2.0 native dependency

This software is based in part on the work of the Independent JPEG Group.

Unmodified TurboJPEG DLLs from the upstream release:
https://github.com/libjpeg-turbo/libjpeg-turbo/releases/tag/3.2.0

The DLL matching the process architecture is loaded from the application's
`runtimes/win-x64/native` or `runtimes/win-x86/native` directory.

| Artifact | SHA-256 |
|---|---|
| libjpeg-turbo-3.2.0-vc-x64.exe | 662761d8ba8dae04aec74023ebaeceb856c2b56b9b59cfd180759d26300dda42 |
| libjpeg-turbo-3.2.0-vc-x86.exe | 86806741d743dbd53a889ff721f2274cd34bc41b4dd40500902df8d99332153e |
| win-x64/native/turbojpeg.dll | 3fbbfc901b08dc376adbb3d7edd46415a6dbc599abc5ae256a7c30384b61b22a |
| win-x86/native/turbojpeg.dll | 2e343eda2118d8151185c266ac84c4ea0e6385a0ce2521c1e35bf286c9d8eb82 |

Installer checksums were verified against the upstream release asset listing
before extracting `bin/turbojpeg.dll`; installers were not executed.

`LICENSE.md`, `README.ijg`, and `README.md` in this directory are upstream
documents shipped with the release. These files and this attribution are
copied into the application's output directory alongside the native DLLs.
