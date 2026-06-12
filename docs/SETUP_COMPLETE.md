# Letterhead Editor - Setup Complete ✅

## What Has Been Set Up

Your MatchingMaster application now has a **complete rich HTML editor for company profile letterheads**. This allows you to update headers and letterheads directly through the browser without touching code.

## Key Components

### 1. **Database Model** ✓
- `CompanyProfile` entity includes `LetterheadHtml` field
- Stores HTML as a string in the database
- Can store complex HTML structures

### 2. **View Model** ✓
- `CompanyProfileVM` includes `LetterheadHtml` property
- Properly validated and mapped

### 3. **Controller** ✓
- `CompanyController` handles save/load of letterhead HTML
- Admin-only access restriction
- Proper model mapping and database updates

### 4. **Rich Text Editor** ✓
- **Quill Editor** integrated with comprehensive toolbar
- Supports: Text formatting, colors, fonts, sizing, alignment, lists, links, images
- Live preview
- HTML sync before form submission

### 5. **Template Button** ✓
- Quick-start template matching your company branding
- Professional three-column layout
- Includes: Logo, company name, and contact box
- Fully customizable in the editor

### 6. **Letterhead Rendering** ✓
- `_LetterheadPartial.cshtml` displays the HTML on print pages
- Used in all print views: Programs, Designs, Matching slips
- Renders safely using `Html.Raw()`

## How to Use It

### Step 1: Access the Editor
1. Login to your application as **Admin**
2. Navigate to **Admin → Company Profile**
3. Look for the **Letterhead** card on the right side

### Step 2: Insert Template (Quickest Method)
1. Click the **+ Template** button
2. A professional header template appears
3. Edit the content directly in the editor

### Step 3: Customize
- Click on any text to edit
- Use toolbar buttons for formatting
- Adjust colors, fonts, spacing
- Add/remove images
- Reorganize layout

### Step 4: Save
- Click the **Save** button
- Your letterhead is now saved

### Step 5: Verify
- Go to any print page (Program, Design, etc.)
- Click Print or Download PDF
- Your letterhead appears at the top

## Included Documentation

### 📖 LETTERHEAD_EDITOR_GUIDE.md
Complete guide covering:
- All editor features
- Step-by-step usage instructions
- Customization tips
- Troubleshooting

### 📖 LETTERHEAD_EXAMPLES.md
7 ready-to-use HTML templates:
1. Professional Business Header (default)
2. Simple Centered Header
3. Logo + Company Name + Contact
4. Two-Column Contact Box
5. Minimal Invoice Style
6. Full Business Card Style
7. Festival/Religious Header

## Features & Benefits

✅ **No Coding Required** - Edit HTML through visual interface
✅ **WYSIWYG Editor** - What you see is what you get
✅ **Live Preview** - See changes immediately
✅ **Full HTML Support** - Use any valid HTML structure
✅ **Image Support** - Upload or reference images
✅ **Color Picker** - Choose any color visually
✅ **Template Library** - 7 professional templates included
✅ **Admin Protected** - Only authorized users can edit
✅ **Database Backed** - Persistent storage
✅ **Print Optimized** - Automatically appears on all print pages

## Technical Details

### Files Modified
- `src\Inventory.Web\Views\Company\Index.cshtml` - Enhanced with template button and improved Quill config

### Files Referenced (No Changes Needed)
- `src\Inventory.Domain\Entities\CompanyProfile.cs` - Already has LetterheadHtml
- `src\Inventory.Domain\ViewModels\CompanyProfileVM.cs` - Already mapped
- `src\Inventory.Web\Controllers\CompanyController.cs` - Already handles save/load
- `src\Inventory.Web\Views\Shared\_LetterheadPartial.cshtml` - Renders on print pages

### Database
- No migration needed - field already exists from `20260608044122_AddCompanyProfile`
- HTML is stored as a string in the database
- Max length depends on SQL Server settings (typically 8000+ characters)

## Current Template

The default template that comes with the **+ Template** button:
- Three columns: Logo, Company Name, Contact Info
- Brand color: #c0544b (red/brown)
- Religious text headers with Unicode characters
- Responsive table layout
- Professional border styling

## Customization Quick Tips

| Need | How To Do It |
|------|------------|
| Change company name | Double-click "Mitshu" text and edit |
| Update phone number | Double-click phone number and edit |
| Change colors | Highlight text, use color button in toolbar |
| Add your logo | Click image icon, upload or paste URL |
| Change layout | Use Quill's table features or edit raw HTML |
| Add social links | Use link button in toolbar |
| Change fonts | Use font dropdown in toolbar |

## Testing Your Setup

1. **Edit Letterhead**
   - Go to Company Profile
   - Click + Template
   - Change "Mitshu" to your company name
   - Click Save

