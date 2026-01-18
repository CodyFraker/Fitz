# mise Setup Guide

This project uses [mise](https://mise.jdx.dev/) (formerly rtx) for tool version management. mise automatically installs and manages the correct versions of Node.js and npm for this project.

## What is mise?

mise is a tool version manager that:
- Automatically installs the correct Node.js version (20.x)
- Manages npm versions
- Ensures consistent tooling across all developers
- Works on Windows, Mac, and Linux

## Installation

### Windows

**Option 1: Using winget**
```powershell
winget install jdx.mise
```

**Option 2: Using Scoop**
```powershell
scoop install mise
```

**Option 3: Using Chocolatey**
```powershell
choco install mise
```

### Mac

**Using Homebrew:**
```bash
brew install mise
```

### Linux

**Using the install script:**
```bash
curl https://mise.run | sh
```

Or see https://mise.jdx.dev/getting-started.html for other installation methods.

## Setup

After installing mise, you need to activate it in your shell. There are two ways:

### Option 1: Quick Activation (Current Session Only)

Run this in your PowerShell terminal:
```powershell
mise activate pwsh | Out-String | Invoke-Expression
```

Or use the helper script:
```powershell
cd Fitz.Web
.\activate-mise.ps1
```

### Option 2: Permanent Activation (Recommended)

To make mise activate automatically in all future PowerShell sessions:

**Method A: Use the setup script**
```powershell
cd Fitz.Web
.\setup-mise-permanent.ps1
```

**Method B: Manual setup**
1. Open your PowerShell profile:
   ```powershell
   notepad $PROFILE
   ```
2. Add this line:
   ```powershell
   mise activate pwsh | Out-String | Invoke-Expression
   ```
3. Save and restart PowerShell

After activation, verify it works:
```powershell
node --version
npm --version
```

### Bash/Zsh (Mac/Linux)
Add to your `~/.bashrc` or `~/.zshrc`:
```bash
eval "$(mise activate bash)"  # for bash
eval "$(mise activate zsh)"   # for zsh
```

## Using mise in this Project

1. Navigate to the project directory:
   ```bash
   cd Fitz.Web
   ```

2. Install the tools specified in `.mise.toml`:
   ```bash
   mise install
   ```
   This will automatically install Node.js 20 and the latest npm.

3. Verify installation:
   ```bash
   node --version  # Should show v20.x.x
   npm --version   # Should show latest npm version
   ```

4. Now you can use npm normally:
   ```bash
   npm install
   npm run dev
   ```

## How it Works

- `.mise.toml` specifies which tools and versions this project needs
- When you run `mise install`, it installs those tools
- mise automatically activates the correct versions when you're in the project directory
- No need to manually manage Node.js versions or PATH variables

## Benefits

- ✅ Consistent Node.js versions across all developers
- ✅ Automatic tool installation
- ✅ No manual PATH configuration needed
- ✅ Works seamlessly with existing npm scripts
- ✅ Supports multiple projects with different Node.js versions

## Troubleshooting

### mise command not found
- Make sure mise is installed and in your PATH
- Restart your terminal after installation
- Run the activation command for your shell

### Tools not installing
- Check your internet connection
- Try `mise install --verbose` for more details
- Ensure you have write permissions in the mise directory

### Node.js version mismatch
- Run `mise install` again to ensure the correct version is installed
- Check `.mise.toml` for the specified version

## More Information

- mise documentation: https://mise.jdx.dev/
- mise GitHub: https://github.com/jdx/mise
