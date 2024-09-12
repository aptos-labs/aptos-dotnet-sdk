namespace Aptos;

public static class MimeType
{
    /// <summary>
    /// JSON representation, used for transaction submission and accept type JSON output
    /// </summary>
    public const string JSON = "application/json";

    /// <summary>
    /// BCS representation, used for accept type BCS output
    /// </summary>
    public const string BCS = "application/x-bcs";

    /// <summary>
    /// BCS representation, used for transaction submission in BCS input
    /// </summary>
    public const string BCS_SIGNED_TRANSACTION = "application/x.aptos.signed_transaction+bcs";

    /// <summary>
    /// BCS representation, used for view function in BCS
    /// </summary>
    public const string BCS_VIEW_FUNCTION = "application/x.aptos.view_function+bcs";
}
