project(CoralTesting CXX)

add_executable(CoralTesting Source/Main.cpp)
target_link_libraries(CoralTesting PRIVATE CoralNative CoralManagedTesting)