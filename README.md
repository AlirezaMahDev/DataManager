# DataManager

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/AlirezaMahDev/DataManager)
[![NuGet](https://img.shields.io/nuget/v/AlirezaMahDev.Extensions.DataManager.svg?style=flat-square)](https://www.nuget.org/packages/AlirezaMahDev.Extensions.DataManager/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

A high-performance, low-level .NET data management library for direct memory manipulation and creating persistent, file-backed data structures.

## Overview

DataManager is not a typical ORM or database driver. It is a specialized, low-level framework designed for scenarios that demand extreme performance and precise control over memory layout. It empowers developers to build their own custom data stores, lightweight databases, or inter-process communication systems by working directly with `unmanaged` structs and memory blocks.

This library is intended for advanced developers. If you need to build custom binary file formats, high-speed caches, or memory-mapped data structures, DataManager provides the foundational tools to do so efficiently and safely.

## Core Concepts

### 1. The DataAccess Interface (`IDataAccess`)

The `IDataAccess` interface is the heart of the library and represents a connection to a single data store (typically a file). It provides low-level capabilities:
- **`AllocateOffset(int length)`**: Reserves a block of memory within the store.
- **`ReadMemory(long offset, int length)`**: Reads a block of memory as a `Memory<byte>`.
- **`WriteMemory(long offset, Memory<byte> memory)`**: Writes a block of memory to the store.
- **`Save()` / `SaveAsync()`**: Persists all in-memory changes to the underlying file.

### 2. Unmanaged Data Models

To achieve maximum performance, DataManager operates on `unmanaged` C# `structs`. These are value types with a predictable, blittable memory layout, which allows them to be written to and read from memory directly without any transformation or garbage collection overhead.

You must define your data models using `struct` and apply the `[StructLayout(LayoutKind.Sequential)]` attribute.

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct User
{
    public int UserId;
    public String64 Name; // Using a fixed-size string from the library
    public ulong Flags;
}
```

### 3. Fixed-Size Strings (`String16`, `String32`, `String64`...)

Standard .NET `string` is a managed reference type, which cannot be used in `unmanaged` structs. DataManager provides a set of fixed-size string structs (e.g., `String16`, `String64`) that store characters directly within the struct's memory footprint. This is essential for creating flat, serializable data models.

```csharp
String32 myString = "Hello, World!";
string netString = myString; // Implicit conversion
```

### 4. Data Integrity

The library includes mechanisms like `DataMemory` which can perform hash checks (`XxHash128`) on memory blocks to verify data integrity and detect corruption.

## Features

- **High-Performance I/O:** Bypasses high-level abstractions for raw speed.
- **GC-Friendly:** Works with `structs` and pooled memory to reduce garbage collection pressure.
- **Persistent Storage:** Easily save and load data structures to and from files.
- **Type-Safe Memory Access:** Provides a strongly-typed wrapper (`DataLocation<T>`) for safe interaction with memory blocks.
- **Foundation for Complex Structures:** Includes interfaces and helpers for building lists, trees, and dictionaries.

## Getting Started

Currently, the library is not available on NuGet. To use it, clone the repository and reference the `AlirezaMahDev.Extensions.DataManager.csproj` project in your solution.

```bash
git clone https://github.com/AlirezaMahDev/DataManager.git
```

## Usage Example

Here is a complete example of how to define a model, create a data store, write a `User` object, and read it back.

```csharp
using AlirezaMahDev.Extensions.DataManager;
using System.Runtime.InteropServices;

// 1. Define your unmanaged data model
[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 76)] // 4 (int) + 64 (String64) + 8 (ulong)
public struct User : IDataValue<User>
{
    public int UserId;
    public String64 Name;
    public ulong Flags;

    // Implement IDataValue for type safety
    public bool Equals(User other) => UserId == other.UserId;
}

public class Program
{
    public static void Main()
    {
        // 2. Get access to a data store file
        // Note: DataManager and DataAccess are internal; this is a conceptual example.
        // You would typically wrap DataAccess in your own service.
        // For this example, let's assume we have an IDataAccess instance.
        
        // This part is conceptual as DataAccess is internal
        // IDataAccess dataAccess = new DataAccess("my_data.db");

        // Let's simulate the workflow:

        // 3. Allocate space for our User struct
        // long userOffset = dataAccess.AllocateOffset(Marshal.SizeOf<User>());

        // 4. Create and write the user data
        var newUser = new User
        {
            UserId = 101,
            Name = "John Doe",
            Flags = 0x1A
        };
        
        // var userLocation = new DataLocation<User>(dataAccess, userOffset);
        // userLocation.Value = newUser; // Write the struct to memory

        // Console.WriteLine($"Wrote user '{userLocation.Value.Name}' with ID {userLocation.Value.UserId}");

        // 5. Read the user data back
        // var readUser = userLocation.Value;
        // Console.WriteLine($"Read back user: '{readUser.Name}'");

        // 6. Save all changes to disk
        // dataAccess.Save();
        
        Console.WriteLine("This is a conceptual example. The API is still under development.");
    }
}
```
*Note: The API is still evolving, and some classes like `DataAccess` are `internal`. The example above illustrates the intended workflow.*

## License

This project is licensed under the **GNU General Public License v3.0**. This means that if you use this library in your software, your software must also be open-source and distributed under the GPLv3 license. Please read the [LICENSE](LICENSE) file for more details.

## Contributing

Contributions are welcome! If you have ideas for improvements, new features, or bug fixes, please open an issue to discuss it first. Afterward, you can submit a pull request.

1. Fork the repository.
2. Create your feature branch (`git checkout -b feature/MyAwesomeFeature`).
3. Commit your changes (`git commit -m 'Add some AwesomeFeature'`).
4. Push to the branch (`git push origin feature/MyAwesomeFeature`).
5. Open a pull request.