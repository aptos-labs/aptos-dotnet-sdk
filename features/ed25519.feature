Feature: Ed25519
"""
Ed25519 is a public-key cryptographic algorithm used for digital signatures. This specification aims to cover Ed25519 signatures and key generation.

All key values are represented in BCS (Binary Canonical Serialization) format as a Hex string. It's important to use a Deserializer ensure compatibility.
"""

  Scenario Outline: Ed25519 private keys derive correct public keys
    Given private key <key>
    When I derive public key
    Then the result should be hex <value>

    Examples:
      | key                                                                  | value                                                                |
      | 0x2020178e71a0b74579625eae1df7d2e72d92d8b83aa5816ccca2de6a69fc4136ed | 0x2066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace |
      | 0x209ab467852bd91823d362b4ebf6665b15120c3b4ba9d6d4481fd8e0d0f90621bb | 0x204c78bd74a2d41a6f5caf3e521c67972a113908658a8205aba55e59754ed507d5 |
      | 0x2000c2f07b3daa77ec1dcac5216ff18a2d269b052b046dc78db293d110911dfb2d | 0x205332101b2f5102d1852259da230acccf0ddb1b610cabd4222b142b620997706e |

  Scenario Outline: Ed25519 private keys sign messages
    Given private key <key>
    When I sign message <message>
    Then the result should be hex <value>

    Examples:
      | key                                                                  | message     | value                                                                                                                                |
      | 0x2020178e71a0b74579625eae1df7d2e72d92d8b83aa5816ccca2de6a69fc4136ed | hello world | 0x40400461806f41581b75459e5db432e9d4da0881ac0ef0b7f8bb1c4b8e108d4ec2dd88abf5ff1e00616b2c338c375e245470e15d75f69aec465fa090001df19505 |
      | 0x209ab467852bd91823d362b4ebf6665b15120c3b4ba9d6d4481fd8e0d0f90621bb | aptos       | 0x40477edbb1bef42b709c6c1883a13709dc7342e6d8239a3618315152e008a31a3a6f0a77a70100bbf40ad75c96da06528a8ec3239c6af34b1a77834cd586e4c606 |
      | 0x2000c2f07b3daa77ec1dcac5216ff18a2d269b052b046dc78db293d110911dfb2d | random      | 0x40316d8c30fdc7ccf7a35f2a263985961916908b77631c1a8ea91aa7e321a8e9fe2c217261cd737315299a801be6b8d8562e1bcdd2ffe059aa43c241015725a20e |

  Scenario Outline: Verify signatures using Ed25519 private keys
    Given private key <key>
    When I verify signature <signature> with message <message>
    Then the result should be bool <value>

    Examples:
      | key                                                                  | signature                                                                                                                            | message     | value |
      | 0x2020178e71a0b74579625eae1df7d2e72d92d8b83aa5816ccca2de6a69fc4136ed | 0x40400461806f41581b75459e5db432e9d4da0881ac0ef0b7f8bb1c4b8e108d4ec2dd88abf5ff1e00616b2c338c375e245470e15d75f69aec465fa090001df19505 | hello world | true  |
      | 0x209ab467852bd91823d362b4ebf6665b15120c3b4ba9d6d4481fd8e0d0f90621bb | 0x40477edbb1bef42b709c6c1883a13709dc7342e6d8239a3618315152e008a31a3a6f0a77a70100bbf40ad75c96da06528a8ec3239c6af34b1a77834cd586e4c606 | aptos       | true  |
      | 0x2000c2f07b3daa77ec1dcac5216ff18a2d269b052b046dc78db293d110911dfb2d | 0x40316d8c30fdc7ccf7a35f2a263985961916908b77631c1a8ea91aa7e321a8e9fe2c217261cd737315299a801be6b8d8562e1bcdd2ffe059aa43c241015725a20e | experience  | false |

  Scenario Outline: Derive Ed25519 private key from derivation path
    Given mnemonic <mnemonic>
    When I derive from derivation path <path>
    Then the result should be hex <value>

    Examples:
      | mnemonic                                                                         | path                | value                                                                |
      | claw swim mixed dance neck shop wool stool swarm inch umbrella universe          | m/44'/637'/0'/0'/0' | 0x2065f366fad2ea26bc88af6022f4acc56a0fdce58566b4696f784fdee046a63785 |
      | pumpkin cousin pyramid pull announce steak mom junior method present knee reform | m/44'/637'/0'/0'/0' | 0x20f3d9b99bae2674f65a857a8c6d24456806a23f1a6a8791721f6a295582bfdd01 |
      | bright similar note plastic wheel tide daughter desk silver rifle uncle alien    | m/44'/637'/0'/0'/0' | 0x208b672952f3e3c0db2fa94ff1c00c5f6a09a32289d55ac075af6ed126c7fdcd65 |
