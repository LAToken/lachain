using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Protobuf;
using Org.BouncyCastle.Math;
using Phorkus.Core.Blockchain;
using Phorkus.Core.Storage;
using Phorkus.Crypto;
using Phorkus.Hermes;
using Phorkus.Hermes.Crypto;
using Phorkus.Hermes.Crypto.Key;
using Phorkus.Hermes.Signer;
using Phorkus.Logger;
using Phorkus.Networking;
using Phorkus.Proto;
using Phorkus.Utility.Utils;

namespace Phorkus.Core.Threshold
{
    public class ThresholdManager : IThresholdManager
    {
        private readonly IGlobalRepository _globalRepository;
        private readonly ICrypto _crypto;
        private readonly IValidatorManager _validatorManager;
        private readonly INetworkContext _networkContext;
        private readonly ILogger<IThresholdManager> _logger;

        private readonly IPartyManager _partyManager
            = new PartyManager();

        private readonly IDictionary<PublicKey, SignerState> _signerStates
            = new Dictionary<PublicKey, SignerState>();

        private byte[][][] _messagePerValidator;

        private readonly object _messageChanged = new object();

        public ThresholdManager(
            IGlobalRepository globalRepository,
            ICrypto crypto,
            IValidatorManager validatorManager,
            ILogger<IThresholdManager> logger,
            INetworkContext networkContext
        )
        {
            _globalRepository = globalRepository;
            _crypto = crypto;
            _validatorManager = validatorManager;
            _logger = logger;
            _networkContext = networkContext;
            _messagePerValidator = new byte[validatorManager.Validators.Count + 1][][];
        }

        public ThresholdKey GeneratePrivateKey()
        {
            throw new NotImplementedException();
        }

