#include "reCLR.h"
#include "DomainWorker.h"
#include "DomainReloader.h"

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <string>
#include <iostream>
#include <AccCtrl.h>
#include <Sddl.h>
#include <tlhelp32.h>
#include <sstream>
#include <msclr\marshal_cppstd.h>
#include <msclr\marshal.h>
#include <shellapi.h>

using namespace System;
using namespace System::Reflection;
using namespace System::Diagnostics;
using namespace System::IO;
using namespace System::Threading;
using namespace System::Windows::Forms;
using namespace System::Runtime::InteropServices;
using namespace System::Diagnostics;
using namespace msclr::interop;
using namespace refw;

static CLRParameters* clrParams = NULL;

// Parses a command line string into its argument according to the Windows default
static array<String^>^ SplitArgs(String^ command_line) {
	if (command_line->Length == 0)
		return gcnew array<String^>(0);
	array<String^>^ split_args;
	int argc;
	auto command_line_w = marshal_as<std::wstring>(command_line);
	wchar_t** args = CommandLineToArgvW(command_line_w.c_str(), &argc);

	if (args == nullptr) {
		throw gcnew ArgumentException("Couldn't parse command line string");
	}

	split_args = gcnew array<String^>(argc);

	for (int i=0; i<argc; i++) {
		split_args[i] = gcnew String(*args);
		args ++;
	}

	LocalFree(args);

	return split_args;
}

// Dummy helper function for resolving assembly names to fix a .NET bug
Assembly^ domain_AssemblyResolve(Object^ sender, ResolveEventArgs^ args) {
	auto parts = args->Name->Split(',');
	auto file = Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location) + "\\" + parts[0]->Trim() + ".dll";
	return Assembly::LoadFrom(file);
}

reCLR::DomainWorker^ CreateNewDomain(String^ assembly_path) {
	AppDomain^ new_domain = nullptr;
	auto base_path = Path::GetDirectoryName(assembly_path);

	// Set up domain settings
	auto app = gcnew AppDomainSetup();
	app->ApplicationBase = base_path;
	app->PrivateBinPath = base_path;
	app->ShadowCopyFiles = "true";

	// Properly load application's .config file
	app->ConfigurationFile = Path::Combine(base_path, Path::GetFileName(assembly_path) + ".config");

	// Create a new domain with a random name
	auto domain_name = gcnew String("refwDomain_") + Guid::NewGuid().ToString();
	new_domain = AppDomain::CreateDomain(domain_name, nullptr, app);
	// Create a DomainWorker class inside the new domain to load our assembly through.
	// This way the newly loaded assembly will appear inside the new domain
		
	auto worker = (reCLR::DomainWorker^)(new_domain->CreateInstanceAndUnwrap(
		Assembly::GetExecutingAssembly()->FullName,
		reCLR::DomainWorker::typeid->FullName));
	worker->ParentDomain = AppDomain::CurrentDomain;

	// Store a reference to our assembly's domain in the DefaultDomain's reCLR::Loader instance.
	// This is for potential other managed code invoked by native code looking for the injectee's domain.
	reCLR::Loader::RefwDomain = new_domain;

	return worker;
}

reCLR::DomainWorker^ reCLR::Loader::LoadAssemblyInDomain(String^ assembly_path, String^ assembly_args) {
	auto worker = CreateNewDomain(assembly_path);

	reCLR::Loader::LoadedAssemblyPath = assembly_path;
	reCLR::Loader::LoadedAssemblyArgs = assembly_args;
	// Finally load the assembly inside the domain
	worker->LoadAssembly(assembly_path, SplitArgs(assembly_args));
	return worker;
}

void reCLR::Loader::test() {
	auto test = AppDomain::CurrentDomain->IsDefaultAppDomain();
	MessageBox::Show("test" + test.ToString());
}

void reCLR::Loader::UnloadAndReloadDomain() {
	auto worker = DomainWorker::RefwDomainWorker;
	if (worker != nullptr) {
		auto state = StateCapture::CaptureState();

		auto reloader = gcnew DomainReloader(state);
		worker->ParentDomain->DoCallBack(gcnew CrossAppDomainDelegate(reloader, &reCLR::DomainReloader::Reload));
		return;
	}
}

