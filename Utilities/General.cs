using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XenoEngine.Utilities
{
    public partial struct Rslt
    {
        private int m_nRsltFlags;

        public static Rslt operator |(Rslt rslt, int nFlag)
        {
            rslt.m_nRsltFlags |= nFlag;
            return rslt;
        }

        public static Rslt operator |(Rslt rslt, ResultValue eFlag)
        {
            rslt.m_nRsltFlags |= (int)eFlag;
            return rslt;
        }

        public static Rslt operator &(Rslt rslt, int nFlag)
        {
            rslt.m_nRsltFlags &= nFlag;
            return rslt;
        }

        public static Rslt operator &(Rslt rslt, ResultValue eFlag)
        {
            rslt.m_nRsltFlags &= (int)eFlag;
            return rslt;
        }

        public bool Contains(ResultValue eFlag)
        {
            return 0 == (m_nRsltFlags & (int)eFlag);
        }

        public bool Contains(int nFlag)
        {
            return 0 == (m_nRsltFlags & nFlag);
        }

        public void ClearRslt()
        {
            m_nRsltFlags = 0;
        }
    }

    [Flags]
    public enum ResultValue
    {
        Fail,
        Success,
        NonAuthoritative,
        Invalid_Sender,
        Invalid_Receiver,
    }
}
