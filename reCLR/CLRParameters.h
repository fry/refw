#pragma once

struct CLRParameters {
	char DotNetFile[512];
	unsigned long WakeUpThreadID;
	bool DisplayErrors;

	char DotNetArgs[1024];
};
