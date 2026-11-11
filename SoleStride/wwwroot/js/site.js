// SoleStride - Core Frontend Interactive Helper Scripts

document.addEventListener('DOMContentLoaded', function () {
    // 1. Password Visibility Toggle
    document.querySelectorAll('[data-toggle="password"]').forEach(function (button) {
        button.addEventListener('click', function () {
            const targetId = this.getAttribute('data-target');
            const input = document.getElementById(targetId);
            if (!input) return;

            if (input.type === 'password') {
                input.type = 'text';
                this.classList.add('active');
            } else {
                input.type = 'password';
                this.classList.remove('active');
            }
        });
    });

    // 2. Quantity Stepper Controls
    document.querySelectorAll('.qty-step-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = this.parentElement.querySelector('input[type="number"]');
            if (!input) return;

            let val = parseInt(input.value) || 1;
            const min = parseInt(input.getAttribute('min')) || 1;
            const max = parseInt(input.getAttribute('max')) || 999;

            if (this.classList.contains('qty-minus')) {
                if (val > min) val--;
            } else if (this.classList.contains('qty-plus')) {
                if (val < max) val++;
            }

            input.value = val;
            // Dispatch change event for automatic cart recalculation
            input.dispatchEvent(new Event('change', { bubbles: true }));
        });
    });

    // 3. Global AJAX Add-To-Cart with Flying Orange Bubble & No Page Reload
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (form && form.action && form.action.toLowerCase().includes('/cart/addtocart')) {
            e.preventDefault();
            e.stopPropagation();

            const submitBtn = form.querySelector('button[type="submit"]') || form.querySelector('button');
            const originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';

            // Launch flying orange bubble animation immediately from button to cart icon
            flyBubbleToCart(submitBtn || form);

            // Loading state on button
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = `
                    <span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                    <span>Adding...</span>
                `;
            }

            const formData = new FormData(form);

            fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            })
            .then(function () {
                // Fetch updated cart count from CartController.CartCount()
                return fetch('/Cart/CartCount');
            })
            .then(function (res) {
                return res.json();
            })
            .then(function (count) {
                updateCartBadges(count);

                // Success visual state on the clicked button
                if (submitBtn) {
                    submitBtn.innerHTML = `
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                            <path d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0"/>
                        </svg>
                        <span>Added to Bag!</span>
                    `;
                }

                // Show floating notification toast
                showCartToast("Item Added to Bag", "Your footwear selection has been added successfully.");

                setTimeout(function () {
                    if (submitBtn) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalBtnHtml;
                    }
                }, 1600);
            })
            .catch(function (err) {
                console.error("Cart error:", err);
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalBtnHtml;
                }
            });
        }
    });
});

