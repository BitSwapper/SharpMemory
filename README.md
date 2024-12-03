# SharpMemory
A lightweight utility for external hacking in C# on Windows


Current Capabilities:
Reading / Writing memory of another process.
Module scanning / retrieval.
Pattern scanning.
Multi level pointer support.

Define Addresses with the "Address" type (not int, long, intptr, etc.)
Define Pointers with the "MemoryPointer<T>" type (use the MemoryPointer.AddressBeingPointedTo property to resolve a given pointer)

Basic usage example depicting reading, writing, and pattern scanning.
https://github.com/SleepyHex/SharpMemory/releases/tag/1.1
