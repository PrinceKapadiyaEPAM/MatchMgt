# Company Profile - Letterhead Editor Guide

## Overview
The Company Profile letterhead editor allows you to create and customize HTML headers that appear on all print views in your application. You can now edit the header design directly through a rich text editor without touching code.

## Features

### 1. **Rich Text Editor (Quill)**
- **Text Formatting**: Bold, Italic, Underline, Strikethrough
- **Font & Size**: Multiple fonts and sizes
- **Colors**: Text color and background color options
- **Alignment**: Left, Center, Right, Justify
- **Lists**: Ordered and unordered lists
- **Links & Images**: Embed external links and upload images
- **Advanced**: Subscript, Superscript, Indentation, Blockquotes, Code blocks

### 2. **Header Template Button**
A quick-start template is available that matches your company branding:
- Three-column layout with religious text
- Logo placeholder (left column)
- Company name with branding (center column)
- Contact information box (right column)
- Professional styling with brand colors

### 3. **Live HTML Editing**
The editor generates semantic HTML that can be directly used in print views. All content is automatically synced to the form before submission.

## How to Use

### Step 1: Navigate to Company Profile
- Log in as an Admin user
- Go to **Admin** → **Company Profile**

### Step 2: Access the Letterhead Editor
- In the right column, you'll see the **Letterhead** editor
- This is where you can design your header

### Step 3: Using the Template (Recommended)
1. Click the **+ Template** button in the card header
2. A professional header template will be inserted
3. Customize the content:
   - Update company name (replace "Mitshu")
   - Change phone numbers
   - Update address
   - Change email
   - Modify logo URL if needed
   - Adjust colors and fonts

### Step 4: Manual Editing
You can also manually create the header by:
1. Using the toolbar buttons to format text
2. Creating tables for layout
3. Inserting images
4. Adjusting colors and styling

### Step 5: Preview & Save
1. The editor shows a live preview
2. Click **Save** to save your letterhead
3. The letterhead will automatically appear on all print pages

## Template Customization Tips

### Changing Colors
- The template uses `#c0544b` (red/brown) for the brand color
- To change: Find and replace all occurrences of this hex code in the editor

### Adding Your Logo
- In the template, the logo is referenced from `/assets/Images/logo.png`
- Replace this URL with your logo path
- OR upload your image using the Image button in the toolbar

### Adjusting Spacing
- Use padding and margin properties to adjust spacing
- Example: `style="padding: 15px; margin-bottom: 20px;"`

### Font Sizes
- Use the size dropdown in the toolbar: Small, Normal, Large, Huge
- Or specify custom sizes: `font-size: 14px;`

### Making It Responsive
The editor supports responsive design. Use styles that work well when printed to PDF and on different paper sizes.

## Supported HTML Elements

The editor supports:
- Text formatting tags: `<b>`, `<i>`, `<u>`, `<s>`
- Structure: `<div>`, `<table>`, `<tr>`, `<td>`, `<p>`, `<span>`
- Lists: `<ul>`, `<ol>`, `<li>`
- Media: `<img>`, `<a>`
- Styling: Full `style` attribute support
- Unicode: Special characters like ॥

## Print Output

When you print or download documents as PDF:
1. The letterhead HTML is automatically rendered at the top
2. Document content appears below the letterhead
3. The layout is optimized for A4 paper size
4. Page breaks respect the letterhead positioning

## Example Use Cases

### Professional Letterhead
- Company name and branding
- Full contact information
- Company registration details (GSTIN, PAN, CIN)
- Website and social links

### Invoice Header
- Company logo
- Tagline or company motto
- Quick reference contact details
- Invoice-specific header

### Report Header
- Company title
- Report title
- Date generated
- Document type indicator

## Best Practices

1. **Keep it Simple**: Avoid overly complex designs that may not print well
2. **Use Web-Safe Fonts**: Arial, Helvetica, Georgia, Times New Roman
3. **Test Print**: Always do a test print before finalizing
4. **Optimize Images**: Use compressed images for faster loading
5. **Consistent Branding**: Match your company's brand guidelines
6. **Responsive Design**: Test how it looks on different screen sizes

## Troubleshooting

### Letterhead not showing on print?
- Ensure you clicked **Save** after editing
- Check that `LetterheadHtml` field is not empty
- Verify the print view includes `<partial name="_LetterheadPartial" />`

### Images not displaying?
- Verify the image path is correct and accessible
- Use absolute URLs or URLs relative to your domain
- Ensure image files are in the correct folder

### Styling not applied?
- Check for CSS conflicts
- Use inline styles (`style` attribute) for best results
- Avoid external CSS references if possible

### Special characters not showing?
- The editor supports Unicode characters like ॥
- Copy-paste directly into the editor
- Or use HTML entities: `&#8739;` for ॥

## Technical Details

### Where is the HTML stored?
- Database: `CompanyProfile.LetterheadHtml` column
- Retrieved by: `_LetterheadPartial.cshtml`
- Rendered as raw HTML: `@Html.Raw(profile.LetterheadHtml)`

### What happens to the HTML?
1. User edits in Quill editor
2. On form submit, Quill HTML is synced to hidden input
3. Controller receives `LetterheadHtml` via `CompanyProfileVM`
4. Saved to database
5. On print pages, retrieved and rendered using `Html.Raw()`

### Security Considerations
- The HTML is rendered as-is without sanitization
- Only Admin users can edit the letterhead
- Consider sanitizing if non-admin users will edit this in the future

## Admin Only Feature

This letterhead editor is restricted to Admin users:
```csharp
[Authorize(Roles = "Admin")]
public class CompanyController : Controller
```

Only administrators can access and modify company profiles.
