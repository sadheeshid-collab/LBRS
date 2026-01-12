namespace LBRS.Book.DBContext.Enums
{
    public enum ReservationStatusTypes
    {
        /// <summary>
        /// Reservation is active and valid
        /// </summary>
        Reserved = 0,

        /// <summary>
        /// Reservation is completed (book was picked up)
        /// </summary>
        PickedUp = 1,

        /// <summary>
        /// Reservation is completed (book was picked up)
        /// </summary>
        Returned = 2,

        /// <summary>
        /// Reservation has been cancelled by the user
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Reservation has expired (not picked up in time)
        /// </summary>
        Expired = 4
    }
}
