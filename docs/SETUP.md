# Documentation Setup Guide

## Quick GitHub Pages Setup

1. **Enable GitHub Pages**:
   - Repository Settings → Pages
   - Source: Deploy from branch
   - Branch: `main`, Folder: `/docs`

2. **Your documentation will be live at**:
   `https://tyfweb.github.io/two_license_management/`

## CSS Issue Resolution

The CSS issue has been resolved with:

✅ **Proper Theme Configuration** (`_config.yml`):
```yaml
theme: just-the-docs
remote_theme: just-the-docs/just-the-docs
```

✅ **Custom Styling** (`assets/css/style.scss`):
- TechWayFit blue branding (#2563eb)
- Professional table styling
- Print-optimized layouts
- Mobile responsive design

✅ **GitHub Actions** (`.github/workflows/pages.yml`):
- Automatic deployment on push to main
- Uses standard Jekyll build process

✅ **Simplified Dependencies** (`Gemfile`):
- GitHub Pages compatible
- Minimal required dependencies

## Verification Steps

1. Push changes to main branch
2. Check Actions tab for successful build
3. Access documentation at GitHub Pages URL
4. Verify CSS is loading properly

## Local Testing (Optional)

```bash
cd docs
bundle install
bundle exec jekyll serve
# Open http://localhost:4000/two_license_management/
```

## Troubleshooting

- **CSS not loading**: Check `assets/css/style.scss` has Jekyll front matter `---`
- **Build failing**: Check Actions tab for specific errors
- **Navigation broken**: Verify front matter in all `.md` files
