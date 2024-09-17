namespace Aptos.Exceptions
{
    public enum TypeTagParserExceptionReason
    {
        InvalidTypeTag,
        UnexpectedGenericType,
        UnexpectedTypeArgumentClose,
        UnexpectedWhitespaceCharacter,
        UnexpectedComma,
        TypeArgumentCountMismatch,
        MissingTypeArgumentClose,
        MissingTypeArgument,
        UnexpectedPrimitiveTypeArguments,
        UnexpectedVectorTypeArgumentCount,
        UnexpectedStructFormat,
        InvalidModuleNameCharacter,
        InvalidStructNameCharacter,
        InvalidAddress,
    }

    public class TypeTagParserException(string typeTagStr, TypeTagParserExceptionReason reason)
        : BaseException($"Failed to parse TypeTag '{typeTagStr}', {GetInvalidReason(reason)}")
    {
        private static string GetInvalidReason(TypeTagParserExceptionReason? typeTag) =>
            typeTag switch
            {
                TypeTagParserExceptionReason.InvalidTypeTag => "unknown type",
                TypeTagParserExceptionReason.UnexpectedGenericType => "unexpected generic type",
                TypeTagParserExceptionReason.UnexpectedTypeArgumentClose => "unexpected '>'",
                TypeTagParserExceptionReason.UnexpectedWhitespaceCharacter =>
                    "unexpected whitespace character",
                TypeTagParserExceptionReason.UnexpectedComma => "unexpected ','",
                TypeTagParserExceptionReason.TypeArgumentCountMismatch =>
                    "type argument count doesn't match expected amount",
                TypeTagParserExceptionReason.MissingTypeArgumentClose => "no matching '>' for '<'",
                TypeTagParserExceptionReason.MissingTypeArgument => "no type argument before ','",
                TypeTagParserExceptionReason.UnexpectedPrimitiveTypeArguments =>
                    "primitive types not expected to have type arguments",
                TypeTagParserExceptionReason.UnexpectedVectorTypeArgumentCount =>
                    "vector type expected to have exactly one type argument",
                TypeTagParserExceptionReason.UnexpectedStructFormat =>
                    "unexpected struct format, must be of the form 0xaddress::module_name::struct_name",
                TypeTagParserExceptionReason.InvalidModuleNameCharacter =>
                    "module name must only contain alphanumeric or '_' characters",
                TypeTagParserExceptionReason.InvalidStructNameCharacter =>
                    "struct name must only contain alphanumeric or '_' characters",
                TypeTagParserExceptionReason.InvalidAddress => "struct address must be valid",
                _ => "unknown error",
            };
    }
}
