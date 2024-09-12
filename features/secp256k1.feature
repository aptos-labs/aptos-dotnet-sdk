Feature: Secp256k1
"""
Secp256k1 is a public-key cryptographic algorithm used for digital signatures. This specification aims to cover Secp256k1 signatures and key generation.

All key values are represented in BCS (Binary Canonical Serialization) format as a Hex string. It's important to use a Deserializer ensure compatibility.
"""

  Scenario Outline: Secp256k1 private keys derive correct public keys
    Given private key <key>
    When I derive public key
    Then the result should be hex <value>

    Examples:
      | key                                                                  | value                                                                                                                                  |
      | 0x20009e455c271746a009bfa80525aede0c3c75cc4dedf996fd4df2e3d72707eb0d | 0x4104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 |
      | 0x20337367d6a7c1b80ee3465eca7234bd30b9ededb742aab54a9fccc3e657a466e3 | 0x41042c203adf73f73adc79b3aa8c1d26d859aeac720c638b21d5bb189af16b2ed24836e698b53650f806be0ac2a87f4868e99dd404cd3785439607595c9d2cf90fc1 |
      | 0x20ec9fcd3c3b269ccf8d5f1abddd792ed7445b7c5e193486bfe1d019bb713bebdb | 0x41041e832c2c067ad83003e697842a9567a7b7d3a9740aa84efbcda8cd13ac44feb5ff1ebe3b76d931541b5e4b702e25d3cc504369eb191c52871de0ecef3043fd15 |

  Scenario Outline: Secp256k1 private keys sign messages
    Given private key <key>
    When I sign message <message>
    Then the result should be hex <value>

    Examples:
      | key                                                                  | message     | value                                                                                                                                |
      | 0x20009e455c271746a009bfa80525aede0c3c75cc4dedf996fd4df2e3d72707eb0d | hello world | 0x40882b4ddbc14df439c222300c051d1039a62d36026ecd5789fd777224fef0ab7e07bb0891c5b411ec0938456a4b250067e8ff6c996d31f0186019521fc8f6fb25 |
      | 0x20337367d6a7c1b80ee3465eca7234bd30b9ededb742aab54a9fccc3e657a466e3 | aptos       | 0x40d7e57555345ab41f3e936a49ffa1c47a9857fbc3041b5df5fbb9e6bf93304b540445031001c4d8c328632878641354d0323fe5dfcb91d9cb305261024f031678 |
      | 0x20ec9fcd3c3b269ccf8d5f1abddd792ed7445b7c5e193486bfe1d019bb713bebdb | random      | 0x40e593a7eeee0e8007398f2ef1512a3ab3fdcc0bb2d2536a21a15cfa5d7034753043e4e728ceb150d68a90dc22666b15897cb1918caac736d4576b5aab5747466d |

  Scenario Outline: Verify signatures using Secp256k1 private keys
    Given private key <key>
    When I verify signature <signature> with message <message>
    Then the result should be bool <value>

    Examples:
      | key                                                                  | signature                                                                                                                            | message     | value |
      | 0x20009e455c271746a009bfa80525aede0c3c75cc4dedf996fd4df2e3d72707eb0d | 0x40882b4ddbc14df439c222300c051d1039a62d36026ecd5789fd777224fef0ab7e07bb0891c5b411ec0938456a4b250067e8ff6c996d31f0186019521fc8f6fb25 | hello world | true  |
      | 0x20337367d6a7c1b80ee3465eca7234bd30b9ededb742aab54a9fccc3e657a466e3 | 0x40d7e57555345ab41f3e936a49ffa1c47a9857fbc3041b5df5fbb9e6bf93304b540445031001c4d8c328632878641354d0323fe5dfcb91d9cb305261024f031678 | aptos       | true  |
      | 0x20ec9fcd3c3b269ccf8d5f1abddd792ed7445b7c5e193486bfe1d019bb713bebdb | 0x40e593a7eeee0e8007398f2ef1512a3ab3fdcc0bb2d2536a21a15cfa5d7034753043e4e728ceb150d68a90dc22666b15897cb1918caac736d4576b5aab5747466d | experience  | false |

  Scenario Outline: Derive Secp256k1 private key from derivation path
    Given mnemonic <mnemonic>
    When I derive from derivation path <path>
    Then the result should be hex <value>

    Examples:
      | mnemonic                                                                         | path              | value                                                                |
      | claw swim mixed dance neck shop wool stool swarm inch umbrella universe          | m/44'/637'/0'/0/0 | 0x20a6545793f6d2dea9f8394c0ee71c7b2ba9e35adf8f9081c2243b69d6d3072b4b |
      | pumpkin cousin pyramid pull announce steak mom junior method present knee reform | m/44'/637'/0'/0/0 | 0x206eaa191a8649444398b565c21cbeb32dc83bbb19c01f9323c5747e31d0e980de |
      | bright similar note plastic wheel tide daughter desk silver rifle uncle alien    | m/44'/637'/0'/0/0 | 0x20ae803c3ce20f7b01bcb401588cfb9faea05e470fdb2a83f2720780b348962443 |