        public byte[] SignData(KeyPair keyPair, string curveType, byte[] message)
        {
            var myIndex = _validatorManager.GetValidatorIndex(keyPair.PublicKey);

            _logger.LogInformation($"Current validator index is {myIndex}");

            var connectedNodes = _networkContext.ActivePeers.Count;
            _logger.LogInformation(
                $"Waiting for other validators connected {connectedNodes}/{_validatorManager.Validators.Count}");
            while (connectedNodes != _validatorManager.Validators.Count)
            {
                Thread.Sleep(1000);
                var nextConnected = _networkContext.ActivePeers.Count;
                if (nextConnected == connectedNodes)
                    continue;
                connectedNodes = nextConnected;
                _logger.LogInformation(
                    $"Waiting for other validators connected {connectedNodes}/{_validatorManager.Validators.Count}");
            }

            var sharesForTest = new[]
            {
                "0080BCFE193210C6E7F2CB11B41C51ED2586B3F9C1938C939F606ED41B904B76933FC89873440442C0EBDC4EC928305EF76C794DD372C4ADB5BF5155E63D6EC85A26A43BC0A5561C2448DA366F788F763550570B5598C251BC2FDE0DC889DE69DDE87B481740A138223E84C1CCFE043667736F593A12FBCB7BD9A690E658315C1C10078DCD5C888A592C8572C0D74BF469461E53693F9BDB85EF8C323C8A687042E991DD82BBFB1AD0BBB941473F0A29F79E537670A1D0EF9C716E7852FFC93BE98D94C1C1D5259F36BCAB2F4DCA11C1889E4CE966BA45E2367D246DF14719FC9927EE85CB8A05609333B7F9188A6F3B3425CA2B70736BC42D8404AE06260ECC01000000030000000200000200036A7D746528417FFD4D318EE7E53AB27738327ABB37811B683546022CD6F36B116FC4EFFA333539B45D2AD7DD3224BD80A900F646DFE9104A60C43BE37519570D65339EC8407C9ED73B9940F4DD16E7EEFDAB54872E6448DCA6BA98B1FEA6A2A8A70C3F8DB4DB6F1EEE03AD701716FC39E68BBD68E657BB3E1046692E7C321FBD1C33EBF56387AC5C4194CED226F4653EDFCC500054D25E518A792E8EEB4BDFA5B089D25C455793419FC69BAFB36D9609797BC8142A296602FB327DB3353001E934255CAC5640F6CF53EFC6A0FABA57CD0A3A380345EC05F9B6A9BCFA242EC50F39C9BA7D955EF71238564334A8A943805A16772E212BC0662241B026FBF57E2674B74D286A53F4FB28FFD349BAEED12858927033E60845E1AD87A3773F75E7A846A4BEB8FA75F81FC1B135EAA3CE4044126894716778B64E9C213E8444A060551C311B37893027FC08F820347F061A2A2339734A0ED2A44E688B4A96F265978EDDA49B08E233FBF63022277E11D8E1D629E080E204057ECACADC12AC88C55298BB809BC6368FDBE4158552899E6E79DEE1484487C6E799C24991A18629E0029FBFEDFDBF3D50B1662E10BC5B44123A3628EDE5B754142C266C73F46A0752D0D03AD1E82B676E053E77BDF0A701C3256E1A59DF01CB388D131E7339CDB96EBCE26D9A79ABA3653AC7804D08D952A4714C4A77B34A872988098EFE1E58282A6500000200178141E125294AAB046D73C7A236B13DB36639A5A36D021ED00CB0EB6DA1B2041F8381AA290105D93EEB55843153F73BE40ACDE733A03FD47B1D7A78B5BB3047B5C3AAE60BAB51D2F4CD08AE3CA67CFBE115D2763904190FFCF416040927C14BF25ADA956FE091A20589878269D0E12A279F58F030FB872699659CFE8B1258732A75C750810B18293539AC3C58B38F022E39C601042236140BF7C63BE83B550CD4274090C7B63DC35DE23029300B9B9D42DCDD000946F345C55B44244B64F92B6FC7FB22E66EAC0B84311C2D5BFA54169C48DBB77A66C5EE53EDDAD197350A18D747E17709A4C9160C2791054A284584F96FE980749B6F1A9D2275615167CB8A142E8A038991537EA157C74F5BA39CC6312EA339689250678FE25956D146F6534DB5F020E057503066A2D56C5899F07D127C84D06A579B52C73B99A4619C9E4134086F14144F7C9FF17574A25263CBC7F2697452F3D5C710141C52B247108CF40062AC784A218983A2D55AE08032B5851E12F1B345134E03FC8E87397376A3D6206C123152F3E9D19C90AD8699DD63F9D0376787351AFA334618020619073977B04FD97D856C061A52ED4E84F1557F76228CFD0B25966C72BFB1E65F7A6B6C1A4B449C22A98574AEC0282535884BC24505ACEB002AA0D2736DBC94D9803D0C3076267C5845502B779CC1224891C415D835F53947DFDF8F1E445615C51F3EEC5E000002001BF799AFDD19CD6014BC6789440FFD749ED91071AEE3AFBFE48B80A831139F87B4FE929817565835ED0D4CE618E52706E11AAE2C57AE0A24A9F205ECE5C60520B3B915B9FFB68FAAA0098CF83749B4EEA5CFB33F5B85445AC4D86EAD818D2E8F3472F13ABA2357FC359F97E53D4CC0D60D7245E0C60A0AE01DEA3DD8A3CC2676EB6AB83FAF2B0DD43D98FA19EB765CFCB7ADF8E6420B4AA7F7A74EA8CB56A36C247D14467D25AB679E320CE7F115342C62B0CCAB2F25FF4C291B499C0D1C9B582B9937DAE9104D7B558AA5A03B65F88C8E303663735B2AEE9E52087D46A124DEF414382A872F90EA46966562BC55DAB96684A603693F12DA5E8AAE3DB3ADF0F80449B04FD309154EF72B6D88E4D5CF6F85105DC1E208144B5845B597131E923138CA20BFCDAE3DB38576133E85C09F405864B658371A4E514EDE003AB85FADE9FF072D2E423BFC1A3F16DAA03B49349374AB0CD1C1F8125819694472A31B6A002751C252693609C6FD89B9A677ADCACA21D80706BA2B5A009F50B487F0D63488C2B05CA4FDDEF5AA6255F7DAB272A282BA9DF861AB806C86D9CA4DD4614D62C85558F1B1FB16CDCC13915ACFF1C4078A52D811EC010D3EE333B246C667BE5069A8B4A00624FF00B83EDB1D80C9B3211314B5B7820B0330C9B213173A3B223F38A744F1158F57FD8DC7FAB7E936A4517DC1573C422A843C08F01EA4B8BECCFCD2000002003573E647AED48B6C0FCB9E3551E31F89FD1309AB3FDD183D205888DF3D71675A04B0B053994A6D080DC86FF3E043B77F02228A437E209E884E49CE48FA14D7A4CAA4FAF29963C77A42DA9B1A7CFEA06EDE2D51EC2D5FC7C7FACE0A32C22E37907A5217D76279F21FCE9329FCEC9FE9751E0E041A6DDE2BFD16D45F026A8B84032D613A870FE0A55B2E3C3038692359A120762692800F43E4360A7940132F405E03B41EBFB42A4C6576EAFD1B3804012A52232DA18EF0AF843B62F39EFD3FB2C02D4CDDEE4F6173387FC77DAAF2E71FA9537B1D3574EBFB76967F95B327DC87D16297901CD4F18A78E9089E42C912E81CFC20FAA7B93A5FF7D77D4E57648DB720D89027709ED630B3D2B22B09EDB1E755BEA11DF4BD6F85DBCD0891439479E7EAF04F2D732F8C263AF52731A5D7B956EC18F5D339E807393F81EC6848C4143AD8EB81F0EB835A7B212CD961FD3C36B91D1AFD5D367DE144DC090E549469FC5F3F2AC00695A5A6A67D1AA8DE8D706BE39ACC37B5F59F448BC721D4961EA3B4C27A32FAC419CF3ADE60CD1ABACCA4971F7CAC012F5F74DFC1F77B99DB76484D536CE4DFFA6E4EE607EB89463B733E5A9DE69C7FA2C97F133A1B3CC28574F8BC6384F1AA869F22A96CCADD4908F1EC3F2B1F934D0663672D488BF210F63C2E73247171672530594562AC0C978B09944DB2F20FEEA42B85828B6606B6716CEAC0C56900000101000000010000020002133CD72797DCB9C0D42C1492E0C90687CB126BC62FCCF0F03C39A83984144891A1AF71C2A463C4827C34AE98C6334C977FBA54C4DBA4047515C6B0A6E537243CFC941075B71ED9E810811D85A946634E5BFF8C8B6066A3489CF227AA4BCF834382E59C43786EA9638E7D5D0A0BEAC73A391B55B2DBF21D639142DCA8A6D056825CA260D805002C8BBFAF13C4E416513153A4693301CFD966D850DA8ABDB640D6A353E80899476D78CCF8FCCB70DA4E4C24E1A8D83F563AFDA6B088C9258D32AB625AD52E8F016C98525442425C778298078D74A0E63B101ACB95AA0C3F07A70325ADD944D970BD2DA81E8B2F356BC770F99CDE321A82DC22EF85275FFE85205CB7805FC672FF2BCF1BC8AB28B2C35DDF1C8F1F68D7DE6E9DFA2123B12199D4CCF2F4A5E2DDB3C5BB85290B45D30BF1EA2F143C5B853BEDFD49EC5722151586B95B32E90B36A0BDD30CFE25AAA0440B63FEFE90B9127A964F574059D56481FE4B26614CB2C8149024725F425C5BDA75EC4F431B1FF0F02098291D63A2BA423EEA6EF13899A3E78396726B53205F9A775DC152A6A19A2041C0EEAFAB80531F4414747A9BEFAB4C6028BA557377AE47D22BBA2C6C58815B2635CD51D9E7CB19EE56ABD926253930B08D4B135B94E753ADA925DFC736ED9357DBD81FDE284380C705AEF8CBFD85C553FD4A9ED32B92E39247CC0D8F930C4E81E045EB74424FDA860000091D",
                "0080BCFE193210C6E7F2CB11B41C51ED2586B3F9C1938C939F606ED41B904B76933FC89873440442C0EBDC4EC928305EF76C794DD372C4ADB5BF5155E63D6EC85A26A43BC0A5561C2448DA366F788F763550570B5598C251BC2FDE0DC889DE69DDE87B481740A138223E84C1CCFE043667736F593A12FBCB7BD9A690E658315C1C10078DCD5C888A592C8572C0D74BF469461E53693F9BDB85EF8C323C8A687042E991DD82BBFB1AD0BBB941473F0A29F79E537670A1D0EF9C716E7852FFC93BE98D94C1C1D5259F36BCAB2F4DCA11C1889E4CE966BA45E2367D246DF14719FC9927EE85CB8A05609333B7F9188A6F3B3425CA2B70736BC42D8404AE06260ECC01000000030000000200000200036A7D746528417FFD4D318EE7E53AB27738327ABB37811B683546022CD6F36B116FC4EFFA333539B45D2AD7DD3224BD80A900F646DFE9104A60C43BE37519570D65339EC8407C9ED73B9940F4DD16E7EEFDAB54872E6448DCA6BA98B1FEA6A2A8A70C3F8DB4DB6F1EEE03AD701716FC39E68BBD68E657BB3E1046692E7C321FBD1C33EBF56387AC5C4194CED226F4653EDFCC500054D25E518A792E8EEB4BDFA5B089D25C455793419FC69BAFB36D9609797BC8142A296602FB327DB3353001E934255CAC5640F6CF53EFC6A0FABA57CD0A3A380345EC05F9B6A9BCFA242EC50F39C9BA7D955EF71238564334A8A943805A16772E212BC0662241B026FBF57E2674B74D286A53F4FB28FFD349BAEED12858927033E60845E1AD87A3773F75E7A846A4BEB8FA75F81FC1B135EAA3CE4044126894716778B64E9C213E8444A060551C311B37893027FC08F820347F061A2A2339734A0ED2A44E688B4A96F265978EDDA49B08E233FBF63022277E11D8E1D629E080E204057ECACADC12AC88C55298BB809BC6368FDBE4158552899E6E79DEE1484487C6E799C24991A18629E0029FBFEDFDBF3D50B1662E10BC5B44123A3628EDE5B754142C266C73F46A0752D0D03AD1E82B676E053E77BDF0A701C3256E1A59DF01CB388D131E7339CDB96EBCE26D9A79ABA3653AC7804D08D952A4714C4A77B34A872988098EFE1E58282A6500000200178141E125294AAB046D73C7A236B13DB36639A5A36D021ED00CB0EB6DA1B2041F8381AA290105D93EEB55843153F73BE40ACDE733A03FD47B1D7A78B5BB3047B5C3AAE60BAB51D2F4CD08AE3CA67CFBE115D2763904190FFCF416040927C14BF25ADA956FE091A20589878269D0E12A279F58F030FB872699659CFE8B1258732A75C750810B18293539AC3C58B38F022E39C601042236140BF7C63BE83B550CD4274090C7B63DC35DE23029300B9B9D42DCDD000946F345C55B44244B64F92B6FC7FB22E66EAC0B84311C2D5BFA54169C48DBB77A66C5EE53EDDAD197350A18D747E17709A4C9160C2791054A284584F96FE980749B6F1A9D2275615167CB8A142E8A038991537EA157C74F5BA39CC6312EA339689250678FE25956D146F6534DB5F020E057503066A2D56C5899F07D127C84D06A579B52C73B99A4619C9E4134086F14144F7C9FF17574A25263CBC7F2697452F3D5C710141C52B247108CF40062AC784A218983A2D55AE08032B5851E12F1B345134E03FC8E87397376A3D6206C123152F3E9D19C90AD8699DD63F9D0376787351AFA334618020619073977B04FD97D856C061A52ED4E84F1557F76228CFD0B25966C72BFB1E65F7A6B6C1A4B449C22A98574AEC0282535884BC24505ACEB002AA0D2736DBC94D9803D0C3076267C5845502B779CC1224891C415D835F53947DFDF8F1E445615C51F3EEC5E000002001BF799AFDD19CD6014BC6789440FFD749ED91071AEE3AFBFE48B80A831139F87B4FE929817565835ED0D4CE618E52706E11AAE2C57AE0A24A9F205ECE5C60520B3B915B9FFB68FAAA0098CF83749B4EEA5CFB33F5B85445AC4D86EAD818D2E8F3472F13ABA2357FC359F97E53D4CC0D60D7245E0C60A0AE01DEA3DD8A3CC2676EB6AB83FAF2B0DD43D98FA19EB765CFCB7ADF8E6420B4AA7F7A74EA8CB56A36C247D14467D25AB679E320CE7F115342C62B0CCAB2F25FF4C291B499C0D1C9B582B9937DAE9104D7B558AA5A03B65F88C8E303663735B2AEE9E52087D46A124DEF414382A872F90EA46966562BC55DAB96684A603693F12DA5E8AAE3DB3ADF0F80449B04FD309154EF72B6D88E4D5CF6F85105DC1E208144B5845B597131E923138CA20BFCDAE3DB38576133E85C09F405864B658371A4E514EDE003AB85FADE9FF072D2E423BFC1A3F16DAA03B49349374AB0CD1C1F8125819694472A31B6A002751C252693609C6FD89B9A677ADCACA21D80706BA2B5A009F50B487F0D63488C2B05CA4FDDEF5AA6255F7DAB272A282BA9DF861AB806C86D9CA4DD4614D62C85558F1B1FB16CDCC13915ACFF1C4078A52D811EC010D3EE333B246C667BE5069A8B4A00624FF00B83EDB1D80C9B3211314B5B7820B0330C9B213173A3B223F38A744F1158F57FD8DC7FAB7E936A4517DC1573C422A843C08F01EA4B8BECCFCD2000002003573E647AED48B6C0FCB9E3551E31F89FD1309AB3FDD183D205888DF3D71675A04B0B053994A6D080DC86FF3E043B77F02228A437E209E884E49CE48FA14D7A4CAA4FAF29963C77A42DA9B1A7CFEA06EDE2D51EC2D5FC7C7FACE0A32C22E37907A5217D76279F21FCE9329FCEC9FE9751E0E041A6DDE2BFD16D45F026A8B84032D613A870FE0A55B2E3C3038692359A120762692800F43E4360A7940132F405E03B41EBFB42A4C6576EAFD1B3804012A52232DA18EF0AF843B62F39EFD3FB2C02D4CDDEE4F6173387FC77DAAF2E71FA9537B1D3574EBFB76967F95B327DC87D16297901CD4F18A78E9089E42C912E81CFC20FAA7B93A5FF7D77D4E57648DB720D89027709ED630B3D2B22B09EDB1E755BEA11DF4BD6F85DBCD0891439479E7EAF04F2D732F8C263AF52731A5D7B956EC18F5D339E807393F81EC6848C4143AD8EB81F0EB835A7B212CD961FD3C36B91D1AFD5D367DE144DC090E549469FC5F3F2AC00695A5A6A67D1AA8DE8D706BE39ACC37B5F59F448BC721D4961EA3B4C27A32FAC419CF3ADE60CD1ABACCA4971F7CAC012F5F74DFC1F77B99DB76484D536CE4DFFA6E4EE607EB89463B733E5A9DE69C7FA2C97F133A1B3CC28574F8BC6384F1AA869F22A96CCADD4908F1EC3F2B1F934D0663672D488BF210F63C2E73247171672530594562AC0C978B09944DB2F20FEEA42B85828B6606B6716CEAC0C56900000101000000020000020003F47C3ADB2AF92A35BC4A7A3F53C87BD517990B0F4AD229C72DAA4C0B072AD9042024BACE0100C1596E3F75201A27E8642E79B46455CA25840137DF2D0CBEB49576AFF15164667911066FF5F7C7F49C2D898BA8B55308D183C56A54D90B6A0001C5039FB3DD81159362DB2B745251F7947FC32FC22A48AED33D58F6ADDE9711AD78BB38C0B62D26FFB75508AA905945396A0DC6087BCF8E35C2759EF1B5D5CEEC5ACCD1262EF4A3E44023B3D15B7D4CEEF4A9D458E490A63761F40CF5EB018C2F8EFF53A6A2C23DA7CE0A571A8ACEF62EEB83001E7098C0188ED78994989311AB6B572BD5A4E117D4A05AABD0606929B093C7857FABE4E750367DA9EA70857E1B6944AE245905CFC79B816C58930E6066A9EE285F08E9571918AAC7BFC646B6C3B3597CE22A8D8745F8724963F638106738EFCEEB4C4943AD2E4A4E4DAFEC67F0DA964F4D338D00BDCAE7D6B585232EA718F6D7342B308E89C1AA20E264ED85C74CBC541D8731BE351F29D00783BFDB421C70A7CBC59E27386158E2399F72D7F22934CDCA0ADF6AA293D05C73F38C582D98810B4C972695618F64BED21367F7128A4743B9F98A07E4E4627EC0E226260C56A9C4989BA750EF9DFC50EB2001683E4EA448E0E13DF56744F25E4C1B679E851D116DD4B23D0AC13908C4E1CF7056804F851D52A1D59540F3886AC8980DB38E0FCE2168916BA905E29EBAFBE504E50000091D",
                "0080BCFE193210C6E7F2CB11B41C51ED2586B3F9C1938C939F606ED41B904B76933FC89873440442C0EBDC4EC928305EF76C794DD372C4ADB5BF5155E63D6EC85A26A43BC0A5561C2448DA366F788F763550570B5598C251BC2FDE0DC889DE69DDE87B481740A138223E84C1CCFE043667736F593A12FBCB7BD9A690E658315C1C10078DCD5C888A592C8572C0D74BF469461E53693F9BDB85EF8C323C8A687042E991DD82BBFB1AD0BBB941473F0A29F79E537670A1D0EF9C716E7852FFC93BE98D94C1C1D5259F36BCAB2F4DCA11C1889E4CE966BA45E2367D246DF14719FC9927EE85CB8A05609333B7F9188A6F3B3425CA2B70736BC42D8404AE06260ECC01000000030000000200000200036A7D746528417FFD4D318EE7E53AB27738327ABB37811B683546022CD6F36B116FC4EFFA333539B45D2AD7DD3224BD80A900F646DFE9104A60C43BE37519570D65339EC8407C9ED73B9940F4DD16E7EEFDAB54872E6448DCA6BA98B1FEA6A2A8A70C3F8DB4DB6F1EEE03AD701716FC39E68BBD68E657BB3E1046692E7C321FBD1C33EBF56387AC5C4194CED226F4653EDFCC500054D25E518A792E8EEB4BDFA5B089D25C455793419FC69BAFB36D9609797BC8142A296602FB327DB3353001E934255CAC5640F6CF53EFC6A0FABA57CD0A3A380345EC05F9B6A9BCFA242EC50F39C9BA7D955EF71238564334A8A943805A16772E212BC0662241B026FBF57E2674B74D286A53F4FB28FFD349BAEED12858927033E60845E1AD87A3773F75E7A846A4BEB8FA75F81FC1B135EAA3CE4044126894716778B64E9C213E8444A060551C311B37893027FC08F820347F061A2A2339734A0ED2A44E688B4A96F265978EDDA49B08E233FBF63022277E11D8E1D629E080E204057ECACADC12AC88C55298BB809BC6368FDBE4158552899E6E79DEE1484487C6E799C24991A18629E0029FBFEDFDBF3D50B1662E10BC5B44123A3628EDE5B754142C266C73F46A0752D0D03AD1E82B676E053E77BDF0A701C3256E1A59DF01CB388D131E7339CDB96EBCE26D9A79ABA3653AC7804D08D952A4714C4A77B34A872988098EFE1E58282A6500000200178141E125294AAB046D73C7A236B13DB36639A5A36D021ED00CB0EB6DA1B2041F8381AA290105D93EEB55843153F73BE40ACDE733A03FD47B1D7A78B5BB3047B5C3AAE60BAB51D2F4CD08AE3CA67CFBE115D2763904190FFCF416040927C14BF25ADA956FE091A20589878269D0E12A279F58F030FB872699659CFE8B1258732A75C750810B18293539AC3C58B38F022E39C601042236140BF7C63BE83B550CD4274090C7B63DC35DE23029300B9B9D42DCDD000946F345C55B44244B64F92B6FC7FB22E66EAC0B84311C2D5BFA54169C48DBB77A66C5EE53EDDAD197350A18D747E17709A4C9160C2791054A284584F96FE980749B6F1A9D2275615167CB8A142E8A038991537EA157C74F5BA39CC6312EA339689250678FE25956D146F6534DB5F020E057503066A2D56C5899F07D127C84D06A579B52C73B99A4619C9E4134086F14144F7C9FF17574A25263CBC7F2697452F3D5C710141C52B247108CF40062AC784A218983A2D55AE08032B5851E12F1B345134E03FC8E87397376A3D6206C123152F3E9D19C90AD8699DD63F9D0376787351AFA334618020619073977B04FD97D856C061A52ED4E84F1557F76228CFD0B25966C72BFB1E65F7A6B6C1A4B449C22A98574AEC0282535884BC24505ACEB002AA0D2736DBC94D9803D0C3076267C5845502B779CC1224891C415D835F53947DFDF8F1E445615C51F3EEC5E000002001BF799AFDD19CD6014BC6789440FFD749ED91071AEE3AFBFE48B80A831139F87B4FE929817565835ED0D4CE618E52706E11AAE2C57AE0A24A9F205ECE5C60520B3B915B9FFB68FAAA0098CF83749B4EEA5CFB33F5B85445AC4D86EAD818D2E8F3472F13ABA2357FC359F97E53D4CC0D60D7245E0C60A0AE01DEA3DD8A3CC2676EB6AB83FAF2B0DD43D98FA19EB765CFCB7ADF8E6420B4AA7F7A74EA8CB56A36C247D14467D25AB679E320CE7F115342C62B0CCAB2F25FF4C291B499C0D1C9B582B9937DAE9104D7B558AA5A03B65F88C8E303663735B2AEE9E52087D46A124DEF414382A872F90EA46966562BC55DAB96684A603693F12DA5E8AAE3DB3ADF0F80449B04FD309154EF72B6D88E4D5CF6F85105DC1E208144B5845B597131E923138CA20BFCDAE3DB38576133E85C09F405864B658371A4E514EDE003AB85FADE9FF072D2E423BFC1A3F16DAA03B49349374AB0CD1C1F8125819694472A31B6A002751C252693609C6FD89B9A677ADCACA21D80706BA2B5A009F50B487F0D63488C2B05CA4FDDEF5AA6255F7DAB272A282BA9DF861AB806C86D9CA4DD4614D62C85558F1B1FB16CDCC13915ACFF1C4078A52D811EC010D3EE333B246C667BE5069A8B4A00624FF00B83EDB1D80C9B3211314B5B7820B0330C9B213173A3B223F38A744F1158F57FD8DC7FAB7E936A4517DC1573C422A843C08F01EA4B8BECCFCD2000002003573E647AED48B6C0FCB9E3551E31F89FD1309AB3FDD183D205888DF3D71675A04B0B053994A6D080DC86FF3E043B77F02228A437E209E884E49CE48FA14D7A4CAA4FAF29963C77A42DA9B1A7CFEA06EDE2D51EC2D5FC7C7FACE0A32C22E37907A5217D76279F21FCE9329FCEC9FE9751E0E041A6DDE2BFD16D45F026A8B84032D613A870FE0A55B2E3C3038692359A120762692800F43E4360A7940132F405E03B41EBFB42A4C6576EAFD1B3804012A52232DA18EF0AF843B62F39EFD3FB2C02D4CDDEE4F6173387FC77DAAF2E71FA9537B1D3574EBFB76967F95B327DC87D16297901CD4F18A78E9089E42C912E81CFC20FAA7B93A5FF7D77D4E57648DB720D89027709ED630B3D2B22B09EDB1E755BEA11DF4BD6F85DBCD0891439479E7EAF04F2D732F8C263AF52731A5D7B956EC18F5D339E807393F81EC6848C4143AD8EB81F0EB835A7B212CD961FD3C36B91D1AFD5D367DE144DC090E549469FC5F3F2AC00695A5A6A67D1AA8DE8D706BE39ACC37B5F59F448BC721D4961EA3B4C27A32FAC419CF3ADE60CD1ABACCA4971F7CAC012F5F74DFC1F77B99DB76484D536CE4DFFA6E4EE607EB89463B733E5A9DE69C7FA2C97F133A1B3CC28574F8BC6384F1AA869F22A96CCADD4908F1EC3F2B1F934D0663672D488BF210F63C2E73247171672530594562AC0C978B09944DB2F20FEEA42B85828B6606B6716CEAC0C56900000101000000030000020005D5BB9E8EBE159AAAA468DFEBC6C7F122641FAA5865D7629E1F1AEFDC8A4169769E9A03D95D9DBE30604A3BA76E1C8430DD391403CFF04692ECA90DB3344644EDF0CBD22D11AE1839FC5ECE69E6A2D50CB717C4DF45AAFFBEEDE28207CB047CC00721A324429381C33738F9DE98B927EEC66B09D1789F4042E96F10B3165DCCD894D410A9675A2173AEFAFD903C9C3941807722DDF5CF4304AC9A6358ADF55D021245BA43C4A1DA4FB34E6AD746204B91C471FFD989CB11711D379122B075E5B3BBA3D21EB6830EB749C06BF2B92669C5CF788B9BFAF670165219691CF21E7C53B1007E667051727B9896CC718B668BF02DF22CCD3D46F27D7D762C74E285DBDA1B08FC823F0C73C01B3A2D88735962EE374D315539F43F9437346BCE6AF398BA73BE53E1776748D06BBB878219642EE442CB617B1356995D12A845794AC3492859F9B58F307943A888D187C06A0251EA32EF1DAF43E686C42C13E7EF65590D4373175B88464EEC45CBF45DB2ABA54097E99E34779A4C2DD8999460D084A370F9E37862FA71D751AEB53565C7877E38FD6FAF6FF7942CE9023019D223D3B0AA10A013EB8447C7AFA10E6F8A0A160479ECF3271CD8B5F37BA96EA6C7EE74E8E225F16F6B9C894B3A413ED161034F7B8F611443147276E6BDA699F1AB9B5B5FE5FAF0116EA7BDE5D6849C7202659D37D4D4538EB33E1688D02B7F5201B57A2F440000091D",
            };

            var pk = new PaillierPrivateThresholdKey(HexUtil.hexToBytes(sharesForTest[myIndex % sharesForTest.Length]),
                4289, true);
            var plainRnd = new Random(123456789);
            var curveParams = new CurveParams(curveType);
            var privateKey =
                new BigInteger("d95d6db65f3e2223703c5d8e205d98e3e6b470f067b0f94f6c6bf73d4301ce48", 16);
            var randomness = Util.randomFromZnStar(curveParams.Q, plainRnd);
            var encryptedPrivateKey = new Paillier(pk).encrypt(privateKey, randomness);
            var publicKey = curveParams.G.Multiply(privateKey.Mod(curveParams.Q)).Normalize();

            _logger.LogInformation($"Public key: {publicKey}");

            var share = new ThresholdKey
            {
                PrivateKey = encryptedPrivateKey.ToByteArray().ToPrivateKey(),
                Validators = {_validatorManager.Validators},
                PublicKey = publicKey.GetEncoded(true).ToPublicKey(),
                PrivateShare = pk.toByteArray().ToPrivateKey()
            };

            /* TODO: "i think we have to use password encryption for starting blockchain with private shares" */
//            var share = _globalRepository.GetShare();
            if (share is null)
                throw new Exception("You don't have threshold share");
            var signer = _partyManager.CreateSignerProtocol(
                share.PrivateShare.Buffer.ToByteArray(),
                share.PrivateKey.Buffer,
                share.PublicKey.Buffer.ToByteArray(),
                curveType);
            signer.Initialize(message);
            /* rounds */
            _logger.LogInformation("Working on round #1");
            var r1 = _Broadcast(signer.Round1(), keyPair, signer);
            _logger.LogInformation("Working on round #2");
            var r2 = _Broadcast(signer.Round2(r1), keyPair, signer);
            _logger.LogInformation("Working on round #3");
            var r3 = _Broadcast(signer.Round3(r2), keyPair, signer);
            _logger.LogInformation("Working on round #4");
            var r4 = _Broadcast(signer.Round4(r3), keyPair, signer);
            _logger.LogInformation("Working on round #5");
            var r5 = _Broadcast(signer.Round5(r4), keyPair, signer);
            _logger.LogInformation("Working on round #6");
            var r6 = _Broadcast(signer.Round6(r5), keyPair, signer);
            _logger.LogInformation("Calculating signature");
            var final = signer.Finalize(r6);
            _logger.LogInformation($"Signatre generated R:{final.r}, S:{final.s}");
            return padTo32(final.r).Concat(padTo32(final.s)).ToArray();
        }

