﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Security.Cryptography;
using Org.BouncyCastle.Math.EC;

namespace Nerdbank.Zcash;

public partial class Zip32HDWallet
{
	/// <summary>
	/// Contains types and methods related to the Orchard pool.
	/// </summary>
	public static partial class Orchard
	{
		/// <inheritdoc cref="Create(ReadOnlySpan{byte}, bool)"/>
		/// <param name="mnemonic">The mnemonic phrase from which to generate the master key.</param>
		public static ExtendedSpendingKey Create(Bip39Mnemonic mnemonic, bool testNet = false) => Create(Requires.NotNull(mnemonic).Seed, testNet);

		/// <summary>
		/// Creates a master key for the Orchard pool.
		/// </summary>
		/// <param name="seed">The seed for use to generate the master key. A given seed will always produce the same master key.</param>
		/// <param name="testNet"><see langword="true" /> to create a key for use on the Zcash testnet; <see langword="false"/> otherwise.</param>
		/// <returns>A master extended spending key.</returns>
		public static ExtendedSpendingKey Create(ReadOnlySpan<byte> seed, bool testNet = false)
		{
			// Rust: assert!(seed.len() >= 32 && seed.len() <= 252);
			Span<byte> blakeOutput = stackalloc byte[64]; // 512 bits
			Blake2B.ComputeHash(seed, blakeOutput, new Blake2B.Config { Personalization = "ZcashIP32Orchard"u8, OutputSizeInBytes = blakeOutput.Length });

			SpendingKey spendingKey = new(blakeOutput[..32]);
			ChainCode chainCode = new(blakeOutput[32..]);

			return new ExtendedSpendingKey(
				spendingKey,
				chainCode,
				parentFullViewingKeyTag: default,
				depth: 0,
				childNumber: 0,
				testNet);
		}

		/// <summary>
		/// Implement the ToScalar_Orchard function.
		/// </summary>
		private static BigInteger ToScalar(ReadOnlySpan<byte> input) => BigInteger.Remainder(LEOS2IP(input), Curves.Pallas.Curve.Order.ToNumerics());

		private static BigInteger ToBase(ReadOnlySpan<byte> input) => BigInteger.Remainder(LEOS2IP(input), Curves.Pallas.Curve.Q.ToNumerics());

		private static readonly SpendAuthSigOrchard SpendAuthSig = new();

		private class SpendAuthSigOrchard : SpendAuthSigBase
		{
			internal override Org.BouncyCastle.Math.EC.ECPoint BasePoint => Curves.Pallas.BasePoint;

			internal override FpCurve Curve => Curves.Pallas.Curve;
		}
	}
}