// Flying Orange Bubble Animation into Cart Icon
function flyBubbleToCart(sourceEl) {
    // 1. Detect active visible cart target (desktop navbar or mobile bottom navigation)
    const desktopCart = document.getElementById('desktopCartBtn');
    const mobileCart = document.getElementById('mobileCartBtn');
    let targetEl = desktopCart;

    if (window.innerWidth < 768 && mobileCart && mobileCart.offsetParent !== null) {
        targetEl = mobileCart;
    } else if (desktopCart && desktopCart.offsetParent !== null) {
        targetEl = desktopCart;
    } else {
        targetEl = mobileCart || desktopCart;
    }

    if (!targetEl) return;

    // 2. Compute Start Coordinates
    let startX = window.innerWidth / 2;
    let startY = window.innerHeight / 2;

    if (sourceEl) {
        const sRect = sourceEl.getBoundingClientRect();
        startX = sRect.left + sRect.width / 2;
        startY = sRect.top + sRect.height / 2;
    }

    // 3. Compute Target Coordinates
    const tRect = targetEl.getBoundingClientRect();
    const endX = tRect.left + tRect.width / 2;
    const endY = tRect.top + tRect.height / 2;

    const deltaX = endX - startX;
    const deltaY = endY - startY;

    // Helper to spawn a flying bubble with parabolic trajectory
    function spawnBubble(size, delayMs, isMain) {
        const bubble = document.createElement('div');
        bubble.className = isMain ? 'flying-orange-bubble' : 'flying-bubble-trail';
        bubble.style.width = size + 'px';
        bubble.style.height = size + 'px';
        bubble.style.left = (startX - size / 2) + 'px';
        bubble.style.top = (startY - size / 2) + 'px';
        document.body.appendChild(bubble);

        // Control point for smooth parabolic arc
        const midX = deltaX * 0.5;
        // Arc upwards higher when the path is longer
        const arcElevation = Math.max(70, Math.min(Math.abs(deltaX) * 0.35 + 50, 160));
        const midY = deltaY * 0.45 - arcElevation;

        setTimeout(function () {
            const animation = bubble.animate([
                {
                    transform: 'translate(0px, 0px) scale(0.65)',
                    opacity: 0.95
                },
                {
                    transform: `translate(${midX}px, ${midY}px) scale(1.3)`,
                    opacity: 1,
                    offset: 0.5
                },
                {
                    transform: `translate(${deltaX}px, ${deltaY}px) scale(0.35)`,
                    opacity: 0.8
                }
            ], {
                duration: 700,
                easing: 'cubic-bezier(0.2, 0.9, 0.35, 1)',
                fill: 'forwards'
            });

            animation.onfinish = function () {
                bubble.remove();
                if (isMain) {
                    // Trigger bump / bounce on the cart icon when bubble arrives
                    targetEl.classList.remove('cart-bump');
                    void targetEl.offsetWidth; // trigger reflow
                    targetEl.classList.add('cart-bump');
                    setTimeout(function () {
                        targetEl.classList.remove('cart-bump');
                    }, 600);
                }
            };
        }, delayMs);
    }

    // Spawn 1 main vibrant orange bubble + 2 trail bubbles
    spawnBubble(26, 0, true);
    spawnBubble(16, 60, false);
    spawnBubble(10, 120, false);
}

// Update Cart Badge Numbers & Animations
function updateCartBadges(count) {
    const desktopBadge = document.getElementById('cartCount');
    const mobileBadge = document.getElementById('mobileCartCount');

    [desktopBadge, mobileBadge].forEach(function (badge) {
        if (!badge) return;
        badge.textContent = count;
        if (count > 0) {
            badge.classList.remove('d-none');
            badge.classList.remove('badge-pop');
            void badge.offsetWidth; // Trigger DOM reflow for CSS animation
            badge.classList.add('badge-pop');
        } else {
            badge.classList.add('d-none');
        }
    });
}

// Show Floating Cart Toast
let toastTimeout = null;
function showCartToast(title, message) {
    const toast = document.getElementById('cartToast');
    if (!toast) return;

    const titleEl = document.getElementById('toastTitle');
    const descEl = document.getElementById('toastDesc');
    if (titleEl && title) titleEl.textContent = title;
    if (descEl && message) descEl.textContent = message;

    toast.classList.add('show');

    if (toastTimeout) clearTimeout(toastTimeout);
    toastTimeout = setTimeout(function () {
        toast.classList.remove('show');
    }, 3500);
}

// 4. Avatar Live Preview (Used in EditProfile)
function previewAvatar(event) {
    const input = event.target;
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const previewImg = document.getElementById('avatarPreview');
            const container = document.getElementById('avatarPreviewContainer');
            if (previewImg && previewImg.tagName === 'IMG') {
                previewImg.src = e.target.result;
            } else if (container) {
                container.innerHTML = `<img src="${e.target.result}" id="avatarPreview" alt="Avatar Preview" class="profile-avatar-large" />`;
            }
        };
        reader.readAsDataURL(input.files[0]);
    }
}
