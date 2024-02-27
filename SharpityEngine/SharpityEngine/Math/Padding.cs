using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Padding
    {
        // Public
        [DataMember]
        public int LeftOffset;
        [DataMember]
        public int RightOffset;
        [DataMember]
        public int TopOffset;
        [DataMember]
        public int BottomOffset;

        // Constructor
        public Padding(int left, int right, int top, int bottom)
        {
            this.LeftOffset = left;
            this.RightOffset = right;
            this.TopOffset = top;
            this.BottomOffset = bottom;
        }
    }
}