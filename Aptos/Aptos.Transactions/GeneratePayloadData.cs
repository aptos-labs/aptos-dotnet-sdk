namespace Aptos;

public abstract class GeneratePayloadData(List<object>? typeArguments = null)
{
    public List<object>? TypeArguments = typeArguments;
}

public abstract class GenerateTransactionPayloadDataWithAbi(string function, List<object>? functionArguments, List<object>? typeArguments = null, EntryFunctionAbi? abi = null) : GeneratePayloadData(typeArguments)
{
    /// <summary>
    /// The ABI of the function that will be used to check the type and function arguments of the transaction.
    /// </summary>
    /// <remarks>
    /// If the ABI is not provided, a remote ABI will be fetch from the blockchain.
    /// </remarks>
    public EntryFunctionAbi? Abi = abi;

    /// <summary>
    /// The function to be called on the module. This should be in the format of "address::module_name::function_name". 
    /// </summary>
    public string Function = function;

    /// <summary>
    /// A list of either strings or <see cref="TransactionArgument"/>  objects representing the function arguments.
    /// If the function arguments are strings, they will be converted to <see cref="TransactionArgument"/> objects using the <see cref="TransactionArgument.ConvertArgument(object, TypeTag, List{TypeTag})"/> method.
    /// </summary>
    /// <remarks>
    /// The function arguments are expected to be in the same order and type as the function's expected ABI.
    /// </remarks>
    public List<object>? FunctionArguments = functionArguments;

}

/// <summary>
/// Represents a payload data for a view function.
/// </summary>
/// <param name="function">The function to be called on the module. This should be in the format of "address::module_name::function_name".</param>
/// <param name="functionArguments">A list of either strings or <see cref="TransactionArgument"/> objects representing the function arguments. If the function arguments are strings, they will be converted to <see cref="TransactionArgument"/> objects using the <see cref="TransactionArgument.ConvertArgument(object, TypeTag, List{TypeTag})"/> method.</param>
/// <param name="typeArguments">A list of <see cref="TypeTag"/> objects representing the type arguments of the function.</param>
/// <param name="abi">The ABI of the function that will be used to check the type and function arguments of the transaction.</param>
public class GenerateViewFunctionPayloadData(string function, List<object>? functionArguments, List<object>? typeArguments = null, ViewFunctionAbi? abi = null) : GeneratePayloadData(typeArguments)
{
    /// <inheritdoc cref="GenerateTransactionPayloadDataWithAbi.Abi"/>
    public ViewFunctionAbi? Abi = abi;

    /// <inheritdoc cref="GenerateTransactionPayloadDataWithAbi.Function"/>
    public string Function = function;

    /// <inheritdoc cref="GenerateTransactionPayloadDataWithAbi.FunctionArguments"/>
    public List<object>? FunctionArguments = functionArguments;

}

/// <summary>
/// Represents a payload data for an entry function.
/// </summary>
/// <param name="function">The function to be called on the module. This should be in the format of "address::module_name::function_name".</param>
/// <param name="functionArguments">A list of either strings or <see cref="TransactionArgument"/> objects representing the function arguments. If the function arguments are strings, they will be converted to <see cref="TransactionArgument"/> objects using the <see cref="TransactionArgument.ConvertArgument(object, TypeTag, List{TypeTag})"/> method.</param>
/// <param name="typeArguments">A list of <see cref="TypeTag"/> objects representing the type arguments of the function.</param>
/// <param name="abi">The ABI of the function that will be used to check the type and function arguments of the transaction.</param>
public class GenerateEntryFunctionPayloadData(string function, List<object>? functionArguments = null, List<object>? typeArguments = null, EntryFunctionAbi? abi = null) : GenerateTransactionPayloadDataWithAbi(function, functionArguments, typeArguments, abi)
{
    public GenerateEntryFunctionPayloadData(GenerateEntryFunctionPayloadData data, EntryFunctionAbi? abi = null) : this(data.Function, data.FunctionArguments, data.TypeArguments, abi) { }
}

public class GenerateMultisigPayloadData(string function, AccountAddress multisigAddress, List<object>? functionArguments = null, List<object>? typeArguments = null, EntryFunctionAbi? abi = null) : GenerateTransactionPayloadDataWithAbi(function, functionArguments, typeArguments, abi)
{
    /// <summary>
    /// The address of the multisig account.
    /// </summary>
    public AccountAddress MultisigAddress = multisigAddress;

    public GenerateMultisigPayloadData(string function, string multisigAddress, List<object>? typeArguments = null, List<object>? functionArguments = null, EntryFunctionAbi? abi = null) : this(function, AccountAddress.FromString(multisigAddress), functionArguments, typeArguments, abi) { }
}


public class GenerateScriptPayloadData(byte[] bytecode, List<IScriptFunctionArgument>? functionArguments = null, List<object>? typeArguments = null) : GeneratePayloadData(typeArguments)
{
    /// <summary>
    /// The bytecode of the script to be executed.
    /// </summary>
    public byte[] Bytecode = bytecode;

    /// <summary>
    /// A list of <see cref="IScriptFunctionArgument"/> objects representing the function arguments.
    /// </summary>
    public List<IScriptFunctionArgument>? FunctionArguments = functionArguments;

}
