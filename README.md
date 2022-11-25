# SharpMemory
A lightweight utility for external hacking in C# on Windows

Basic usage example depicting reading, writing, and pattern scanning.

FIRST: in your solution explorer, right click Dependencies and add a project references to the correct SharpMemory dll

'''
using System.Diagnostics;
using SharpMemory;
using SharpMemory.Native;

string GameProcessName = "CoDWaW.exe";
Address healthAddress = 0xDEADBEEF;
Address moneyAddress = 0xFADEDBEE;
var mem = SharpMem.Inst;
var desiredAccessLevel = NativeData.ProcessAccessFlags.VMRead | NativeData.ProcessAccessFlags.VMWrite | NativeData.ProcessAccessFlags.VMOperation;

//YOU MUST CALL THIS INITIALIZE FUNCTION BEFORE ATTEMPTING TO USE ANY OF SHARPMEMS FUNCTIONALITY
mem.Initialize(GameProcessName, desiredAccessLevel);


//Reading a variable default way
int healthValue = mem.ReadFuncs.Read<int>(healthAddress);
//Reading a variable via Address extension
float moneyValue = healthAddress.Read<float>();


//writing an address default way
mem.WriteFuncs.Write<float>(moneyAddress, 99999999f);
//writing an address via extension
moneyAddress.Write(99999999f);
//the above line implicty assumes it is writing a float because of the F at the end of the number. If we want to explicity tell it we do it with <> brackets like so:
moneyAddress.Write<float>(99999999f);


ProcessModule MainModule = mem.ModuleFuncs.GetModule(GameProcessName);

string bytePattern = "89 B7 BC 20 00 00";

long patternBaseAddress;
if(mem.PatternScanning.PatternScanModule(MainModule, bytePattern, out patternBaseAddress))
    Console.WriteLine(patternBaseAddress.ToString("X"));

Be sure to match the bit type of all 3 (The game, your hack, and the SharpMem dll) (i.e. all 64bit or all x86)