        private byte[] padTo32(BigInteger value)
        {
            var buffer = value.ToByteArray().Skip(1).ToArray();
            var result = new byte[32];
            if (buffer.Length > result.Length)
                throw new ArgumentOutOfRangeException(nameof(buffer));
            Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
            return result;
        }

        public ThresholdRequest HandleThresholdMessage(ThresholdRequest thresholdMessage, PublicKey publicKey)
        {
            var validatorIndex = _validatorManager.GetValidatorIndex(publicKey);

            byte[] bytes;
            using (var stream = new MemoryStream(thresholdMessage.Message.ToByteArray()))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadInt32() != validatorIndex)
                    throw new Exception("Invalid validator index specified");
                var state = (SignerState) reader.ReadByte();
                _logger.LogInformation($"Handled state message {state}, from {validatorIndex}");
                var len = reader.ReadLength();
                bytes = reader.ReadBytes((int) len);
                lock (_messageChanged)
                {
                    var msgs = _ValidatorMessages((int) validatorIndex);
                    msgs[(byte) state] = bytes;
                    Monitor.PulseAll(_messageChanged);
                }
            }

            return new ThresholdRequest
            {
                Message = ByteString.CopyFrom(bytes)
            };
        }

        private IEnumerable<T> _Broadcast<T>(T message, KeyPair keyPair, ISignerProtocol signer)
            where T : ISignerMessage, new()
        {
            var validatorIndex = (int) _validatorManager.GetValidatorIndex(keyPair.PublicKey);

            /* serialize */
            byte[] bytes;
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(validatorIndex);
                writer.Write((byte) signer.CurrentState);
                var raw = message.ToByteArray();
                writer.WriteLength(raw.Length);
                writer.Write(raw);
                bytes = stream.ToArray();
            }

            _logger.LogInformation($"Broadcasting state {signer.CurrentState}, i'm {validatorIndex}");

            /* store */
            lock (_messageChanged)
            {
                var msgs = _ValidatorMessages(validatorIndex);
                msgs[(byte) signer.CurrentState] = bytes;
                Monitor.PulseAll(_messageChanged);
            }

            /* broadcast */
            foreach (var peer in _networkContext.ActivePeers.Values)
            {
                /*peer.ThresholdService.ExchangeMessage(new ThresholdMessage
                {
                    Message = ByteString.CopyFrom(bytes)
                }, keyPair);*/
            }

            /* collect */
            lock (_messageChanged)
            {
                while (!_HasMessages(signer.CurrentState, out var collected))
                {
                    _logger.LogInformation(
                        $"Waiting for messages on state ({signer.CurrentState}), got only {collected}/{_validatorManager.Validators.Count}");
                    Monitor.Wait(_messageChanged);
                }
            }

            var result = new List<T>();
            foreach (var validator in _validatorManager.Validators)
            {
                var index = _validatorManager.GetValidatorIndex(validator);
                var msgs = _ValidatorMessages((int) index);
                if (msgs[(byte) signer.CurrentState] is null)
                    throw new Exception($"Invalid validator's state, waiting for ({signer.CurrentState})");
                var t = new T();
                t.fromByteArray(msgs[(byte) signer.CurrentState]);
                result.Add(t);
            }

            return result;
        }

        private bool _HasMessages(SignerState state, out int total)
        {
            var exists = 0;
            foreach (var validator in _validatorManager.Validators)
            {
                var msgs = _ValidatorMessages((int) _validatorManager.GetValidatorIndex(validator));
                if (msgs[(byte) state] is null)
                    continue;
                ++exists;
            }

            total = exists;
            if (exists < _validatorManager.Validators.Count)
                return false;
            return true;
        }

        private byte[][] _ValidatorMessages(int validatorIndex)
        {
            try
            {
                var list = _messagePerValidator.ElementAt(validatorIndex);
                if (list != null)
                    return list;
            }
            catch (ArgumentOutOfRangeException)
            {
                // ignore
            }

            _messagePerValidator[validatorIndex] = new byte[0x100][];
            return _messagePerValidator[validatorIndex];
        }
    }
}