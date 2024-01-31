using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Reports
{
    public class QRGen
    {
        public static Bitmap GetQRCodeBitmap(string wording)
        {
            // Retrieve the parameters from the QueryString
            CodeDescriptor codeParams = CodeDescriptor.Init(wording);

            // Encode the content
            if (codeParams == null || !codeParams.TryEncode())
            {
                return null;
            }

            // Render the QR code as an image
            return codeParams.RenderToBitmap();
        }

        public static MemoryStream GetQRCodeMemoryStream(string wording)
        {
            // Retrieve the parameters from the QueryString
            var codeParams = CodeDescriptor.Init(wording);

            // Encode the content
            if (codeParams == null || !codeParams.TryEncode())
            {
                return null;
            }

            // Render the QR code as an image
            return codeParams.RenderToMemory();
        }

        public static byte[] GetQRCodeData(string wording)
        {
            // Retrieve the parameters from the QueryString
            var codeParams = CodeDescriptor.Init(wording);

            // Encode the content
            if (codeParams == null || !codeParams.TryEncode())
            {
                return null;
            }

            // Render the QR code as an image
            MemoryStream ms = codeParams.RenderToMemory();

            return ms?.ToArray(); ;
        }


        /// <summary>
        /// Class containing the description of the QR code and wrapping encoding and rendering.
        /// </summary>
        internal class CodeDescriptor
        {
            public ErrorCorrectionLevel Ecl;
            public string Content;
            public QuietZoneModules QuietZones;
            public int ModuleSize;
            public BitMatrix Matrix;

            /// <summary>
            /// Parse QueryString that define the QR code properties
            /// </summary>
            /// <param name="request">HttpRequest containing HTTP GET data</param>
            /// <returns>A QR code descriptor object</returns>
            public static CodeDescriptor Init(string wording)
            {
                var cp = new CodeDescriptor()
                {
                    Ecl = ErrorCorrectionLevel.M,
                    QuietZones = QuietZoneModules.Two,
                    ModuleSize = 4,
                    Content = wording
                };

                return cp;
            }

            public static bool EnumTryParse<T>(string strType, out T result)
            {
                string strTypeFixed = strType.Replace(' ', '_');
                if (Enum.IsDefined(typeof(T), strTypeFixed))
                {
                    result = (T)Enum.Parse(typeof(T), strTypeFixed, true);
                    return true;
                }
                else
                {
                    foreach (string value in Enum.GetNames(typeof(T)))
                    {
                        if (value.Equals(strTypeFixed, StringComparison.OrdinalIgnoreCase))
                        {
                            result = (T)Enum.Parse(typeof(T), value);
                            return true;
                        }
                    }
                    result = default(T);
                    return false;
                }
            }

            /// <summary>
            /// Encode the content with desired parameters and save the generated Matrix
            /// </summary>
            /// <returns>True if the encoding succeeded, false if the content is empty or too large to fit in a QR code</returns>
            public bool TryEncode()
            {
                var encoder = new QrEncoder(Ecl);
                QrCode qr;
                if (!encoder.TryEncode(Content, out qr))
                    return false;

                Matrix = qr.Matrix;
                return true;
            }

            /// <summary>
            /// Render the Matrix as a PNG image
            /// </summary>
            /// <param name="ms">MemoryStream to store the image bytes into</param>
            internal Bitmap RenderToBitmap()
            {
                MemoryStream ms = new MemoryStream();
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(Matrix, ImageFormat.Png, ms);
                ms.Position = 0;
                Bitmap bm = new Bitmap(ms);
                //bm = new Bitmap(bm, 70, 70);
                return bm;
            }

            /// <summary>
            /// Render the Matrix as a PNG image
            /// </summary>
            /// <param name="ms">MemoryStream to store the image bytes into</param>
            internal MemoryStream RenderToMemory()
            {
                MemoryStream ms = new MemoryStream();
                var render = new GraphicsRenderer(new FixedModuleSize(ModuleSize, QuietZones));
                render.WriteToStream(Matrix, ImageFormat.Png, ms);
                ms.Position = 0;

                return ms;
            }
        }
    }
}