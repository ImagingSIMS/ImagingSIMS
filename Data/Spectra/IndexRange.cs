using System;

namespace ImagingSIMS.Data.Spectra
{
    internal class IndexRange : IComparable
    {
        int _startIndex;
        int _endIndex;

        public IndexRange()
        {
            _startIndex = -1;
            _endIndex = -1;
        }
        public IndexRange(int StartIndex, int EndIndex)
        {
            _startIndex = StartIndex;
            _endIndex = EndIndex;
        }

        public bool IsInRange(uint Index)
        {
            if (_startIndex < 0 || _endIndex < 0)
            {
                throw new ArgumentException("Invalid mass range.");
            }

            return (Index >= _startIndex && Index <= _endIndex);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj.GetType() != typeof(IndexRange))
                throw new ArgumentException("Cannont compare different types.");

            IndexRange i = (IndexRange)obj;

            if (StartIndex < i.StartIndex) return -1;
            if (StartIndex == i.StartIndex) return 0;
            return 1;
        }

        public int StartIndex
        {
            get
            {
                return _startIndex;
            }
            set
            {
                _startIndex = value;
            }
        }
        public int EndIndex
        {
            get
            {
                return _endIndex;
            }
            set
            {
                _endIndex = value;
            }
        }
    }
}
