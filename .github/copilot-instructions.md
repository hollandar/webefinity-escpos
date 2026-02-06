# Copilot Instructions for webefinity-escpos

## Repository Overview

This is a modern .NET library for controlling ESC/POS thermal printers. The library provides a comprehensive API for formatting receipts, printing images, barcodes, QR codes, and managing printer connections over network (TCP/IP), serial port, or file-based outputs.

**Repository Type:** .NET Class Library with Console Application Example  
**Primary Language:** C# (.NET 10.0)  
**Target Framework:** .NET 10.0  
**Repository Size:** ~15 files across 4 main projects  
**Architecture:** Multi-project solution with clean separation between commands, printers, console demo, and tests

## Project Structure

### Main Projects

1. **EscPos.Commands** (`EscPos.Commands/`)
   - Core ESC/POS command generation library
   - Contains `PrintCommands` class with low-level printer command methods
   - XML template system with `ReceiptXmlParser` for declarative receipt generation
   - `ReceiptTemplateContext` for variable substitution in templates
   - `PrintBuffer` for building command sequences
   - Schema validation via `ReceiptSchema.xsd`

2. **EscPos.Printers** (`EscPos.Printers/`)
   - Printer connection implementations
   - `IPPrinter` for network/TCP connections
   - `SerialPrinter` for serial port connections
   - `FilePrinter` for file-based output (testing)
   - `StatusHelper` for querying printer status

3. **EscPos.Console** (`EscPos.Console/`)
   - Example console application demonstrating library usage
   - Shows text formatting, image printing, QR codes, and status checking

4. **EscPos.UnitTests** (`EscPos.UnitTests/`)
   - Comprehensive test suite with 244 tests using xUnit
   - Tests for all command generation, XML parsing, and template substitution

### Key Files in Repository Root

- `README.md` - Comprehensive documentation with examples
- `ReceiptXmlParser.md` - XML template system guide
- `ReceiptTemplateVariables.md` - Template variable syntax documentation
- `ReceiptConditionals.md` - Conditional rendering guide
- `ReceiptLoops.md` - Loop iteration documentation
- `webefinity-escpos.slnx` - Solution file
- `.gitignore` - Standard Visual Studio .NET gitignore

### Configuration Files

- `.github/workflows/dotnet.yml` - CI/CD workflow for build and test
- `EscPos.Commands/ReceiptSchema.xsd` - XML schema for receipt validation

## Build & Test Instructions

### Prerequisites

- **.NET 10.0 SDK** (confirmed version: 10.0.102)
- No external dependencies required (pure .NET implementation)
- For serial printer testing: Windows/Linux/macOS with serial port access

### Build Commands (ALWAYS run in sequence)

1. **Restore dependencies** (always run first after cloning or pulling changes):
   ```bash
   dotnet restore
   ```
   - Takes ~2-3 seconds
   - Restores all 4 projects
   - **Must be run before any build or test commands**

2. **Build the solution**:
   ```bash
   dotnet build
   ```
   - Takes ~10 seconds
   - Builds all projects in dependency order
   - Outputs to `bin/Debug/net10.0/` in each project directory
   - **No warnings or errors expected in clean build**

3. **Alternative: Build without restore** (only if restore was just run):
   ```bash
   dotnet build --no-restore
   ```

### Testing Commands

1. **Run all tests**:
   ```bash
   dotnet test
   ```
   - Takes ~4-5 seconds
   - Runs all 244 unit tests
   - **All tests should pass** - 244 passed, 0 failed expected
   - Uses xUnit framework

2. **Run tests with verbose output**:
   ```bash
   dotnet test --verbosity normal
   ```

3. **Run tests without rebuild** (if just built):
   ```bash
   dotnet test --no-build --verbosity normal
   ```

### Running the Console Example

```bash
dotnet run --project EscPos.Console/EscPos.Console.csproj
```
- Demonstrates library features
- Does not require actual printer hardware (uses FilePrinter for testing)

### Validated Command Sequences

**Standard development workflow:**
```bash
dotnet restore && dotnet build && dotnet test
```

**Quick iteration after changes:**
```bash
dotnet build && dotnet test --no-build
```

## CI/CD Pipeline

### GitHub Actions Workflow (`.github/workflows/dotnet.yml`)

**Triggers:**
- Push to `master` branch
- Pull requests targeting `master` branch

