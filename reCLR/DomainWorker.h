#pragma once

using namespace System;
using namespace System::Reflection;
using namespace System::Windows::Forms;

namespace reCLR {
	public ref class DomainWorker: MarshalByRefObject {
	public:
		void LoadAssembly(String^ name, array<String^>^ args) {
			auto ass = Assembly::LoadFrom(name);
			auto ep = ass->EntryPoint;

			auto arg_count = ep->GetParameters()->Length;
			if (arg_count == 1) {
				array<Object^>^ parameters = { args };
				ep->Invoke(nullptr, parameters);
			} else if (arg_count == 0) {
				ep->Invoke(nullptr, nullptr);
			} else {
				throw gcnew ArgumentException(String::Format("Entry point expects an unexpected number of arguments: {0}", arg_count));
			}
		}
	};
}
