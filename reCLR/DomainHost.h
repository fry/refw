#pragma once

#include "CLRParameters.h"

using namespace System;

namespace reCLR {
	public ref class DomainHost {
	public:
		AppDomain^ Domain;

		String^ LoadedAssemblyPath;
		String^ LoadedAssemblyArgs;
	}
}