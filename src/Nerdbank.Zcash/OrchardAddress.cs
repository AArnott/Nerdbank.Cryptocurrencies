﻿// Copyright (c) Andrew Arnott. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Nerdbank.Zcash;

/// <summary>
/// A Unified Address that carries a single Orchard receiver.
/// </summary>
[DebuggerDisplay($"{{{nameof(Address)},nq}}")]
public class OrchardAddress : UnifiedAddress
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly OrchardReceiver receiver;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly ZcashNetwork network;
	private ReadOnlyCollection<ZcashAddress>? receivers;

	/// <inheritdoc cref="OrchardAddress(string, in OrchardReceiver, ZcashNetwork)"/>
	public OrchardAddress(in OrchardReceiver receiver, ZcashNetwork network = ZcashNetwork.MainNet)
		: base(CreateAddress(receiver, network))
	{
		this.receiver = receiver;
		this.network = network;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OrchardAddress"/> class.
	/// </summary>
	/// <param name="address"><inheritdoc cref="ZcashAddress(string)" path="/param"/></param>
	/// <param name="receiver">The encoded receiver.</param>
	/// <param name="network">The network to which this address belongs.</param>
	internal OrchardAddress(string address, in OrchardReceiver receiver, ZcashNetwork network = ZcashNetwork.MainNet)
		: base(address)
	{
		this.receiver = receiver;
		this.network = network;
	}

	/// <inheritdoc/>
	public override ZcashNetwork Network => this.network;

	/// <inheritdoc/>
	public override IReadOnlyList<ZcashAddress> Receivers => this.receivers ??= new ReadOnlyCollection<ZcashAddress>(new[] { this });

	/// <inheritdoc/>
	internal override byte UnifiedAddressTypeCode => 0x03;

	/// <inheritdoc/>
	internal override int ReceiverEncodingLength => this.receiver.Span.Length;

	/// <inheritdoc/>
	public override TPoolReceiver? GetPoolReceiver<TPoolReceiver>() => AsReceiver<OrchardReceiver, TPoolReceiver>(this.receiver);

	/// <inheritdoc/>
	internal override int GetReceiverEncoding(Span<byte> output)
	{
		ReadOnlySpan<byte> receiverSpan = this.receiver.Span;
		receiverSpan.CopyTo(output);
		return receiverSpan.Length;
	}

	private static unsafe string CreateAddress(in OrchardReceiver receiver, ZcashNetwork network)
	{
		string humanReadablePart = network switch
		{
			ZcashNetwork.MainNet => HumanReadablePartMainNet,
			ZcashNetwork.TestNet => HumanReadablePartTestNet,
			_ => throw new NotSupportedException(Strings.UnrecognizedNetwork),
		};

		Span<byte> padding = stackalloc byte[16];
		InitializePadding(network, padding);
		Span<byte> buffer = stackalloc byte[GetUAContributionLength<OrchardReceiver>() + padding.Length];
		int written = 0;
		written += WriteUAContribution(receiver, buffer);
		padding.CopyTo(buffer.Slice(written));
		written += padding.Length;

		F4Jumble(buffer);

		Span<char> address = stackalloc char[Bech32.GetEncodedLength(humanReadablePart.Length, buffer.Length)];
		int finalLength = Bech32.Bech32m.Encode(humanReadablePart, buffer, address);
		Assumes.True(address.Length == finalLength);
		return new(address);
	}
}