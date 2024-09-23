Feature: SingleKey
"""
Single is a verification key used to authenticate a public key. This specification aims to covers SingleKey signatures, serialization, and authentication keys.

All key values are represented in BCS (Binary Canonical Serialization) format as a Hex string. It's important to use a Deserializer ensure compatibility.
"""

  Scenario Outline: SingleKey serialization
    Given <type> <key>
    When I serialize
    Then the result should be bcs <value>

    Examples:
      | type      | key                                                                                                                                    | value                                                                                                                                    |
      | ed25519   |                                                                   0x2066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace |                                                                   0x002066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace |
      | secp256k1 | 0x4104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 | 0x014104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 |
      | keyless   |           0x1b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 |           0x031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 |

  Scenario Outline: SingleKey authentication key
    Given <type> <key>
    When I derive authentication key
    Then the result should be bcs <value>

    Examples:
      | type      | key                                                                                                                                    | value                                                              |
      | ed25519   |                                                                   0x2066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace | 0x73312812b63e652652f5a2aecfcb42ec051923c3456329d9500eb0e919e3903d |
      | secp256k1 | 0x4104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 | 0xe6e936c0c9c41d8c1f0490de291789677d64467ef38f1fff7111cad9546eee44 |
      | keyless   |           0x1b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 | 0x3c38e3250bf5e446947a9fd4bdfd41e76c3812e8061abf40702febde99b68d51 |

  Scenario Outline: Verify signatures using SingleKey
    Given <type> <key>
    When I verify signature <signature> with message <message>
    Then the result should be bool <value>

    Examples:
      | type      | key                                                                                                                                    | signature                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | message     | value |
      | ed25519   |                                                                   0x2066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               0x40400461806f41581b75459e5db432e9d4da0881ac0ef0b7f8bb1c4b8e108d4ec2dd88abf5ff1e00616b2c338c375e245470e15d75f69aec465fa090001df19505 | hello world | true  |
      | ed25519   |                                                                   0x2066e6dde4a271c5fcc68e9bda668ac41ff67049ea75e4db103c7927934b1e4ace |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               0x40400461806f41581b75459e5db432e9d4da0881ac0ef0b7f8bb1c4b8e108d4ec2dd88abf5ff1e00616b2c338c375e245470e15d75f69aec465fa090001df19505 | N/A         | false |
      | secp256k1 | 0x4104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               0x40882b4ddbc14df439c222300c051d1039a62d36026ecd5789fd777224fef0ab7e07bb0891c5b411ec0938456a4b250067e8ff6c996d31f0186019521fc8f6fb25 | hello world | true  |
      | secp256k1 | 0x4104ffaf07a5268f92f86aa8be6a8047aaf2cceb33c3ac69628d9dde4e2ffdbe2d21551ecb9f69a6a79a509a7328a6641d81b4bb1fb39d4955549dad93d44ccc9d50 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               0x40882b4ddbc14df439c222300c051d1039a62d36026ecd5789fd777224fef0ab7e07bb0891c5b411ec0938456a4b250067e8ff6c996d31f0186019521fc8f6fb25 | N/A         | false |
      | keyless   |           0x1b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 | 0x000016e4ab03e4a1f199b26b32ecc1ad3f7d8676ed332eef87f29f1198582366af88a91fcb2cfd0ac6599bd720e8815de8a288cabd5c21706a7d675e80365e2e6f1056c753e59d1d597b41ac34e5db0334982bfd2fc1c8cc167edb866b9e78c0eda96155be3d53755fdc7922028d789546b3929b3c50cfdd290d86b3eae5b45c070f809698000000000000000100405f4891933c8ac11391f82a2ef6c51131da95c3cc56e48daca75e2153c13faa48050f893fb97d72719ef1a7872c0c5f164a670c0b8325b5608d3cbcca7d2ed20d4c7b22616c67223a225253323536222c226b6964223a2262323632306435653766313332623532616665383837356364663337373663303634323439643034222c22747970223a224a5754227dc03d04670000000000206c2adb07d4bdf1462fa4090578805f654c00d5316b600f2ac7b7a92f727016f6004063b9bd27870d78845daa78effc11fc0acbadaf377a5df2791fe45f4ea0d9f10ea267b73692e7870fce3d37288631b691baf920bef64cba2996259cbabf419600 | hello world | true  |
      | keyless   |           0x1b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 | 0x000016e4ab03e4a1f199b26b32ecc1ad3f7d8676ed332eef87f29f1198582366af88a91fcb2cfd0ac6599bd720e8815de8a288cabd5c21706a7d675e80365e2e6f1056c753e59d1d597b41ac34e5db0334982bfd2fc1c8cc167edb866b9e78c0eda96155be3d53755fdc7922028d789546b3929b3c50cfdd290d86b3eae5b45c070f809698000000000000000100405f4891933c8ac11391f82a2ef6c51131da95c3cc56e48daca75e2153c13faa48050f893fb97d72719ef1a7872c0c5f164a670c0b8325b5608d3cbcca7d2ed20d4c7b22616c67223a225253323536222c226b6964223a2262323632306435653766313332623532616665383837356364663337373663303634323439643034222c22747970223a224a5754227dc03d04670000000000206c2adb07d4bdf1462fa4090578805f654c00d5316b600f2ac7b7a92f727016f6004063b9bd27870d78845daa78effc11fc0acbadaf377a5df2791fe45f4ea0d9f10ea267b73692e7870fce3d37288631b691baf920bef64cba2996259cbabf419600 | N/A         | false |