**Steps:**
1. Checkout code (`actions/checkout@v4`)
2. Setup .NET 10.0.x (`actions/setup-dotnet@v4`)
3. Restore dependencies (`dotnet restore`)
4. Build solution (`dotnet build --no-restore`)
5. Run tests (`dotnet test --no-build --verbosity normal`)

**Expected outcome:** All steps must succeed for PR approval.

## Code Style & Conventions

### Naming Conventions
- Public API methods use PascalCase (e.g., `PrintCommands.AlignCenter()`)
- Async methods end with `Async` suffix (e.g., `ConnectAsync()`, `SendAsync()`)
- Private fields use camelCase with underscore prefix (follow existing patterns)

### Code Organization
- Commands are static methods in `PrintCommands` class
- Each command returns `byte[]`
- Use `PrintBuffer` for building command sequences
- XML elements map to command methods

### Testing Practices
- Test class names end with `Tests` (e.g., `ReceiptXmlParserTests`)
- Test method names use pattern: `MethodName_Scenario_ExpectedBehavior`
- Use xUnit attributes: `[Fact]`, `[Theory]`, `[InlineData]`
- Tests validate exact byte sequences for ESC/POS commands

## Common Patterns

### Adding New ESC/POS Commands

1. Add static method to `PrintCommands` class in `EscPos.Commands/PrintCommands.cs`
2. Method should return `byte[]` with ESC/POS command sequence
3. Add XML element support in `ReceiptXmlParser.cs` if applicable
4. Update `ReceiptSchema.xsd` for XML validation
5. Add unit tests in `EscPos.UnitTests/`

### Adding XML Template Features

1. Update `ReceiptXmlParser.cs` to handle new element
2. Modify `ReceiptSchema.xsd` to include new schema definition
3. Add tests in `ReceiptXmlParserTests.cs`
4. Document in appropriate markdown file (ReceiptXmlParser.md, etc.)

## Important Notes for Code Changes

### Build Requirements
- **ALWAYS run `dotnet restore`** before building after dependency changes
- Build time is approximately 10 seconds - if it times out, increase wait time
- Test execution takes 4-5 seconds for full suite

### Testing Requirements
- All 244 tests must pass before submitting changes
- Add tests for any new commands or features
- Validate byte sequences match ESC/POS specification

### Dependencies
- **Zero external dependencies** - maintain this principle
- No image processing libraries required (custom BMP parsing implemented)
- Uses only built-in .NET libraries

### Breaking Changes to Avoid
- Don't change public API signatures in `PrintCommands`
- Don't modify `ReceiptSchema.xsd` in breaking ways
- Maintain backward compatibility with XML templates

### Known Patterns
- Use `using var buffer = new PrintBuffer()` for building commands
- Printer connections implement `IPrinter` interface
- All printers support async operations
- Status queries return enumerations of status flags

## XML Template System

The library includes a declarative XML template system for receipt generation:

- Template variables use `${variable}` syntax
- Supports nested properties: `${Order.Customer.Name}`
- `<if>` elements for conditional rendering
- `<for>` loops for iterating collections
- All ESC/POS commands available as XML elements
- Schema validation via `ReceiptSchema.xsd`

See dedicated documentation files for details:
- `ReceiptXmlParser.md` - Main guide
- `ReceiptTemplateVariables.md` - Variable substitution
- `ReceiptConditionals.md` - Conditional logic
- `ReceiptLoops.md` - Iteration patterns

## Supported ESC/POS Features

- Text formatting: bold, underline, invert, font selection, character sizing
- Alignment: left, center, right
- Image printing: BMP files (1/4/8/16/24/32-bit, RLE compression support)
- Barcodes: various formats with height/width/HRI position control
- QR codes: Model 2 with size and error correction configuration
- Paper control: line feeds, form feeds, full/partial cut
- Cash drawer: pulse commands
- Status queries: paper, drawer, printer state
- Internationalization: code pages, character sets

## Important Reminders

- **Trust these build instructions** - they have been validated and work correctly
- Search the codebase only if instructions are incomplete or found to be incorrect
- The CI pipeline matches local build commands exactly
- All code uses .NET 10.0 - don't target earlier frameworks
- Repository follows standard .NET project structure
- No Docker, containers, or special environment setup required
- Serial printer code requires OS-level serial port support but builds/tests run without hardware
