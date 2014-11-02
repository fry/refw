#pragma once

using namespace System;
using namespace System::Threading;

namespace reCLR {
	[Serializable]
	public ref class DomainReloader {
	protected:
		refw::StateCapture^ savedCapture;

		void ReloadThread() {
			AppDomain::Unload(reCLR::Loader::RefwDomain);

			auto worker = reCLR::Loader::LoadAssemblyInDomain(reCLR::Loader::LoadedAssemblyPath, reCLR::Loader::LoadedAssemblyArgs);

			worker->RestoreState(savedCapture);
		}
	public:
		DomainReloader(refw::StateCapture^ capture) {
			savedCapture = capture;
		}

		void Reload() {
			if (reCLR::Loader::RefwDomain == nullptr || reCLR::Loader::LoadedAssemblyPath == nullptr || reCLR::Loader::LoadedAssemblyArgs == nullptr)
				throw gcnew System::NullReferenceException("UnloadAndReloadDomain: No assembly currently loaded");

			auto thread = gcnew Thread(gcnew ThreadStart(this, &DomainReloader::ReloadThread));
			thread->Start();
		}
	};
}
