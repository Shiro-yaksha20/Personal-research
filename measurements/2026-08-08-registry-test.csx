using Microsoft.Win32;

// Exact same string SystemInfoViewModel uses:
var bad = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
var good = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";

using var hive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

Console.WriteLine($"BAD path (double backslash): '{bad}'");
using var k1 = hive.OpenSubKey(bad);
Console.WriteLine($"  -> {(k1 is null ? "NULL (broken)" : $"opened, EditionID={k1.GetValue("EditionID")}")}");

Console.WriteLine($"GOOD path (single backslash): '{good}'");
using var k2 = hive.OpenSubKey(good);
Console.WriteLine($"  -> {(k2 is null ? "NULL" : $"opened, EditionID={k2.GetValue("EditionID")}")}");
