﻿using Coral.Managed.Interop;

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Coral.Managed;

public static class Marshalling
{
	struct ArrayContainer
	{
		public IntPtr Data;
		public int Length;
	};

	public static void MarshalReturnValue(object? InValue, Type? InType, IntPtr OutValue)
	{
		if (InType == null)
			return;

		if (InType.IsSZArray)
		{
			var array = InValue as Array;
			var elementType = InType.GetElementType();
			CopyArrayToBuffer(OutValue, array, elementType);
		}
		else if (InValue is string str)
		{
			NativeString nativeString = str;
			Marshal.StructureToPtr(nativeString, OutValue, false);
		}
		else if (InValue is NativeString nativeString)
		{
			Marshal.StructureToPtr(nativeString, OutValue, false);
		}
		else if (InType.IsPointer)
		{
			unsafe
			{
				if (InValue == null)
				{
					Marshal.WriteIntPtr(OutValue, IntPtr.Zero);
				}
				else
				{
					void* valuePointer = Pointer.Unbox(InValue);
					Buffer.MemoryCopy(&valuePointer, OutValue.ToPointer(), IntPtr.Size, IntPtr.Size);
				}
			}
		}
		else
		{
			var valueSize = Marshal.SizeOf(InType);
			var handle = GCHandle.Alloc(InValue, GCHandleType.Pinned);

			unsafe
			{
				Buffer.MemoryCopy(handle.AddrOfPinnedObject().ToPointer(), OutValue.ToPointer(), valueSize, valueSize);
			}

			handle.Free();
		}
	}

	public static object? MarshalArray(IntPtr InArray, Type? InElementType)
	{
		if (InElementType == null)
			return null;

		var arrayContainer = MarshalPointer<ArrayContainer>(InArray);
		var elements = Array.CreateInstance(InElementType, arrayContainer.Length);
		int elementSize = Marshal.SizeOf(InElementType);

		unsafe
		{
			for (int i = 0; i < arrayContainer.Length; i++)
			{
				IntPtr source = (IntPtr)(((byte*)arrayContainer.Data.ToPointer()) + (i * elementSize));
				elements.SetValue(Marshal.PtrToStructure(source, InElementType), i);
			}
		}

		return elements;
	}

	public static void CopyArrayToBuffer(IntPtr InBuffer, Array? InArray, Type? InElementType)
	{
		if (InArray == null || InElementType == null)
			return;

		var elementSize = Marshal.SizeOf(InElementType);
		int byteLength = InArray.Length * elementSize;
		var mem = Marshal.AllocHGlobal(byteLength);

		int offset = 0;

		foreach (var elem in InArray)
		{
			var elementHandle = GCHandle.Alloc(elem, GCHandleType.Pinned);

			unsafe
			{
				Buffer.MemoryCopy(elementHandle.AddrOfPinnedObject().ToPointer(), ((byte*)mem.ToPointer()) + offset, elementSize, elementSize);
			}

			offset += elementSize;

			elementHandle.Free();
		}

		ArrayContainer container = new()
		{
			Data = mem,
			Length = InArray.Length
		};

		var handle = GCHandle.Alloc(container, GCHandleType.Pinned);
		
		unsafe
		{
			Buffer.MemoryCopy(handle.AddrOfPinnedObject().ToPointer(), InBuffer.ToPointer(), Marshal.SizeOf<ArrayContainer>(), Marshal.SizeOf<ArrayContainer>());
		}

		handle.Free();
	}

	public static object? MarshalPointer(IntPtr InValue, Type InType)
	{
		if (InType.IsPointer || InType == typeof(IntPtr))
			return InValue;

		// NOTE(Peter): Marshal.PtrToStructure<bool> doesn't seem to work
		//				instead we have to read it as a single byte and check the raw value
		if (InType == typeof(bool))
			return Marshal.PtrToStructure<byte>(InValue) > 0;

		if (InType == typeof(string))
		{
			var nativeString = Marshal.PtrToStructure<NativeString>(InValue);
			return nativeString.ToString();
		}
		else if (InType == typeof(NativeString))
		{
			return Marshal.PtrToStructure<NativeString>(InValue);
		}

		if (InType.IsSZArray)
			return MarshalArray(InValue, InType.GetElementType());

		if (InType.IsGenericType)
		{
			if (InType == typeof(NativeArray<>).MakeGenericType(InType.GetGenericArguments().First()))
			{
				var elements = Marshal.ReadIntPtr(InValue, 0);
				var elementCount = Marshal.ReadInt32(InValue, Marshal.SizeOf<IntPtr>());
				var genericType = typeof(NativeArray<>).MakeGenericType(InType.GetGenericArguments().First());
				return TypeHelper.CreateInstance(genericType, elements, elementCount);
			}
		}

		return Marshal.PtrToStructure(InValue, InType);	
	}
	public static T? MarshalPointer<T>(IntPtr InValue) => Marshal.PtrToStructure<T>(InValue);

	public static IntPtr[] NativeArrayToIntPtrArray(IntPtr InNativeArray, int InLength)
	{
		try
		{
			if (InNativeArray == IntPtr.Zero || InLength == 0)
				return Array.Empty<IntPtr>();

			IntPtr[] result = new IntPtr[InLength];

			for (int i = 0; i < InLength; i++)
				result[i] = Marshal.ReadIntPtr(InNativeArray, i * Marshal.SizeOf<nint>());

			return result;
		}
		catch (Exception ex)
		{
			ManagedHost.HandleException(ex);
			return Array.Empty<IntPtr>();
		}
	}

	public static object?[]? MarshalParameterArray(IntPtr InNativeArray, int InLength, MethodBase? InMethodInfo)
	{
		if (InMethodInfo == null)
			return null;

		if (InNativeArray == IntPtr.Zero || InLength == 0)
			return null;

		var parameterInfos = InMethodInfo.GetParameters();
		var parameterPointers = NativeArrayToIntPtrArray(InNativeArray, InLength);
		var result = new object?[parameterPointers.Length];

		for (int i = 0; i < parameterPointers.Length; i++)
			result[i] = MarshalPointer(parameterPointers[i], parameterInfos[i].ParameterType);

		return result;
	}
	
}