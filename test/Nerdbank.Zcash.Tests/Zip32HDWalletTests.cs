﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

public class Zip32HDWalletTests : TestBase
{
	private readonly ITestOutputHelper logger;

	public Zip32HDWalletTests(ITestOutputHelper logger)
	{
		this.logger = logger;
	}

	[Theory, PairwiseData]
	public void CreateSaplingMasterKey(ZcashNetwork network)
	{
		Bip39Mnemonic mnemonic = Bip39Mnemonic.Create(32);
		this.logger.WriteLine($"Mnemonic: {mnemonic}");
		Zip32HDWallet.Sapling.ExtendedSpendingKey spendingKey = Zip32HDWallet.Sapling.Create(mnemonic, network);
		Assert.Equal(0, spendingKey.Depth);
		Assert.Equal(0u, spendingKey.ChildIndex);
		Assert.Equal(network, spendingKey.Network);
		Assert.NotNull(spendingKey.FullViewingKey);
		Assert.NotEqual(default, spendingKey.FullViewingKey.Fingerprint);
	}

	[Theory, PairwiseData]
	public void CreateOrchardMasterKey(ZcashNetwork network)
	{
		this.logger.WriteLine($"Mnemonic: {Mnemonic}");
		Zip32HDWallet.Orchard.ExtendedSpendingKey masterSpendingKey = Zip32HDWallet.Orchard.Create(Mnemonic, network);
		Assert.Equal(0, masterSpendingKey.Depth);
		Assert.Equal(0u, masterSpendingKey.ChildIndex);
		Assert.Equal(network, masterSpendingKey.Network);
		Assert.NotNull(masterSpendingKey.FullViewingKey);

		Zip32HDWallet.Orchard.ExtendedSpendingKey accountSpendingKey = masterSpendingKey.Derive(Zip32HDWallet.CreateKeyPath(0));
		Assert.NotNull(accountSpendingKey.FullViewingKey);

		Assert.NotEqual(default, masterSpendingKey.Fingerprint);
	}

	[Fact]
	public void CreateOrchardAddressFromSeed()
	{
		Bip39Mnemonic mnemonic = Bip39Mnemonic.Parse("badge bless baby bird anger wage memory extend word isolate equip faith");
		Zip32HDWallet.Orchard.ExtendedSpendingKey masterSpendingKey = Zip32HDWallet.Orchard.Create(mnemonic, ZcashNetwork.MainNet);
		Zip32HDWallet.Orchard.ExtendedSpendingKey accountSpendingKey = masterSpendingKey.Derive(Zip32HDWallet.CreateKeyPath(0));
		OrchardReceiver receiver = accountSpendingKey.FullViewingKey.CreateReceiver(0);
		OrchardAddress address = new(receiver);
		this.logger.WriteLine(address);
		Assert.Equal("u1zpfqm4r0cc5ttvt4mft6nvyqe3uwsdcgx65s44sd3ar42rnkz7v9az0ez7dpyxvjcyj9x0sd89yy7635vn8fplwvg6vn4tr6wqpyxqaw", address.Address);
	}

	[Fact]
	public void CreateSaplingAddressFromSeed()
	{
		Bip39Mnemonic mnemonic = Bip39Mnemonic.Parse("badge bless baby bird anger wage memory extend word isolate equip faith");
		Zip32HDWallet.Sapling.ExtendedSpendingKey masterSpendingKey = Zip32HDWallet.Sapling.Create(mnemonic, ZcashNetwork.MainNet);
		Zip32HDWallet.Sapling.ExtendedSpendingKey accountSpendingKey = masterSpendingKey.Derive(Zip32HDWallet.CreateKeyPath(0));
		BigInteger diversifierIndex = 0;
		Assert.True(accountSpendingKey.FullViewingKey.Key.TryCreateReceiver(ref diversifierIndex, out SaplingReceiver receiver));
		Assert.Equal(1, diversifierIndex); // index 0 was invalid in this case.
		SaplingAddress address = new(receiver);
		this.logger.WriteLine(address);
		Assert.Equal("zs1duqpcc2ql7zfjttdm2gpawe8t5ecek5k834u9vdg4mqhw7j8j39sgjy8xguvk2semyd4ujeyj28", address.Address);
	}

	[Fact]
	public void CreateSaplingAddressFromSeed_ViaFVK()
	{
		Zip32HDWallet.Sapling.ExtendedFullViewingKey masterFullViewingKey = Zip32HDWallet.Sapling.Create(Mnemonic, ZcashNetwork.MainNet).FullViewingKey;
		Zip32HDWallet.Sapling.ExtendedFullViewingKey childFVK = masterFullViewingKey.Derive(3);
		BigInteger diversifierIndex = 0;
		Assert.True(childFVK.Key.TryCreateReceiver(ref diversifierIndex, out SaplingReceiver receiver));
		Assert.Equal(3, diversifierIndex); // indexes 0-2 were invalid in this case.
		SaplingAddress address = new(receiver);
		this.logger.WriteLine(address);
		Assert.Equal("zs134p2zqc6lnrywwdrrm522f5745ctlvc0lnuvlpauwrrjydjrkkq7f4v98wkg669uf5zm54zlc8g", address.Address);
	}

	[Fact]
	public void MnemonicCtorInitializesProperties()
	{
		Zip32HDWallet wallet = new(Mnemonic, ZcashNetwork.TestNet);
		Assert.Equal(ZcashNetwork.TestNet, wallet.Network);
		Assert.Same(Mnemonic, wallet.Mnemonic);
		Assert.Equal(Mnemonic.Seed.ToArray(), wallet.Seed.ToArray());
	}

	[Fact]
	public void SeedCtorInitializesProperties()
	{
		Zip32HDWallet wallet = new(Mnemonic.Seed, ZcashNetwork.TestNet);
		Assert.Equal(ZcashNetwork.TestNet, wallet.Network);
		Assert.Null(wallet.Mnemonic);
		Assert.Equal(Mnemonic.Seed.ToArray(), wallet.Seed.ToArray());
	}

	[Fact]
	public void CreateOrchardAccount()
	{
		Zip32HDWallet wallet = new(Mnemonic, ZcashNetwork.TestNet);
		Zip32HDWallet.Orchard.ExtendedSpendingKey account = wallet.CreateOrchardAccount(1);
		Assert.Equal(1u | Bip32HDWallet.HardenedBit, account.ChildIndex);

		Assert.Equal(new OrchardAddress(account.FullViewingKey.CreateReceiver(0), ZcashNetwork.TestNet), account.DefaultAddress);
	}

	[Fact]
	public void CreateSaplingAccount()
	{
		Zip32HDWallet wallet = new(Mnemonic, ZcashNetwork.TestNet);
		Zip32HDWallet.Sapling.ExtendedSpendingKey account = wallet.CreateSaplingAccount(1);
		Assert.Equal(1u | Bip32HDWallet.HardenedBit, account.ChildIndex);

		BigInteger diversifier = BigInteger.Zero;
		Assert.True(account.FullViewingKey.Key.TryCreateReceiver(ref diversifier, out SaplingReceiver receiver));
		Assert.Equal(new SaplingAddress(receiver, ZcashNetwork.TestNet), account.DefaultAddress);
	}
}
