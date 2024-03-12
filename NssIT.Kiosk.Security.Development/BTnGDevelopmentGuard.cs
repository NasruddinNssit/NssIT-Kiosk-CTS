using NssIT.Kiosk.AppDecorator.DomainLibs.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Security.Development
{
    public class BTnGDevelopmentGuard : IBTnGGuardInfo
    {
        private static BTnGDevelopmentGuard _guard = new BTnGDevelopmentGuard();

        public static IBTnGGuardInfo GuardInfo
        {
            get
            {
                return _guard;
            }
        }

        public string MerchantId { get; } = "KTMB";
        //public string MerchantId { get; } = "Genting-Kiosk";

        public string WebApiUrl { get; } = @"http://203.115.193.19/paymentgateway/api";

        private BTnGDevelopmentGuard() { }

        public string PublicKey
        {
            get
            {
                return @"-----BEGIN PUBLIC KEY-----
        MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzXDPep1R/Up8yRy3IiVw
        PPJETvB52L0fk8ACVNO+8DPznDLnfkUrXAXKdPGBjMv4IwzKhG3WSwDmibcsq1mA
        CLWnq1GovVM3OF98NK8HGA3o5jBIaPukmWd7KScTQvam/S9tUZbvmbX+62CPOdwE
        ZnMxeLAi2cDzflSd3og4YLs6vnLZ/1aLY2fTW4uf8jwwJFmmzp/16lurfTGJU6AZ
        wc3uD5MY0kaKKxXRKG69xLFz50+ElWGgHBY5flkSviPT3EEdDSIzDLQ5FbYtgUmq
        ZmWKUE40IIrfVGMASSM4dFFAZVMxx3P5syBG6PomliLp0Ksfn3/gRzA8cOhIGHM+
        iQIDAQAB
        -----END PUBLIC KEY-----";
            }
        }
        //        public string PublicKey
        //        {
        //            get
        //            {
        //                return @"-----BEGIN PUBLIC KEY-----
        //MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA7DjfkSbm8uWqHx22OheE
        //UBJ3pNABGjyWgg3Pl5Ej/4i6VBjGGMr2XbC/V46F8E4LGj+ZuF+XHeYSmdzdzcXy
        //hrIVTxLKCdkio05aS2h/bIkltY4HcZh2Dzy45wZprw54h0X/B8ARiofSufxTRrcX
        //7B0TJOpZ1CkVcsNA6kBMdRvjGfcJFQ1d+XNeHl7t4r3i8jwjiAxw25lNJdHrTfeT
        //maRkFPsWeQbCm02Ith5bMso/tBQ6YEDCZ2Lne5ohTg19XzerotpC/86o0I7MuXVk
        //0zn7TQlqdbj3qi2opL4pDEMSA7IcVuTEZiQNk3XyhnrDujvt7viomv5OF71meezt
        //XwIDAQAB
        //-----END PUBLIC KEY-----
        //";
        //            }
        //        }
        public string PrivateKey
        {
            get
            {
                return @"-----BEGIN RSA PRIVATE KEY-----
        MIIEowIBAAKCAQEAts382izmQVYo2iZyp+vMXxdmItahEFk0bqo2tBnPnJH4VWBx
        KAmOrF4g7iSKYB3JpNl0BDUDDJmbB2T4EY08V9XBH5jvJQYht1kapAtPzUkpobQa
        iitye21uDBwXtQengSCriS0AmOSt9+E4Sp1Cr7JIJMYTz6+TA6T8/rJsUPu5jDST
        ecwB1wsVWM0dchC+pr99hXRyFSBdKFzYR6bqzk2x33CKemhTGmidQZSU9AT+JFEX
        eOhu+nCEqPMH3AQBn87oxTMIVa+y6eQeH5ESUvvGtL6q/hsTLJ8mgQfnpudFYKkF
        VC2B4qPKHp05/UmCQGmYu1BEC6plx2KfcsXeWwIDAQABAoIBACnNDIavQa+rDghk
        Rk+TodYUuaw5u/bLDyxHC98/D7RTxJ9xQC7RkKhllc4e2O2Fojhp6ReVL69P6J1W
        P0t0KlpDbLfW9shWkJsmauscExF0K2rojjEOIk2LBmEKg3lH2Mu0NsVVXw+XKxjF
        jWOydi9K6yhNivYwxcNNlPSAxDf4FKVfnqXYtC6Wh/60lCuzHDaaFaQwQ1GTPQY0
        6AGZCPXwn4z1tHwmY3GIY9JLcHYaYSLqqwvlrLnOfFyp/4JWK6qcivYgK+hhWfOv
        tDir1u0K7JfComH0cwJthAoKknYMgNlIxuoUuYHWIokR25cLRXQhBYRO2mnZo7qu
        yJOEwKECgYEA5ep79GKPfZMNXHj3/XeeigNZy8c9+XyAeH9+Wl2TGfVx3glO8wJm
        m9eZmHIlntPC5AWtmxIoGYWedrtVnL2Xn+FELKCv6oUphqp7/nqez2GddenkuP+c
        ptDp7rLeordwOe8PdEKHt3mmCEjCxmgJ4FV3Ovh0u281TyECsIqP+UMCgYEAy4s8
        ZXaRhO1EgKbETPgZ2xic73DUhx6CMYG3ijSKMFJ+dm+UdvIqpxonp4cMyrW3ePlv
        n680Ey9GCt4qhJDzXlk9DO6UgYiM907a9Fm/ZXgEICngxqDd7sk7jDNzXXSXJQmB
        PNZGTk/9CIlrRD+vI6dB3/Et5Mbjwb5rFXC2SQkCgYAKXPldWJvzIw+1HVbAPAYP
        XntLrh1jA3Oe+tAtLo6U2vVY9r5yQadyWtN3hZ2gfRcJxB/BH55jGBy+aU9Ak2Mk
        N7kk8dE8Fuh6Q3D3VXuXCWVZjUNb+1mKQ1xn//P9DZunYNknemA3quoK8Yyl+MaJ
        MBEBvXU1hZu3h1thrb0zlQKBgQCZgONGndn3Br1fzOVEKuPNAU3xogUV9eM4FNzn
        hOImuUAYb+PmpJGYPjhjtozmH49D09Hj+szqHv/S2GP2YB66K9DH/PHQkrvFExo7
        p6eZjZ6G5y5WfiGBoQ+gl3jMpU4Lp5Ro3ixdiSOKGaDk8qZR3CTpD8mNvJUtmz7F
        B6DhiQKBgCciNAL2GTULA4vsj03Eply7vqg7v4x4HFp9achqLHg5h2byZRmid/OL
        G5SWJirwJrsPmp1P8gV9ulSZMEjbHvAHlAZFx6Og08S89vfMZNEIDts43ZyA1pOK
        j5tU4pwO2Xl0I0a29x7tk4LkfU3N6jgiRU2I5NBr4I6lvoVl0FtL
        -----END RSA PRIVATE KEY-----";
            }
        }

//        public string PrivateKey
//        {
//            get
//            {
//                return @"-----BEGIN RSA PRIVATE KEY-----
//MIIEogIBAAKCAQEA7DjfkSbm8uWqHx22OheEUBJ3pNABGjyWgg3Pl5Ej/4i6VBjG
//GMr2XbC/V46F8E4LGj+ZuF+XHeYSmdzdzcXyhrIVTxLKCdkio05aS2h/bIkltY4H
//cZh2Dzy45wZprw54h0X/B8ARiofSufxTRrcX7B0TJOpZ1CkVcsNA6kBMdRvjGfcJ
//FQ1d+XNeHl7t4r3i8jwjiAxw25lNJdHrTfeTmaRkFPsWeQbCm02Ith5bMso/tBQ6
//YEDCZ2Lne5ohTg19XzerotpC/86o0I7MuXVk0zn7TQlqdbj3qi2opL4pDEMSA7Ic
//VuTEZiQNk3XyhnrDujvt7viomv5OF71meeztXwIDAQABAoIBAAyjZQAMwccgbH//
//r0k1CuTcfWmO9hAjn7XTlqti84xvZ0X/5qawQorJXbi0gmFUpkQVlQbdQ7uGHLFT
//X3aWmZ7alkibFmjSejJHNeOE8r7+03VUOlJkjXnL449Qs4TPKbmrEAr9HAh6Xdbf
//iMu8GLE5mQBHnYi8A07BjY6QOXGmcvyGq+LoxHayS3XMqoiW8rMknLYCIEFM0tiH
//qI340VKLFDBpakN7DgPis5T6T3KEeoRMn7TAJZ7kOaX2ZcMx72TOIBbDt2pPjiwZ
//DYuzrwfprF7C5laHZ/fNRlrNHaR3aIP3HbP2x3znuDj2Qeym7LiLSAmZ5Xq8TZE7
//7kFbExkCgYEA77CKxgCb0+XILhf2DzyaQOKY85uMnwXYtlCNSNsZ/LRzKtVTyLXC
//RwYJK4PnIIJpf8hvcoA811yeV0YXd69pI7Jhe0t5kJ0i+ORcK6q30+x9NgSmfc/O
//vmuFBkWTEuh37V+GGwjRMHFmwME8RuI3/r57pcFQVtzyt6sDkq9vCC0CgYEA/Evt
//XRP1LMJzINT43wtFx+w0V900CiTRM13b1WmwDtNZLhdCQZoXLgzOauWcalps5Cyj
//JlkT7daHGPgB9vfC8o3kEZArge274nitp2cwPJp2dwg9K6x68Xv+jnVm+Lj/27Nc
//Vo0lmlr2s5hK7wNIfFonYRug8HhicsGZ3I5CFzsCgYADBkxKAVn2C5FCG6VMPeyI
//YxOhB1Z37z/z1QJjZ/hk+vX1FWoQP1LrSIU3GlwBSgQAy8u0OzAJW67X1ReYuArw
//m2CliKdJDuKRF3ieSHI3Z2WRF+/t6IBoYLz1/kzD0VEqGql7j9nr7ZJpXkbfch1Y
//xpiS1+Smo/UgIjVSVYpmJQKBgE1e84Hx1uRNFYViCphMGrA+zaaMXaMmu0knZX1D
//FgzV36l01IrNuIIUx1Cc7aTn6jDnR3lBJA1yFqgflmqofI5p2SFRtyX3RAElxQtr
//Z62OKPpD+o5kLKfVwLDrbBKmmUBE9vJMAtYdQz68W26E8TvBYgYztN1J7dcHbCnp
//Lnd1AoGAMmoeXLcqLQFKdVkZGTKU1Lvw2FVqZUK5bfsF53eSL7LDrIby1+pJ0aKz
//cTtGEy0wH7rf5//0J8a9i41+1rDYPk4/rlOjjc3sf1ZB7AysvzUetZ/cc/vayGKt
//puYksC6+L4nq8cylqPgIXqFIC29RYtVPyIdH4RPTHvPT6ch/R6s=
//-----END RSA PRIVATE KEY-----
//";
//            }
//        }

        public string HashSecretKey { get; } = "77BB49EB60E241AC85D108400C38AA6C";
        public string TVMKey { get; } = "6131396BC88E43F2B8538FDB60331C7C";
    }
}