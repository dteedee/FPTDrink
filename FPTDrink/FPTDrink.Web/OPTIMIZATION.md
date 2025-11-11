# Tối Ưu Hóa Website FPTDrink

## 📁 Cấu Trúc Thư Mục Images

**Vị trí đúng:** `FPTDrink.Web/wwwroot/images/`

### Cấu trúc đề xuất:
```
wwwroot/
└── images/
    ├── products/          # Hình ảnh sản phẩm
    ├── banners/          # Banner quảng cáo, carousel
    ├── categories/       # Hình ảnh danh mục
    ├── icons/            # Icon, logo nhỏ
    ├── placeholders/     # Ảnh placeholder mặc định
    └── uploads/          # Ảnh người dùng upload (nếu có)
```

**Lưu ý:**
- Tên file: chữ thường, không dấu, dùng dấu gạch ngang (-)
- Kích thước: khuyến nghị < 500KB
- Format: JPG (ảnh thật), PNG (nền trong suốt), WebP (tối ưu nhất)

---

## ✅ Các Tối Ưu Hóa Đã Thực Hiện

### 1. **Performance (Hiệu Suất)**

#### CSS & JavaScript
- ✅ Preload critical resources (Bootstrap, site.css)
- ✅ Defer JavaScript để không block rendering
- ✅ Minification với `asp-append-version` (cache busting)
- ✅ Lazy loading images (native + Intersection Observer fallback)
- ✅ Skeleton loading states

#### Images
- ✅ Lazy loading cho tất cả images
- ✅ Width/Height attributes để tránh layout shift
- ✅ Responsive images với object-fit

### 2. **SEO (Tối Ưu Tìm Kiếm)**

- ✅ Meta tags đầy đủ (description, keywords, author)
- ✅ Open Graph tags (Facebook sharing)
- ✅ Twitter Card tags
- ✅ Semantic HTML (main, header, footer, nav)
- ✅ Skip link cho accessibility
- ✅ Lang attribute (vi)

### 3. **User Experience (Trải Nghiệm Người Dùng)**

#### Toast Notifications
- ✅ Hệ thống thông báo toast tự động
- ✅ Tích hợp với TempData (SuccessMessage, ErrorMessage)
- ✅ 4 loại: success, error, warning, info
- ✅ Tự động đóng sau 3 giây

#### Form Enhancements
- ✅ HTML5 validation
- ✅ Loading states khi submit
- ✅ Quantity controls (+/-) với validation
- ✅ Smooth scroll

#### Visual Feedback
- ✅ Hover effects trên cards
- ✅ Button animations
- ✅ Loading spinners
- ✅ Skeleton screens

### 4. **Accessibility (Tiếp Cận)**

- ✅ Skip link (bỏ qua đến nội dung chính)
- ✅ ARIA labels
- ✅ Focus states rõ ràng
- ✅ Semantic HTML
- ✅ Reduced motion support

### 5. **Code Quality**

- ✅ Error handling với try-catch
- ✅ Debounce cho search
- ✅ Modular JavaScript (FPTDrink namespace)
- ✅ CSS utility classes
- ✅ Responsive design

---

## 🚀 Cách Sử Dụng

### Toast Notifications

Trong Controller:
```csharp
TempData["SuccessMessage"] = "Thông báo thành công!";
TempData["ErrorMessage"] = "Có lỗi xảy ra!";
```

Trong JavaScript:
```javascript
window.FPTDrink.Toast.show('Thông báo', 'success', 3000);
// Types: 'success', 'error', 'warning', 'info'
```

### Loading Overlay

```javascript
window.FPTDrink.LoadingOverlay.show();
window.FPTDrink.LoadingOverlay.hide();
```

### Lazy Loading Images

Tự động hoạt động với:
```html
<img src="image.jpg" loading="lazy" width="300" height="220" alt="Description" />
```

### Quantity Controls

Tự động hoạt động với:
```html
<div data-quantity>
    <button class="btn-minus">-</button>
    <input type="number" name="quantity" value="1" />
    <button class="btn-plus">+</button>
</div>
```

---

## 📊 Performance Checklist

- [x] Lazy loading images
- [x] Defer JavaScript
- [x] Preload critical CSS
- [x] Minify CSS/JS
- [x] Cache busting
- [x] Optimize images (width/height)
- [x] Reduce layout shift
- [x] Smooth scrolling
- [x] Debounce search
- [x] Error handling

---

## 🔧 Tùy Chỉnh

### Thay đổi thời gian hiển thị Toast:
Sửa trong `site.js`:
```javascript
Toast.show(message, type, duration = 3000); // Đổi 3000 thành giá trị khác (ms)
```

### Thêm custom styles:
Thêm vào `site.css` hoặc trong `@section Styles` của view.

### Thêm meta tags cho từng trang:
```csharp
ViewData["Description"] = "Mô tả trang này";
ViewData["Title"] = "Tiêu đề trang";
```

---

## 📝 Notes

- Tất cả images nên đặt trong `wwwroot/images/`
- Sử dụng relative path: `~/images/...`
- Placeholder image: `~/images/placeholder.png`
- Toast notifications tự động hiển thị từ TempData
- Lazy loading hoạt động tự động cho tất cả images

---

## 🎯 Next Steps (Tùy Chọn)

1. **CDN**: Sử dụng CDN cho Bootstrap/jQuery
2. **Service Worker**: Thêm PWA support
3. **Image Optimization**: Tự động resize/compress images
4. **Analytics**: Thêm Google Analytics
5. **Error Tracking**: Thêm Sentry hoặc tương tự
6. **A/B Testing**: Thêm testing framework

---

**Last Updated:** 2025-01-XX
**Version:** 1.0.0

