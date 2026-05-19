using System;

namespace NoSuchStudio.Common.Text {

    public class CSVException : Exception {
        public CSVException(string msg) : base(msg) { }
    }
}