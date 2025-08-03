# RevisionControl

A lightweight, user-friendly Revit add-in for bulk managing sheet revisions.  
**RevisionControl** lets you assign or remove revisions on multiple sheets at once, with a modern WPF interface and Material Design look.

---

## Features

- **View all sheets** in the project, including current revision status.
- **Multi-select sheets** via checkboxes, with “Select All” and “Deselect All” buttons.
- **Browse all Revit revisions** in a ComboBox (shows revision number and description).
- **Apply revision**: Add a revision to all selected sheets in one click.
- **Remove revision**: Remove a revision from all selected sheets in one click.
- **Material Design UI** with dark mode toggle.
- Designed to be **safe** (uses External Events for Revit API access).

---

## Getting Started

### Prerequisites

- **Revit 2022+**
- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)
- [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) (already referenced in the project)
- [ricaun.Revit.UI](https://github.com/ricaun-io/Revit-UI)

### Installation

1. **Build the solution** in Visual Studio.
2. Copy the output `.dll` and `.addin` file to your Revit Addins folder:  
   `C:\ProgramData\Autodesk\Revit\Addins\2024\`
3. Start Revit. You’ll find the **RevisionControl** panel under the **"RK Tools"** ribbon tab.

### Usage

1. **Launch RevisionControl** from the “RK Tools” tab.
2. **Select sheets** you wish to modify.  
   Use the checkboxes, or click “Select All”.
3. **Pick a revision** from the ComboBox.
4. Click **Apply** to assign, or **Remove** to remove, the revision from selected sheets.
5. (Optional) Toggle between dark and light themes with the theme switch.

### Uninstallation

- Delete the `.dll` and `.addin` files from your Revit Addins folder.

---

## Developer Notes

- Uses [Revit External Events](https://www.revitapidocs.com/2024/daea087c-cf8e-d681-9120-42ff4b9ebf3d.htm) for all Revit API write operations.
- All revision changes are done **in addition** (the “current revision” is set by Revit rules: highest issued sequence).
- All feedback is provided in the Revit UI thread (safe for Revit API use).

---

## Contributing

PRs and issues are welcome!  
If you want to extend the UI or logic, please follow C# best practices and keep the code readable.

---

## License

MIT License.  
See [LICENSE](LICENSE) for details.

---

## Credits

- UI powered by [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- Ribbon integration via [ricaun.Revit.UI](https://github.com/ricaun-io/Revit-UI)
