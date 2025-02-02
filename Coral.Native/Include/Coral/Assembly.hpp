#pragma once

#include "Core.hpp"
#include "ReflectionType.hpp"

namespace Coral {

	enum class AssemblyLoadStatus
	{
		Success,
		FileNotFound,
		FileLoadFailure,
		InvalidFilePath,
		InvalidAssembly,
		UnknownError
	};

	class ManagedAssembly
	{
	public:
		int32_t GetAssemblyID() const { return m_AssemblyID; }
		AssemblyLoadStatus GetLoadStatus() const { return m_LoadStatus; }
		std::string_view GetName() const { return m_Name; }

		void AddInternalCall(std::string_view InClassName, std::string_view InVariableName, void* InFunctionPtr);
		void UploadInternalCalls();

		TypeId GetTypeId(std::string_view InClassName) const;

		const std::vector<ReflectionType>& GetTypes() const { return m_ReflectionTypes; }
		
	private:
		HostInstance* m_Host = nullptr;
		int32_t m_AssemblyID = -1;
		AssemblyLoadStatus m_LoadStatus = AssemblyLoadStatus::UnknownError;
		std::string m_Name;

	#if defined(CORAL_WIDE_CHARS)
		std::vector<std::wstring> m_InternalCallNameStorage;
	#else
		std::vector<std::string> m_InternalCallNameStorage;
	#endif
		
		std::vector<InternalCall> m_InternalCalls;

		std::vector<ReflectionType> m_ReflectionTypes;
		
		friend class HostInstance;
		friend class AssemblyLoadContext;
	};

	class AssemblyLoadContext
	{
	public:
		ManagedAssembly& LoadAssembly(std::string_view InFilePath);
		const std::vector<ManagedAssembly>& GetLoadedAssemblies() const { return m_LoadedAssemblies; }

	private:
		int32_t m_ContextId;
		std::vector<ManagedAssembly> m_LoadedAssemblies;

		HostInstance* m_Host = nullptr;

		friend class HostInstance;
	};

}
