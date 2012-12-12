refw
====

refw is a relatively simple and compact .NET library for injecting managed .NET assemblies into applications on Windows. It is written in C# and C++.NET for the more native parts. The intent is to also include other functionality and libraries that common or not-so-common injected applications may require. Currently this helper functions and wrappers for cross-thread execution, DirectX 9 EndScene hooking, address offset manipulation for converting function pointers to delegates and dealing with ASLR and a SOCKS proxy hook.

Except for EasyHook the library does not depend on any other libraries.

Injecting
---------

Injection into new processes is handled through the function refw.Loader.CreateProcessAndInject and friends. It deals with creating a new suspended process, loading itself inside of it, communicating any parameters, loading the .NET assembly and finally invoking the assembly's entry point.
The entry point is a standard static function of the type
	static void Main(string[] args)
or
	static void Main()
In the second case any arguments passed to the injected assembly through CreateProcessAndInject are lost.

Once execution has been handed off to the injected assembly, it must wake up the host process's main thread through refw.Loader.WakeUpProcess. This is intended to give the assembly time to load and initialize before the host process can do any of that.


SOCKS Proxy Hook
----------------

Preliminary support for detouring a process' connect() attempts through a SOCKS5 proxy server has been added in the form of a set of Windows API hooks. Proxy hook state, SOCKS host and authentication details are controlled through the refw.Proxy static class.
