using System.Text.RegularExpressions;
using Aptos.Exceptions;

namespace Aptos;

partial class TypeTag
{
    public class TypeTagState(int savedExpectedTypes, string savedStr, List<TypeTag> savedTypes)
    {
        public int SavedExpectedTypes = savedExpectedTypes;
        public string SavedStr = savedStr;
        public List<TypeTag> SavedTypes = savedTypes;
    }

    /// <summary>
    /// All types are made of a few parts they're either:
    /// 1. A simple type e.g. u8
    /// 2. A standalone struct e.g. 0x1::account::Account
    /// 3. A nested struct e.g. 0x1::coin::Coin<0x1234::coin::MyCoin>
    ///
    /// There are a few more special cases that need to be handled, however.
    /// 1. Multiple generics e.g 0x1::pair::Pair<u8, u16>
    /// 2. Spacing in the generics e.g. 0x1::pair::Pair< u8 , u16>
    /// 3. Nested generics of different depths e.g. 0x1::pair::Pair<0x1::coin::Coin<0x1234::coin::MyCoin>, u8>
    /// 4. Generics for types in ABIs are filled in with placeholders e.g T1, T2, T3
    /// </summary>
    /// <param name="typeStr">The string representation of the type</param>
    /// <param name="allowGenerics">Whether or not to allow generics in the type</param>
    /// <returns>The parsed type</returns>
    public static TypeTag Parse(string typeStr, bool allowGenerics = false)
    {
        Stack<TypeTagState> saved = [];
        // This represents the internal types for a type tag e.g. '0x1::coin::Coin<innerTypes>'
        List<TypeTag> innerTypes = [];
        // This represents the current parsed types in a comma list e.g. 'u8, u8'
        List<TypeTag> curTypes = [];
        // This represents the current character index
        int cur = 0;
        // This represents the current working string as a type or struct name
        string currentStr = "";
        int expectedTypes = 1;

        // Iterate through each character, and handle the border conditions
        while (cur < typeStr.Length)
        {
            char c = typeStr[cur];

            if (c == '<')
            {
                // Start of a type argument, push current state onto a stack
                saved.Push(new TypeTagState(expectedTypes, currentStr, curTypes));

                // Clear the current state
                curTypes = [];
                currentStr = "";
                expectedTypes = 1;
            }
            else if (c == '>')
            {
                // Process last type, if there is no type string, then don't parse it
                if (currentStr != "")
                {
                    TypeTag newType = ParseType(currentStr, innerTypes, allowGenerics);
                    curTypes.Add(newType);
                }

                // Pop off the outer brace
                bool hasPopped = saved.TryPop(out TypeTagState? savedPop);

                // If there's nothing left, there were too many '>'
                if (!hasPopped || savedPop == null)
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.UnexpectedTypeArgumentClose
                    );

                // If the expected types don't match the number of commas, then we also fail
                if (expectedTypes != curTypes.Count)
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.TypeArgumentCountMismatch
                    );

                // Add in the new created type, shifting the current types to the inner types
                innerTypes = curTypes;
                curTypes = savedPop.SavedTypes;
                currentStr = savedPop.SavedStr;
                expectedTypes = savedPop.SavedExpectedTypes;
            }
            else if (c == ',')
            {
                // Commas means we need to start parsing a new tag, push the previous one to the curTypes

                // No top level commas (not in a type <> are allowed)
                if (saved.Count == 0)
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.UnexpectedComma
                    );
                // If there was no actual value before the comma, then it's missing a type argument
                if (currentStr.Length == 0)
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.MissingTypeArgument
                    );

                // Process characters before as a type
                TypeTag newType = ParseType(currentStr, innerTypes, allowGenerics);

                // Parse type tag and push it on the types
                innerTypes = [];
                curTypes.Add(newType);
                currentStr = "";
                expectedTypes += 1;
            }
            else if (WhitespaceRegex().IsMatch(c.ToString()))
            {
                // This means we should save what we have and everything else should skip until the next
                bool parsedTypeTag = false;
                if (currentStr.Length != 0)
                {
                    TypeTag newType = ParseType(currentStr, innerTypes, allowGenerics);

                    // Parse type tag and push it on the types
                    innerTypes = [];
                    curTypes.Add(newType);
                    currentStr = "";
                    parsedTypeTag = true;
                }

                // Skip ahead on any more whitespace
                cur = ConsumeWhitespace(typeStr, cur);

                if (cur >= typeStr.Length)
                    continue;

                // The next space MUST be a comma, or a closing > if there was something parsed before
                // e.g. `u8 u8` is invalid but `u8, u8` is valid
                char nextChar = typeStr[cur];
                if (cur < typeStr.Length && parsedTypeTag && nextChar != ',' && nextChar != '>')
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.UnexpectedWhitespaceCharacter
                    );

                continue;
            }
            else
            {
                // Any other characters just append to the current string
                currentStr += c;
            }

            cur += 1;
        }

        // This prevents a missing '>' on type arguments
        if (saved.Count > 0)
            throw new TypeTagParserException(
                typeStr,
                TypeTagParserExceptionReason.MissingTypeArgumentClose
            );

        // This prevents `u8, u8` as an input
        switch (curTypes.Count)
        {
            case 0:
                return ParseType(currentStr, innerTypes, allowGenerics);
            case 1:
                if (currentStr != "")
                    throw new TypeTagParserException(
                        typeStr,
                        TypeTagParserExceptionReason.UnexpectedComma
                    );
                return curTypes[0];
            default:
                throw new TypeTagParserException(
                    typeStr,
                    TypeTagParserExceptionReason.UnexpectedWhitespaceCharacter
                );
        }
    }

    /// <summary>
    /// Parses a type tag with internal types associated
    /// </summary>
    /// <param name="str">The string representation of the type</param>
    /// <param name="types">The internal types associated with the type</param>
    /// <param name="allowGenerics">Whether or not to allow generics in the type</param>
    /// <returns>The parsed type</returns>
    public static TypeTag ParseType(string str, List<TypeTag> types, bool allowGenerics)
    {
        string trimmedStr = str.Trim();
        if (IsPrimitive(trimmedStr))
        {
            if (types.Count > 0)
                throw new TypeTagParserException(
                    str,
                    TypeTagParserExceptionReason.UnexpectedPrimitiveTypeArguments
                );
        }

        switch (str.ToLower())
        {
            case "signer":
                return new TypeTagSigner();
            case "bool":
                return new TypeTagBool();
            case "address":
                return new TypeTagAddress();
            case "u8":
                return new TypeTagU8();
            case "u16":
                return new TypeTagU16();
            case "u32":
                return new TypeTagU32();
            case "u64":
                return new TypeTagU64();
            case "u128":
                return new TypeTagU128();
            case "u256":
                return new TypeTagU256();
            case "vector":
                if (types.Count != 1)
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.UnexpectedVectorTypeArgumentCount
                    );
                return new TypeTagVector(types[0]);
            default:
                // Reference will have to handle the inner type
                if (RefTypeRegex().IsMatch(trimmedStr))
                {
                    string actualType = trimmedStr.Substring(1);
                    return new TypeTagReference(ParseType(actualType, types, allowGenerics));
                }

                // Generics are always expected to be T0 or T1
                if (GenericTypeRegex().IsMatch(trimmedStr))
                {
                    if (!allowGenerics)
                        throw new TypeTagParserException(
                            str,
                            TypeTagParserExceptionReason.UnexpectedGenericType
                        );
                    return new TypeTagGeneric(uint.Parse(trimmedStr.Split("T")[1]));
                }

                // If the value doesn't contain a colon, then we'll assume it isn't trying to be a struct
                if (!trimmedStr.Contains(':'))
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.InvalidTypeTag
                    );

                // Parse for a struct tag

                string[] structParts = trimmedStr.Split("::");
                if (structParts.Length != 3)
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.UnexpectedStructFormat
                    );

                // Validate struct address
                AccountAddress? address;
                try
                {
                    address = AccountAddress.FromString(structParts[0]);
                }
                catch
                {
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.InvalidAddress
                    );
                }

                // Validate identifier characters
                if (!ValidIdentifierRegex().IsMatch(structParts[1]))
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.InvalidModuleNameCharacter
                    );
                if (!ValidIdentifierRegex().IsMatch(structParts[2]))
                    throw new TypeTagParserException(
                        str,
                        TypeTagParserExceptionReason.InvalidStructNameCharacter
                    );

                return new TypeTagStruct(
                    new StructTag(address, structParts[1], structParts[2], types)
                );
        }
    }

    private static int ConsumeWhitespace(string tagStr, int pos)
    {
        int i = pos;
        for (; i < tagStr.Length; i += 1)
        {
            char innerChar = tagStr[i];

            if (!WhitespaceRegex().IsMatch(innerChar.ToString()))
            {
                // If it's not colons, and it's an invalid character, we will stop here
                break;
            }
        }
        return i;
    }

    private static bool IsPrimitive(string str) =>
        str.ToLower() switch
        {
            "bool" => true,
            "u8" => true,
            "u16" => true,
            "u32" => true,
            "u64" => true,
            "u128" => true,
            "u256" => true,
            "address" => true,
            "signer" => true,
            _ => false,
        };

    /// <summary>
    /// Tells if the string is a valid Move identifier. It can only be alphanumeric and '_'.
    /// </summary>
    private static Regex ValidIdentifierRegex() => new(@"^[_a-zA-Z0-9]+$");

    /// <summary>
    /// Tells if a type is a generic type from the ABI, this will be of the form T0, T1, ...
    /// </summary>
    private static Regex GenericTypeRegex() => new(@"^T[0-9]+$");

    /// <summary>
    /// Tells if the character is a whitespace character. Does not work for multiple characters.
    /// </summary>
    private static Regex WhitespaceRegex() => new(@"\s");

    /// <summary>
    /// Tells if a type is a reference type (starts with &).
    /// </summary>
    private static Regex RefTypeRegex() => new(@"^&.+$");
}
