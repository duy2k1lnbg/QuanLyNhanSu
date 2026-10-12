namespace DA
{
    using System.Data.Entity;

    public partial class MyEntities
    {
        public virtual DbSet<TB_USER_DEVICE> TB_USER_DEVICE { get; set; }
        public virtual DbSet<TB_PUSH_OUTBOX> TB_PUSH_OUTBOX { get; set; }
    }
}
