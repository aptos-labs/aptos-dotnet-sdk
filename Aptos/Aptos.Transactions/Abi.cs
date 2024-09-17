namespace Aptos;

public class FunctionAbi(
    List<MoveFunctionGenericTypeParam> typeParameters,
    List<TypeTag> parameters
)
{
    public List<MoveFunctionGenericTypeParam> TypeParameters = typeParameters;
    public List<TypeTag> Parameters = parameters;
}

public class EntryFunctionAbi(
    int signers,
    List<MoveFunctionGenericTypeParam> typeParameters,
    List<TypeTag> parameters
) : FunctionAbi(typeParameters, parameters)
{
    public int Signers = signers;
}

public class ViewFunctionAbi(
    List<TypeTag> returnTypes,
    List<MoveFunctionGenericTypeParam> typeParameters,
    List<TypeTag> parameters
) : FunctionAbi(typeParameters, parameters)
{
    public List<TypeTag> ReturnTypes = returnTypes;
}
