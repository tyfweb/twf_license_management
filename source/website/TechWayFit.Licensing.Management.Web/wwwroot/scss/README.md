# SCSS Architecture - TechWayFit License Management

This project uses a modern SCSS architecture following the **7-1 pattern** for better maintainability, scalability, and organization.

## 🏗️ Architecture Overview

The SCSS files are organized using the 7-1 architecture pattern:

```
scss/
│
├── abstracts/          # Sass tools and helpers
│   ├── _variables.scss # Global variables
│   ├── _functions.scss # Custom functions
│   ├── _mixins.scss    # Reusable mixins
│   └── _utilities.scss # Utility classes
│
├── base/               # Boilerplate code
│   ├── _reset.scss     # CSS reset/normalize
│   └── _animations.scss # Global animations
│
├── components/         # UI components
│   ├── _buttons.scss   # Button styles
│   ├── _cards.scss     # Card components
│   ├── _forms.scss     # Form elements
│   └── _stats-cards.scss # Dashboard stats cards
│
├── layout/             # Layout-related sections
│   ├── _header.scss    # Header/navigation
│   └── _sidebar.scss   # Sidebar navigation
│
├── pages/              # Page-specific styles
│   └── _dashboard.scss # Dashboard page styles
│
└── main.scss           # Main file that imports everything
```

## 🎨 Design System

### Colors
- **Primary**: `$primary` (#0066cc)
- **Success**: `$success` (#28a745)
- **Warning**: `$warning` (#ffc107)
- **Danger**: `$danger` (#dc3545)
- **Info**: `$info` (#17a2b8)

### Spacing Scale
- `$spacing-xs`: 0.25rem (4px)
- `$spacing-sm`: 0.5rem (8px)
- `$spacing-md`: 1rem (16px)
- `$spacing-lg`: 1.5rem (24px)
- `$spacing-xl`: 2rem (32px)
- `$spacing-2xl`: 3rem (48px)

### Typography
- `$font-size-xs`: 0.75rem (12px)
- `$font-size-sm`: 0.875rem (14px)
- `$font-size-base`: 1rem (16px)
- `$font-size-lg`: 1.125rem (18px)
- `$font-size-xl`: 1.25rem (20px)
- `$font-size-2xl`: 1.5rem (24px)

### Breakpoints
- `$mobile`: 320px
- `$tablet`: 768px
- `$desktop`: 1024px
- `$wide`: 1200px

## 🛠️ Setup & Compilation

### Prerequisites
- Node.js (v14 or higher)
- npm or yarn

### Installation
```bash
npm install
```

### Development
Watch for changes and compile SCSS automatically:
```bash
npm run scss:watch
```

### Build for Development
Compile SCSS with source maps and expanded output:
```bash
npm run scss:dev
```

### Build for Production
Compile SCSS with compressed output:
```bash
npm run scss:prod
```

## 📝 Usage Guidelines

### Variables
Use the predefined variables for consistency:
```scss
.my-component {
    color: $primary;
    padding: $spacing-md;
    font-size: $font-size-lg;
}
```

### Mixins
Use mixins for common patterns:
```scss
.my-button {
    @include button-variant($primary, $white);
    @include flex-center;
}
```

### Utility Classes
Use utility classes for quick styling:
```html
<div class="d-flex justify-content-between align-items-center p-md mb-lg">
    <h1 class="text-primary">Title</h1>
    <button class="btn btn-primary">Action</button>
</div>
```

## 🎯 Component Guidelines

### Creating New Components
1. Create a new file in `components/` folder
2. Follow the BEM naming convention
3. Use existing variables and mixins
4. Import the new file in `main.scss`

Example component structure:
```scss
// _my-component.scss
.my-component {
    // Base styles
    
    &__element {
        // Element styles
    }
    
    &--modifier {
        // Modifier styles
    }
    
    // States
    &:hover,
    &:focus {
        // Interactive states
    }
    
    // Responsive
    @include tablet {
        // Tablet styles
    }
    
    @include desktop {
        // Desktop styles
    }
}
```

## 🔧 Available Mixins

### Layout Mixins
- `@include flex-center` - Center content with flexbox
- `@include flex-between` - Space between with flexbox
- `@include absolute-center` - Absolutely center an element

### Button Mixins
- `@include button-variant($bg, $color)` - Create button variants
- `@include button-size($padding, $font-size)` - Button sizing

### Responsive Mixins
- `@include mobile { ... }` - Mobile styles
- `@include tablet { ... }` - Tablet and up
- `@include desktop { ... }` - Desktop and up
- `@include wide { ... }` - Wide screen and up

### Component Mixins
- `@include stats-card` - Apply stats card styling
- `@include card-shadow` - Add card shadow

## 📦 Available Utility Classes

### Spacing
- `.m-{size}` - Margin (xs, sm, md, lg, xl, 2xl)
- `.p-{size}` - Padding (xs, sm, md, lg, xl, 2xl)
- `.mt-{size}`, `.mr-{size}`, etc. - Directional spacing

### Display
- `.d-none`, `.d-block`, `.d-flex`, `.d-grid`
- `.justify-content-{value}`, `.align-items-{value}`

### Typography
- `.text-{color}` - Text colors
- `.text-{size}` - Text sizes
- `.font-weight-{weight}` - Font weights

### Background
- `.bg-{color}` - Background colors

## 🎨 Theme System

The SCSS architecture supports theming through CSS custom properties. All color variables are mapped to CSS custom properties, allowing for dynamic theme switching.

### Creating a New Theme
1. Define theme colors in `_variables.scss`
2. Update the CSS custom properties
3. Use the theme variables in components

## 🚀 Performance Considerations

- The compiled CSS is optimized for production with compression
- Unused utility classes are kept for flexibility
- CSS custom properties enable efficient theme switching
- Modular architecture allows for tree-shaking in the future

## 📖 Best Practices

1. **Use Variables**: Always use predefined variables for colors, spacing, and typography
2. **Follow BEM**: Use Block-Element-Modifier methodology for class naming
3. **Mobile First**: Write responsive styles mobile-first
4. **Modular Approach**: Keep components isolated and reusable
5. **Consistent Naming**: Follow the established naming conventions
6. **Documentation**: Comment complex calculations and non-obvious code

## 🔄 Migration from Existing CSS

To migrate from the existing CSS files:

1. **Gradual Migration**: Replace CSS imports one by one
2. **Test Thoroughly**: Ensure visual consistency during migration
3. **Update References**: Update HTML class names if needed
4. **Remove Old Files**: Clean up old CSS files after migration

### Current CSS Files to Replace
- `site.css` - Main application styles
- `licensehub.css` - License hub specific styles
- `product-management.css` - Product management styles

## 🤝 Contributing

When contributing to the SCSS codebase:
1. Follow the established architecture patterns
2. Use existing variables and mixins
3. Test changes across different screen sizes
4. Update documentation for new components
5. Run the build process before committing

## 📚 Resources

- [Sass Documentation](https://sass-lang.com/documentation)
- [7-1 Architecture Pattern](https://sass-guidelin.es/#the-7-1-pattern)
- [BEM Methodology](http://getbem.com/)
- [CSS Custom Properties](https://developer.mozilla.org/en-US/docs/Web/CSS/Using_CSS_custom_properties)
