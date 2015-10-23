﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LibLSLCC.CodeValidator.Exceptions
{
    [Serializable]
    public class LSLInvalidSubsetNameException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LSLInvalidSubsetNameException()
        {
        }

        /// <summary>
        /// Construct with message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public LSLInvalidSubsetNameException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construct with message and inner exception.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public LSLInvalidSubsetNameException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Serializable constructor.
        /// </summary>
        /// <param name="info">SerializationInfo.</param>
        /// <param name="context">StreamingContext.</param>
        protected LSLInvalidSubsetNameException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
