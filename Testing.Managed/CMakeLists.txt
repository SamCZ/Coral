project(CoralManagedTesting LANGUAGES CSharp)

file(GLOB_RECURSE CORAL_MANAGED_SOURCE Source/*.cs)

add_library(CoralManagedTesting SHARED ${CORAL_MANAGED_SOURCE})

set_target_properties(CoralManagedTesting PROPERTIES
				DOTNET_SDK "Microsoft.NET.Sdk"
				DOTNET_TARGET_FRAMEWORK "net7.0")

target_compile_options(CoralManagedTesting PRIVATE "/unsafe")
set_target_properties(CoralManagedTesting PROPERTIES VS_GLOBAL_Nullable enable)

target_link_libraries(CoralManagedTesting PRIVATE CoralManaged)