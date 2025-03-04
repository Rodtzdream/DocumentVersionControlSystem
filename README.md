# Document Version Control System (0.9.0-beta)

A desktop application for managing document versions, built with .NET 8 and WPF. Features include versioning, external editing, adaptive UI scaling, and security mechanisms to prevent external tampering. The application is powered by SQLite, EF Core, DiffPlex, and Serilog. Please note that the search and differencing features are not implemented for this version.

## Features

- **Internal File System (UI-based)**: Documents are managed within the application, offering a structured way to handle versions.
- **External Editing**: Documents open in the user's preferred editor, ensuring seamless workflow integration.
- **Versioning**: Full copies of each document version are stored, allowing easy restoration.
- **Two Methods for Version Switching**:
  - *Switch to this version and delete newer versions*
  - *Switch to this version and save it as the latest version*
- **Security Mechanisms**: The system detects external modifications to original files and prevents unintended changes.
- **Adaptive UI Scaling**: The interface dynamically adjusts to different display settings for better usability.
- **Error Handling**: Robust exception handling ensures stability and logs critical issues.
- **Logging**: Powered by Serilog, all significant actions are recorded for debugging and auditing.

## Installation Instructions for Windows

1. **Download the Archive:**
   - Go to the [Releases page](https://github.com/Rodtzdream/DocumentVersionControlSystem/releases) and download the `0.9.0-beta` archive containing the executable file.

2. **Extract the Archive:**
   - Locate the downloaded archive file and extract its contents to a folder of your choice.

3. **Run the Application:**
   - Navigate to the `/bin/Release/net8.0-windows` and double-click on `DocumentVersionControlSystem.exe` file to launch the application.

## Additional Scripts

Scripts are located in the `scripts/` folder:

- `clean_logs.bat` – Deletes old log files.
- `uninstall.bat` – Removes all application files.

## Testing Instructions

1. **Functional Testing:**
   - Test all implemented features including internal file system, versioning and external editing.
   - Verify that the application logs are generated correctly in `%LocalAppData%\DocumentVersionControlSystem\log.txt`.

2. **Performance Testing:**
   - Evaluate the application's performance with different sizes and types of documents.
   - Check the application's responsiveness during heavy usage.

3. **Bug Reporting:**
   - Report any bugs or issues on the [Issues page](https://github.com/Rodtzdream/DocumentVersionControlSystem/issues).
   - Provide detailed information including steps to reproduce, screenshots, and log files if applicable.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Acknowledgements

- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [DiffPlex](https://github.com/mmanela/diffplex)
- [Serilog](https://serilog.net/)
