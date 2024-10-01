Feature: MultiKey
"""
MultiKey is a verification key used to authenticate a collection of public keys. This specification aims to covers SingleKey signatures, serialization, and authentication keys.

All key values are represented in BCS (Binary Canonical Serialization) format as a Hex string. It's important to use a Deserializer ensure compatibility.
"""

  Scenario Outline: MultiKey serialization
    Given public_keys <types>|<keys>|<sigs_required>
    When I serialize
    Then the result should be bcs <value>

    Examples:
      | types                     | keys                                                                                                                                                                                                                                                                                                                                     | sigs_required | value                                                                                                                                                                                                                                                                                                                                        |
      | ed25519                   |                                                                                                                                                                                                                                                                     0x200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0 |             1 |                                                                                                                                                                                                                                                                   0x0100200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d001 |
      | ed25519,secp256k1         |                                                                                                                              0x200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0,0x4104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1 |             2 |                                                                                                                             0x0200200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c102 |
      | ed25519,secp256k1,keyless | 0x200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0,0x4104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1,0x1b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d668814 |             3 | 0x0300200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d66881403 |

  Scenario Outline: MultiKey authentication key
    Given multikey <key>
    When I derive authentication key
    Then the result should be bcs <value>

    Examples:
      | key                                                                                                                                                                                                                                                                                                                                          | value                                                              |
      |                                                                                                                                                                                                                                                                   0x0100200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d001 | 0xeae954a9632b281c3857a829c9b675aaea33c509715b970674696021ace47b80 |
      |                                                                                                                             0x0200200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c102 | 0x01c881e216e1d932e4234d4dd94f72e2d73a603378a8b8f8729a38158513a46a |
      | 0x0300200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d66881403 | 0x466a486fe03922d9bc238814f2172708aad1ec7d5e01e81f93e833640250c509 |

  Scenario Outline: Verify signatures using MultiKey
    Given multikey <key>
    When I verify signature <signature> with message <message>
    Then the result should be bool <value>

    Examples:
      | key                                                                                                                                                                                                                                                                                                                                          | signature                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | message     | value |
      |                                                                                                                                                                                                                                                                   0x0100200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d001 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       0x010040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040480000000 | hello world | true  |
      |                                                                                                                                                                                                                                                                   0x0100200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d001 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       0x010040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040480000000 | N/A         | false |
      |                                                                                                                             0x0200200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c102 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   0x020040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e04c0000000 | hello world | true  |
      |                                                                                                                             0x0200200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c102 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   0x020040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e04c0000000 | N/A         | false |
      | 0x0300200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d66881403 | 0x030040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e03000054703992d7100d497f6d8592c11b9c21bad4df6eb550e0f6733a69a40ef7f6822671b7441d1f94e41a553449748d3f92fb5c0c9cf975118e70c04ee26c259015cc37817ee103df84cd7d1164ddcf6d183e82048aa94a1d99f593d1ed0f0d3190178b03ee384ce48da6924a7298c55116a982bf83f9b02f1f3b9a149e7b69e7828096980000000000000001004081fac171fab172ed87bba2e2fab0f100c3661904acef65ead94d246306f2a56ccdbf05770055880559ad99066e3d93a751e650c68e85e323af5ed0eb7063d6014c7b22616c67223a225253323536222c226b6964223a2235616166663437633231643036653236366363653339356232313435633763366434373330656135222c22747970223a224a5754227d60940d67000000000020d582fe23fc7a311fd24eaab42009643a76d1e89690111c00fea0cd270c3b57f7004078dd355b81fcb4d566f98c6bbb4cba484c9433ed769fe6fb60a614103f5846d46d09a8534a63150e565bd461570bd84d0bda0d3066498f06e6de725cde114f0204e0000000 | hello world | true  |
      | 0x0300200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d66881403 | 0x030040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e03000054703992d7100d497f6d8592c11b9c21bad4df6eb550e0f6733a69a40ef7f6822671b7441d1f94e41a553449748d3f92fb5c0c9cf975118e70c04ee26c259015cc37817ee103df84cd7d1164ddcf6d183e82048aa94a1d99f593d1ed0f0d3190178b03ee384ce48da6924a7298c55116a982bf83f9b02f1f3b9a149e7b69e7828096980000000000000001004081fac171fab172ed87bba2e2fab0f100c3661904acef65ead94d246306f2a56ccdbf05770055880559ad99066e3d93a751e650c68e85e323af5ed0eb7063d6014c7b22616c67223a225253323536222c226b6964223a2235616166663437633231643036653236366363653339356232313435633763366434373330656135222c22747970223a224a5754227d60940d67000000000020d582fe23fc7a311fd24eaab42009643a76d1e89690111c00fea0cd270c3b57f7004078dd355b81fcb4d566f98c6bbb4cba484c9433ed769fe6fb60a614103f5846d46d09a8534a63150e565bd461570bd84d0bda0d3066498f06e6de725cde114f0204e0000000 | N/A         | false |

  Scenario Outline: Signing using MultiKey
    Given multikey_account <key>|<signer_types>|<signers>
    When I sign message <message>
    Then the result should be bcs <value>

    Examples:
      | key                                                                                                                                                                                                                                                                                                                                          | signer_types                                           | signers                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | message     | value                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
      |                                                                                                                                                                                                                                                                   0x0100200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d001 | ed25519_ed25519_pk                                     |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 0x20d47eaa8e6e887557479181e8b712558cf01b45aec598a029f62bd597952add0c | hello world |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       0x010040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040480000000 |
      |                                                                                                                             0x0200200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c102 | ed25519_ed25519_pk,single_secp256k1_pk                 |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            0x20d47eaa8e6e887557479181e8b712558cf01b45aec598a029f62bd597952add0c,0x208d2fd975b327c79099692c9541e168822723440bdfa00477538aa459b0cdac87 | hello world |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   0x020040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e04c0000000 |
      | 0x0300200f219f051bb5741bd220f970a1811257f84ac07333d293edbbe1074ed63680d0014104f73b017dc2c6dedc9227c0a69b73a06c30b20cca757c511af2ebc8047cc8ebb30f642ad8485c131278dadec8d5004895dd5d41953b1c417eb4fb0cbf4e1e34c1031b68747470733a2f2f6163636f756e74732e676f6f676c652e636f6d20efbfb94b89da565468e11b1a7d29eb3be1032637ec8d21b61f5220511d66881403 | ed25519_ed25519_pk,single_secp256k1_pk,account_keyless | 0x20d47eaa8e6e887557479181e8b712558cf01b45aec598a029f62bd597952add0c,0x208d2fd975b327c79099692c9541e168822723440bdfa00477538aa459b0cdac87,0xe80665794a68624763694f694a53557a49314e694973496d74705a434936496a566859575a6d4e44646a4d6a466b4d445a6c4d6a593259324e6c4d7a6b31596a49784e44566a4e324d325a4451334d7a426c595455694c434a30655841694f694a4b5631516966512e65794a7063334d694f694a6f64485277637a6f764c32466a59323931626e527a4c6d6476623264735a53356a623230694c434a68656e41694f6949304d4463304d4467334d5467784f544975595842776379356e6232396e624756316332567959323975644756756443356a623230694c434a68645751694f6949304d4463304d4467334d5467784f544975595842776379356e6232396e624756316332567959323975644756756443356a623230694c434a7a645749694f6949784d5459774e7a45794d7a63344d6a55314e6a637a4f5441334d4445694c434a686446396f59584e6f496a6f6951334a785132315a58336c4e656c56716358566e4d6c564d5a57316155534973496d3576626d4e6c496a6f694d5463304d4463344f444d354e444d774e546b774f5455334d4467314e6a51784e7a4d314f5445774d6a67304e7a59334d5441314d444d794e4441314e6a49324d7a4d334e4467354f446b334d5451774f4449354e7a63794d7a4d794f4459344e7a67784e694973496d6c68644349364d5463794e7a637a4e6a59774f4377695a586877496a6f784e7a49334e7a51774d6a413466512e64625f545f6d344d36756b385555466f31353544323053615142774b4d4543635744544632594d376c73497768415a70756e5274727243507449396558706b68647a55664c577a776a5830547155316e62576d5045475546654578366364336d45333948314533305f4a4c625451546c64586d724c7364416e39764f525f4551314138327a684d6e625f69684f456c535776464f6f5a733778626a385a4e63774f2d75444d4c3358487869496d66634a416c6b455a4251613854726b37617155616c496568674e744b5038797a556531787835775a6b552d384b614b544168753055496478624f7a49493249324e694b494571794e757567714c5475383332494b554d595442536c334739696474427078706862594c3463394e754e5f63374b7a656d30663038494977357764383171477a3455714d693969784e704846566e6d7059694c72694f4279434744776e7473775552484103737562cee71b4107ab42fd8c6b76d3804aac22c56576518cf0d20035a94a21f50a3100200a9ca553253084a6f5d6502735b6f031a82713751db6880a7e2317cd9d338a0460940d6700000000dfe1e844534c64b7877c4cb6b70bce58eb80b159c2cdd71c9a4739be927c7e0054703992d7100d497f6d8592c11b9c21bad4df6eb550e0f6733a69a40ef7f6822671b7441d1f94e41a553449748d3f92fb5c0c9cf975118e70c04ee26c259015cc37817ee103df84cd7d1164ddcf6d183e82048aa94a1d99f593d1ed0f0d3190178b03ee384ce48da6924a7298c55116a982bf83f9b02f1f3b9a149e7b69e7828096980000000000000001004081fac171fab172ed87bba2e2fab0f100c3661904acef65ead94d246306f2a56ccdbf05770055880559ad99066e3d93a751e650c68e85e323af5ed0eb7063d601 | hello world | 0x030040677f502c84bc57f0a803b240a7b880d459f396eb1f977af13a2122709e99d1766133cee322f4d60b71b5c7edf296933d3871e3e595ef0a7eb1683b1d144280040140aed6f05e7584612b8b362702b2c42c16e94360a4554f50a279552c30b91cac3f22c645e502833d81fb5e82fe5789b40c358fcd69c98e80e01dde276bb5818a0e03000054703992d7100d497f6d8592c11b9c21bad4df6eb550e0f6733a69a40ef7f6822671b7441d1f94e41a553449748d3f92fb5c0c9cf975118e70c04ee26c259015cc37817ee103df84cd7d1164ddcf6d183e82048aa94a1d99f593d1ed0f0d3190178b03ee384ce48da6924a7298c55116a982bf83f9b02f1f3b9a149e7b69e7828096980000000000000001004081fac171fab172ed87bba2e2fab0f100c3661904acef65ead94d246306f2a56ccdbf05770055880559ad99066e3d93a751e650c68e85e323af5ed0eb7063d6014c7b22616c67223a225253323536222c226b6964223a2235616166663437633231643036653236366363653339356232313435633763366434373330656135222c22747970223a224a5754227d60940d67000000000020d582fe23fc7a311fd24eaab42009643a76d1e89690111c00fea0cd270c3b57f7004078dd355b81fcb4d566f98c6bbb4cba484c9433ed769fe6fb60a614103f5846d46d09a8534a63150e565bd461570bd84d0bda0d3066498f06e6de725cde114f0204e0000000 |
