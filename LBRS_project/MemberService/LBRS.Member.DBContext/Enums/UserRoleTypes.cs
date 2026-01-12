namespace LBRS.Member.DBContext.Enums
{
    public enum UserRoleTypes
    {
        /// <summary>
        /// Administrator with full permissions (can manage catalog)
        /// </summary>
        Admin = 1,

        /// <summary>
        /// Regular user with basic permissions (can reserve books)
        /// </summary>
        Member = 2

    }
}
