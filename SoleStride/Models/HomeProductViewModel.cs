namespace SoleStride.Models
{
    /// <summary>
    /// ViewModel for Home/Index product cards, containing shoe information and stock quantities
    /// </summary>
    public class HomeProductViewModel
    {
        /// <summary>
        /// The shoes product information
        /// </summary>
        public required Shoes Shoes { get; set; }

        /// <summary>
        /// Number of shoes currently in stock (Status = Available)
        /// </summary>
        public int QuantityAvailable { get; set; }

        /// <summary>
        /// Number of shoes sold (Status = Sold)
        /// </summary>
        public int QuantitySold { get; set; }
    }
}
