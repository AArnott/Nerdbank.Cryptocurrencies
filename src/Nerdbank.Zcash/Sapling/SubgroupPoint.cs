﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Nerdbank.Zcash.FixedLengthStructs;

namespace Nerdbank.Zcash.Sapling;

/// <summary>
/// A subgroup point.
/// </summary>
internal readonly struct SubgroupPoint
{
	private readonly Bytes32 value;

	/// <summary>
	/// Initializes a new instance of the <see cref="SubgroupPoint"/> struct.
	/// </summary>
	/// <param name="value">The value of the buffer.</param>
	internal SubgroupPoint(ReadOnlySpan<byte> value)
	{
		this.value = new(value);
	}

	/// <summary>
	/// Gets the buffer. Always 32 bytes in length.
	/// </summary>
	internal readonly ReadOnlySpan<byte> Value => this.value.Value;
}
