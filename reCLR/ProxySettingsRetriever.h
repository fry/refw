#pragma once
namespace reCLR {
	public ref class ProxySettingsRetriever: MarshalByRefObject {
	public:
		bool IsEnabled();
	};
}