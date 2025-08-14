# SCSS Migration Guide - TechWayFit License Management

## 🎯 Quick Start

Your new SCSS architecture is ready! Follow these steps to get started:

### 1. Install Dependencies
```bash
cd source/website/TechWayFit.Licensing.Management.Web
npm install
```

### 2. Compile SCSS
```bash
# Development build (with source maps)
./build-scss.sh dev

# Or watch for changes
./build-scss.sh watch

# Production build (compressed)
./build-scss.sh prod
```

### 3. Update Layout Files

Replace the current CSS imports in your layout files with the compiled CSS:

**In `Views/Shared/_Layout.cshtml`** (around line 12):
```html
<!-- REPLACE THIS -->
<link href="~/css/site.css" rel="stylesheet">

<!-- WITH THIS -->
<link href="~/css/compiled.css" rel="stylesheet">
```

**In `Views/Shared/_LicenseHubLayout.cshtml`**:
```html
<!-- REPLACE THESE -->
<link href="~/css/licensehub.css" rel="stylesheet">
<link href="~/css/product-management.css" rel="stylesheet">
<link href="~/css/site.css" rel="stylesheet">

<!-- WITH THIS -->
<link href="~/css/compiled.css" rel="stylesheet">
```

## 🗂️ What You Get

### ✅ Organized Structure
```
wwwroot/scss/
├── abstracts/          # Variables, mixins, functions
├── base/              # Reset and base styles
├── components/        # Reusable UI components
├── layout/            # Header, sidebar, navigation
├── pages/             # Page-specific styles
└── main.scss          # Main entry point
```

### ✅ Design System
- **Consistent Colors**: Primary, success, warning, danger palettes
- **Spacing Scale**: From 4px to 48px in logical increments
- **Typography Scale**: 6 font sizes with proper line heights
- **Responsive Breakpoints**: Mobile-first approach

### ✅ Utility Classes
```html
<!-- Spacing -->
<div class="p-md mb-lg">Content</div>

<!-- Layout -->
<div class="d-flex justify-content-between align-items-center">

<!-- Typography -->
<h1 class="text-primary text-xl font-weight-bold">

<!-- Colors -->
<div class="bg-primary text-white">
```

### ✅ Component Mixins
```scss
.my-button {
    @include button-variant($primary, $white);
    @include flex-center;
}

.my-card {
    @include card-shadow;
    @include stats-card;
}
```

## 🎨 Available Components

### Stats Cards (Your Fixed Issue!)
```html
<div class="stats-card">
    <div class="stats-card__icon">
        <i class="fas fa-users"></i>
    </div>
    <div class="stats-card__content">
        <h3 class="stats-card__number">1,234</h3>
        <p class="stats-card__label">Active Users</p>
    </div>
</div>
```

### Buttons
```html
<button class="btn btn-primary">Primary</button>
<button class="btn btn-secondary">Secondary</button>
<button class="btn btn-success">Success</button>
```

### Cards
```html
<div class="card">
    <div class="card-header">
        <h3 class="card-title">Card Title</h3>
    </div>
    <div class="card-body">
        Card content
    </div>
</div>
```

### Forms
```html
<div class="form-group">
    <label class="form-label">Email</label>
    <input type="email" class="form-control">
</div>
```

## 🔧 Development Workflow

### Watch Mode (Recommended for Development)
```bash
./build-scss.sh watch
```
This will:
- Watch all SCSS files for changes
- Automatically recompile when files change
- Generate source maps for debugging
- Keep running until you stop it (Ctrl+C)

### One-time Builds
```bash
# Development build
./build-scss.sh dev

# Production build
./build-scss.sh prod
```

### Clean Compiled Files
```bash
./build-scss.sh clean
```

## 📝 Customization

### Colors
Edit `wwwroot/scss/abstracts/_variables.scss`:
```scss
$primary: #0066cc;        // Your brand primary
$success: #28a745;        // Success green
$warning: #ffc107;        // Warning yellow
$danger: #dc3545;         // Error red
```

### Spacing
```scss
$spacing-xs: 0.25rem;     // 4px
$spacing-sm: 0.5rem;      // 8px
$spacing-md: 1rem;        // 16px
$spacing-lg: 1.5rem;      // 24px
$spacing-xl: 2rem;        // 32px
```

### Typography
```scss
$font-size-xs: 0.75rem;   // 12px
$font-size-sm: 0.875rem;  // 14px
$font-size-base: 1rem;    // 16px
$font-size-lg: 1.125rem;  // 18px
```

## 🎯 Migration Steps

### Phase 1: Setup (Do This First)
1. ✅ Install npm dependencies
2. ✅ Test SCSS compilation
3. ✅ Update one layout file to use compiled CSS
4. ✅ Test that styles work correctly

### Phase 2: Gradual Migration
1. Move custom styles from old CSS files to appropriate SCSS files
2. Update component HTML to use new class names if needed
3. Test each page after migration
4. Remove old CSS files once migration is complete

### Phase 3: Optimization
1. Add any missing components to SCSS
2. Optimize utility class usage
3. Set up production build process
4. Configure CSS purging if needed

## 🚀 Production Deployment

### Build Process
```bash
# Build for production (compressed CSS)
./build-scss.sh prod
```

### Integration with ASP.NET Core
Add to your `Startup.cs` or `Program.cs`:
```csharp
// In development, you might want to serve source maps
if (app.Environment.IsDevelopment())
{
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            if (ctx.File.Name.EndsWith(".map"))
            {
                ctx.Context.Response.Headers.Add("Cache-Control", "no-cache");
            }
        }
    });
}
```

### CI/CD Pipeline
Add to your build pipeline:
```yaml
- name: Install Node.js
  uses: actions/setup-node@v3
  with:
    node-version: '18'

- name: Install dependencies
  run: npm install
  working-directory: source/website/TechWayFit.Licensing.Management.Web

- name: Build SCSS
  run: npm run scss:prod
  working-directory: source/website/TechWayFit.Licensing.Management.Web
```

## 🤔 FAQ

### Q: Do I need to update my HTML?
**A:** Mostly no! The new SCSS maintains compatibility with existing class names. You'll mainly just need to update CSS file references in layout files.

### Q: Can I still use Bootstrap classes?
**A:** Yes! Bootstrap is still included and all Bootstrap classes work as before.

### Q: What about the existing CSS variables?
**A:** All the CSS custom properties you were using are maintained in the new system.

### Q: How do I add new components?
**A:** Create a new file in `wwwroot/scss/components/` and import it in `main.scss`.

### Q: Can I use both old and new CSS during migration?
**A:** Yes, but be careful about specificity conflicts. The compiled CSS should load last.

## 📞 Troubleshooting

### SCSS Won't Compile
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
```

### Styles Not Updating
```bash
# Clear compiled CSS and rebuild
./build-scss.sh clean
./build-scss.sh dev
```

### Missing Styles After Migration
1. Check that `compiled.css` is being loaded
2. Verify that old CSS files are removed from layout
3. Check browser developer tools for any 404 errors

### Build Script Permission Issues
```bash
chmod +x build-scss.sh
```

## 🎉 Benefits You'll Get

1. **Better Organization**: No more hunting through massive CSS files
2. **Consistency**: Design system ensures consistent spacing, colors, and typography
3. **Maintainability**: Modular architecture makes updates easier
4. **Performance**: Compiled CSS is optimized and compressed
5. **Developer Experience**: SCSS features like variables and mixins
6. **Future-Proof**: Easy to extend and customize as your app grows

---

**Ready to start?** Run `./build-scss.sh dev` and update your layout file! 🚀
