using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Security.Live.Klang
{
    public class BTnGLiveKlangGuard : IBTnGGuardInfo
    {
        private static BTnGLiveKlangGuard _guard = new BTnGLiveKlangGuard();

        public static IBTnGGuardInfo GuardInfo
        {
            get
            {
                return _guard;
            }
        }

        public string MerchantId { get; } = "KlangSentral";

        public string WebApiUrl { get; } = @"https://paymentgateway.nssit.com.my/api";

        private BTnGLiveKlangGuard() { }

        public string PublicKey
        {
            get
            {
                return @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAmlnJoTzXqqYTWiv2DAq3
j7jQe7fGaOh8gJEK7fBL8sf2a5Cpx+to6aGTassZgPKfd2ECUr9KK3EdYxeUQhZW
QSEeDlY8hlQ+LIHz83t8SaHDjhSsQIaDOXLCNBQ4ZUiRGTS94tpw2jux995ZErzq
zfvK8XvXbg7V4FLgprWU/ctqB0yqZpG8JxNcCn2wFH4HrVNcPwpVyFWcfEvuIEdH
VpnLL/hBcrZMEnt+fUSkR/q7p04YZrYy0ubGKbyK9JD6nMernPfLY5/01NWVrJa9
EmsSgHzjTsff2aGkktTpriEfxBpQbqySVoxzQQwCnNQmfcPixqO26jO62c4ApmJP
DQIDAQAB
-----END PUBLIC KEY-----";
            }
        }

        public string PrivateKey
        {
            get
            {
                return @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAr4G0buwwKo9qoFvZs3uNk5RFpIO2GhwFBtI+t3TNI+wnCXy8
G670NnnhiciKCCPfJV585VCUuIDQ/ZJigv1Vrbcnf8ru2l+SWWB58zILqV8+K9fZ
Eayc/9KQTS6bPTBkBAhVwAlzHZmAvBuKqbLxIsk44hFnkSrx5+Qb5U00MGSFi5jQ
659yzNDKTPvNb7xLeO77HEXXJYBxTwkNN7fibGa1aSsuYd/nLDILIbQRpvZMkuA0
cvoqd015vjAwVo+o/6ml9BIoUwe7rYwcD2iUGVwGVMI759lvC4M3Bq2lOMdJWQjk
gdwQWW4aAw5Cpiq9bU9fqZTlEAfcT1TaKzvEVQIDAQABAoIBAAEP8JRN9aoBnYxE
Qg27YmCm9cJIfHetUmfM1KMDn/eRsa11sHsdFKP9GjJdS7M/lFWDkY9JwjyuUEAt
/KP8/eHD9Ac/+9tygfjsg+XG7cR5Vm+pf3TYmJ1WcZfQqmaKaRs6ea+53up/nVnb
c3wSIIeCCZ1vVt3NZr1tPLCnRk5CAD+psRRtESVsgue2kfEHPNnewgImdOvG4IUe
hY49NUyjSqd6x9/3Ac2HK/KQdV8b/ByyNtAQX0GnPfgvbhixQrhuwaHmbD5RsKfG
fZb5YKc0G4qS/Ynnj5s5sXorck8Rrk1LElLmmSDUEaywdkZGoq+shO3WFzL7FjHK
7XsxWNkCgYEAvNfQTRf1e+k7VgNk5CQdYZkWuisjjRtpadaHBl8RM70ZZR38FXqM
bEZSK+f/WEWogNiGz0nBXW7uSzvmlZcychMxKQQ+BLco1ZYMCIhmkWF6XRcEQDpN
VGvQuxV+dUnYepLZ378mTdxPiwwVDNhtIIPfJoTP98WHPcP0R1ZpfYsCgYEA7evB
JtD82VComm9cMIvT6Zb0TGcnMO8dSpesnc3VBGd/rgVq44BbX+CDRqgEyt5+Dmhk
CcSjgwNROhCogiTjZuExBjGfIB5HroEIchkZfBrHXYHPHs+wIW49brFXUnN1BaXj
ipY6sFulw2V1yp/6Fnk5n1BtWQXoaXx0Z0xTwZ8CgYEAt+MLQlcHQ37suagacZG9
TpFtMSHav4gm0NoGp6yhKpmEBhP5qcyourKLR5fk7Mb/+19dfrJaaaogvv/4XEhb
5ftMTp2D6sGUMM+orhJsljqulcFY8VYUgRVvrw2SE5IyC8G+CQOwv/a9OjAsu1Jw
iU4AWRjRDnu5YurSjzjVBlsCgYEAs10bVWcSZ8aOBTdU2ehgTuaVsA/IFPT/MR/d
a24kXFimggvlhDNdL4+ziA3c5xALGlOKr26GEpvqnYq3br3ejc3RRqkcHHuXcLAo
LNRNSSlrCUx2V/UdBGb2ez/I5AjUvPhN7UnKXvAIrKENxz8Jq+2iCj/437c7emLv
4nxBXGMCgYA4Tzs/33BnPgA6Paj6XhPzpgrEyERG5GGJZyhqJYEuhe1wRpD9yh/T
E9+8p2w4Zb+OwAfz+kxTQAq4X6cPxWMvJAlajYFMWW9OFAPXpchr5d8Tlbm9P2r4
lLXVK3JfSjAgU5KLSw+zgLL6wZ3maEscckSeK/+66f38uHXp4Pmz7Q==
-----END RSA PRIVATE KEY-----";
            }
        }

        public string HashSecretKey { get; } = "43E7F2C88A824F9BA27C927CACE2D08F";
        public string TVMKey { get; } = "1DD523D4D26A481F923407086C428AF7";
    }
}