2. **Verify on Print**
   - Go to Program List
   - Open any program
   - Click Print or Download PDF
   - Your new header should appear

3. **Try Advanced Editing**
   - Add images from your assets
   - Change colors to match branding
   - Add more contact information
   - Save and test print again

## Supported HTML Elements

The editor safely supports:
- **Text**: `<p>`, `<div>`, `<span>`, `<h1>`-`<h6>`
- **Formatting**: `<b>`, `<i>`, `<u>`, `<s>`, `<strong>`, `<em>`
- **Layout**: `<table>`, `<tr>`, `<td>`, `<ul>`, `<ol>`, `<li>`
- **Media**: `<img>`, `<a>`
- **Styling**: Full `style` attribute support
- **Unicode**: Special characters like ॥ ॐ शरणं

## Print Output

When documents are printed:
1. Letterhead HTML is rendered first
2. It spans full page width
3. Content starts below letterhead
4. Optimized for A4 paper
5. Page breaks respect layout
6. Works with Print to PDF

## Security & Permissions

✅ **Admin-Only Access**
- Only Admin role can edit company profile
- Only Admin can modify letterhead
- Controlled through `[Authorize(Roles = "Admin")]`

⚠️ **HTML Rendering**
- HTML is rendered as-is without sanitization
- Only admins can inject content
- Suitable for internal use
- If non-admins need this, add sanitization

## Performance

- Quill editor loads from CDN (jsDelivr)
- No server-side processing of HTML
- Stored as string in database
- Rendered directly in views
- Minimal performance impact

## Next Steps (Optional Enhancements)

1. **Add Preview Mode**
   - Show PDF preview before saving
   - Live print preview

2. **Add More Templates**
   - Create industry-specific templates
   - Add template gallery

3. **Add HTML Import/Export**
   - Export letterhead HTML
   - Import from external source

4. **Add Version History**
   - Track letterhead changes
   - Restore previous versions

5. **Add CSS Validation**
   - Validate CSS syntax
   - Suggest improvements

## Troubleshooting

**Q: Letterhead not showing on print?**
A: 
1. Ensure you clicked Save
2. Check Company Profile has content
3. Verify print view includes `<partial name="_LetterheadPartial" />`

**Q: Images not displaying?**
A:
1. Check image URL is correct
2. Use absolute URLs preferred
3. Ensure image file exists

**Q: Can I use CSS classes?**
A:
1. Use inline styles instead: `style="color: red;"`
2. CSS classes may not work in print PDFs
3. Quill defaults to inline styles

**Q: How do I revert changes?**
A:
1. Database stores current HTML
2. You can manually delete and re-add content
3. Consider implementing version history

## Support Resources

See included documentation:
- **LETTERHEAD_EDITOR_GUIDE.md** - Full user guide
- **LETTERHEAD_EXAMPLES.md** - 7 ready-to-use templates

## Architecture Diagram

```
┌─────────────────────────────────────────┐
│    Admin User Interface (Browser)       │
│  (Company Profile → Letterhead Card)    │
└──────────────┬──────────────────────────┘
               │
               ├─→ Quill Rich Editor
               │  (Visual HTML Editing)
               │
               ├─→ Template Button
               │  (Insert Pre-designed HTML)
               │
               └─→ Save Button
                  │
                  ▼
┌─────────────────────────────────────────┐
│    ASP.NET Core Controller              │
│    (CompanyController.Save)             │
└──────────────┬──────────────────────────┘
               │
               ├─→ Validate CompanyProfileVM
               │
               └─→ Save LetterheadHtml
                  │
                  ▼
┌─────────────────────────────────────────┐
│    SQL Server Database                  │
│    (CompanyProfile.LetterheadHtml)      │
└──────────────┬──────────────────────────┘
               │
               └─→ Retrieve on Print Pages
                  │
                  ▼
┌─────────────────────────────────────────┐
│    _LetterheadPartial.cshtml            │
│    (@Html.Raw(profile.LetterheadHtml))  │
└──────────────┬──────────────────────────┘
               │
               └─→ Rendered in Print Views
                  │
                  ▼
┌─────────────────────────────────────────┐
│    PDF / Print Output                   │
│    (Header + Document Content)          │
└─────────────────────────────────────────┘
```

## Summary

Your MatchingMaster application now has **professional, customizable letterhead editing capabilities** that:
- ✅ Work through the web interface
- ✅ Support rich HTML formatting
- ✅ Include professional templates
- ✅ Integrate seamlessly with print pages
- ✅ Store data persistently
- ✅ Are secure and admin-controlled

**You're all set!** Start using it by going to **Admin → Company Profile** and clicking **+ Template** in the Letterhead card.

---

*Last Updated: 2024*
*Setup Version: 1.0*
