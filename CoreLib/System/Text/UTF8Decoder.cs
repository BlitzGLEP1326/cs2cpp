////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.CompilerServices;

namespace System.Text
{
    internal class UTF8Decoder : Decoder
    {

        public override void Convert(byte[] bytes, int byteIndex, int byteCount,
            char[] chars, int charIndex, int charCount, bool flush,
            out int bytesUsed, out int charsUsed, out bool completed)
        {
            throw new NotImplementedException();
        }
    }
}


