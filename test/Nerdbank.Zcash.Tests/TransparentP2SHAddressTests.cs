﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

public class TransparentP2SHAddressTests : TestBase
{
	[Fact]
	public void Ctor_Receiver()
	{
		byte[] hash = new byte[20];
		TransparentP2SHReceiver receiver = new(hash);
		TransparentP2SHAddress addr = new(receiver);
		Assert.Equal("t3JZcvsuaXE6ygokL4XUiZSTrQBUoPYFnXJ", addr.Address);
	}

	[Fact]
	public void GetPoolReceiver()
	{
		Assert.NotNull(ZcashAddress.Parse(ValidTransparentP2SHAddress).GetPoolReceiver<TransparentP2SHReceiver>());
		Assert.Null(ZcashAddress.Parse(ValidTransparentP2SHAddress).GetPoolReceiver<TransparentP2PKHReceiver>());
		Assert.Null(ZcashAddress.Parse(ValidTransparentP2SHAddress).GetPoolReceiver<SaplingReceiver>());
	}
}
