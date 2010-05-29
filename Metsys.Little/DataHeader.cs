namespace Metsys.Little
{
    internal struct DataHeader
    {
        public static readonly DataHeader Default = new DataHeader{IsNull = false, IsAmbiguous = false,};
        public bool IsNull{ get; set;}
        public bool IsAmbiguous{ get; set;}
    }
}