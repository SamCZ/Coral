cmake_minimum_required(VERSION 3.15)
project(Coral)

add_subdirectory(Coral.Managed)
add_subdirectory(NetCore/7.0.7)
add_subdirectory(Coral.Native)

if (true)
	add_subdirectory(Testing.Managed)
	add_subdirectory(Testing)
endif()