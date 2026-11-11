using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoleStride.Models
{
    public enum ShoeFit
    {
        RunsSmall = 1,  // Chật (-0.5 size)
        TrueToSize = 2, // Vừa vặn chuẩn size
        RunsLarge = 3   // Rộng (+0.5 size)
    }

    public class ProductReview
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Shoes? Product { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [ForeignKey(nameof(Username))]
        public User? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [MaxLength(120)]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Please write your review comment.")]
        [MaxLength(1000, ErrorMessage = "Review comment cannot exceed 1000 characters.")]
        public string Comment { get; set; }

        [Required]
        public ShoeFit FitFeedback { get; set; } = ShoeFit.TrueToSize;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsVerifiedPurchase { get; set; } = false;

        public int HelpfulCount { get; set; } = 0;

        public bool IsVisible { get; set; } = true;

        [MaxLength(1000)]
        public string? ShopReply { get; set; }

        public DateTime? ShopRepliedAt { get; set; }
    }
}
