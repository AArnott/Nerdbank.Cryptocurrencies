﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Cryptocurrencies;

public interface IExtendedKey
{
	byte Depth { get; }

	IExtendedKey Derive(uint childNumber);
}
