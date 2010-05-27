namespace Metsys.Little
{
    internal struct DataHeader
    {
        public static readonly DataHeader Default = new DataHeader {IsNull = false};
        public bool IsNull{ get; set;}
    }
}