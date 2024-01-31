using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Security.Live
{
    public class BTnGLiveGuard : IBTnGGuardInfo
    {
        private static BTnGLiveGuard _guard = new BTnGLiveGuard();

        public static IBTnGGuardInfo GuardInfo
        {
            get
            {
                return _guard;
            }
        }

        public string MerchantId { get; } = "MelakaSentral";

        public string WebApiUrl { get; } = @"https://paymentgateway.nssit.com.my/api";

        private BTnGLiveGuard() { }

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
MIIEogIBAAKCAQEAwJu75vbS/ROE/NJQslN9RcVWDsS+NS+WxzubEojUa89UoAnn
qaCLT99HJDarWZYzf+W2b3n42OaWzdNhA+HIX3c7CaAKiTCsL8S5DG+F1vX+Cpek
6/8WJQqmb/D9kg+YzawllmAxJHPvF22/vs8IUqkSlNCPmT059ZB3aYapHofsnSpe
7p40seCFB72QdnyQ+AsEE64p9vhd+0RHaVCmjTNHUmS448PwoEVg6GR5nbK3YSRI
mrz18mBpiKF9pK857AyQsmQKyngProKh94kD3awmbPO0dveC2oZKr6/oPvlZZHoL
0/vkRL85ykcMyyRTJ8ecMSHohiyitPLsYOva3wIDAQABAoIBAEYy/IMrhq6yhI/Y
34x/HXNmfv0W+/RKifvHX7ebo0A/6qoBsiuC1gOgbz9lpYIbWl3NDXcHz4jOUjxm
RD+z9OHGhP/3Ep/522f1nTRJGVDFi3e02WkiKaIKhara/LBVeu3LRk/Oc1rD3mp9
l5slvfTSpnESqqPBSvSTWNJfQBhd8u4NXqWajihx8d8LYK0YJehAPrl0H4RZmDin
lOFMJJsHmJQBBsopAr42CxJjblp43qfREILriS+0HgeFe93sE5i51B8ka5tczjbf
k7ugZCS0B+ne8nmkFGOm0iTj2DgNK73fGo5zfR0e/RJyHlSjupaxCsZhUJuSrC12
2l/8yvECgYEAxIE5qM02eHFl5CxIGTq2sH1KTRfnIbU1tOqVCdkN8ZsJLU0h+fRA
6JaQ0ydL1kDQiS1VAsydKWVGw3ivJg5iGMpYZPD3D+pRgqq+iLIMcBJXQf/idevW
xmGCHW4TSSJ2eHrcsfNvFdXBiD0oKuyXsRDoY83vXip1CbMjpC7/wUUCgYEA+uyA
Rs6skPEwdZcsrMt8n0p6rPn9QLZFG84LdaDiHv6YPVmXQZHzFa3TaBaNhByFp38d
RYxRWx/fxtGuyPxE1tQt0+dnKbwzDVQ24KB6PkyVbUpgRaHX2zwrnqFBbKzypMaa
HBsWxIYjzZ2tX/bVsULL6bSK+KdvEFXIFN7Iw9MCgYAN01W+NaoGcVwM4Ly14bam
1jnbELp3WauMrhCMX9QmUpUjtdCVKIVEmAtaf+JLEcZaUHExwDmyhuiiqiQQlmR6
gAoWGAoZ+Y+AlQjQz04muHOhNiK1z0EONiAUeAEtXRpewa4zawA+1gpGzp673meR
0rG3C+8yfeQ8KXlxfMkLzQKBgH1cRsIZYzGOri0xd8pkZ0CVzzA4PQRli6VWjXcZ
B5+AhsKvzdeeeRmtnF72VYFZVpTV9uPWNJcYF46XF4GmNyY1HygtBedd9QuFtV0I
D8qsLENL00k2kEchipFX8v2+cgNMjZXZGsjYU27YhdPZ1+0VeBascnnD/MLmeG2k
2BytAoGACI/18Kgf3XyR2NbI08SMBUjZFipnVjdtY65/js15XCXeucBL32vcHms9
7RnqW7wFfgSeSjBzDsBsfjt43a6jmjIRBjnPgnS7zG+JfBArEJ7Ywj8E4OhUmw5Z
RhA4xC5NwvOA41gj0XWBM7bnWbXHOHpcpyBeyAVFkZjzCVA8Pc8=
-----END RSA PRIVATE KEY-----";
            }
        }

        public string HashSecretKey { get; } = "43E7F2C88A824F9BA27C927CACE2D08F";
        public string TVMKey { get; } = "1DD523D4D26A481F923407086C428AF7";
    }
}