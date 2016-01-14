#include "reCLR.h"
#include "ProxySettingsRetriever.h"

bool reCLR::ProxySettingsRetriever::IsEnabled() {
	return reCLR::Loader::ProxyEnabled;
}