void ThreadInitialize() {
	// Determine location of the current assembly
	//auto this_path = Path::GetDirectoryName(Assembly::GetExecutingAssembly()->Location);
	try {
		auto handler = gcnew ResolveEventHandler(domain_AssemblyResolve);
		AppDomain::CurrentDomain->AssemblyResolve += handler;

		// And build the full path to the assembly to load relative to that
		auto launch_assembly = gcnew String(clrParams->DotNetFile);
		auto assembly_args = gcnew String(clrParams->DotNetArgs);
		// Load the assembly and invoke the entry point
		reCLR::Loader::LoadAssemblyInDomain(launch_assembly, assembly_args);
	} catch (Exception^ e) {
		if (clrParams->DisplayErrors)
			MessageBox::Show(e->ToString());
		Environment::Exit(-1);
	}
}

extern "C" __declspec(dllexport) void WakeUpProcess() {
	if (clrParams == NULL || clrParams->WakeUpThreadID == 0)
		return;
	HANDLE handle = OpenThread(THREAD_SUSPEND_RESUME, FALSE, clrParams->WakeUpThreadID);
	ResumeThread(handle);
	CloseHandle(handle);
	clrParams->WakeUpThreadID = 0;
}

extern "C" __declspec(dllexport) DWORD WINAPI Initialize(LPVOID threadData) {
	clrParams = (CLRParameters*)threadData;

	// Launch the new assembly in a different thread so the injector can continue
	auto thread = gcnew Thread(gcnew ThreadStart(ThreadInitialize));
	thread->SetApartmentState(ApartmentState::STA);
	thread->Start();

	return 1;
}


static const const char* DLL_FILE = "reCLR.dll";
#ifdef _WIN64
	static const const char* DLL_EXPORT = "Initialize";
#else
	static const const char* DLL_EXPORT = "_Initialize@4";
#endif

/*void EnableDebugPriv() {
    HANDLE hToken;
    LUID sedebugnameValue;
    TOKEN_PRIVILEGES tkp;
    ZeroMemory(&tkp, sizeof(tkp));

    if(!OpenProcessToken(GetCurrentProcess(), (TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY), &hToken))
        return;
    
    if(!LookupPrivilegeValue(NULL, SE_DEBUG_NAME, &sedebugnameValue)) {
        CloseHandle(hToken);
        return;
    }
    
    tkp.PrivilegeCount = 1;
    tkp.Privileges[0].Luid = sedebugnameValue;
    tkp.Privileges[0].Attributes = SE_PRIVILEGE_ENABLED;

    AdjustTokenPrivileges(hToken, FALSE, &tkp, sizeof tkp, NULL, NULL);
    CloseHandle(hToken);
    return;
}*/

void InjectDLL(DWORD process_id, const std::string& dll_name, CLRParameters* parameters) {
  //EnableDebugPriv();
  HANDLE process;
  LPVOID RemoteString, LoadLibAddy;  
  LPVOID RemoteParameters;

  if (!process_id)
   throw std::runtime_error("Invalid process id");

  process = OpenProcess(PROCESS_ALL_ACCESS, false, process_id);

  if (!process) {
   std::ostringstream ss;
   ss << "OpenProcess() failed for " << process_id << ": " << GetLastError();
   throw std::runtime_error(ss.str().c_str());
  }

  LoadLibAddy = (LPVOID)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
  RemoteString = (LPVOID)VirtualAllocEx(process, NULL, dll_name.size(), MEM_RESERVE|MEM_COMMIT, PAGE_READWRITE);
  BOOL wpmError = WriteProcessMemory(process, (LPVOID)RemoteString, dll_name.c_str(), dll_name.size(), NULL);
  if (!wpmError) {
	  std::ostringstream ss;
	  ss << "WriteProcessMemory failed: " << GetLastError();
	  throw std::runtime_error(ss.str());
  }
  HANDLE remoteThread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)LoadLibAddy, (LPVOID)RemoteString, NULL, NULL);
  // wait for LoadLibrary to finish
  if (WaitForSingleObject(remoteThread, INFINITE) != WAIT_OBJECT_0)
	  throw std::runtime_error("WaitForSingleObject failed on LoadLibrary call");
  CloseHandle(remoteThread);

  // search module in target process
  HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, process_id);
  MODULEENTRY32 modEntry;
	modEntry.dwSize = sizeof(MODULEENTRY32);
  HMODULE ourModule = 0;
  if (Module32First(snapshot, &modEntry))
  {
	  do 
	  {
		  std::string moduleName(modEntry.szModule);
		  if (moduleName.find(DLL_FILE) != std::string::npos)
		  {
			  ourModule = modEntry.hModule;
			  break;
		  }
	  } while (Module32Next(snapshot, &modEntry));
  }
  if (!ourModule) {
		std::ostringstream ss;
		ss << "Module was not successfully injected into target process: " << GetLastError();
	  throw std::runtime_error(ss.str());
	}

  // load module locally
  HMODULE local = LoadLibraryEx(dll_name.c_str(), NULL, DONT_RESOLVE_DLL_REFERENCES);
  if (!local)
	  throw std::runtime_error("Couldn't load module locally");
  LPVOID exportAddr = GetProcAddress(local, DLL_EXPORT);
  if (exportAddr == NULL)
	  throw std::runtime_error("Couldn't get export address from module");

  // check if module has a dynamic image base and correct it
  if (local != ourModule)
	  exportAddr = (LPVOID)((DWORD)ourModule + ((DWORD)exportAddr - (DWORD)local));

  // allocate space for parameters data
  RemoteParameters = (LPVOID)VirtualAllocEx(process, NULL, sizeof(CLRParameters), MEM_RESERVE|MEM_COMMIT, PAGE_READWRITE);
  WriteProcessMemory(process, (LPVOID)RemoteParameters, parameters, sizeof(CLRParameters), NULL);
  // ... and call it with the application to launch as parameter
  remoteThread = CreateRemoteThread(process, NULL, NULL, (LPTHREAD_START_ROUTINE)exportAddr, (LPVOID)RemoteParameters, NULL, NULL);
  if (WaitForSingleObject(remoteThread, INFINITE) != WAIT_OBJECT_0)
	  throw std::runtime_error("WaitForSingleObject failed on export call");
  CloseHandle(remoteThread);

  //WaitForDLL(snapshot, process_id, dll_name);

  // all done
  CloseHandle(snapshot);
  CloseHandle(process);
} 

