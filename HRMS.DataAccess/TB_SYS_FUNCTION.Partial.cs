namespace DA
{
    public partial class TB_SYS_FUNCTION
    {
        // RIGHT_TYPE da duoc dua vao EDMX va materialize truc tiep tu TB_SYS_FUNCTION.cs.
        // Helper xac dinh loai quyen theo dac ta muc 14:
        public bool IsLoginRight => string.Equals(RIGHT_TYPE, "LOGIN", System.StringComparison.OrdinalIgnoreCase);
        public bool IsCategoryRight => string.Equals(RIGHT_TYPE, "CATEGORY", System.StringComparison.OrdinalIgnoreCase);
        public bool IsBusinessFunction => string.IsNullOrEmpty(RIGHT_TYPE) || string.Equals(RIGHT_TYPE, "FUNCTION", System.StringComparison.OrdinalIgnoreCase);
    }
}
