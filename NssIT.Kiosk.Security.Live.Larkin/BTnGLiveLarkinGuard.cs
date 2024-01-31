using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Security.Live
{
    public class BTnGLiveLarkinGuard : IBTnGGuardInfo
    {
        private static BTnGLiveLarkinGuard _guard = new BTnGLiveLarkinGuard();

        public static IBTnGGuardInfo GuardInfo
        {
            get
            {
                return _guard;
            }
        }

        public string MerchantId { get; } = "LarkinSentral";

        public string WebApiUrl { get; } = @"https://paymentgateway.nssit.com.my/api";

        private BTnGLiveLarkinGuard() { }

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
MIIEpAIBAAKCAQEAnlDwjmGCwZt+RwLYbTlUoOp0Kpj+i5KIFhItcSsEE1JKLvvC
2upD2Qy6YxJ4/9QZ3ZPUMNw/dbXXwTgshNfxxj1CyYEUJAxbLKmnnlX057rAf15p
4WItggLjUonEwurj8n8d6IXD65BmLeN4E5oLAIm8CPAUf3mdKwUt5xmAvxlLa83N
h2846xJ4v35i4w9OvP+64RQp+1cTaixXYTstUGc4ahf4UD65Rb/F2opeJiaj1Ezo
iLiQTktHO8oyTTbeeZyLpNRi20UWwD3osQcWWAMR+wJWhenedx3klNUwHlVf71N2
CWsX94UedhrXmKcfrPTb1cetE4asS3MDtEYlGQIDAQABAoIBABRX90l8i0Zguk3a
tj/21YKmDg5pQZBi4HzhkhhQPEJVWTYHYQcrglNyWMMhXQ4WzcoU1HgA3WC9YL+x
/bqGW0mrlpj5LsLhGZg4JtNoXUOn0lOrqLRvHsTtpRN2o6OednMzVX8LLV1oXVD5
QXVXXqGPkWuemiu1T3Z3QGpJcytbG4GKqQmGbBcMNT1i89xtPCRw5DmLuVr8Zrit
k6Jr4tioCzA/uLzUqwZMLGTsamy0wNi0rmO5SKRA4HJbQayyCtALaj5BfvPXLLTI
vdrECaO6F7WX3zh0R/7Df2TqJi1Ii9S5LRXny50nbsfebXihEETMiLpRwb++VH+h
1RBdS58CgYEAx1zRO+GfKA3zUSu+2NBhlynlJ8bLUjo8gXj7Qnr1FYKWITunrFh3
qHqWMsRguqzPn5Z0niIYBi7WTJJ9m+pBey9cUpqJR11qWq1l4clfavVBc22J637D
46++P42VKDw+Kmnqtp8ouR5+ufxpzKHNk4c/nknnCjZdl75ng4TAHWcCgYEAy0rp
dg43nU4iQv5dXDh9wvQWvUhHYvzb9VPHoqWxGjRDPb+AouP18b4hc82+iNN7HPEG
/q5wIltbPab7kyaM5KXRr39dkW1YIGecgLIgbhBuMWLpARB+7UPuZAJDcqTz5dqd
meLDF4T8uk+st0aZUZqr99UIGz3TLqoQuSlTmX8CgYEAggFcDHGgKS2XuxoM1TRm
P345ikvPsOp7JG8xYnPuOL20iy1stS3nsf1mzupSrckdh9NEbw6KnhWS99IbiT4k
v2DuEovl85zOSx2CL8/yojwXF1+aLfC3a1XieW0plFhcranuPnLhyn5BY6VflOr6
tTbgXKeSu40iUV7//7rk0QcCgYAYWON3ESLWAyqVTXOO/PEK7ULf7o0mLNSHID6T
mzaHxeSbu5jDaXvmNPg6r1R3fKSNBczpqaiiZMCbtKkCqaZMKBPh6eYh6a+ZSe9z
dmHt7KfV4Fu5lektr2oYrjC3xUu0pqcjbAYjhfjis3IjDSCFpnmcF/Z9lLz7JMEV
+/4YLwKBgQCu8e1t5wlenaBEAdF1t2Dw64Na0HCfqSStzt32hQiD0lFKpaoFCwWW
6AET7F6UnJmt+aKbVO++fWiAi+DlH5GrtJLL+T0VsTDZeJCvdYj3rkOqYmIEwguz
LnOKi3DCcYE4+1dFBeMj/wf8PL50rMETp64PO1rNJ2r47d64g/m3DQ==
-----END RSA PRIVATE KEY-----";
            }
        }

        public string HashSecretKey { get; } = "43E7F2C88A824F9BA27C927CACE2D08F";
        public string TVMKey { get; } = "1DD523D4D26A481F923407086C428AF7";
    }
}