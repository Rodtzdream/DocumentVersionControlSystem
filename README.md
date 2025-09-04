# DocumentVersionControlSystem

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET version](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
[![Last Release](https://img.shields.io/github/v/release/rodtzdream/DocumentVersionControlSystem?label=Last%20Release&color=blue)](https://github.com/Rodtzdream/DocumentVersionControlSystem/releases/tag/v0.9.0-beta)
## Table of Contents
- [About the Project](#about-the-project)
- [Features](#features)
- [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)
- [Configuration](#configuration)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## About the Project
A desktop application for managing document versions, built with .NET 8 and WPF. It allows users to manage original documents, track changes, and revert to previous versions seamlessly. The application supports opening documents in external editors and provides a user-friendly interface for version control.

## Features
- **Internal File System (UI-based):** Documents are managed within the application, offering a structured way to handle versions.
- **External Editing:** Documents open in the user's preferred editor, ensuring seamless workflow integration.
- **Versioning:** Full copies of each document version are stored, allowing easy restoration.
- **Two Methods for Version Switching:**
  - Switch to this version and delete newer versions
  - Switch to this version and save it as the latest version
- **Security Mechanisms:** The system detects external modifications to original files and prevents unintended changes.
- **Adaptive UI Scaling:** The interface dynamically adjusts to different display settings for better usability.
- **Error Handling:** Robust exception handling ensures stability and logs critical issues.
- **Logging:** Powered by Serilog, all significant actions are recorded for debugging and auditing.

## Getting Started
### Prerequisites
- **.NET 8 Runtime:** Ensure that the .NET 8 runtime is installed on your machine. You can download it from the [.NET official website](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
- **Windows OS:** This application is designed to run on Windows operating systems.
- **Text Editor:** Ensure you have a text editor installed (e.g., Notepad, VSCode, etc.) for editing documents.

### Installation
1. **Download the Archive:**
  - Go to the [Releases page](https://github.com/Rodtzdream/DocumentVersionControlSystem/releases) and download the latest archive containing the executable file.
2. **Extract the Archive:**
  - Locate the downloaded archive file and extract its contents to a folder of your choice.
3. **Run the Application:**
  - Navigate to the `/bin/Release/net8.0-windows` and double-click on `DocumentVersionControlSystem.exe` file to launch the application.

## Configuration
Scripts are located in the `scripts/` folder:

- `clean_logs.bat` – Deletes old log files.
- `uninstall.bat` – Removes all application files.

## Roadmap
- **v1.0.0:** Initial stable release with core features.
- **Future Plans:**
  - Editing documents within the application.
  - Folder structure for document organization.
  - Project management features.
  - Search / Sort functionality.
  - File comparison tools.
  - Settings menu.
  - Better UI/UX improvements.
  - And more...

## Contributing
Contributions are welcome! Please fork the repository and create a pull request with your changes. Ensure to follow the coding standards and include tests for new features.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details

## Contact
- **GitHub:** [Rodtzdream](https://github.com/Rodtzdream)
- **Email:** <olijnikura@gmail.com>
- **LinkedIn:** [Yurii Oliinyk](https://www.linkedin.com/in/yurii-oliinyk-a3b891292/)
