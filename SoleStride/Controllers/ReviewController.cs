using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoleStride.Models;

namespace SoleStride.Controllers
{
    public class ReviewController : Controller
    {
        private readonly SoleStrideDbContext _context;

        public ReviewController(SoleStrideDbContext context)
        {
            _context = context;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        private bool IsAdmin() => HttpContext.Session.GetString("Role") == "Admin";
        private bool IsStaff() => HttpContext.Session.GetString("Role") == "Staff";

        // POST: Review/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid productId, int rating, string? title, string comment, ShoeFit fitFeedback)
        {
            var username = HttpContext.Session.GetString("Username");
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (string.IsNullOrEmpty(username))
            {
                if (isAjax)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để gửi đánh giá sản phẩm.", requireLogin = true });
                }
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", "Shoes", new { id = productId }) });
            }

            var product = await _context.Shoes.FindAsync(productId);
            if (product == null)
            {
                if (isAjax) return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                return NotFound();
            }

            if (rating < 1 || rating > 5)
            {
                if (isAjax) return Json(new { success = false, message = "Vui lòng chọn số sao từ 1 đến 5." });
                TempData["ReviewError"] = "Vui lòng chọn số sao đánh giá hợp lệ.";
                return RedirectToAction("Details", "Shoes", new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                if (isAjax) return Json(new { success = false, message = "Vui lòng nhập nội dung đánh giá." });
                TempData["ReviewError"] = "Vui lòng nhập nội dung đánh giá.";
                return RedirectToAction("Details", "Shoes", new { id = productId });
            }

            // Check if user is a verified buyer (has a Delivered order containing this product)
            var isVerified = await _context.Orders
                .Where(o => o.Username == username && o.Status == "Delivered")
                .AnyAsync(o => o.OrderDetails.Any(od => od.ProductId == productId));

            var existingReview = await _context.ProductReviews
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.Username == username);

            if (existingReview != null)
            {
                // Update existing review
                existingReview.Rating = rating;
                existingReview.Title = title?.Trim();
                existingReview.Comment = comment.Trim();
                existingReview.FitFeedback = fitFeedback;
                existingReview.CreatedAt = DateTime.Now;
                existingReview.IsVerifiedPurchase = isVerified;
                existingReview.IsVisible = true;
                _context.Update(existingReview);
            }
            else
            {
                // Create new review
                var review = new ProductReview
                {
                    ProductId = productId,
                    Username = username,
                    Rating = rating,
                    Title = title?.Trim(),
                    Comment = comment.Trim(),
                    FitFeedback = fitFeedback,
                    CreatedAt = DateTime.Now,
                    IsVerifiedPurchase = isVerified,
                    IsVisible = true,
                    HelpfulCount = 0
                };
                _context.ProductReviews.Add(review);
            }

            await _context.SaveChangesAsync();

            if (isAjax)
            {
                return Json(new { success = true, message = "Cảm ơn bạn! Đánh giá đã được đăng thành công." });
            }

            TempData["ReviewSuccess"] = "Cảm ơn bạn! Đánh giá của bạn đã được lưu thành công.";
            TempData["NewReviewJustSubmitted"] = true;
            return Redirect($"{Url.Action("Details", "Shoes", new { id = productId })}#reviews-section");
        }

        // POST: Review/MarkHelpful
        [HttpPost]
        public async Task<IActionResult> MarkHelpful(int reviewId)
        {
            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá." });
            }

            review.HelpfulCount += 1;
            await _context.SaveChangesAsync();

            return Json(new { success = true, helpfulCount = review.HelpfulCount });
        }

        // POST: Review/ShopReply (Admin/Staff only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShopReply(int reviewId, string reply, string? returnUrl)
        {
            if (!IsAdmin() && !IsStaff())
            {
                return Unauthorized();
            }

            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            review.ShopReply = string.IsNullOrWhiteSpace(reply) ? null : reply.Trim();
            review.ShopRepliedAt = string.IsNullOrWhiteSpace(reply) ? null : DateTime.Now;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Details", "Shoes", new { id = review.ProductId });
        }

        // POST: Review/ToggleVisibility (Admin/Staff only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(int reviewId, string? returnUrl)
        {
            if (!IsAdmin() && !IsStaff())
            {
                return Unauthorized();
            }

            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            review.IsVisible = !review.IsVisible;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Details", "Shoes", new { id = review.ProductId });
        }

        // POST: Review/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int reviewId, string? returnUrl)
        {
            var currentUsername = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(currentUsername))
            {
                return Unauthorized();
            }

            var review = await _context.ProductReviews.FindAsync(reviewId);
            if (review == null) return NotFound();

            // Only author or Admin/Staff can delete
            if (review.Username != currentUsername && !IsAdmin() && !IsStaff())
            {
                return Forbid();
            }

            _context.ProductReviews.Remove(review);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Details", "Shoes", new { id = review.ProductId });
        }
    }
}
