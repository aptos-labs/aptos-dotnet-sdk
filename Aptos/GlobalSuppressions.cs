// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Usage",
    "CA2200:Rethrow to preserve stack details",
    Justification = "<Pending>",
    Scope = "member",
    Target = "~M:Aptos.TransactionBuilder.GenerateRawTransaction(Aptos.AptosClient,Aptos.AccountAddress,Aptos.TransactionPayload,Aptos.AccountAddress,Aptos.TransactionBuilder.GenerateTransactionOptions)~System.Threading.Tasks.Task{Aptos.RawTransaction}"
)]
[assembly: SuppressMessage(
    "Usage",
    "CA2200:Rethrow to preserve stack details",
    Justification = "<Pending>",
    Scope = "member",
    Target = "~M:Aptos.AccountClient.LookupOriginalAccountAddress(System.String)~System.Threading.Tasks.Task{Aptos.AccountAddress}"
)]
