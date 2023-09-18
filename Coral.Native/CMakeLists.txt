project(CoralNative)

file(GLOB_RECURSE CORAL_NATIVE_SOURCE Source/*.cpp)

add_library(CoralNative STATIC ${CORAL_NATIVE_SOURCE})
target_precompile_headers(CoralNative PUBLIC Source/CoralPCH.hpp)
target_include_directories(CoralNative PUBLIC Include/Coral)
target_include_directories(CoralNative PUBLIC Include)

target_link_libraries(CoralNative PUBLIC CoralManaged)
target_link_libraries(CoralNative PUBLIC NetCore::NetCore)