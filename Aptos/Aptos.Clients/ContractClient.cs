namespace Aptos;

using Aptos.Core;

public class ContractClient(AptosClient client)
{
    private readonly AptosClient _client = client;

    public async Task<MoveFunction?> GetFunctionAbi(
        string moduleAddress,
        string moduleName,
        string functionName
    )
    {
        MoveModuleBytecode module = await _client.Account.GetModule(moduleAddress, moduleName);
        return module.Abi?.ExposedFunctions.Find(func => func.Name == functionName);
    }

    public async Task<EntryFunctionAbi> GetEntryFunctionAbi(
        string moduleAddress,
        string moduleName,
        string functionName,
        ulong? ledgerVersion = null
    )
    {
        var functionAbi = await GetFunctionAbi(moduleAddress, moduleName, functionName);

        // If there's no ABI, then the function is invalid
        if (functionAbi == null)
            throw new ArgumentException(
                $"Function not found for '{moduleAddress}::{moduleName}::{functionName}'"
            );

        // Non-entry functions also can't be used
        if (functionAbi.IsEntry == false)
            throw new ArgumentException(
                $"Function '{moduleAddress}::{moduleName}::{functionName}' is not an entry function"
            );

        // Remove the signers argument
        int firstNonSignerIndex = Utilities.FindFirstNonSignerIndex(functionAbi);
        List<TypeTag> parameters = [];
        for (int i = firstNonSignerIndex; i < functionAbi.Parameters.Count; i++)
            parameters.Add(TypeTag.Parse(functionAbi.Parameters[i], true));

        return new EntryFunctionAbi(
            signers: firstNonSignerIndex,
            functionAbi.GenericTypeParams,
            parameters
        );
    }

    public async Task<ViewFunctionAbi> GetViewFunctionAbi(
        string moduleAddress,
        string moduleName,
        string functionName
    )
    {
        var functionAbi = await GetFunctionAbi(moduleAddress, moduleName, functionName);

        // If there's no ABI, then the function is invalid
        if (functionAbi == null)
            throw new ArgumentException(
                $"Function not found for '{moduleAddress}::{moduleName}::{functionName}'"
            );

        // View functions can't be used
        if (functionAbi.IsView == false)
            throw new ArgumentException(
                $"Function '{moduleAddress}::{moduleName}::{functionName}' is not a view function"
            );

        // Type tag parameters for the function
        List<TypeTag> parameters = [];
        for (int i = 0; i < functionAbi.Parameters.Count; i++)
            parameters.Add(TypeTag.Parse(functionAbi.Parameters[i], true));

        // Return types for the function
        List<TypeTag> returnTypes = [];
        for (int i = 0; i < functionAbi.Return.Count; i++)
            returnTypes.Add(TypeTag.Parse(functionAbi.Return[i], true));

        return new ViewFunctionAbi(returnTypes, functionAbi.GenericTypeParams, parameters);
    }

    /// <summary>
    /// Calls the Move view function with the given payload and returns the result.
    /// </summary>
    /// <param name="data">The payload of the view function to call.</param>
    /// <param name="ledgerVersion">The ledger version to use for the view function call.</param>
    /// <returns>An array of Move values representing the result of the view function.</returns>
    public async Task<List<object>> View(
        GenerateViewFunctionPayloadData data,
        ulong? ledgerVersion = null
    ) => await View<List<object>>(data, ledgerVersion);

    /// <summary>
    /// Calls the Move view function with the given payload and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the result of the view function.</typeparam>
    /// <param name="data">The payload of the view function to call.</param>
    /// <param name="ledgerVersion">The ledger version to use for the view function call.</param>
    /// <returns>An instance of the specified type T representing the result of the view function.</returns>
    public async Task<T> View<T>(GenerateViewFunctionPayloadData data, ulong? ledgerVersion = null)
        where T : class
    {
        Dictionary<string, string> queryParams = [];
        if (ledgerVersion != null)
        {
            queryParams.Add("ledger_version", ledgerVersion.ToString()!);
        }

        var viewFunctionPayload = await TransactionBuilder.GenerateViewFunctionPayload(
            _client,
            data
        );
        var response = await _client.PostFullNode<T>(
            new(
                path: "view",
                originMethod: "view",
                body: viewFunctionPayload.BcsToBytes(),
                contentType: MimeType.BCS_VIEW_FUNCTION,
                queryParams: queryParams
            )
        );

        return response.Data;
    }
}
