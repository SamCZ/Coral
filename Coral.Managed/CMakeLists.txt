project(CoralManaged LANGUAGES CSharp)

file(GLOB_RECURSE CORAL_MANAGED_SOURCE Source/*.cs)

add_library(CoralManaged SHARED ${CORAL_MANAGED_SOURCE})

set_target_properties(CoralManaged PROPERTIES
				DOTNET_SDK "Microsoft.NET.Sdk"
				DOTNET_TARGET_FRAMEWORK "net7.0")

target_compile_options(CoralManaged PRIVATE "/unsafe")
set_target_properties(CoralManaged PROPERTIES VS_GLOBAL_Nullable enable)
set_target_properties(CoralManaged PROPERTIES VS_GLOBAL_RootNamespace Coral.Managed)
set_target_properties(CoralManaged PROPERTIES VS_GLOBAL_EnableDynamicLoading true)

set_target_properties(CoralManaged PROPERTIES OUTPUT_NAME Coral.Managed)