int reCLR::Loader::CreateProcessAndInject(String^ process_name, String^ command_line, String^ assembly, bool display_errors) {
	return reCLR::Loader::CreateProcessAndInject(process_name, command_line, assembly, "", display_errors);
}

int reCLR::Loader::CreateProcessAndInject(String^ process_name, String^ command_line, String^ assembly, String^ assembly_args, bool display_errors) {
	marshal_context context;
	// Set up CreateProcess structures
  STARTUPINFO si;
  PROCESS_INFORMATION pi;
  ZeroMemory(&si, sizeof(si));
  si.cb = sizeof(si);
  ZeroMemory(&pi, sizeof(pi));
  
	auto process_directory = Path::GetDirectoryName(process_name);
	auto command_line_char = (char*)(void*)Marshal::StringToHGlobalAnsi(String::Format("\"{0}\" {1}", process_name, command_line));

  auto error = CreateProcess(context.marshal_as<const char*>(process_name),
    command_line_char,
    NULL,
    NULL,
    FALSE,
    CREATE_DEFAULT_ERROR_MODE | CREATE_SUSPENDED | CREATE_NEW_CONSOLE,
    NULL,
    context.marshal_as<const char*>(process_directory),
    &si,
    &pi);

	if (!error) {
		auto error_code = GetLastError();
		throw gcnew InvalidOperationException(String::Format("Failed to create process {0}, error code {1}", process_name, error_code));
	}

	reCLR::Loader::Inject(pi.dwProcessId, assembly, assembly_args, display_errors, pi.dwThreadId);
	return pi.dwProcessId;
}

void reCLR::Loader::Inject(int process_id, String^ assembly, String^ assembly_args, bool display_errors, int wakeup_thread_id) {
	// Expand assembly name if it points to a library
	if (File::Exists(assembly))
		assembly = Path::GetFullPath(assembly);
	
	auto full_CLR_name = Path::GetFullPath(gcnew String(DLL_FILE));

	CLRParameters parameters;

	// Copy assembly name into parameters structure
	std::string assembly_str = marshal_as<std::string>(assembly);
  memcpy(parameters.DotNetFile, assembly_str.c_str(), assembly_str.size());
  parameters.DotNetFile[assembly_str.size()] = 0;

  parameters.WakeUpThreadID = wakeup_thread_id;
	parameters.DisplayErrors = display_errors;

	// Copy assembly arguments into parameters structure
	std::string assembly_args_str = marshal_as<std::string>(assembly_args);
	memcpy(parameters.DotNetArgs, assembly_args_str.c_str(), assembly_args_str.size());
	parameters.DotNetArgs[assembly_args_str.size()] = 0;

	try {
		InjectDLL(process_id, marshal_as<std::string>(full_CLR_name), &parameters);
	} catch (std::exception& ex) {
		throw gcnew InvalidOperationException(marshal_as<String^>(ex.what()));
	}
}

