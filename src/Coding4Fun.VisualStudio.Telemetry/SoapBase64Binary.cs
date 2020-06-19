using System;
using System.Collections.Generic;
using System.Text;

namespace Coding4Fun.VisualStudio.ApplicationInsights
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class SoapBase64Binary 
    {
        byte[] _value;

        public SoapBase64Binary()
        {
        }

        public SoapBase64Binary(byte[] value)
        {
            _value = value;
        }

        public byte[] Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public static string XsdType
        {
            get { return "base64Binary"; }
        }

        public string GetXsdType()
        {
            return XsdType;
        }

        public static SoapBase64Binary Parse(string value)
        {
            return new SoapBase64Binary(Convert.FromBase64String(value));
        }

        public override string ToString()
        {
            return Convert.ToBase64String(_value);
        }
    }
}
